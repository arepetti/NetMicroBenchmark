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
using MicroBench.Engine;
using MicroBench.Engine.Calculations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutomaticTest
{
	[TestClass]
	public sealed class BasicStatisticsTest : TestBase
	{
		[TestMethod]
		public void ValuesHaveRequiredOrder()
		{
			// We need EXACTLY this order because of how HtmlOutputRenderer and its HTML template files
			// are implemented. If things will change there (with more robust code) then this test may
			// be simplified.
			// Note that with a small number of repetitions this check may fail because of "random" function call overhead, if
			// this test fails on some environment you may need to increase this number (I keep it low just to make test faster).
			var benchmarkResults = CreateEngine<AleatoryBenchmark>(new BenchmarkOptions() { Repetitions = 10 }).Execute();

			var statisticCalculator = new BasicStatistics();
			var statisticResults = statisticCalculator.Calculate(benchmarkResults.Single().Methods.Single());

			var distribution = statisticCalculator.GetHeaders().Select(x => (TimeSpan)statisticResults[x]).ToArray();
			for (int i = 1; i < distribution.Length; ++i)
			{
				Assert.IsTrue(distribution[i] > distribution[i - 1],
					String.Format("Item #{0} has value {1} but it should be greater than item #{2} (with value {3}.", i, distribution[i], i - 1, distribution[i - 1]));
			}
		}

		[TestMethod]
		public void AllResultsHaveName()
		{
			var benchmarkResults = CreateEngine<AleatoryBenchmark>(new BenchmarkOptions() { Repetitions = 1 }).Execute();

			var statisticCalculator = new BasicStatistics();
			var statisticResults = statisticCalculator.Calculate(benchmarkResults.Single().Methods.Single());

			Assert.AreEqual(statisticResults.Count, statisticCalculator.GetHeaders().Count());
		}
	}
}
