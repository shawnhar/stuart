using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;

namespace Stuart
{
    public enum SelectionMode
    {
        Rectangle,
        Ellipse,
        Freehand,
        MagicWand
    }


    public enum SelectionOperation
    {
        Replace,
        Add,
        Subtract,
        Invert
    }


    // DOM type representing a group of effects that are applied to a region of the photo.
    public class EditGroup : Observable, IDisposable
    {
        public Photo Parent { get; private set; }

        public ObservableCollection<Effect> Effects { get; } = new ObservableCollection<Effect>();

        CanvasRenderTarget regionMask;

        byte[] currentRegionMask;
        byte[] previousRegionMask;

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


        public bool ShowRegion
        {
            get { return showRegion; }
            set { SetField(ref showRegion, value); }
        }

        bool showRegion = true;


        public SelectionMode RegionSelectionMode { get; set; }
        public SelectionOperation RegionSelectionOperation { get; set; }


        public float RegionFeather
        {
            get { return regionFeather; }
            set { SetField(ref regionFeather, value); }
        }

        float regionFeather;


        public int RegionDilate
        {
            get { return regionDilate; }
            set { SetField(ref regionDilate, value); }
        }

        int regionDilate;


        public bool CanUndo
        {
            get { return canUndo; }
            set { SetField(ref canUndo, value); }
        }

        bool canUndo;


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
                Rect? bounds = null;

                // Apply all our effects in turn.
                foreach (var effect in Effects)
                {
                    image = effect.Apply(image, ref bounds);
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

                    if (bounds.HasValue)
                    {
                        image = new CropEffect
                        {
                            Source = image,
                            SourceRectangle = bounds.Value
                        };
                    }
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
                    Source = new BorderEffect { Source = mask },
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
            if (regionMask == null)
            {
                // Demand-create our region mask image.
                regionMask = new CanvasRenderTarget(SourceBitmap.Device, Parent.Size.X, Parent.Size.Y, 96);
            }
            else
            {
                // Back up the previous mask, to support undo.
                previousRegionMask = currentRegionMask;
            }

            // Prepare an ICanvasImage holding the edit to be applied.
            ICanvasImage editMask;

            if (RegionSelectionMode == SelectionMode.MagicWand)
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
                CanvasComposite compositeMode;

                switch (RegionSelectionOperation)
                {
                    case SelectionOperation.Replace:
                        drawingSession.Clear(Colors.Transparent);
                        compositeMode = CanvasComposite.SourceOver;
                        break;

                    case SelectionOperation.Add:
                        compositeMode = CanvasComposite.SourceOver;
                        break;

                    case SelectionOperation.Subtract:
                        compositeMode = CanvasComposite.DestinationOut;
                        break;

                    case SelectionOperation.Invert:
                        compositeMode = CanvasComposite.Xor;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                drawingSession.DrawImage(editMask, Vector2.Zero, regionMask.Bounds, 1, CanvasImageInterpolation.Linear, compositeMode);
            }

            // Back up the mask, so we can recover from lost devices.
            currentRegionMask = regionMask.GetPixelBytes();

            CanUndo = true;
        }


        public void UndoRegionEdit()
        {
            if (previousRegionMask != null)
            {
                regionMask.SetPixelBytes(previousRegionMask);
                currentRegionMask = previousRegionMask;
                previousRegionMask = null;
            }
            else
            {
                regionMask.Dispose();
                regionMask = null;
                currentRegionMask = null;
            }

            CanUndo = false;
        }


        public bool DisplayRegionMask(CanvasDrawingSession drawingSession, float zoomFactor, bool editInProgress)
        {
            if (!IsEnabled || !IsEditingRegion || !ShowRegion || regionMask == null)
                return false;

            if (editInProgress && RegionSelectionOperation == SelectionOperation.Replace)
                return false;

            drawingSession.Blend = CanvasBlend.SourceOver;

            if (!editInProgress)
            {
                // Gray out everything outside the region.
                var mask = new ColorMatrixEffect
                {
                    Source = GetRegionMask(),

                    ColorMatrix = new Matrix5x4
                    {
                        // Set RGB = gray.
                        M51 = 0.5f,
                        M52 = 0.5f,
                        M53 = 0.5f,

                        // Invert and scale the mask alpha.
                        M44 = -0.75f,
                        M54 = 0.75f,
                    }
                };

                drawingSession.DrawImage(mask);
            }

            // Magenta region border.
            var border = GetSelectionBorder(regionMask, zoomFactor);

            drawingSession.DrawImage(border);

            return true;
        }


        public void DisplayRegionEditInProgress(CanvasDrawingSession drawingSession, List<Vector2> points, float zoomFactor)
        {
            if (RegionSelectionMode == SelectionMode.MagicWand)
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
                case SelectionMode.Rectangle:
                    {
                        Vector2 min = Vector2.Min(start, end);
                        Vector2 size = Vector2.Abs(start - end);

                        return CanvasGeometry.CreateRectangle(resourceCreator, min.X, min.Y, size.X, size.Y);
                    }

                case SelectionMode.Ellipse:
                    {
                        Vector2 center = (start + end) / 2;
                        Vector2 radius = Vector2.Abs(start - end) / 2;

                        return CanvasGeometry.CreateEllipse(resourceCreator, center, radius.X, radius.Y);
                    }

                case SelectionMode.Freehand:
                    {
                        return CanvasGeometry.CreatePolygon(resourceCreator, points.ToArray());
                    }

                default:
                    throw new NotSupportedException();
            }
        }


        public void RecoverAfterDeviceLost()
        {
            if (regionMask != null)
            {
                var size = regionMask.Size.ToVector2();

                regionMask.Dispose();
                regionMask = new CanvasRenderTarget(SourceBitmap.Device, size.X, size.Y, 96);
                regionMask.SetPixelBytes(currentRegionMask);
            }
        }
    }
}
