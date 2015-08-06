using System.Collections.Generic;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stuart
{
    // Provides application-specific behavior to supplement the default Application class.
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }


        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Initialize(args.PreviousExecutionState);
        }


        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            Initialize(args.Files);
        }


        void Initialize(object launchArg)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), launchArg);
            }
            else
            {
                ((MainPage)rootFrame.Content).TryLoadPhoto(launchArg as IReadOnlyList<IStorageItem>);
            }

            Window.Current.Activate();
        }
    }
}
