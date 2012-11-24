#pragma once

namespace NativeComponent
{
    public ref class WindowsPhoneRuntimeComponent sealed
    {
    public:
        WindowsPhoneRuntimeComponent();
		void Initialize(Windows::Phone::Media::Capture::PhotoCaptureDevice^ captureDevice);

		void NewViewfinderFrame( Platform::WriteOnlyArray<int,1U>^ frameData);

	private:
		
		void ConvertToGrayOriginal( Platform::WriteOnlyArray<int,1U>^ frameData);
		void ConvertToGrayNeon    ( Platform::WriteOnlyArray<int,1U>^ frameData);

		Windows::Phone::Media::Capture::PhotoCaptureDevice^ m_camera;
		bool m_processingFrame;
    };
}