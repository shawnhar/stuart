using Microsoft.Graphics.Canvas;
using System.Linq;

namespace Stuart
{
    // Caches expensive image effect results in a rendertarget, to minimize repeated recomputation.
    class CachedImage
    {
        CanvasRenderTarget cachedImage;

        bool isCacheValid;
        object[] cacheKeys;


        public ICanvasImage Get(params object[] keys)
        {
            return (isCacheValid && keys.SequenceEqual(cacheKeys)) ? cachedImage : null;
        }


        public ICanvasImage Cache(Photo photo, ICanvasImage image, params object[] keys)
        {
            if (cachedImage == null)
            {
                cachedImage = new CanvasRenderTarget(photo.SourceBitmap.Device, photo.Size.X, photo.Size.Y, 96);
            }

            using (var drawingSession = cachedImage.CreateDrawingSession())
            {
                drawingSession.Blend = CanvasBlend.Copy;
                drawingSession.DrawImage(image);
            }

            isCacheValid = true;
            cacheKeys = keys;

            return cachedImage;
        }


        public void Reset()
        {
            isCacheValid = false;
        }
    }
}
