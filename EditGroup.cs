﻿using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using Windows.UI;

namespace Stuart
{
    public enum RegionSelectionMode
    {
        Rectangle,
        Ellipse,
        Freehand,
        MagicWand
    }


    // DOM type representing a group of effects that are applied to a region of the photo.
    public class EditGroup : Observable, IDisposable
    {
        public Photo Parent { get; private set; }

        public ObservableCollection<Effect> Effects { get; } = new ObservableCollection<Effect>();

        CanvasRenderTarget regionMask;

        CanvasBitmap SourceBitmap => Parent.SourceBitmap;


        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetField(ref isEnabled, value); }
        }

        bool isEnabled = true;


        public bool IsEditingRegion
        {
            get { return isEditingRegion; }

            set
            {
                SetField(ref isEditingRegion, value);

                // There can be only one!
                if (value)
                {
                    foreach (var edit in Parent.Edits.Where(e => e != this))
                    {
                        edit.IsEditingRegion = false;
                    }
                }
            }
        }

        bool isEditingRegion;


        public RegionSelectionMode RegionSelectionMode { get; set; }

        public bool RegionAdd { get; set; }
        public bool RegionSubtract { get; set; }


        public int RegionDilate
        {
            get { return regionDilate; }
            set { SetField(ref regionDilate, value); }
        }

        int regionDilate;


        public float RegionFeather
        {
            get { return regionFeather; }
            set { SetField(ref regionFeather, value); }
        }

        float regionFeather;


        public EditGroup(Photo parent)
        {
            Parent = parent;

            Effects.CollectionChanged += (sender, e) => NotifyCollectionChanged(sender, e, "Effects");

            Effects.Add(new Effect(this));
        }


        public void Dispose()
        {
            Parent.Edits.Remove(this);
        }


        public ICanvasImage Apply(ICanvasImage image)
        {
            if (IsEnabled)
            {
                var originalImage = image;

                // Apply all our effects in turn.
                foreach (var effect in Effects)
                {
                    image = effect.Apply(image);
                }

                // Mask so these effects only alter a specific region of the image?
                if (regionMask != null)
                {
                    var selectedRegion = new CompositeEffect
                    {
                        Sources = { image, GetRegionMask() },
                        Mode = CanvasComposite.DestinationIn
                    };

                    image = new CompositeEffect
                    {
                        Sources = { originalImage, selectedRegion }
                    };
                }
            }

            return image;
        }


        public ICanvasImage GetRegionMask()
        {
            ICanvasImage mask = regionMask;

            // Expand or contract the selection?
            if (regionDilate != 0)
            {
                mask = new MorphologyEffect
                {
                    Source = mask,
                    Mode = (regionDilate > 0) ? MorphologyEffectMode.Dilate : MorphologyEffectMode.Erode,
                    Height = Math.Abs(regionDilate),
                    Width = Math.Abs(regionDilate)
                };
            }

            // Feather the selection?
            if (regionFeather > 0)
            {
                mask = new GaussianBlurEffect
                {
                    Source = mask,
                    BlurAmount = regionFeather,
                    BorderMode = EffectBorderMode.Hard
                };
            }

            return mask;
        }


        public void EditRegionMask(List<Vector2> points, float zoomFactor)
        {
            // Demand-create our region mask image.
            if (regionMask == null)
            {
                var bitmapSize = SourceBitmap.Size.ToVector2();

                regionMask = new CanvasRenderTarget(SourceBitmap.Device, bitmapSize.X, bitmapSize.Y, 96);
            }

            // Prepare an image holding the edit to be applied.
            ICanvasImage editMask;

            if (RegionSelectionMode == RegionSelectionMode.MagicWand)
            {
                // Magic wand selection is already an image.
                editMask = GetMagicWandMask(points, zoomFactor);
            }
            else
            {
                // Draw selection geometry into a command list.
                var commandList = new CanvasCommandList(regionMask.Device);

                using (var drawingSession = commandList.CreateDrawingSession())
                {
                    var geometry = GetSelectionGeometry(drawingSession, points);

                    drawingSession.FillGeometry(geometry, Colors.White);
                }

                editMask = commandList;
            }

            // Apply the edit.
            using (var drawingSession = regionMask.CreateDrawingSession())
            {
                if (!RegionAdd && !RegionSubtract)
                {
                    drawingSession.Clear(Colors.Transparent);
                }

                // Add modes use standard SourceOver blending.
                CanvasComposite compositeMode = CanvasComposite.SourceOver;

                // Subtract modes use Xor (if add+subtract are both set) or DestinationOut (regular subtract).
                if (RegionSubtract)
                {
                    compositeMode = RegionAdd ? CanvasComposite.Xor : CanvasComposite.DestinationOut;
                }

                drawingSession.DrawImage(editMask, Vector2.Zero, regionMask.Bounds, 1, CanvasImageInterpolation.Linear, compositeMode);
            }
        }


        public void DisplayRegionSelection(CanvasDrawingSession drawingSession, List<Vector2> points, float zoomFactor)
        {
            if (RegionSelectionMode == RegionSelectionMode.MagicWand)
            {
                // Display a magic wand selection.
                var mask = GetMagicWandMask(points, zoomFactor);
                var border = GetSelectionBorder(mask, zoomFactor);

                drawingSession.Blend = CanvasBlend.Add;
                drawingSession.DrawImage(mask, Vector2.Zero, SourceBitmap.Bounds, 0.25f);

                drawingSession.Blend = CanvasBlend.SourceOver;
                drawingSession.DrawImage(border);
            }
            else
            {
                // Display a geometric shape selection.
                var geometry = GetSelectionGeometry(drawingSession, points);

                drawingSession.Blend = CanvasBlend.Add;
                drawingSession.FillGeometry(geometry, Color.FromArgb(0x20, 0xFF, 0xFF, 0xFF));

                drawingSession.Blend = CanvasBlend.SourceOver;
                drawingSession.DrawGeometry(geometry, Colors.Magenta, 1f / zoomFactor);
            }
        }


        ICanvasImage GetMagicWandMask(List<Vector2> points, float zoomFactor)
        {
            // What color did the user click on?
            Vector2 clickPoint = Vector2.Clamp(points.First(), Vector2.Zero, SourceBitmap.Size.ToVector2() - Vector2.One);

            Color clickColor = SourceBitmap.GetPixelColors((int)clickPoint.X, (int)clickPoint.Y, 1, 1).Single();

            // How far they have dragged = selection tolerance.
            float dragDistance = Vector2.Distance(points.First(), points.Last());

            float chromaTolerance = Math.Min(dragDistance / 512 * zoomFactor, 1);

            return new ColorMatrixEffect
            {
                Source = new ChromaKeyEffect
                {
                    Source = SourceBitmap,
                    Color = clickColor,
                    Tolerance = chromaTolerance,
                    InvertAlpha = true
                },

                ColorMatrix = new Matrix5x4
                {
                    // Preserve alpha.
                    M44 = 1,

                    // Set RGB = white.
                    M51 = 1,
                    M52 = 1,
                    M53 = 1,
                }
            };
        }


        ICanvasImage GetSelectionBorder(ICanvasImage mask, float zoomFactor)
        {
            // Scale so our border will always be the same width no matter how the image is zoomed.
            var scaleToCurrentZoom = new ScaleEffect
            {
                Source = mask,
                Scale = new Vector2(zoomFactor)
            };

            // Find edges of the selection.
            var detectEdges = new EdgeDetectionEffect
            {
                Source = scaleToCurrentZoom,
                Amount = 0.1f
            };

            // Colorize.
            var colorItMagenta = new ColorMatrixEffect
            {
                Source = detectEdges,

                ColorMatrix = new Matrix5x4
                {
                    M11 = 1,
                    M13 = 1,
                    M14 = 1,
                }
            };

            // Scale back to the original size.
            return new ScaleEffect
            {
                Source = colorItMagenta,
                Scale = new Vector2(1 / zoomFactor)
            };
        }


        CanvasGeometry GetSelectionGeometry(ICanvasResourceCreator resourceCreator, List<Vector2> points)
        {
            Vector2 start = points.First();
            Vector2 end = points.Last();

            switch (RegionSelectionMode)
            {
                case RegionSelectionMode.Rectangle:
                    {
                        Vector2 min = Vector2.Min(start, end);
                        Vector2 size = Vector2.Abs(start - end);

                        return CanvasGeometry.CreateRectangle(resourceCreator, min.X, min.Y, size.X, size.Y);
                    }

                case RegionSelectionMode.Ellipse:
                    {
                        Vector2 center = (start + end) / 2;
                        Vector2 radius = Vector2.Abs(start - end) / 2;

                        return CanvasGeometry.CreateEllipse(resourceCreator, center, radius.X, radius.Y);
                    }

                case RegionSelectionMode.Freehand:
                    {
                        return CanvasGeometry.CreatePolygon(resourceCreator, points.ToArray());
                    }

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
