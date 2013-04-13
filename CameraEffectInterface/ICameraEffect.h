#pragma once

namespace CameraEffectInterface
{
	/// <summary>
	/// The ICameraEffect interface definition
	/// This interface can be implemented either from managed or from native code.
	/// </summary>

	public interface class ICameraEffect {

		/// <summary>
		/// The camera device, the effect will poll the preview frames from it
		/// </summary>
		property Windows::Phone::Media::Capture::PhotoCaptureDevice^ CaptureDevice
		{
			void set( Windows::Phone::Media::Capture::PhotoCaptureDevice^ captureDevice);
		}

		/// <summary>
		/// The buffer where image data is written once the effect has been applied.  
		/// </summary>
		property Windows::Storage::Streams::IBuffer^ OutputBuffer
		{
			void set( Windows::Storage::Streams::IBuffer^ OutputBuffer );
		}

		/// <summary>
		/// The dimensions of the output buffer
		/// </summary>
		property Windows::Foundation::Size OutputBufferSize
		{
			void set( Windows::Foundation::Size outputBufferSize);
		}

		/// <summary>
		/// Get a frame from the camera and apply an effect on it
		/// </summary>
		/// <param name="processedBuffer">A buffer with the camera data with the effect applied</param>
		/// <returns>A task that completes when effect has been applied</returns>
		Windows::Foundation::IAsyncAction^ GetNewFrameAndApplyEffect();


		/// <summary>
		/// Change the type of effect
		/// </summary>
		void ChangeEffectType();

	};

}