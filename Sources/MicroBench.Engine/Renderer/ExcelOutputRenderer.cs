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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MicroBench.Engine.Renderer
{
	/// <summary>
	/// Specialization of <see cref="OutputRenderer"/> to create a Microsoft Excel document
	/// containing all raw measures.
	/// </summary>
	public sealed class ExcelOutputRenderer : OutputRenderer
	{
		public override byte[] Render(IEnumerable<Benchmark> benchmarks)
		{
			using (var document = CreateAndFillSpreadsheet(benchmarks))
			{
				return InMemoryDocumentToBytes(document);
			}
		}

		public override void RenderTo(string outputPath, IEnumerable<Benchmark> benchmarks)
		{
			Debug.Assert(benchmarks != null);

			using (var document = CreateAndFillSpreadsheet(benchmarks))
			{
				document.SaveAs(outputPath);
			}
		}

		private static SpreadsheetLight.SLDocument CreateAndFillSpreadsheet(IEnumerable<Benchmark> benchmarks)
		{
			Debug.Assert(benchmarks != null);

			// NOTE: this renderer ignores Statistic property content, it may even be null because
			// here we only export raw data.

			var document = new SpreadsheetLight.SLDocument();

			foreach (var benchmark in benchmarks)
				AddWorksheetForBenchmark(document, benchmark);

			// SL can't change name of current worksheet then we left first default one empty and we delete it now.
			DeleteFirstWorksheet(document);

			return document;
		}

		private static void AddWorksheetForBenchmark(SpreadsheetLight.SLDocument document, Benchmark benchmark)
		{
			Debug.Assert(document != null);
			Debug.Assert(benchmark != null);

			// Each benchmark has its own worksheet
			document.AddWorksheet(benchmark.Name);

			// First row is test name and rows under it are measures, each column is a separate test
			int columnIndex = 1;
			foreach (var method in benchmark.Methods)
				WriteMeasuresForSingleTest(document, columnIndex++, method);
		}

		private static void WriteMeasuresForSingleTest(SpreadsheetLight.SLDocument document, int columnIndex, BenchmarkedMethod method)
		{
			Debug.Assert(document != null);
			Debug.Assert(columnIndex > 0);
			Debug.Assert(method != null);

			int rowIndex = 1;

			document.SetCellValue(rowIndex++, columnIndex, method.Name);

			foreach (var measure in method.Measures)
				document.SetCellValue(rowIndex++, columnIndex, measure.TotalMilliseconds);
		}

		private static void DeleteFirstWorksheet(SpreadsheetLight.SLDocument document)
		{
			Debug.Assert(document != null);

			// If we have just one worksheet then test was empty (!) or something went wrong,
			// just do nothing and let caller manage an erroneous situation (if any)
			var worksheets = document.GetWorksheetNames();
			if (worksheets.Count > 1)
				document.DeleteWorksheet(worksheets[0]);
		}

		private static byte[] InMemoryDocumentToBytes(SpreadsheetLight.SLDocument document)
		{
			Debug.Assert(document != null);

			using (var stream = new MemoryStream())
			{
				document.SaveAs(stream);

				return stream.ToArray();
			}
		}
	}
}
