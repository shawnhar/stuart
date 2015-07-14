using Microsoft.Graphics.Canvas;
using System;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Threading.Tasks;

namespace Stuart
{
    class Photo : Observable
    {
        // Fields.
        CanvasDevice device;
        CanvasBitmap sourceBitmap;


        // Properties.
        public ObservableCollection<PhotoEdit> Edits { get; } = new ObservableCollection<PhotoEdit>();

        public Vector2 Size => sourceBitmap.Size.ToVector2();


        // Methods.
        public Photo(CanvasDevice device)
        {
            this.device = device;

            Edits.CollectionChanged += (sender, e) => NotifyCollectionChanged(sender, e, "Edits");

            Edits.Add(new PhotoEdit(this));
        }


        public async Task Load(string filename)
        {
            sourceBitmap = await CanvasBitmap.LoadAsync(device, filename);
        }


        public void Draw(CanvasDrawingSession ds)
        {
            ds.Units = CanvasUnits.Pixels;

            ds.DrawImage(sourceBitmap);
        }
    }
}
