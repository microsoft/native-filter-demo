// NativeComponent.cpp

#include "pch.h"
#include <client.h>
#include <windows.h>
#include <robuffer.h>
#if defined(_M_ARM)
#include <arm_neon.h>
#endif

using namespace NativeComponent;
using namespace Platform;
using namespace Windows::Phone::Media::Capture;
using namespace Windows::Foundation;
using namespace Windows::Storage::Streams;
using namespace Microsoft::WRL;

WindowsPhoneRuntimeComponent::WindowsPhoneRuntimeComponent()
{
}

void WindowsPhoneRuntimeComponent::CaptureDevice::set (PhotoCaptureDevice^ device)
{
	m_camera = device;
	Windows::Foundation::Size cameraFrameResolution = device->PreviewResolution;
	int numberOfPixels = int(cameraFrameResolution.Width) * int (cameraFrameResolution.Height);
	m_cameraPreviewBuffer = ref new Array<int>(numberOfPixels);
}

void WindowsPhoneRuntimeComponent::OutputBuffer::set (Windows::Storage::Streams::IBuffer^ outputBuffer)
{
	// Com magic to retrieve the pointer to the pixel buffer.
	Object^ obj = outputBuffer;
	ComPtr<IInspectable> insp(reinterpret_cast<IInspectable*>(obj));
	ComPtr<IBufferByteAccess> bufferByteAccess;
	ThrowIfFailed(insp.As(&bufferByteAccess));
	m_pixelsBuffer = nullptr;
	ThrowIfFailed(bufferByteAccess->Buffer(&m_pixelsBuffer));
}

void WindowsPhoneRuntimeComponent::OutputBufferSize::set (Windows::Foundation::Size bufferSize)
{
	m_outputBufferSize = bufferSize;
}

IAsyncAction^ WindowsPhoneRuntimeComponent::GetNewFrameAndApplyEffect()
{
	return concurrency::create_async([this](){

		m_camera->GetPreviewBufferArgb(m_cameraPreviewBuffer);

#if defined(_M_ARM)
		ConvertToGrayNeon(m_cameraPreviewBuffer,m_pixelsBuffer);
#else
		ConvertToGrayOriginal(m_cameraPreviewBuffer,m_pixelsBuffer);
#endif

	});
}

void WindowsPhoneRuntimeComponent::ChangeEffectType()
{
}

// The gray convertion and its NEON optimization is copied from http://hilbert-space.de/?p=22

void WindowsPhoneRuntimeComponent::ConvertToGrayOriginal( Platform::WriteOnlyArray<int,1U>^ inputBuffer,
														 byte * outputBuffer)
{	
	uint8 * src = (uint8 *) inputBuffer->Data;
	uint8 * dest = (uint8 *) outputBuffer;
	ConvertToGrayOriginal(src, dest, inputBuffer->Length);
}


inline void WindowsPhoneRuntimeComponent::ThrowIfFailed(HRESULT hr)
{
	if (FAILED(hr))
	{
		// Set a breakpoint on this line to catch Win32 API errors.
		throw Platform::Exception::CreateException(hr);
	}
}

void WindowsPhoneRuntimeComponent::ConvertToGrayOriginal( uint8 * src, uint8* dest, int length)
{
	int i;
	for (i=0; i<length; i++)
	{
		int r = *src++; // load red
		int g = *src++; // load green
		int b = *src++; // load blue 
		src++; //Alpha

		// build weighted average:
		int y = (r*77)+(g*151)+(b*28);

		// undo the scale by 256 and write to memory:

		*dest++ = (y>>8);
		*dest++ = (y>>8);
		*dest++ = (y>>8);
		*dest++ = 0xFF;

	}
}

// The same function, but implemented with Neon intrinsic. 
// For a good introduction to NEON: http://www.stanford.edu/class/ee282/10_handouts/lect.10.arm_soc.pdf
#if defined(_M_ARM)
void WindowsPhoneRuntimeComponent::ConvertToGrayNeon( Platform::WriteOnlyArray<int,1U>^ inputBuffer,
													 byte * outputBuffer)
{
	uint8 * src = (uint8 *) inputBuffer->Data;
	uint8 * dest = (uint8 *) outputBuffer;

	int n = inputBuffer->Length;

	uint8x8_t rfac = vdup_n_u8 (77);
	uint8x8_t gfac = vdup_n_u8 (151);
	uint8x8_t bfac = vdup_n_u8 (28);
	n/=8;

	uint8x8x4_t interleaved;
	interleaved.val[3] = vdup_n_u8 (0xFF); //Alpha value

	for (int i=0; i<n; i++)
	{
		uint16x8_t  temp;
		uint8x8x4_t rgb  = vld4_u8 (src);

		temp = vmull_u8 (rgb.val[0],      rfac);
		temp = vmlal_u8 (temp,rgb.val[1], gfac);
		temp = vmlal_u8 (temp,rgb.val[2], bfac);

		interleaved.val[0] = vshrn_n_u16 (temp, 8);
		interleaved.val[1] = interleaved.val[0];
		interleaved.val[2] = interleaved.val[0];

		vst4_u8 (dest, interleaved);
		src  += 8*4;
		dest += 8*4;
	}
} 

#endif
