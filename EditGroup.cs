using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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

        public Region Region { get; private set; }

        public ObservableCollection<Effect> Effects { get; } = new ObservableCollection<Effect>();


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


        public float RegionFeather
        {
            get { return regionFeather; }
            set { SetField(ref regionFeather, value); }
        }

        float regionFeather;


        public float RegionExpand
        {
            get { return regionExpand; }
            set { SetField(ref regionExpand, value); }
        }

        float regionExpand;


        public float RegionSimplify
        {
            get { return regionSimplify; }
            set { SetField(ref regionSimplify, value); }
        }

        float regionSimplify;


        public EditGroup(Photo parent)
        {
            Parent = parent;

            Region = new Region(this);

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

                if (Region.Mask != null)
                {
                    ICanvasImage mask = Region.Mask;

                    // Feather the selection?
                    if (RegionFeather > 0)
                    {
                        mask = new GaussianBlurEffect
                        {
                            Source = mask,
                            BlurAmount = RegionFeather,
                            BorderMode = EffectBorderMode.Hard
                        };
                    }

                    // Mask out only the selected region of our processed image.
                    var selectedRegion = new CompositeEffect
                    {
                        Sources = { image, mask },
                        Mode = CanvasComposite.DestinationIn
                    };

                    // Blend the selected region over the original unprocessed image.
                    image = new CompositeEffect
                    {
                        Sources = { originalImage, selectedRegion }
                    };
                }
            }

            return image;
        }
    }
}
