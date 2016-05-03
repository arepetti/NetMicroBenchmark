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
using MicroBench;

namespace ManualTest
{
	// NOTE: this is not a true benchmark but a test for engine options. Do not use this code as reference or tested "algorithms" in production code!
	[Benchmark(Name = "Matrix multiplication", Description = "This is a test for benchmarking engine")]
	public sealed class Benchmark
	{
		public Benchmark()
		{
			_rnd = new Random();

			_a = Create(Rows, Columns);
			_b = Create(Columns, Rows);
			_c = Create(Rows, Rows);
		}

		[BenchmarkedMethod(Name = "Textbook implementation")]
		public void Test1()
		{
			for (int i = 0; i < Rows; ++i)
			{
				for (int j = 0; j < Rows; ++j)
				{
					for (int k = 0; k < Columns; ++k)
					{
						_c[i, j] += _a[i, k] * _b[k, j];
					}
				}
			}
		}

		[BenchmarkedMethod(Name = "Inner loop moved outside")]
		public void Test2()
		{
			for (int i = 0; i < Rows; ++i)
			{
				for (int k = 0; k < Columns; ++k)
				{
					for (int j = 0; j < Rows; ++j)
					{
						_c[i, j] += _a[i, k] * _b[k, j];
					}
				}
			}
		}

		[BenchmarkedMethod(Name = "Manual loop unrolling")]
		public void Test3()
		{
			for (int i = 0; i < Rows; ++i)
			{
				for (int j = 0; j < Rows; ++j)
				{
					for (int k = 0; k < Columns; k += 2)
					{
						_c[i, j] += _a[i, k] * _b[k, j];
						_c[i, j] += _a[i, k + 1] * _b[k + 1, j];
					}
				}
			}
		}

		private const int Rows = 256;
		private const int Columns = 16;

		private readonly Random _rnd;
		private readonly double[,] _a, _b, _c;

		private double[,] Create(int rows, int columns)
		{
			var matrix = new double[rows, columns];
			for (int row = 0; row < rows; ++row)
			{
				for (int column = 0; column < columns; ++column)
				{
					matrix[row, column] = _rnd.NextDouble();
				}
			}

			return matrix;
		}
	}
}
