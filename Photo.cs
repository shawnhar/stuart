using System;
using Microsoft.Graphics.Canvas;
using System.Threading.Tasks;

namespace Stuart
{
    class Photo
    {
        CanvasDevice device;
        CanvasBitmap sourceBitmap;


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
