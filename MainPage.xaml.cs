using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
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

        float? lastDrawnZoomFactor;


        public MainPage()
        {
            this.InitializeComponent();

            DataContext = photo;

            photo.PropertyChanged += Photo_PropertyChanged;

            // Hide the status bar.
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var action = StatusBar.GetForCurrentView().HideAsync();
            }
        }


        async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                FileTypeFilter = { ".jpg", ".jpeg", ".png" }
            };

            StorageFile file;

            // On Phone, PickSingleFileAsync throws if we call it immediately on page load.
            // Let's just catch that, wait a bit, then try again.
            retryAfterBogusFailure:

            try
            {
                file = await picker.PickSingleFileAsync();
            }
            catch (UnauthorizedAccessException)
            {
                await Task.Delay(100);

                goto retryAfterBogusFailure;
            }

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
            catch (Exception exception)
            {
                string message = "Error loading photo.";

                if (exception is ArgumentException)
                {
                    message += "\n\nThis image is too high a resolution for your GPU to load into a single texture. " +
                               "And this app is not sophisticated enough to split it into multiple smaller textures. " +
                               "Sorry!";
                }

                await new MessageDialog(message).ShowAsync();
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
            var sizeRatio = viewSize / photoSize;
            var zoomFactor = Math.Min(sizeRatio.X, sizeRatio.Y) * 0.95f;

            scrollView.ChangeView(0, 0, zoomFactor);
        }


        void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (photo.SourceBitmap == null)
                return;

            var drawingSession = args.DrawingSession;

            drawingSession.Units = CanvasUnits.Pixels;
            drawingSession.Blend = CanvasBlend.Copy;

            // Draw the main photo image.
            var image = photo.GetImage();

            drawingSession.DrawImage(image);

            // Highlight the current region (if any).
            lastDrawnZoomFactor = null;

            var zoomFactor = ConvertDipsToPixels(scrollView.ZoomFactor);

            foreach (var edit in photo.Edits)
            {
                if (edit.DisplayRegionMask(drawingSession, zoomFactor, editingRegion != null))
                {
                    lastDrawnZoomFactor = scrollView.ZoomFactor;
                }
            }

            // Display any in-progress region edits.
            if (editingRegion != null)
            {
                editingRegion.DisplayRegionEditInProgress(drawingSession, regionPoints, zoomFactor);
            }
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
            regionPoints.Add(ConvertDipsToPixels(e.GetCurrentPoint(canvas)));

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

            editingRegion.EditRegionMask(regionPoints, ConvertDipsToPixels(scrollView.ZoomFactor));

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
            regionPoints.AddRange(from point in e.GetIntermediatePoints(canvas)
                                  select ConvertDipsToPixels(point));

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


        void ScrollView_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate &&
                lastDrawnZoomFactor.HasValue &&
                lastDrawnZoomFactor != scrollView.ZoomFactor)
            {
                canvas.Invalidate();
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


        float ConvertDipsToPixels(float value)
        {
            return value * canvas.Dpi / 96;
        }


        Vector2 ConvertDipsToPixels(PointerPoint point)
        {
            var value = point.Position.ToVector2();

            return new Vector2(ConvertDipsToPixels(value.X),
                               ConvertDipsToPixels(value.Y));
        }


        void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            const string url = "http://www.github.com/shawnhar/stuart";

            var operation = Launcher.LaunchUriAsync(new Uri(url));
        }
    }
}
