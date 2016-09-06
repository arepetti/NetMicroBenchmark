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
using System.Linq;
using System.Reflection;
using System.Threading;
using MicroBench.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutomaticTest
{
	[TestClass]
	public sealed class EngineTests : TestBase
	{
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ThrowsIfWrongArguments()
		{
			CreateEngine<KnownBenchmark>(null);
		}

		[TestMethod]
		public void ReturnsExpectedResultCount()
		{
			var results = CreateEngine<KnownBenchmark>(new BenchmarkOptions() { Repetitions = 1 }).Execute();

			Assert.AreEqual(1, results.Count(), "Only one test had to be executed.");
		}

		[TestMethod]
		public void CanDiscoverTypesInAssembly()
		{
			var engine = new BenchmarkEngine(new BenchmarkOptions() { Repetitions = 1 }, new Assembly[] { GetType().Assembly });
			var results = engine.Execute();

			Assert.AreEqual(3, results.Count(), "This test library contains three valid benchmark classes.");
		}

		[TestMethod]
		public void TestsHaveAppropriateName()
		{
			var results = CreateEngine<KnownBenchmark>(new BenchmarkOptions() { Repetitions = 1 }).Execute();
			var benchmark = results.Single();

			Assert.IsNotNull(benchmark.Methods.SingleOrDefault(x => x.Name == "Test1"));
			Assert.IsNotNull(benchmark.Methods.SingleOrDefault(x => x.Name == "Test2"));
			Assert.IsNotNull(benchmark.Methods.SingleOrDefault(x => x.Name == "CustomNamedTest3"));
		}

		[TestMethod]
		public void MeasuredValuesAreAsExpected()
		{
			// We need few repetitions to ensure we're not just measuring calling overhead (in this case, for example,
			// Test3 may be faster than Test2 if KnownBenchmark._tick is small enough). If this happen then we may need
			// to increase repetitions for this test.
			var results = CreateEngine<KnownBenchmark>(new BenchmarkOptions() { Repetitions = 5 }).Execute();
			var benchmark = results.Single();

			var test1 = benchmark.Methods.Single(x => x.Name == "Test1");
			var test2 = benchmark.Methods.Single(x => x.Name == "Test2");
			var test3 = benchmark.Methods.Single(x => x.Name == "CustomNamedTest3");

			var test1Time = test1.Measures.Sum(x => x.TotalMilliseconds);
			var test2Time = test2.Measures.Sum(x => x.TotalMilliseconds);
			var test3Time = test3.Measures.Sum(x => x.TotalMilliseconds);

			Assert.IsTrue(test1Time < test2Time && test2Time < test3Time);
		}

		[TestMethod]
		public void CanQuicklyCreateReport()
		{
			var tempPath = BenchmarkEngine.ExecuteAndRenderWithDefaults(null, new BenchmarkOptions() { Repetitions = 1 });
			Assert.IsTrue(File.Exists(tempPath));
			
			File.Delete(tempPath);
		}

        [TestMethod]
        public void CanMeasureSingleMethod()
        {
            var result = BenchmarkEngine.ExecuteSingle(typeof(SingleBenchmark));
            Assert.IsTrue(result >= TimeSpan.FromMilliseconds(1), "Average execution time cannot be smaller than minimum expected value.");
            Assert.IsTrue(result <= TimeSpan.FromMilliseconds(10), "Average execution time cannot be too much higher than maximum expected value.");
        }

        private sealed class SingleBenchmark
        {
            private readonly Random _rnd = new Random();

            public void MethodToBenchmark()
            {
                Thread.Sleep(1 + _rnd.Next(0, 5));
            }
        }
	}
}
