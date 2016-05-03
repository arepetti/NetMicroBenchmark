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

namespace MicroBench.Engine
{
	/// <summary>
	/// Contains all options used by a <see cref="BenchmarkEngine"/> to find
	/// and execute a set of benchmarks.
	/// </summary>
	public sealed class BenchmarkOptions
	{
		/// <summary>
		/// Gets/sets the method used by the search engine to find benchmarks to execute.
		/// </summary>
		/// <value>
		/// The method used to search benchmarks that must be executed from a given set of assemblies
		/// and their methods to test. Default value is <see cref="BencharkSearchMethod.Declarative"/>
		/// which means that classes and methods are located using attributes.
		/// </value>
		/// <seealso cref="BenchmarkAttribute"/>
		/// <see cref="BenchmarkedMethodAttribute"/>
		public BencharkSearchMethod SearchMethod
		{
			get { return _searchMethod; }
			set { _searchMethod = value; }
		}

		/// <summary>
		/// Indicates whether each single tests is executed on its own AppDomain.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if each single test is executed (and measured) in a new
		/// fresh AppDomain (which will be disposed immediately after execution). This procedures
		/// reduces interference between each run but it will be slower to execute and may cause additional
		/// system resources pressure. Default value is <see langword="true"/>. Note that disabling isolation
		/// also enable execution of benchmarks defined in dynamic assemblies and in assemblies loaded
		/// with <c>Assembly.LoadFrom()</c>.
		/// </value>
		public bool RunTestsInIsolation
		{
			get { return _runTestsInIsolation; }
			set { _runTestsInIsolation = value; }
		}

		/// <summary>
		/// Indicates whether if each test method must be first called without measuring.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if each test method must be firt called without actually meauring its
		/// execution time, a second (measured) call will then be performed. This behavior is used to minimize
		/// bias caused by JIT compilation but it may also change results because of caching (at some level).
		/// This value should be always <see langword="true"/> (default value) but when working with a high data volume or for
		/// I/O centric tests you may also want to make this <see langword="false"/> (or using custom ad-hoc
		/// clean-up methods to ensure caching won't affect measures too much). In that case you may also want to ignore
		/// best/worst measures (see, for example, <see cref="MicroBench.Engine.Calculations.BasicStatistics.CutTails"/>).
		/// Note that default value may be overridden by single tests using <see cref="BenchmarkedMethodAttribute.WarmUp"/>
		/// however comparison made using different settings may produce unexpected, unrealistic or even <em>wrong</em> results.
		/// </value>
		public bool WarmUp
		{
			get { return _warmUp; }
			set { _warmUp = value; }
		}

		/// <summary>
		/// Gets/sets the number of repetitions for each test.
		/// </summary>
		/// <value>
		/// The number of repetitions for each test. Default value is 100 but it may be overridden by single
		/// tests using <see cref="BenchmarkedMethodAttribute.Repetitions"/>. A reasonably high number of repetitions
		/// will give better statistical results (also to remove outliers). However note that other factors may play
		/// an important role when repeating the same code again and again (see discussion about <see cref="WarmUp"/>).
		/// </value>
		public int Repetitions
		{
			get { return _repetitions; }
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException();

				_repetitions = value;
			}
		}

		private BencharkSearchMethod _searchMethod = BencharkSearchMethod.Declarative;
		private bool _runTestsInIsolation = true;
		private bool _warmUp = true;
		private int _repetitions = 100;
	}

}
