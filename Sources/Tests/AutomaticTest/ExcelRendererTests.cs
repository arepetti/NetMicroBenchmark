//
// The MIT License (MIT)
// Copyright (c) 2016 Adriano Repetti

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// - The above copyright notice and this permission notice shall be
//   included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
// USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Linq;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using MicroBench.Engine;
using MicroBench.Engine.Renderer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutomaticTest
{
	[TestClass]
	public sealed class ExcelOutputRendererTests : TestBase
	{
		[TestMethod]
		public void OutputIsValidXlsx()
		{
			// Just one repetition, we do not really care about results
			var engine = CreateEngine<KnownBenchmark>(new BenchmarkOptions() { Repetitions = 1 });
			var renderer = new ExcelOutputRenderer();

			var content = renderer.Render(engine.Execute());

			using (var stream = new MemoryStream(content))
			using (var document = SpreadsheetDocument.Open(stream, false))
			{
				// Unless we find any bug in content then we just check
				// this is a valid XLSX document with one worksheet (it has to contain
				// one worksheet per benchmark).
				Assert.IsNotNull(document.WorkbookPart);
				Assert.AreEqual(document.WorkbookPart.WorksheetParts.Count(), 1);
			}
		}
	}
}
