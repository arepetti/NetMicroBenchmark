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
    /// Indicates that a class represents a benchmark to execute.
    /// </summary>
    /// <remarks>
    /// Class must not be abstract and must have a public default constructor. Use <see cref="BenchmarkedMethodAttribute"/>
    /// to mark each method you want to benchmark and to compare to the others within this benchmark.
    /// </remarks>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class BenchmarkAttribute : Attribute
    {
        /// <summary>
        /// Gets/sets the friendly name of a group of benchmarks.
        /// </summary>
        /// <value>
        /// Friendly name of a group of benchmarks. Output renderers may, for example, order and group
        /// multiple benchmarks according to this property. Default value is <see langword="null"/>.
        /// </value>
        public string Group
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the friendly name of this benchmark.
        /// </summary>
        /// <value>
        /// Friendly name of this benchmark, if omitted then type name will be used. This name will used
        /// to refer to this benchmark. Default value is <see langword="null"/>.
        /// </value>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets a custom and optional description for this benchmark.
        /// </summary>
        /// <value>
        /// A custom and optional description for this benchmark, renderers may display this name
        /// to describe the benchmark. Default value is <see langword="null"/>.
        /// </value>
        public string Description
        {
            get;
            set;
        }
    }
}
