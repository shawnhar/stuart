using System;
using Microsoft.Graphics.Canvas;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Stuart
{
    class Photo
    {
        CanvasDevice device;
        CanvasBitmap sourceBitmap;


        public Size Size { get { return sourceBitmap.Size; } }


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
            ds.DrawImage(sourceBitmap);
        }
    }
}
