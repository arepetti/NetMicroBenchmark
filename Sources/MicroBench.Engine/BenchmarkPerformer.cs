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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Policy;

namespace MicroBench.Engine
{
	/// <summary>
	/// Performs a single test in a benchmark.
	/// </summary>
	/// <remarks>
	/// This class measures execution time (eventually performing a warm-up) for a single method call in a separate fresh
	/// <c>AppDomain</c> which will be then unloaded. Setup and cleanup of benchmark class is also performed here (because
	/// each test is isolated). Note that if one method fail then all execution is stopped and results aren't collected.
	/// </remarks>
	sealed class BenchmarkPerformer : MarshalByRefObject
	{
		public BenchmarkPerformer(bool warmUp)
		{
			_warmUp = warmUp;
		}

		public TimeSpan Run()
		{
			Debug.Assert(MethodToBenchmark != null);

			// Note: here we're not just measuring method body but also its invocation chain,
			// including delegate invocation performed here. For very small bodies this time may be significative,
			// to workaround this we should create an invokable type with Reflection.Emit and just pay the price
			// of a callvirt (method will be called through a known interface like IInvokable.Invoke).
			// HOWEVER note that if calling overhead is comparable to code to benchmark (less than few hundred nanoseconds) then
			// this framework is not the right one to perform the benchmark because many other ignored factors
			// will play an important role.
			var stopwatch = new Stopwatch();

			using (var obj = new DisposableWrapper(MethodToBenchmark.DeclaringType))
			{
				InvokeAll(obj.Instance, SetUpMethods);

				var methodToInvoke = (Action)Delegate.CreateDelegate(typeof(Action), obj.Instance, MethodToBenchmark);

				if (_warmUp)
					methodToInvoke();

				stopwatch.Start();
				methodToInvoke();
				stopwatch.Stop();

				InvokeAll(obj.Instance, CleanUpMethods);
			}

			return stopwatch.Elapsed;
		}

		public override object InitializeLifetimeService()
		{
			return null;
		}

		public static void AccumulateResults(BenchmarkEngine engine, Benchmark benchmark, BenchmarkedMethod method)
		{
			Debug.Assert(method != null);

			for (int i = 0; i < method.Repetitions; ++i)
			{
				if (engine.Options.RunTestsInIsolation)
					PerformSingleBenchmarkOnSeparateAppDomain(benchmark, method);
				else
					PerformSingleBenchmarkOnThisAppDomain(benchmark, method);
			}
		}

		private readonly bool _warmUp;
		private IEnumerable<MethodInfo> SetUpMethods { get; set; }
		private IEnumerable<MethodInfo> CleanUpMethods { get; set; }
		private MethodInfo MethodToBenchmark { get; set; }

		private static void InvokeAll(object instance, IEnumerable<MethodInfo> methods)
		{
			Debug.Assert(instance != null);
			Debug.Assert(methods != null);
			Debug.Assert(!methods.Any(x => x == null));

			foreach (var method in methods)
				method.Invoke(instance, null);
		}

		private static void PerformSingleBenchmarkOnSeparateAppDomain(Benchmark benchmark, BenchmarkedMethod method)
		{
			Debug.Assert(method != null);

			// CHECK: check if approach with AppDomain.DoCallback() is clearer
			var appDomain = CreateAppDomainForTest();

			try
			{
				var performer = (BenchmarkPerformer)(appDomain).CreateInstanceFromAndUnwrap(
					typeof(BenchmarkPerformer).Assembly.Location, typeof(BenchmarkPerformer).FullName,
					true, BindingFlags.Default, null, new object[] { method.WarmUp }, CultureInfo.CurrentCulture, null);

				PerformSingleBenchmark(benchmark, method, performer);
			}
			finally
			{
				AppDomain.Unload(appDomain);
			}
		}

		private static void PerformSingleBenchmarkOnThisAppDomain(Benchmark benchmark, BenchmarkedMethod method)
		{
			PerformSingleBenchmark(benchmark, method, new BenchmarkPerformer(method.WarmUp));

			// We do this to try to minimize GC impact on subsequent calls otherwise we may measure
			// the payload of THIS test on the NEXT one.
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		private static void PerformSingleBenchmark(Benchmark benchmark, BenchmarkedMethod method, BenchmarkPerformer performer)
		{
			Debug.Assert(benchmark != null);
			Debug.Assert(method != null);
			Debug.Assert(performer != null);

			performer.SetUpMethods = benchmark.SetUpMethods;
			performer.CleanUpMethods = benchmark.CleanUpMethods;
			performer.MethodToBenchmark = method.Method;

			method.Measures.Add(performer.Run());
		}

		private static AppDomain CreateAppDomainForTest()
		{
			// To be "compatible" with unit testing we need to be sure all AppDomains have the same application base
			var appDomain = AppDomain.CreateDomain("BenchmarkPerformerAd", 
				new Evidence(AppDomain.CurrentDomain.Evidence),
				new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase });

			appDomain.SetData("__BenchmarkCrossDomainData", 
				AppDomain.CurrentDomain.GetData("__BenchmarkCrossDomainData"));

			return appDomain;
		}

		private sealed class DisposableWrapper : IDisposable
		{
			public DisposableWrapper(Type type)
			{
				_instance = Activator.CreateInstance(type);
			}

			public object Instance
			{
				get { return _instance; }
			}

			public void Dispose()
			{
				var disposable = _instance as IDisposable;
				if (disposable != null)
					disposable.Dispose();
			}

			private readonly object _instance;
		}
	}
}
