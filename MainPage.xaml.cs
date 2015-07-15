using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Stuart
{
    // UI codebehind for the main application page.
    public sealed partial class MainPage : Page
    {
        Photo photo = new Photo();

        StorageFile currentFile;

#if DEBUG
        int drawCount;
#endif


        public MainPage()
        {
            this.InitializeComponent();

            DataContext = photo;

            photo.PropertyChanged += Photo_PropertyChanged;
        }


        async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                FileTypeFilter = { ".jpg", ".jpeg", ".png" }
            };

            var file = await picker.PickSingleFileAsync();

            if (file == null)
                return;

            try
            {
                using (var stream = await file.OpenReadAsync())
                {
                    await photo.Load(canvas.Device, stream);
                }

                currentFile = file;

                ZoomToFitPhoto();
            }
            catch
            {
                await new MessageDialog("Error loading photo").ShowAsync();
            }
        }


        async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentFile == null)
                return;

            var picker = new FileSavePicker
            {
                SuggestedSaveFile = currentFile,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                DefaultFileExtension = ".jpg",
                FileTypeChoices = { { "Image files", new List<string> { ".jpg", ".jpeg", ".png" } } }
            };

            var file = await picker.PickSaveFileAsync();

            if (file == null)
                return;

            try
            {
                var format = file.FileType.Equals(".png", StringComparison.OrdinalIgnoreCase) ? CanvasBitmapFileFormat.Png : CanvasBitmapFileFormat.Jpeg;

                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await photo.Save(stream, format);
                }

                currentFile = file;
            }
            catch
            {
                await new MessageDialog("Error saving photo").ShowAsync();
            }
        }


        void ZoomToFitPhoto()
        {
            // Convert the photo size from pixels to dips.
            var photoSize = photo.Size;

            photoSize.X = canvas.ConvertPixelsToDips((int)photoSize.X);
            photoSize.Y = canvas.ConvertPixelsToDips((int)photoSize.Y);

            // Size the CanvasControl to exactly fit the image.
            canvas.Width = photoSize.X;
            canvas.Height = photoSize.Y;

            // Zoom so the whole image is visible.
            var viewSize = new Vector2((float)scrollView.ActualWidth, (float)scrollView.ActualHeight);
            var sizeRatio =  viewSize / photoSize;
            var zoomFactor = Math.Min(sizeRatio.X, sizeRatio.Y) * 0.95f;

            scrollView.ChangeView(0, 0, zoomFactor);
        }


        void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            photo.Draw(args.DrawingSession);

#if DEBUG
            args.DrawingSession.DrawText((++drawCount).ToString(), 0, 0, Colors.Cyan);
#endif
        }


        void Photo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedEffect":
                    break;

                default:
                    canvas.Invalidate();
                    break;
            }
        }


        void NewEdit_Click(object sender, RoutedEventArgs e)
        {
            photo.Edits.Add(new EditGroup(photo));
        }


        void EditList_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            e.Data.Properties.Add("DragItems", e.Items.ToArray());
        }


        void TrashCan_DragEnter(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }


        void TrashCan_Drop(object sender, DragEventArgs e)
        {
            var items = (object[])e.Data.GetView().Properties["DragItems"];

            foreach (IDisposable item in items)
            {
                item.Dispose();
            }
        }


        void Background_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            photo.SelectedEffect = null;
        }
    }
}
