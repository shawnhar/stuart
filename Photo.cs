using System;
using Microsoft.Graphics.Canvas;

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


        public async void Load(string filename)
        {
            sourceBitmap = await CanvasBitmap.LoadAsync(device, filename);
        }
    }
}
