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
using System.IO;
using System.Text;
using System.Xml;
using MicroBench.Engine;
using MicroBench.Engine.Calculations;
using MicroBench.Engine.Renderer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutomaticTest
{
	[TestClass]
	public sealed class HtmlOutputRendererTest : TestBase
	{
		[TestMethod]
		public void OutputIsValidXhtml()
		{
			// Simple case with US-ASCII content from code and template
			// "compatible" with XHTML
			EnsureOutputIsValidXhtml<KnownBenchmark>();
		}

		[TestMethod]
		public void OutputCorrectlyEncodeStringsFromCode()
		{
			// This test class uses "invalid characters" for an (X)HTML string and must be escaped.
			// Note that we're producing HTML (not XML nor XHTML) and escapinig rules are different (because of
			// extra character references in HTML) but FOR THIS TEST where we just have few common characters
			// then this assumption is still true.
			EnsureOutputIsValidXhtml<BenchmarkWithStrangeName>();
		}

		[TestMethod]
		public void OutputIsUtf8AndHasExpectedTestResults()
		{
			// If output is not UTF8 then this will fail
			var html = Encoding.UTF8.GetString(Render<KnownBenchmark>());

			// Another very simple check, just to be sure output contains "something" about
			// an expected test.
			Assert.IsTrue(html.IndexOf("CustomNamedTest3", StringComparison.InvariantCulture) != -1);
		}

		private void EnsureOutputIsValidXhtml<T>()
		{
			// So far our HTML code is also a valid XML document, relax this check if it won't be true (we may use
			// HTMLAgilityPack for HTML parsing). Output encoding is detected by XmlDocument (we need a separate test
			// to be sure it's UTF8 as expected).
			using (var stream = new MemoryStream(Render<T>()))
			{
				new XmlDocument().Load(stream);
			}
		}

		private byte[] Render<T>()
		{
			var engine = CreateEngine<T>(new BenchmarkOptions() { Repetitions = 1 });
			var renderer = new HtmlOutputRenderer
			{
				Statistics = new BasicStatistics(),
				TemplatePath = Path.Combine(Environment.CurrentDirectory, HtmlOutputRenderer.DefaultReportName)
			};

			// Just one repetition, we do not really care about results
			var output = renderer.Render(engine.Execute());

			// First simple check to be sure we actually have something to test
			Assert.IsTrue(output != null && output.Length > 0);

			return output;
		}
	}
}
