Native Filter Demo
==================

A simple demonstration project for Windows Phone 8 describing a way to create
real-time filters for the camera viewfinder, using native code (C++). The
project also provides a sample filter optimized using the NEON instruction set.

![Screenshot](https://github.com/nokia-developer/native-filter-demo/raw/master/nfd_screenshot.png)

One of the big new features of Windows Phone 8 SDK is the support for C and C++,
also known as native code support. This is a small demo app that shows how one
can exploit that support to create real-time filters for the camera. For
simplicity's sake, the example will implement a simple gray filter, that will
convert camera input on the fly and show the result on screen.

The project also demonstrate how to use ARM Neon intrinsics to improve the speed
of the filters, as well as how to feed camera frame into a MediaElement.

Please have a look at the corresponding ​wiki article that describes the project:
http://developer.nokia.com/Community/Wiki/Real-time_camera_viewfinder_filters_in_Native_code

---

*Copyright (c) 2013-2014 Microsoft Mobile. All rights reserved.*
