﻿//
// ProcessorSample.cs
// This file is part of Ghostscript.NET.Samples project
//
// Author: Josip Habjan (habjan@gmail.com, http://www.linkedin.com/in/habjan) 
// Copyright (c) 2013-2016 by Josip Habjan. All rights reserved.
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

using System;
using System.Collections.Generic;
using Ghostscript.NET.Processor;

namespace Ghostscript.NET.Samples
{
    public class SendToPrinterSample : ISample
    {
        public void Start()
        {
            // YOU NEED TO HAVE ADMINISTRATOR RIGHTS TO RUN THIS CODE

            string printerName = "YourPrinterName";
            string inputFile = @"E:\__test_data\test.pdf";

            using (GhostscriptProcessor processor = new GhostscriptProcessor())
            {
                List<string> switches = new List<string>();
                switches.Add("-empty");
                switches.Add("-dPrinted");
                switches.Add("-dBATCH");
                switches.Add("-dNOPAUSE");
                switches.Add("-dNOSAFER");
                switches.Add("-dPDFFitPage");
                switches.Add("-dNumCopies=1");
                switches.Add("-dNOPROMPT");
                switches.Add("-dQUIET");
                switches.Add("-dNoCancel");
                switches.Add("-sDEVICE=mswinpr2");

                switches.Add("-sOutputFile=%printer%" + printerName);
                switches.Add("-f");
                switches.Add(inputFile);

                processor.StartProcessing(switches.ToArray(), null);
            }
        }
    }
}
