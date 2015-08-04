using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
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


        // Invoked when the application is launched normally by the end user.
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }

            Window.Current.Activate();
        }


        // Invoked when application execution is being suspended.
        void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            deferral.Complete();
        }
    }
}
