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
using System.Linq;
using System.Web;
using Mustache;

namespace MicroBench.Engine.Renderer
{
	/// <summary>
	/// Specialization of <see cref="TextOutputRenderer"/> to create an HTML document
	/// built using a mustache-sharp template.
	/// </summary>
	public sealed class HtmlOutputRenderer : TextOutputRenderer
	{
		/// <summary>
		/// Name (without path) of the default HTML report.
		/// </summary>
		/// <remarks>
		/// Template has one more available tag: <c>{{#join collection selector}}</c>.
		/// </remarks>
		public const string DefaultReportName = "DefaultReport.html";

		// NOTE: order in Headers property (and assumption that Results dictionary also matches that order)
		// is important (see both BasicStatistics and DefaultTemplate.html). This is a fragile solution
		// but it works for this first "release". 

		protected override string EscapeString(string text)
		{
			return HttpUtility.HtmlEncode(text);
		}

		protected override FormatCompiler CreateFormatCompiler()
		{
			var compiler = new FormatCompiler();
			compiler.RegisterTag(new MustacheJoinTagDefinition(), true);

			return compiler;
		}
	}
}
