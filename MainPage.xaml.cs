using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
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

            // Size the CanvasControl to exactly fit the image.
            sender.Width = photo.Size.Width;
            sender.Height = photo.Size.Height;
        }


        void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            photo.Draw(args.DrawingSession);
        }
    }
}
