using Microsoft.Graphics.Canvas;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Storage;

namespace Stuart
{
    // Top level DOM type stores the photo plus a list of edits.
    public class Photo : Observable
    {
        public CanvasBitmap SourceBitmap
        {
            get { return sourceBitmap; }
            private set { SetField(ref sourceBitmap, value); }
        }

        CanvasBitmap sourceBitmap;

        DirectXPixelFormat bitmapFormat;
        byte[] bitmapData;


        public Vector2 Size { get; private set; }


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
            using (var stream = await file.OpenReadAsync())
            {
                SourceBitmap = await CanvasBitmap.LoadAsync(device, stream);
            }

            bitmapFormat = sourceBitmap.Format;
            bitmapData = sourceBitmap.GetPixelBytes();

            Size = sourceBitmap.Size.ToVector2();

            Edits.Clear();
            Edits.Add(new EditGroup(this));

            SelectedEffect = null;
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
                    stream.Size = 0;

                    await renderTarget.SaveAsync(stream, format);
                }
            }
        }


        public void RecoverAfterDeviceLost(CanvasDevice device)
        {
            SourceBitmap = CanvasBitmap.CreateFromBytes(device, bitmapData, (int)Size.X, (int)Size.Y, bitmapFormat);

            foreach (var edit in Edits)
            {
                edit.RecoverAfterDeviceLost();
            }
        }


        public void SaveSuspendedState(BinaryWriter writer)
        {
            writer.Write((int)bitmapFormat);
            writer.WriteByteArray(bitmapData);

            writer.WriteVector2(Size);

            writer.WriteCollection(Edits, edit => edit.SaveSuspendedState(writer));
        }


        public void RestoreSuspendedState(CanvasDevice device, BinaryReader reader)
        {
            bitmapFormat = (DirectXPixelFormat)reader.ReadInt32();
            bitmapData = reader.ReadByteArray();

            Size = reader.ReadVector2();

            reader.ReadCollection(Edits, () => EditGroup.RestoreSuspendedState(this, reader));

            RecoverAfterDeviceLost(device);
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
