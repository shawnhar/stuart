using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
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
using Windows.UI.Xaml.Navigation;

namespace Stuart
{
    // UI codebehind for the main application page.
    public sealed partial class MainPage : Page
    {
        Photo photo = new Photo();

        StorageFile currentFile;
        IReadOnlyList<IStorageItem> navigatedToFiles;

        EditGroup editingRegion;
        readonly List<Vector2> regionPoints = new List<Vector2>();

        float? lastDrawnZoomFactor;


        static readonly List<string> imageFileExtensions = new List<string>
        {
            ".jpg",
            ".jpeg",
            ".png"
        };


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


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigatedToFiles = e.Parameter as IReadOnlyList<IStorageItem>;

            base.OnNavigatedTo(e);
        }


        void canvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            switch (args.Reason)
            {
                case CanvasCreateResourcesReason.FirstTime:
                    // First time initialization: if we were launched via OnFileActivated,
                    // load the target file now, otherwise bring up the file selector.
                    if (!TryLoadPhoto(navigatedToFiles))
                    {
                        LoadButton_Click(null, null);
                    }
                    break;

                case CanvasCreateResourcesReason.NewDevice:
                    // Recovering after a lost device (GPU reset).
                    if (photo.SourceBitmap != null)
                    {
                        var loadTask = photo.ReloadAfterDeviceLost(sender.Device, currentFile);

                        args.TrackAsyncAction(loadTask.AsAsyncAction());
                    }
                    break;

                case CanvasCreateResourcesReason.DpiChanged:
                    // We mostly work in pixels rather than DIPs, so only need
                    // minimal layout updates in response to DPI changes.
                    if (photo.SourceBitmap != null)
                    {
                        ZoomToFitPhoto();
                    }
                    break;
            }
        }


        public bool TryLoadPhoto(IReadOnlyList<IStorageItem> storageItems)
        {
            if (storageItems == null)
                return false;

            var file = GetSingleImageFile(storageItems);

            if (file == null)
                return false;

            LoadPhoto(file);

            return true;
        }


        async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            imageFileExtensions.ForEach(picker.FileTypeFilter.Add);

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

            if (file != null)
            {
                LoadPhoto(file);
            }
        }


        void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SavePhoto(currentFile);
        }


        async void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileSavePicker
            {
                SuggestedSaveFile = currentFile,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                DefaultFileExtension = imageFileExtensions[0],
                FileTypeChoices = { { "Image files", imageFileExtensions } }
            };

            var file = await picker.PickSaveFileAsync();

            if (file != null)
            {
                SavePhoto(file);
            }
        }


        async void LoadPhoto(StorageFile file)
        {
            try
            {
                await photo.Load(canvas.Device, file);

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

                if (canvas.Device.IsDeviceLost(exception.HResult))
                {
                    canvas.Device.RaiseDeviceLost();
                }
            }
        }


        async void SavePhoto(StorageFile file)
        {
            try
            {
                await photo.Save(file);

                currentFile = file;
            }
            catch (Exception exception)
            {
                await new MessageDialog("Error saving photo").ShowAsync();

                if (canvas.Device.IsDeviceLost(exception.HResult))
                {
                    canvas.Device.RaiseDeviceLost();
                }
            }
        }


        void Page_DragEnter(object sender, DragEventArgs e)
        {
            HandleDrop(e, file =>
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.IsCaptionVisible = false;
            });
        }


        void Page_Drop(object sender, DragEventArgs e)
        {
            HandleDrop(e, file =>
            {
                LoadPhoto(file);
            });
        }


        static async void HandleDrop(DragEventArgs e, Action<StorageFile> handleDroppedFile)
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems))
                return;

            var deferral = e.GetDeferral();

            try
            {
                var storageItems = await e.DataView.GetStorageItemsAsync();

                var file = GetSingleImageFile(storageItems);

                if (file != null)
                {
                    handleDroppedFile(file);

                    e.Handled = true;
                }
            }
            finally
            {
                deferral.Complete();
            }
        }


        static StorageFile GetSingleImageFile(IReadOnlyList<IStorageItem> storageItems)
        {
            var files = storageItems.OfType<StorageFile>().ToList();

            if (files.Any(file => !imageFileExtensions.Contains(file.FileType, StringComparer.OrdinalIgnoreCase)))
                return null;

            if (files.Count() != 1)
                return null;

            return files.Single();
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


        void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (editingRegion == null)
                return;

            try
            {
                editingRegion.EditRegionMask(regionPoints, ConvertDipsToPixels(scrollView.ZoomFactor));
            }
            catch (Exception exception) when (canvas.Device.IsDeviceLost(exception.HResult))
            {
                canvas.Device.RaiseDeviceLost();
            }

            editingRegion = null;
            regionPoints.Clear();

            // Restore the manipulation mode so input goes to the parent ScrollViewer again.
            canvas.ManipulationMode = ManipulationModes.System;
            canvas.ReleasePointerCapture(e.Pointer);

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

                e.Handled = true;
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


        void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            const string url = "http://www.github.com/shawnhar/stuart";

            var operation = Launcher.LaunchUriAsync(new Uri(url));
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
    }
}
