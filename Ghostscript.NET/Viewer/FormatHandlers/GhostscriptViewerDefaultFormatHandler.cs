﻿//
// GhostscriptViewerDefaultFormatHandler.cs
// This file is part of Ghostscript.NET library
//
// Author: Josip Habjan (habjan@gmail.com, http://www.linkedin.com/in/habjan) 
// Copyright (c) 2013-2016 by Josip Habjan. All rights reserved.
//
// Author ported some parts of this code from GSView. 
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace Ghostscript.NET.Viewer
{
    internal class GhostscriptViewerDefaultFormatHandler : GhostscriptViewerFormatHandler
    {

        #region Constructor

        public GhostscriptViewerDefaultFormatHandler(GhostscriptViewer viewer) : base(viewer) { }

        #endregion

        #region Initialize

        public override void Initialize()
        {
            
        }

        #endregion

        #region Open

        public override void Open(string filePath)
        {
            
        }

        #endregion

        #region StdInput

        public override void StdInput(out string input, int count)
        {
            input = string.Empty;
        }

        #endregion

        #region StdOutput

        public override void StdOutput(string message)
        {

        }

        #endregion

        #region StdError

        public override void StdError(string message)
        {

        }

        #endregion

        #region InitPage

        public override void InitPage(int pageNumber)
        {
            
        }

        #endregion

        #region ShowPage

        public override void ShowPage(int pageNumber)
        {
            
        }

        #endregion

    }
}
