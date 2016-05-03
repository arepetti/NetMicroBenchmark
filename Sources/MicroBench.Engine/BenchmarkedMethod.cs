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
using System.Reflection;

namespace MicroBench.Engine
{
	/// <summary>
	/// Represents a method to benchmark (a single test in a comparison).
	/// </summary>
	[Serializable]
	public sealed class BenchmarkedMethod
	{
		/// <summary>
		/// Gets the friendly name for this test.
		/// </summary>
		/// <value>
		/// The friendly name for this test, output renderer should use this name
		/// to refer to this test within its benchmark.
		/// </value>
		public string Name
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the optional description for this test.
		/// </summary>
		/// <value>
		/// Optional description for this test, output renderer may use this
		/// (user-defined) text to provide more information about a specific test.
		/// </value>
		public string Description
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the list of measures collected executing this test.
		/// </summary>
		/// <value>
		/// The list of measures collected executing this method. Each measure is
		/// a separate run.
		/// </value>
		public MeasureCollection Measures
		{
			get { return _measures; }
		}

		/// <summary>
		/// Gets/sets the method that must be executed.
		/// </summary>
		/// <value>
		/// The method that must be executed. Please note that this method must be
		/// an instance method and a new instance is created for each run.
		/// </value>
		internal MethodInfo Method
		{
			get;
			set;
		}

		/// <summary>
		/// Indicates whether this method must be executed once without measuring it.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this method must be executed once before actual measure.
		/// Warm-up ensures we are not measuring JIT compiling time but only (approximately!) execution time.
		/// </value>
		internal bool WarmUp
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets the number of time this method must be executed.
		/// </summary>
		/// <value>
		/// Number of times this method must be executed, each run is measured separately on its own
		/// fresh instance in a separate <c>AppDomain</c>.
		/// </value>
		internal int Repetitions
		{
			get;
			set;
		}

		private MeasureCollection _measures = new MeasureCollection();
	}
}
