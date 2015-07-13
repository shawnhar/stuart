using System;
using Microsoft.Graphics.Canvas;
using System.Threading.Tasks;
using System.Numerics;

namespace Stuart
{
    class Photo
    {
        CanvasDevice device;
        CanvasBitmap sourceBitmap;


        public Vector2 Size
        {
            get { return sourceBitmap.Size.ToVector2(); }
        }


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
