#pragma once

namespace NativeComponent
{
	public ref class WindowsPhoneRuntimeComponent sealed : CameraEffectInterface::ICameraEffect
	{
	public:
		WindowsPhoneRuntimeComponent();

		virtual property Windows::Phone::Media::Capture::PhotoCaptureDevice^ CaptureDevice
		{
			void set( Windows::Phone::Media::Capture::PhotoCaptureDevice^ );
		}

		virtual property Windows::Storage::Streams::IBuffer^ OutputBuffer
		{
			void set( Windows::Storage::Streams::IBuffer^ );
		}

		virtual property Windows::Foundation::Size OutputBufferSize
		{
			void set( Windows::Foundation::Size );
		}
	
        virtual Windows::Foundation::IAsyncAction^ GetNewFrameAndApplyEffect();

		virtual void ChangeEffectType();

	private:

		void ConvertToGrayOriginal( Platform::WriteOnlyArray<int,1U>^ inputBuffer,
			byte * outputBuffer);
		void ConvertToGrayOriginal( uint8 * src, uint8* dest, int length);

#if defined(_M_ARM)
		void ConvertToGrayNeon    ( Platform::WriteOnlyArray<int,1U>^ inputBuffer,
			byte * outputBuffer);
#endif
		void ThrowIfFailed(HRESULT hr);
		Windows::Phone::Media::Capture::PhotoCaptureDevice^ m_camera;
		Platform::Array<int>^ m_cameraPreviewBuffer;
		Windows::Foundation::Size m_outputBufferSize;
		byte* m_pixelsBuffer;
		bool m_processingFrame;
	};
}