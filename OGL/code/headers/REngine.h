/*
Reactor 3D MIT License

Copyright (c) 2010 Reiser Games

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#include "common.h"
#include "RScene.h"
#ifndef RENGINE_H
#define RENGINE_H

namespace Reactor
{

	class REngine
	{
	private:
		//rGLDevice gldevice;
		static REngine *_instance;
		REngine();
		~REngine();
		bool _fullscreen;
		RColor clearColor;
		int window;
		
        
	public:

		//rGLDevice* getDevice();
		static REngine* getInstance();
		const RECT& GetScreenSize();
		void Init3DWindowed(RECT &rect, const char* title);
		void Init3DFullscreen(const char* title, RINT width, RINT height, RINT color, RINT depth);
		void Init3DNoRender();
		void ToggleFullscreen();
		void DisplayFPS(RBOOL display, RColor color = RColor(1,1,1,1));
        float GetFPS();
		void OnResize(RINT width, RINT height);
		void Clear(RBOOL DepthOnly = false);
		void RenderToScreen();
		void DestroyAll();
		
	};
};

#endif