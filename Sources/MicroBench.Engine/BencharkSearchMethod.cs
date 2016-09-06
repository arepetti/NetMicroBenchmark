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
    /// Search method to find benchmarks (classes) and tests (methods).
    /// </summary>
    public enum BencharkSearchMethod
    {
        /// <summary>
        /// Attributes <see cref="BenchmarkAttribute"/> and <see cref="BenchmarkedMethodAttribute"/>
        /// are used.
        /// </summary>
        Declarative,

        /// <summary>
        /// Method and classes are found by convention. Benchmarks must be public not abstract classes
        /// with public default constructor and they must start or end with <c>Benchmark</c>.
        /// Methods must be public, non virtual, parameterless, without return value and start with <c>Test</c>
        /// but if there is not any eligible methods then any method which satisfies the other rules is used
        /// regardless its name. Attributes may still be used to override default naming convention.
        /// </summary>
        Convention,

        /// <summary>
        /// As <see cref="BenchmarkSearchMethod.Convention"/> but without naming restrictions.
        /// </summary>
        Everything,
    }
}
