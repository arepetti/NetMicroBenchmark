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
using System.Reflection;

namespace MicroBench.Engine
{
    sealed class BenchmarkFactoryFromAssemblies : BenchmarkFactory
    {
        public BenchmarkFactoryFromAssemblies(BenchmarkOptions options, IEnumerable<Assembly> assemblies)
            : base(options)
        {
            Debug.Assert(options != null);
            Debug.Assert(assemblies != null);

            if (assemblies.Any(x => !IsEligibleAssembly(x)))
                throw new ArgumentException("Cannot use dynamic assemblies or assemblies loaded from a byte stream.");

            _assemblies = assemblies;
        }

        public override Benchmark[] Create()
        {
            Debug.Assert(_assemblies != null);

            var benchmarks = _assemblies
                .SelectMany(x => FindBenchmarks(x.GetExportedTypes().Where(IsEligibleBenchmarkType), Options.SearchMethod))
                .ToArray();

            // If I'm searching by convention and I didn't find any method then I relax this rule
            // and I take any other eligible method regardless its name (see doc BencharkSearchMethod.Convention)
            if (benchmarks.Length == 0 && Options.SearchMethod == BencharkSearchMethod.Convention)
            {
                benchmarks = _assemblies
                    .SelectMany(x => FindBenchmarks(x.GetExportedTypes().Where(IsEligibleBenchmarkType), BencharkSearchMethod.Everything))
                    .ToArray();
            }

            return benchmarks;
        }

        private readonly IEnumerable<Assembly> _assemblies;

        private bool IsEligibleAssembly(Assembly assembly)
        {
            Debug.Assert(assembly != null);

            if (!Options.RunTestsInIsolation)
                return true;

            // This is required because of the way we execute benchmark loading assembly into another AppDomain.
            return !assembly.IsDynamic && !String.IsNullOrEmpty(assembly.Location);
        }
    }
}
