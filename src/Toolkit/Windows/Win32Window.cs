﻿//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

#if NET461
using System;
using System.Windows.Forms;

namespace Xarial.XCad.Toolkit.Windows
{
    public class Win32Window : IWin32Window
    {
        public IntPtr Handle { get; }

        public Win32Window(IntPtr handle) 
        {
            Handle = handle;
        }
    }
}
#endif
