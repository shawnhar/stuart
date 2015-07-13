using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Stuart
{
    public sealed partial class MainPage : Page
    {
        Photo photo;


        public MainPage()
        {
            this.InitializeComponent();
        }


        void canvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            photo = new Photo(sender.Device);

            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }


        async Task CreateResourcesAsync(CanvasControl sender)
        {
            await photo.Load("bran.jpg");

            // Convert the photo size from pixels to dips.
            var photoSize = photo.Size;

            photoSize.X = sender.ConvertPixelsToDips((int)photoSize.X);
            photoSize.Y = sender.ConvertPixelsToDips((int)photoSize.Y);

            // Size the CanvasControl to exactly fit the image.
            sender.Width = photoSize.X;
            sender.Height = photoSize.Y;

            // Zoom so the whole image is visible.
            var viewSize = new Vector2((float)scrollView.ActualWidth, (float)scrollView.ActualHeight);
            var sizeRatio =  viewSize / photoSize;
            var zoomFactor = Math.Min(sizeRatio.X, sizeRatio.Y) * 0.95f;

            scrollView.ChangeView(null, null, zoomFactor, disableAnimation: true);
        }


        void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            photo.Draw(args.DrawingSession);
        }
    }
}
