using Microsoft.Graphics.Canvas;
using System;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace Stuart
{
    // Top level DOM type stores the photo plus a list of edits.
    public class Photo : Observable
    {
        public CanvasBitmap SourceBitmap
        {
            get { return sourceBitmap; }
            set { SetField(ref sourceBitmap, value); }
        }

        CanvasBitmap sourceBitmap;


        public Vector2 Size => sourceBitmap.Size.ToVector2();


        public ObservableCollection<EditGroup> Edits { get; } = new ObservableCollection<EditGroup>();


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


        public async Task Load(CanvasDevice device, StorageFile file)
        {
            await LoadSourceBitmap(device, file);

            Edits.Clear();
            Edits.Add(new EditGroup(this));

            SelectedEffect = null;
        }


        public async Task ReloadAfterDeviceLost(CanvasDevice device, StorageFile file)
        {
            await LoadSourceBitmap(device, file);

            foreach (var edit in Edits)
            {
                edit.RecoverAfterDeviceLost();
            }
        }


        async Task LoadSourceBitmap(CanvasDevice device, StorageFile file)
        {
            using (var stream = await file.OpenReadAsync())
            {
                SourceBitmap = await CanvasBitmap.LoadAsync(device, stream);
            }
        }


        public async Task Save(StorageFile file)
        {
            var image = GetImage();

            // Measure the extent of the image (which may be cropped).
            Rect imageBounds;

            using (var commandList = new CanvasCommandList(sourceBitmap.Device))
            using (var drawingSession = commandList.CreateDrawingSession())
            {
                imageBounds = image.GetBounds(drawingSession);
            }

            // Rasterize the image into a rendertarget.
            using (var renderTarget = new CanvasRenderTarget(sourceBitmap.Device, (float)imageBounds.Width, (float)imageBounds.Height, 96))
            {
                using (var drawingSession = renderTarget.CreateDrawingSession())
                {
                    drawingSession.Blend = CanvasBlend.Copy;

                    drawingSession.DrawImage(image, -(float)imageBounds.X, -(float)imageBounds.Y);
                }

                // Save it out.
                var format = file.FileType.Equals(".png", StringComparison.OrdinalIgnoreCase) ? CanvasBitmapFileFormat.Png : CanvasBitmapFileFormat.Jpeg;

                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await renderTarget.SaveAsync(stream, format);
                }
            }
        }


        public ICanvasImage GetImage()
        {
            ICanvasImage image = sourceBitmap;

            foreach (var edit in Edits)
            {
                image = edit.Apply(image);
            }

            return image;
        }
    }
}
