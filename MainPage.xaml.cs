using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
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

        EditGroup editingRegion;
        readonly List<Vector2> regionPoints = new List<Vector2>();

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
            {
                if (currentFile == null)
                {
                    Application.Current.Exit();
                }

                return;
            }

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
            if (currentFile == null)
                return;

            var drawingSession = args.DrawingSession;

            drawingSession.Units = CanvasUnits.Pixels;
            drawingSession.Blend = CanvasBlend.Copy;

            photo.Draw(args.DrawingSession);

            foreach (var edit in photo.Edits)
            {
                edit.DisplayRegionMask(drawingSession, scrollView.ZoomFactor, editingRegion != null);
            }

            if (editingRegion != null)
            {
                editingRegion.DisplayRegionEditInProgress(drawingSession, regionPoints, scrollView.ZoomFactor);
            }

#if DEBUG
            drawingSession.Blend = CanvasBlend.SourceOver;
            drawingSession.DrawText((++drawCount).ToString(), 0, 0, Colors.Cyan);
#endif
        }


        void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (editingRegion != null)
                return;

            // If any of the edit groups is in edit region mode, set that as our current region.
            editingRegion = photo.Edits.SingleOrDefault(edit => edit.IsEditingRegion);

            if (editingRegion == null)
                return;

            // Add the start point.
            regionPoints.Add(e.GetCurrentPoint(canvas).Position.ToVector2());

            // Set the manipulation mode so we grab all input, bypassing our parent ScrollViewer.
            canvas.ManipulationMode = ManipulationModes.All;
            canvas.CapturePointer(e.Pointer);

            canvas.Invalidate();
            e.Handled = true;
        }


        void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (editingRegion == null)
                return;

            editingRegion.EditRegionMask(regionPoints, scrollView.ZoomFactor);

            editingRegion = null;
            regionPoints.Clear();

            // Restore the manipulation mode so input goes to the parent ScrollViewer again.
            canvas.ManipulationMode = ManipulationModes.System;
            canvas.ReleasePointerCapture(e.Pointer);

            canvas.Invalidate();
            e.Handled = true;
        }


        void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (editingRegion == null)
                return;

            // Add points to the edit region.
            regionPoints.AddRange(e.GetIntermediatePoints(canvas).Select(point => point.Position.ToVector2()));

            canvas.Invalidate();
            e.Handled = true;
        }


        void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.A || e.Key == VirtualKey.Z)
            {
                var currentZoom = scrollView.ZoomFactor;
                var newZoom = currentZoom;

                if (e.Key == VirtualKey.A)
                    newZoom /= 0.9f;
                else
                    newZoom *= 0.9f;

                newZoom = Math.Max(newZoom, scrollView.MinZoomFactor);
                newZoom = Math.Min(newZoom, scrollView.MaxZoomFactor);

                var currentPan = new Vector2((float)scrollView.HorizontalOffset,
                                             (float)scrollView.VerticalOffset);

                var centerOffset = new Vector2((float)scrollView.ViewportWidth,
                                               (float)scrollView.ViewportHeight) / 2;

                var newPan = ((currentPan + centerOffset) * newZoom / currentZoom) - centerOffset;

                scrollView.ChangeView(newPan.X, newPan.Y, newZoom);
            }
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


        void Background_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            photo.SelectedEffect = null;
        }
    }
}
