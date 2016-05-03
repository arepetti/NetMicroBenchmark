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

namespace MicroBench
{
	/// <summary>
	/// Marks e method as part of a benchmark.
	/// </summary>
	/// <remarks>
	/// This attribute is used to mark a method to be executed as a test of a benchmark. Each test
	/// within the same class (marked with <see cref="BenchmarkAttribute"/>) will be executed and then
	/// compared to each other.
	/// </remarks>
	[Serializable]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class BenchmarkedMethodAttribute : Attribute
    {
		/// <summary>
		/// Gets/sets friendly name of this test.
		/// </summary>
		/// <value>
		/// The friendly name displayed to identify this test, if omitted it's method name.
		/// Default value is <see langword="null"/>.
		/// </value>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets a custom and optional description for this test.
		/// </summary>
		/// <value>
		/// A custom and optional description for this test, renderers may display this name
		/// to describe this specific test within a benchmark. Default value is <see langword="null"/>.
		/// </value>
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		/// Indicates wheather method marked with this attribute must be executed once
		/// before measuring its performance.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this method should be executed once before starting
		/// measuring its performance to allow JIT compilation. This is usually a desiderable
		/// behavior unless you also want to compare this overhead in your tests. Default
		/// value is <see langword="true"/>
		/// </value>
		public bool? WarmUp
		{
			get { return _warmUp; }
			set { _warmUp = value; }
		}

		/// <summary>
		/// Indicates how many times this method must be executed to measure
		/// its performance.
		/// </summary>
		/// <value>
		/// How many times this method must be executed to measure its performance. Benchmarking
		/// engine will then use this set of measures to calculate some statistics. This value,
		/// if specified, overrides <c>BenchmarkOptions.Repetitions</c> for this method. Default
		/// value is <see langword="null"/>.
		/// </value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// If specified value is not <see langword="null"/> and it is less than one.
		/// </exception>
		public int? Repetitions
		{
			get { return _repetitions; }
			set
			{
				if (value != null & (int)value <= 0)
					throw new ArgumentOutOfRangeException();

				_repetitions = value;
			}
		}

		private int? _repetitions;
		public bool? _warmUp;
    }
}
