
using System;
using System.Linq;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Phone.Media.Capture;
using Size = Windows.Foundation.Size;
using System.Diagnostics;
using CameraEffectInterface;
using NativeComponent;

namespace NativeFilterDemo
{
    public partial class MainPage : PhoneApplicationPage
    {
        private PhotoCaptureDevice m_camera;
        private readonly ICameraEffect cameraEffect;
        private CameraStreamSource source;
        private DispatcherTimer m_timer;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            cameraEffect = new WindowsPhoneRuntimeComponent();
            BuildApplicationBar();
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton button = new ApplicationBarIconButton(new Uri("/Assets/Icons/next.png", UriKind.Relative));
            button.Text = "Second page";
            button.Click += SecondPageButtonClick;
            ApplicationBar.Buttons.Add(button);
            Loaded += MainPage_Loaded;
        }

        async void MainPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Size targetMediaElementSize = new Size(640, 480);
            double aspectRatio = 4.0/3.0;

            // 1. Open camera 
            if (m_camera == null)
            {
                var captureRes = PhotoCaptureDevice.GetAvailableCaptureResolutions(CameraSensorLocation.Back);
                Size selectedCaptureRes = captureRes.Where(res => Math.Abs(aspectRatio - res.Width/res.Height ) <= 0.1)
                                                    .OrderBy(res => res.Width)
                                                    .Last();
                m_camera = await PhotoCaptureDevice.OpenAsync(CameraSensorLocation.Back, selectedCaptureRes);
                m_camera.SetProperty(KnownCameraGeneralProperties.EncodeWithOrientation, m_camera.SensorLocation == CameraSensorLocation.Back ? m_camera.SensorRotationInDegrees : -m_camera.SensorRotationInDegrees);
               
                var previewRes = PhotoCaptureDevice.GetAvailablePreviewResolutions(CameraSensorLocation.Back);
                Size selectedPreviewRes = previewRes.Where(res => Math.Abs(aspectRatio - res.Width/res.Height ) <= 0.1) 
                                                    .Where(res => (res.Height >= targetMediaElementSize.Height) && (res.Width >= targetMediaElementSize.Width))
                                                    .OrderBy(res => res.Width)
                                                    .First();
                await m_camera.SetPreviewResolutionAsync(selectedPreviewRes);
                cameraEffect.CaptureDevice = m_camera;
            }

            // Always create a new source, otherwise the MediaElement will not start.
            source = new CameraStreamSource(cameraEffect, targetMediaElementSize);
            MyCameraMediaElement.SetSource(source);

            m_timer = new DispatcherTimer();
            m_timer.Interval = new TimeSpan(0, 0, 0, 1, 0); // Tick every 1s.
            m_timer.Tick += m_timer_Tick;
            m_timer.Start();
        }

        void m_timer_Tick(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("FPS 2:" + MyCameraMediaElement.RenderedFramesPerSecond);
        }

        private void SecondPageButtonClick(object sender, EventArgs e)
        {
            if (m_camera == null) return;

            MyCameraMediaElement.Source = null;
            NavigationService.Navigate(new Uri("/SecondPage.xaml", UriKind.Relative));

        }

        private void MyCameraMediaElement_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (cameraEffect != null)
            {
                cameraEffect.ChangeEffectType();
            }
        }
    }
}