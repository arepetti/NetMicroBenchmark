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
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;
using MicroBench.Engine.Renderer;
using MicroBench.Engine.Calculations;

namespace MicroBench.Engine
{
    /// <summary>
    /// Engine to finf and execute a suite of benchmarks.
    /// </summary>
    public sealed class BenchmarkEngine
    {
        /// <summary>
        /// Executes all benchmarks found in the calling assembly and render basic statistical
        /// results into an HTML file.
        /// </summary>
        /// <param name="outputPath">
        /// Full path of output HTML file, existing file will be overwritten. If omitted a new file in the
        /// temporary folder will be created.
        /// </param>
        /// <param name="options">
        /// Options, if omitted uses default for everything but <see cref="BenchmarkOptions.SearchMethod"/> which
        /// is set to <see cref="BencharkSearchMethod.Convention"/>.
        /// </param>
        /// <returns>
        /// Full path of the rendered output file, if <paramref name="outputPath"/> is not omitted then it has the same value.
        /// </returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string ExecuteAndRenderWithDefaults(string outputPath = null, BenchmarkOptions options = null)
        {
            string templatePath = Path.Combine(Environment.CurrentDirectory, HtmlOutputRenderer.DefaultReportName);
            if (String.IsNullOrWhiteSpace(outputPath))
                outputPath = Path.Combine(Path.GetTempPath(), HtmlOutputRenderer.DefaultReportName);

            var engine = new BenchmarkEngine(options ?? new BenchmarkOptions { SearchMethod = BencharkSearchMethod.Convention },
                new Assembly[] { Assembly.GetCallingAssembly() });

            var renderer = new HtmlOutputRenderer();
            renderer.TemplatePath = templatePath;
            renderer.Statistics = new BasicStatistics() { CutTails = true };
            renderer.RenderTo(outputPath, engine.Execute());

            return outputPath;
        }

        /// <summary>
        /// Executes one benchmark in the given type and calculates average execution time.
        /// </summary>
        /// <param name="type">
        /// Type which contains the method to benchmark. This method must contain exactly one method to benchmark and
        /// it may be decorated with <c>BenchmarkedMethodAttribute</c>. Its name has to start with
        /// <c>Test</c> or to be the only public eligible method in the class.
        /// </param>
        /// <param name="options">Options, if omitted then default <see cref="BenchmarkOptions"/> are used.</param>
        /// <returns>
        /// The average execution time for the benchmark contained in the specified type.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="type"/> does not contain exactly one method to benchmark (public non-virtual instance method
        /// with name that starts with <c>Test</c>, name constraint is not mandatory if there is only one public method).
        /// </exception>
        public static TimeSpan ExecuteSingle(Type type, BenchmarkOptions options = null)
        {
            var engine = new BenchmarkEngine(options ?? new BenchmarkOptions { SearchMethod = BencharkSearchMethod.Convention },
                new Type[] { type });

            var results = engine.Execute();
            var benchmark = results.Single();
            var benchmarkedMethod = benchmark.Methods.Single();
            var measures = benchmarkedMethod.Measures.Select(x => x.Ticks).ToArray();

            if (measures.Length == 0)
                return TimeSpan.Zero;

            return new TimeSpan(measures.Sum() / measures.Length);
        }

        /// <summary>
        /// Creates a new <see cref="BenchmarkEngine"/> object searching for benchmarks in all specified assemblies.
        /// </summary>
        /// <param name="options">Options for this engine to define how benchmark are searched and executed.</param>
        /// <param name="assemblies">List of assemblies on which you want to search benchmarks.</param>
        public BenchmarkEngine(BenchmarkOptions options, IEnumerable<Assembly> assemblies)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            if (assemblies == null)
                throw new ArgumentNullException("assemblies");

            Options = options;
            _factory = new BenchmarkFactoryFromAssemblies(options, assemblies);
        }

        /// <summary>
        /// Creates a new <see cref="BenchmarkEngine"/> object using specified benchmarks.
        /// </summary>
        /// <param name="options">Options for this engine to define how benchmark are searched and executed.</param>
        /// <param name="assemblies">
        /// List of all types which implement a benchmark, note that for method discovering each type must still respect
        /// <see cref="BenchmarkOptions.SearchMethod"/> value.
        /// </param>
        public BenchmarkEngine(BenchmarkOptions options, IEnumerable<Type> benchmarks)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            if (benchmarks == null)
                throw new ArgumentNullException("benchmarks");

            Options = options;
            _factory = new BenchmarkFactoryFromTypes(options, benchmarks);
        }

        /// <summary>
        /// Execute all benchmarks (suite) found by this engine and returns their result.
        /// </summary>
        /// <returns>
        /// Results for each executed benchmark. Each element is a single benchmark (without any specific
        /// ordering or grouping) and it contains all executed tests (with all collected timing samples).
        /// </returns>
        public IEnumerable<Benchmark> Execute()
        {
            Debug.Assert(_factory != null);

            var benchmarks = _factory.Create();
            Debug.Assert(benchmarks != null && !benchmarks.Any(x => x == null));

            for (int i = 0; i < benchmarks.Length; ++i)
            {
                int progress = (int)((float)i / benchmarks.Length * 100);

                OnSuiteProgressChanged(new ProgressChangedEventArgs(progress, benchmarks[i].Name));
                benchmarks[i].Perform(this);
            }

            return benchmarks;
        }

        /// <summary>
        /// Event raised to notify progress on suite. Each tick is a new benchmark about to be executed.
        /// </summary>
        public event ProgressChangedEventHandler SuiteProgressChanged;

        /// <summary>
        /// Event raised to notify progress on single benchmark. Each tick is a new method executed
        /// for each benchmark.
        /// </summary>
        /// <remarks>
        /// Please note that reported progress is relative to a single benchmark then it may reach 100%
        /// multiple times (one for each benchmark, as reported for <see cref="SuiteProgressChanged"/>).
        /// </remarks>
        public event ProgressChangedEventHandler BenchmarkProgressChanged;

        /// <summary>
        /// Gets/sets the options for benchmark execution.
        /// </summary>
        /// <value>
        /// Options for benchmark execution.
        /// </value>
        internal BenchmarkOptions Options
        {
            get;
            private set;
        }

        private readonly BenchmarkFactory _factory;

        private void OnSuiteProgressChanged(ProgressChangedEventArgs e)
        {
            var suiteProgressChanged = SuiteProgressChanged;
            if (suiteProgressChanged != null)
                suiteProgressChanged(this, e);
        }

        internal void OnBenchmarkProgressChanged(ProgressChangedEventArgs e)
        {
            var benchmarkProgressChanged = BenchmarkProgressChanged;
            if (benchmarkProgressChanged != null)
                benchmarkProgressChanged(this, e);
        }
    }
}
