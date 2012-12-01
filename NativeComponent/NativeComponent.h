#pragma once

namespace NativeComponent
{
	public ref class WindowsPhoneRuntimeComponent sealed
	{
	public:
		WindowsPhoneRuntimeComponent();
		void Initialize(Windows::Phone::Media::Capture::PhotoCaptureDevice^ captureDevice);

		void NewViewfinderFrame( Platform::WriteOnlyArray<int,1U>^ frameData,
								 Platform::WriteOnlyArray<uint8,1U>^ output);

	private:

		void ConvertToGrayOriginal( Platform::WriteOnlyArray<int,1U>^ frameData,
								    Platform::WriteOnlyArray<uint8,1U>^ output);
		void ConvertToGrayOriginal( uint8 * src, uint8* dest, int length);

#if defined(_M_ARM)
		void ConvertToGrayNeon    ( Platform::WriteOnlyArray<int,1U>^ frameData,
			                        Platform::WriteOnlyArray<uint8,1U>^ output);
#endif

		Windows::Phone::Media::Capture::PhotoCaptureDevice^ m_camera;
		bool m_processingFrame;
	};
}