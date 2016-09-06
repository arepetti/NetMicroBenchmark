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

namespace MicroBench.Engine.Calculations
{
    /// <summary>
    /// Base class for objects used to calculate descriptive values from benchmarks result.
    /// </summary>
    public abstract class Statistics
    {
        /// <summary>
        /// Gets the name of the <em>main</em> value extracted by <see cref="Calculate"/>.
        /// </summary>
        /// <value>
        /// Name (which is also dictionary key) of the <em>main</em> or most important
        /// value extracted by <see cref="Calculate"/>. This is used when we need
        /// one single value which best describe benchmark result (for example average execution time).
        /// </value>
        public abstract string KeyHeader
        {
            get;
        }

        /// <summary>
        /// Gets list of all values calculated by <see cref="Calculate"/>.
        /// </summary>
        /// <returns>
        /// List of all values calculated by <c>Calculate</c>.
        /// </returns>
        public abstract IEnumerable<string> GetHeaders();

        /// <summary>
        /// Calculate a set of values to describe benchmark results.
        /// </summary>
        /// <param name="method">Method which has been benchmarked.</param>
        /// <returns>
        /// Dictionary with values which decribes results of benchmark <paramref name="method"/>. Dictionary
        /// keys are given by <see cref="GetHeaders"/> and values can be both <see cref="Double"/> or <see cref="TimeSpan"/>.
        /// Unit of measure must always be <em>milliseconds</em>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="method"/> is <see langword="null"/>.
        /// </exception>
        public abstract Dictionary<string, object> Calculate(BenchmarkedMethod method);
    }
}
