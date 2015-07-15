using Microsoft.Graphics.Canvas;
using System;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Threading.Tasks;

namespace Stuart
{
    // Top level DOM type stores the photo plus a list of edits.
    public class Photo : Observable
    {
        CanvasBitmap sourceBitmap;

        public ObservableCollection<EditGroup> Edits { get; } = new ObservableCollection<EditGroup>();

        public Vector2 Size => sourceBitmap.Size.ToVector2();


        public Effect SelectedEffect
        {
            get { return selectedEffect; }
            set { SetField(ref selectedEffect, value); }
        }

        Effect selectedEffect;


        public Photo()
        {
            Edits.CollectionChanged += (sender, e) => NotifyCollectionChanged(sender, e, "Edits");

            Edits.Add(new EditGroup(this));
        }


        public async Task Load(CanvasDevice device, string filename)
        {
            sourceBitmap = await CanvasBitmap.LoadAsync(device, filename);
        }


        public void Draw(CanvasDrawingSession ds)
        {
            ICanvasImage image = sourceBitmap;

            foreach (var edit in Edits)
            {
                image = edit.Apply(image);
            }

            ds.Units = CanvasUnits.Pixels;

            ds.DrawImage(image);
        }
    }
}
