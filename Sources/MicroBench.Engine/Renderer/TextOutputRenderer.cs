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
using System.IO;
using System.Linq;
using System.Text;
using Mustache;

namespace MicroBench.Engine.Renderer
{
    /// <summary>
    /// Base class text documents using a (mustache) text template.
    /// </summary>
    /// <remarks>
    /// This renderer may be used directly to render into plain text documents or inherited to
    /// support more advanced features for specific textual formats.
    /// </remarks>
    public class TextOutputRenderer : OutputRenderer
    {
        /// <summary>
        /// Gets/sets full path of template file.
        /// </summary>
        /// <value>
        /// Full path of template file.
        /// </value>
        /// <remarks>
        /// You  must set this property before you call <c>Render()</c> otherwise
        /// <see cref="InvalidOperationException"/> is raised.
        /// </remarks>
        public string TemplatePath
        {
            get;
            set;
        }

        public override byte[] Render(IEnumerable<Benchmark> benchmarks)
        {
            if (Statistics == null)
                throw new InvalidOperationException("You must specify a statistical analysis to perform.");

            if (String.IsNullOrWhiteSpace(TemplatePath))
                throw new InvalidOperationException("You must specify a mustache-sharp template path.");

            // TODO: this method is tightly coupled with text template we use. Ideally we MAY drop Mustache
            // in favor of Razor to support this "calculations" directly inside report template using
            // Benchmark as its ViewModel.
            var result = new
            {
                Timestamp = DateTime.Now,
                Benchmarks = benchmarks.Select(b => new
                {
                    Id = b.Name.GetHashCode(),
                    Group = EscapeString(b.Group),
                    Name = EscapeString(b.Name),
                    Description = EscapeString(b.Description),
                    ExecutionTime = b.ExecutionTime,
                    Headers = Statistics.GetHeaders().Select(x => EscapeString(x)),
                    Tests = b.Methods.Select(t => new TestSet
                    {
                        Name = EscapeString(t.Name),
                        Description = EscapeString(t.Description),
                        Results = Statistics.Calculate(t).ToDictionary(x => EscapeString(x.Key), x => x.Value),
                    }).ToArray()
                }).ToArray()
            };

            // Dropping some LINQ we may avoid this second loop however it should not be an issue because
            // this time should be negligible compared to overall benchmark time.
            foreach (var benchmark in result.Benchmarks)
            {
                var best = benchmark.Tests.OrderBy(x => x.Results[Statistics.KeyHeader]).FirstOrDefault();
                if (best != null)
                    best.IsBest = true;

                foreach (var test in benchmark.Tests)
                    test.SignificativeMeasure = test.Results[Statistics.KeyHeader];
            }

            return Encoding.UTF8.GetBytes(RenderTextTemplate(result));
        }

        protected virtual string EscapeString(string text)
        {
            return text;
        }

        protected virtual FormatCompiler CreateFormatCompiler()
        {
            return new FormatCompiler();
        }

        private sealed class TestSet
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public Dictionary<string, object> Results { get; set; }
            public bool IsBest { get; set; }
            public object SignificativeMeasure { get; set; }
        }

        private string RenderTextTemplate(object model)
        {
            // https://github.com/jehugaleahsa/mustache-sharp
            var compiler = CreateFormatCompiler();
            compiler.RemoveNewLines = false;

            return compiler.Compile(File.ReadAllText(TemplatePath, Encoding.UTF8)).Render(model);
        }
    }
}
