using System;
using Microsoft.Graphics.Canvas;
using System.Threading.Tasks;
using System.Numerics;
using System.Collections.ObjectModel;

namespace Stuart
{
    class Photo
    {
        CanvasDevice device;
        CanvasBitmap sourceBitmap;

        public ObservableCollection<string> Edits { get; } = new ObservableCollection<string>();

        public Vector2 Size => sourceBitmap.Size.ToVector2();


        public Photo(CanvasDevice device)
        {
            this.device = device;
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
