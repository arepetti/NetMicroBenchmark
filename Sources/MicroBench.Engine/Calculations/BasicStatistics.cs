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

namespace MicroBench.Engine.Calculations
{
	/// <summary>
	/// Specialization of <see cref="Statistics"/> to provide some simple dispersion analysis.
	/// </summary>
	/// <remarks>
	/// This class returns these values (in this order): minimum, lower quartile, average,
	/// upper quartile and maximum. Best value (obtained by <see cref="KeyHeader"/>) is average.
	/// </remarks>
	public sealed class BasicStatistics : Statistics
	{
		/// <summary>
		/// Indicates whether <em>tails</em> (best and worst results) will be ignored.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if best and worst test results will be ignored. Default
		/// value is <see langword="false"/>.
		/// </value>
		public bool CutTails
		{
			get;
			set;
		}

		public override string KeyHeader
		{
			get { return Properties.Resources.Average; }
		}

		public override IEnumerable<string> GetHeaders()
		{
			// TODO: please note that ordering should not matter but actual implementation
			// in the default template relies on this exact order (it's what charting component
			// expects for box plot data serie).
			return new string[]
			{
				Properties.Resources.Minimum,
				Properties.Resources.LowerQuartile,
				Properties.Resources.Average,
				Properties.Resources.UpperQuartile,
				Properties.Resources.Maximum,
			};
		}

		public override Dictionary<string, object> Calculate(BenchmarkedMethod method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			var measures = GetMeasuresToAnalyze(method);
			double total = 0, minimum = 0, maximum = 0;

			foreach (var measure in measures)
			{
				total += measure;

				if (minimum == 0) // Simple way, execution time may be small but it is always higher than zero
					minimum = measure;
				else
					minimum = Math.Min(minimum, measure);
					
				maximum = Math.Max(maximum, measure);
			}

			double average = measures.Length > 0 ? (total / measures.Length) : 0.0;

			return new Dictionary<string, object>
			{
				{ Properties.Resources.Minimum, TimeSpanFromMilliseconds(minimum) },
				{ Properties.Resources.LowerQuartile, TimeSpanFromMilliseconds(Percentile(measures, 0.25)) },
				{ Properties.Resources.Average, TimeSpanFromMilliseconds(average) },
				{ Properties.Resources.UpperQuartile, TimeSpanFromMilliseconds(Percentile(measures, 0.75)) },
				{ Properties.Resources.Maximum, TimeSpanFromMilliseconds(maximum) },
			};
		}

		private double[] GetMeasuresToAnalyze(BenchmarkedMethod method)
		{
			var measures = method.Measures.Select(x => x.TotalMilliseconds).OrderBy(x => x);
			int measureCount = measures.Count();

			// CHECK: To cut best and worst results we need at least three samples, it's little
			// bit arbitrary but we may want to increase this threshold to a bigger population
			// because with a small number of samples this cut may be significative.
			if (!CutTails || measureCount <= 2)
				return measures.ToArray();

			return measures.Skip(1).Take(measureCount - 2).ToArray();
		}

		private static TimeSpan TimeSpanFromMilliseconds(double milliseconds)
		{
			// In the range we're working here we don't need to worry about TimeSpan range conversion errors
			// but we really want to - at least - appreciate 1/10th of millisecond
			return new TimeSpan((long)(milliseconds * TimeSpan.TicksPerMillisecond));
		}

		private static double Percentile(double[] sequence, double percentile)
		{
			// http://web.stanford.edu/class/archive/anthsci/anthsci192/anthsci192.1064/handouts/calculating%20percentiles.pdf
			// http://www.techonthenet.com/excel/formulas/percentile.php (see http://stackoverflow.com/a/8137526/1207195)
			double realIndex = percentile * (sequence.Length - 1);
			int index = (int)realIndex;
			double frac = realIndex - index;
			
			if (index + 1 < sequence.Length)
				return sequence[index] * (1 - frac) + sequence[index + 1] * frac;

			return sequence[index];
		}
	}
}
