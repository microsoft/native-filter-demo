using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NativeFilterDemo.Resources;
using NativeComponent;
using Microsoft.Devices;
using Windows.Phone.Media.Capture;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Threading;
using Microsoft.Phone;


namespace NativeFilterDemo
{
    public partial class MainPage : PhoneApplicationPage
    {
        PhotoCaptureDevice m_camera;
        WindowsPhoneRuntimeComponent m_nativeFilter;
    
        DateTime m_startTime;
        DispatcherTimer m_timer;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Shell.PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
            Windows.Foundation.Size resolution = new Windows.Foundation.Size(640, 480);
            m_camera = await PhotoCaptureDevice.OpenAsync(CameraSensorLocation.Back, resolution);

            Windows.Foundation.Size actualResolution = m_camera.PreviewResolution;
            
            // The viewfinderbrush (preview of the original data from camera)
            // is currently very unstable on the device, when used together
            // with a MediaEngine playback component.
            
			//ViewfinderBrush.SetSource(m_camera);

            m_nativeFilter = new WindowsPhoneRuntimeComponent();
            m_nativeFilter.Initialize(m_camera);

            CameraStreamSource source = new CameraStreamSource(m_nativeFilter, actualResolution);
            MyCameraMediaElement.SetSource (source);

            m_startTime = DateTime.Now;
            m_timer = new DispatcherTimer();
            m_timer.Interval = new TimeSpan(0, 0, 0, 1, 0); // Tick every 1s.
            m_timer.Tick += m_timer_Tick;
            m_timer.Start();

        }


        void m_timer_Tick(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("FPS:" + MyCameraMediaElement.RenderedFramesPerSecond );
        }

        

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (m_camera != null)
            {
                ViewfinderBrush.SetSource(m_camera);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnNavigatedFrom");
                base.OnNavigatedFrom(e);
        }
    }
}