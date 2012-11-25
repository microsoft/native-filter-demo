using NativeComponent;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Media;

namespace NativeFilterDemo
{
    public class CameraStreamSource : MediaStreamSource
    {

        private MediaStreamDescription _videoStreamDescription;

        private int _frameTime = 0;
        private long _currentTime = 0;
        private int _frameWidth;
        private int _frameHeight;
        private const int _framePixelSize = 4; // RGBA
        private int _frameBufferSize;
        private int _frameStreamSize;

        private MemoryStream _frameStream;
        private int _frameStreamOffset = 0;
        private Dictionary<MediaSampleAttributeKeys, string> _emptySampleDict =
            new Dictionary<MediaSampleAttributeKeys, string>();
        WindowsPhoneRuntimeComponent _cameraBuffer;

        int[]  _cameraData;
        byte[] _cameraFilteredData;

        public CameraStreamSource(WindowsPhoneRuntimeComponent cameraBuffer, Windows.Foundation.Size size)
        {
            _cameraBuffer = cameraBuffer;
            _frameWidth = (int)size.Width;
            _frameHeight = (int)size.Height;

            _cameraData = new int[_frameWidth * _frameHeight];
            _frameBufferSize = _frameWidth * _frameHeight * _framePixelSize;
            _cameraFilteredData = new byte[_frameBufferSize];
            _frameStreamSize = _frameBufferSize * 4; //Number of frames for buffering : 4 works well.
            _frameStream = new MemoryStream(_frameStreamSize);
        }

        protected override void OpenMediaAsync()
        {
            // Initialize data structures to pass to the Media pipeline via the MediaStreamSource
            Dictionary<MediaSourceAttributesKeys, string> mediaSourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
            Dictionary<MediaStreamAttributeKeys, string> mediaStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
            List<MediaStreamDescription> mediaStreamDescriptions = new List<MediaStreamDescription>();

            mediaStreamAttributes[MediaStreamAttributeKeys.VideoFourCC] = "RGBA";
            mediaStreamAttributes[MediaStreamAttributeKeys.Width] = _frameWidth.ToString();
            mediaStreamAttributes[MediaStreamAttributeKeys.Height] = _frameHeight.ToString();

            _videoStreamDescription = new MediaStreamDescription(MediaStreamType.Video, mediaStreamAttributes);
            mediaStreamDescriptions.Add(_videoStreamDescription);

            // a zero timespan is an infinite video
            mediaSourceAttributes[MediaSourceAttributesKeys.Duration] =
                TimeSpan.FromSeconds(0).Ticks.ToString(CultureInfo.InvariantCulture);

            mediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = false.ToString();

            _frameTime = (int)TimeSpan.FromSeconds((double)1 / 30).Ticks;
            // Report that we finished initializing its internal state and can now
            // pass in frame samples.

            ReportOpenMediaCompleted(mediaSourceAttributes, mediaStreamDescriptions);

        }


        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            if (_frameStreamOffset + _frameBufferSize > _frameStreamSize)
            {
                _frameStream.Seek(0, SeekOrigin.Begin);
                _frameStreamOffset = 0;
            }

            _cameraBuffer.NewViewfinderFrame(_cameraData, _cameraFilteredData);
            _frameStream.Write(_cameraFilteredData, 0, _frameBufferSize);

            MediaStreamSample msSamp = new MediaStreamSample(
           _videoStreamDescription, _frameStream, _frameStreamOffset,
           _frameBufferSize, _currentTime, _emptySampleDict);

            ReportGetSampleCompleted(msSamp);

            _currentTime += _frameTime;
            _frameStreamOffset += _frameBufferSize;

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
            _currentTime = seekToTime;
            this.ReportSeekCompleted(seekToTime);
        }

        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
            throw new NotImplementedException();
        }


    }
}
