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
using NativeCam.Resources;
using NativeComponent;
using Microsoft.Devices;
using Windows.Phone.Media.Capture;
using System.Windows.Media.Imaging;


namespace NativeCam
{
    public partial class MainPage : PhoneApplicationPage
    {
        PhotoCaptureDevice m_camera;
        WriteableBitmap m_wb;
        WindowsPhoneRuntimeComponent m_nativeFilter;
        bool m_processingFrame;
        int[] m_frameData;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Windows.Foundation.Size resolution = new Windows.Foundation.Size(640, 480);
            m_camera = await PhotoCaptureDevice.OpenAsync(CameraSensorLocation.Back, resolution);
            m_frameData = new int[(int)m_camera.PreviewResolution.Height * (int)m_camera.PreviewResolution.Width];
            ViewfinderBrush.SetSource(m_camera);

            m_camera.PreviewFrameAvailable += m_camera_PreviewFrameAvailable;

            m_wb = new WriteableBitmap((int)m_camera.PreviewResolution.Width, (int)m_camera.PreviewResolution.Height);
            this.FilteredViewfinder.Source = m_wb;
            m_processingFrame = false;

            m_nativeFilter = new WindowsPhoneRuntimeComponent();
            m_nativeFilter.Initialize(m_camera);
        }

        void m_camera_PreviewFrameAvailable(ICameraCaptureDevice sender, object args)
        {

            if (m_nativeFilter == null) return;
            if (m_processingFrame) return;
            m_processingFrame = true;
            m_nativeFilter.NewViewfinderFrame(m_frameData);
            Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                // Copy to WriteableBitmap.
                m_frameData.CopyTo(m_wb.Pixels, 0);
                m_wb.Invalidate();
                m_processingFrame = false;
            });

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (m_camera != null)
            {
                ViewfinderBrush.SetSource(m_camera);
            }
        }

    }
}