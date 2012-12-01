// NativeComponent.cpp

#include "pch.h"
#include <windows.h>
#include "NativeComponent.h"
#if defined(_M_ARM)
#include <arm_neon.h>
#endif

using namespace NativeComponent;
using namespace Platform;
using namespace Windows::Phone::Media::Capture;

WindowsPhoneRuntimeComponent::WindowsPhoneRuntimeComponent()
{

}


void WindowsPhoneRuntimeComponent::Initialize(Windows::Phone::Media::Capture::PhotoCaptureDevice^ captureDevice)
{
	m_camera = captureDevice;
	Windows::Foundation::Size viewfinderResolution = m_camera->PreviewResolution;

	m_processingFrame = false;

}


void WindowsPhoneRuntimeComponent::NewViewfinderFrame( Platform::WriteOnlyArray<int,1U>^ inputBuffer,
													  Platform::WriteOnlyArray<uint8,1U>^ outputBuffer)
{
	m_camera->GetPreviewBufferArgb(inputBuffer);
	#if defined(_M_ARM)
	ConvertToGrayNeon(inputBuffer,outputBuffer );
	#else
	ConvertToGrayOriginal(inputBuffer, outputBuffer);
	#endif

	
}


// The gray convertion and its NEON optimization is copied from http://hilbert-space.de/?p=22

void WindowsPhoneRuntimeComponent::ConvertToGrayOriginal( Platform::WriteOnlyArray<int,1U>^ inputBuffer,
														  Platform::WriteOnlyArray<uint8,1U>^ outputBuffer)
{	uint8 * src = (uint8 *) inputBuffer->Data;
	uint8 * dest = (uint8 *) outputBuffer->Data;
	ConvertToGrayOriginal(src, dest, inputBuffer->Length);
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
													  Platform::WriteOnlyArray<uint8,1U>^ outputBuffer)
{
	uint8 * src = (uint8 *) inputBuffer->Data;
	uint8 * dest = (uint8 *) outputBuffer->Data;

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
