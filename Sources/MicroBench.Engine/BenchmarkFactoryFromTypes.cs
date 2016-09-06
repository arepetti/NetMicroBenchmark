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
using System.Linq;

namespace MicroBench.Engine
{
    sealed class BenchmarkFactoryFromTypes : BenchmarkFactory
    {
        public BenchmarkFactoryFromTypes(BenchmarkOptions options, IEnumerable<Type> types)
            : base(options)
        {
            Debug.Assert(options != null);
            Debug.Assert(types != null);

            if (types.Any(x => !IsEligibleBenchmarkType(x)))
                throw new ArgumentException("Cannot use dynamic assemblies or assemblies loaded from a byte stream.");

            _types = types;
        }

        public override Benchmark[] Create()
        {
            Debug.Assert(_types != null);

            var benchmarks = FindBenchmarks(_types, Options.SearchMethod).ToArray();

            // If I'm searching by convention and I didn't find any method then I relax this rule
            // and I take any other eligible method regardless its name (see doc BencharkSearchMethod.Convention)
            if (benchmarks.Length == 0 && Options.SearchMethod == BencharkSearchMethod.Convention)
                benchmarks = FindBenchmarks(_types, BencharkSearchMethod.Everything).ToArray();

            return benchmarks;
        }

        private readonly IEnumerable<Type> _types;
    }
}
