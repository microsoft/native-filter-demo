#pragma once

namespace NativeComponent
{
	public ref class WindowsPhoneRuntimeComponent sealed
	{
	public:
		WindowsPhoneRuntimeComponent();
		void Initialize(Windows::Phone::Media::Capture::PhotoCaptureDevice^ captureDevice);

		void NewViewfinderFrame( Platform::WriteOnlyArray<int,1U>^ inputBuffer,
								 Platform::WriteOnlyArray<uint8,1U>^ outputBuffer);

	private:

		void ConvertToGrayOriginal( Platform::WriteOnlyArray<int,1U>^ inputBuffer,
								    Platform::WriteOnlyArray<uint8,1U>^ outputBuffer);
		void ConvertToGrayOriginal( uint8 * src, uint8* dest, int length);

#if defined(_M_ARM)
		void ConvertToGrayNeon    ( Platform::WriteOnlyArray<int,1U>^ inputBuffer,
			                        Platform::WriteOnlyArray<uint8,1U>^ outputBuffer);
#endif

		Windows::Phone::Media::Capture::PhotoCaptureDevice^ m_camera;
		bool m_processingFrame;
	};
}