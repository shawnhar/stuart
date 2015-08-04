using System.Collections.Generic;
using Windows.ApplicationModel;
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

            this.Suspending += OnSuspending;
        }


        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Initialize(args.PreviousExecutionState == ApplicationExecutionState.Terminated, null);
        }


        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            Initialize(false, args.Files);
        }


        void Initialize(bool wasTerminated, IReadOnlyList<IStorageItem> storageItems)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                if (wasTerminated)
                {
                    // TODO: Load state from previously suspended application
                }

                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), storageItems);
            }
            else
            {
                ((MainPage)rootFrame.Content).TryLoadPhoto(storageItems);
            }

            Window.Current.Activate();
        }


        void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            deferral.Complete();
        }
    }
}
