namespace NativeFilterDemo
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Windows.Foundation;
    using CameraEffectInterface;

    /// <summary>
    /// A source for the media element. Feeds the Media Element with frames coming from the
    /// ICameraEffect implementation.
    /// </summary>
    public class CameraStreamSource : MediaStreamSource
    {
        private readonly Dictionary<MediaSampleAttributeKeys, string> emptySampleDict =
            new Dictionary<MediaSampleAttributeKeys, string>();

        private long currentTime;
        private int frameStreamOffset;
        private int frameTime;
        private MediaStreamDescription videoStreamDescription;

        public CameraStreamSource(ICameraEffect cameraEffect, Size targetMediaElementSize)
        {
            CameraStreamSourceDataSingleton dataSource = CameraStreamSourceDataSingleton.Instance;
            dataSource.Initialize(targetMediaElementSize);
            dataSource.CameraEffect = cameraEffect;
            cameraEffect.OutputBufferSize = targetMediaElementSize;
            cameraEffect.OutputBuffer = dataSource.ImageBuffer.AsBuffer();
        }

        protected override void OpenMediaAsync()
        {
            // Initialize data structures to pass to the Media pipeline via the MediaStreamSource
            Dictionary<MediaSourceAttributesKeys, string> mediaSourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
            Dictionary<MediaStreamAttributeKeys, string> mediaStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
            List<MediaStreamDescription> mediaStreamDescriptions = new List<MediaStreamDescription>();

            CameraStreamSourceDataSingleton dataSource = CameraStreamSourceDataSingleton.Instance;

            mediaStreamAttributes[MediaStreamAttributeKeys.VideoFourCC] = "RGBA";
            mediaStreamAttributes[MediaStreamAttributeKeys.Width] = dataSource.FrameWidth.ToString();
            mediaStreamAttributes[MediaStreamAttributeKeys.Height] = dataSource.FrameHeight.ToString();

            videoStreamDescription = new MediaStreamDescription(MediaStreamType.Video, mediaStreamAttributes);
            mediaStreamDescriptions.Add(videoStreamDescription);

            // a zero timespan is an infinite video
            mediaSourceAttributes[MediaSourceAttributesKeys.Duration] =
                TimeSpan.FromSeconds(0).Ticks.ToString(CultureInfo.InvariantCulture);

            mediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = false.ToString();

            frameTime = (int)TimeSpan.FromSeconds((double)0).Ticks;

            // Report that we finished initializing its internal state and can now
            // pass in frame samples.
            ReportOpenMediaCompleted(mediaSourceAttributes, mediaStreamDescriptions);

        }

        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            CameraStreamSourceDataSingleton dataSource = CameraStreamSourceDataSingleton.Instance;

            if (frameStreamOffset + dataSource.FrameBufferSize > dataSource.FrameStreamSize)
            {
                dataSource.FrameStream.Seek(0, SeekOrigin.Begin);
                frameStreamOffset = 0;
            }

            Task tsk = dataSource.CameraEffect.GetNewFrameAndApplyEffect().AsTask();
           
            // Wait that the asynchroneous call completes, and proceed by reporting 
            // the MediaElement that new samples are ready.
            tsk.ContinueWith((task) =>
            {
                dataSource.FrameStream.Position = 0;

                MediaStreamSample msSamp = new MediaStreamSample(
                    videoStreamDescription, 
                    dataSource.FrameStream, 
                    frameStreamOffset,
                    dataSource.FrameBufferSize,
                    currentTime,
                    emptySampleDict);

                ReportGetSampleCompleted(msSamp);
                currentTime += frameTime;
                frameStreamOffset += dataSource.FrameBufferSize;
            });
        }

        protected override void CloseMedia()
        {
        }

        protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
        {
            throw new NotImplementedException();
        }

        protected override void SeekAsync(long seekToTime)
        {
            currentTime = seekToTime;
            ReportSeekCompleted(seekToTime);
        }

        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
            throw new NotImplementedException();
        }

    }
}