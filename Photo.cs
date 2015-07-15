using Microsoft.Graphics.Canvas;
using System;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Storage.Streams;

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
        }


        public async Task Load(CanvasDevice device, IRandomAccessStream stream)
        {
            sourceBitmap = await CanvasBitmap.LoadAsync(device, stream);

            Edits.Clear();
            Edits.Add(new EditGroup(this));

            SelectedEffect = null;
        }


        public async Task Save(IRandomAccessStream stream, CanvasBitmapFileFormat format)
        {
            using (var renderTarget = new CanvasRenderTarget(sourceBitmap.Device, Size.X, Size.Y, 96))
            {
                using (var drawingSession = renderTarget.CreateDrawingSession())
                {
                    Draw(drawingSession);
                }

                await renderTarget.SaveAsync(stream, format);
            }
        }


        public void Draw(CanvasDrawingSession drawingSession)
        {
            if (sourceBitmap == null)
                return;

            ICanvasImage image = sourceBitmap;

            foreach (var edit in Edits)
            {
                image = edit.Apply(image);
            }

            drawingSession.Units = CanvasUnits.Pixels;
            drawingSession.Blend = CanvasBlend.Copy;

            drawingSession.DrawImage(image);
        }
    }
}
