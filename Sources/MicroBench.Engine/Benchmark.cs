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
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace MicroBench.Engine
{
	/// <summary>
	/// Represents a benchmark and its results.
	/// </summary>
	[Serializable]
	public sealed class Benchmark
	{
		/// <summary>
		/// Gets the list of test methods for this bencmark.
		/// </summary>
		/// <value>
		/// The list of class methods to test for this benchmark. Each method
		/// is a separate test compared with all the others within this benchmark.
		/// </value>
		public BenchmarkedMethodCollection Methods
		{
			get { return _methods; }
		}

		/// <summary>
		/// Gets the friendly name of a group of benchmarks.
		/// </summary>
		/// <value>
		/// Friendly name of a group of benchmarks. Each benchmark is tied
		/// to one single class (and each class represents a benchmark) but multiple
		/// benchmarks can be grouped together using this property. Note that
		/// results will not be aggregated, this relation is only <em>logical</em>
		/// and it <em>may</em> be used by output renderer to group them visually.
		/// </value>
		public string Group
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the friendly name of this benchmark.
		/// </summary>
		/// <value>
		/// Friendly name of this benchmark. Output renderer should use this name
		/// to present its results.
		/// </value>
		public string Name
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the optional description of this benchmark.
		/// </summary>
		/// <value>
		/// Optional description of this benchmark. Output renderer may use it
		/// to provide more (user-defined) information into the benchmark report.
		/// </value>
		public string Description
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the total time required to perform this benchmark.
		/// </summary>
		/// <value>
		/// Total time required to perform this benchmark. Timing is not accurate and it has the same
		/// precision of system time (assume around 50 milliseconds).
		/// </value>
		public TimeSpan ExecutionTime
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets/sets the type of the class which describe this benchmark.
		/// </summary>
		/// <value>
		/// The type of the class which describe this benchmark. Note that there is a 1:1
		/// relation between a benchmark and a class (each class is one benchmark and
		/// each benchmark executes on a single class).
		/// </value>
		internal Type Type
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets the list of methods to be called to prepare a benchmark.
		/// </summary>
		/// <value>
		/// List of methods to be called to prepare a benchmark.
		/// </value>
		/// <remarks>
		/// Default constructor is obviously always invoked for the benchmark class (see <see cref="Benchmark.Type"/>) but
		/// it's possible to specify more methods that must be invoked before each test. Note that execution order is
		/// not granted and these methods will be executed for each method to test because each run is separate from all the others.
		/// </remarks>
		internal MethodInfo[] SetUpMethods
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets the list of methods to be called to cleanup a benchmark.
		/// </summary>
		/// <value>
		/// List of methods to be called to cleanup a benchmark.
		/// </value>
		/// <remarks>
		/// If implemented then <see cref="IDisposable.Dispose"/> is invoked for the benchmark class (see <see cref="Benchmark.Type"/>) but
		/// it's possible to specify more methods that must be invoked before each test. Note that execution order is
		/// not granted and these methods will be executed for each method to test because each run is separate from all the others.
		/// </remarks>
		internal MethodInfo[] CleanUpMethods
		{
			get;
			set;
		}

		/// <summary>
		/// Performs this benchmark.
		/// </summary>
		/// <param name="engine">The engine on which this benchmark is performed.</param>
		internal void Perform(BenchmarkEngine engine)
		{
			Debug.Assert(engine != null);

			var startTimestamp = DateTime.Now;

			for (int i = 0; i < Methods.Count; ++i)
			{
				int progress = (int)((float)i / Methods.Count * 100);

				engine.OnBenchmarkProgressChanged(new ProgressChangedEventArgs(progress, Methods[i].Name));
				BenchmarkPerformer.AccumulateResults(engine, this, Methods[i]);
			}

			ExecutionTime = DateTime.Now - startTimestamp;
		}

		private readonly BenchmarkedMethodCollection _methods = new BenchmarkedMethodCollection();
	}
}
