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
using MicroBench.Engine;
using MicroBench.Engine.Calculations;
using MicroBench.Engine.Renderer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutomaticTest
{
	[TestClass]
	public sealed class TextOutputRendererTests : TestBase
	{
		[TestMethod]
		public void OutputIsUtf8AndHasExpectedTestResults()
		{
			// If output is not UTF8 then this will fail
			var text = Encoding.UTF8.GetString(Render<KnownBenchmark>());

			// Another very simple check, just to be sure output contains "something" about
			// an expected test.
			Assert.IsTrue(text.IndexOf("CustomNamedTest3", StringComparison.InvariantCulture) != -1);
		}

		private byte[] Render<T>()
		{
			// Just one repetition, we do not really care about results
			var engine = CreateEngine<T>(new BenchmarkOptions() { Repetitions = 1 });
			var renderer = new TextOutputRenderer
			{
				Statistics = new BasicStatistics(),
				TemplatePath = Path.Combine(Environment.CurrentDirectory, "PlainTextTemplate.txt")
			};

			var output = renderer.Render(engine.Execute());

			// First simple check to be sure we actually have something to test
			Assert.IsTrue(output != null && output.Length > 0);

			return output;
		}
	}
}
