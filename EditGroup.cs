using System;
using System.Collections.ObjectModel;
using Microsoft.Graphics.Canvas;

namespace Stuart
{
    // DOM type representing a group of effects that are applied to a region of the photo.
    public class EditGroup : Observable, IDisposable
    {
        public Photo Parent { get; private set; }

        public ObservableCollection<Effect> Effects { get; } = new ObservableCollection<Effect>();


        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetField(ref isEnabled, value); }
        }

        bool isEnabled = true;


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
                foreach (var effect in Effects)
                {
                    image = effect.Apply(image);
                }
            }

            return image;
        }
    }
}
