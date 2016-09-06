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
using System.Collections.ObjectModel;

namespace MicroBench.Engine
{
    /// <summary>
    /// Represents a collection of measures (where a measure is the execution time
    /// of a single run of each test).
    /// </summary>
    [Serializable]
    public sealed class MeasureCollection : Collection<TimeSpan>
    {
        /// <summary>
        /// Calculates the sum of all measures in this collections.
        /// </summary>
        /// <returns>
        /// The sum of all measures in this collection.
        /// </returns>
        public TimeSpan Sum()
        {
            if (Count == 0)
                return TimeSpan.Zero;

            return new TimeSpan(Items.Sum(x => x.Ticks));
        }

        /// <summary>
        /// Calculates the average of all measures in this collections.
        /// </summary>
        /// <returns>
        /// The average of all measures in this collection or <see cref="TimeSpan.Zero"/>
        /// if this collection is empty.
        /// </returns>
        public TimeSpan Average()
        {
            if (Count == 0)
                return TimeSpan.Zero;

            return new TimeSpan(Items.Sum(x => x.Ticks) / Count);
        }
    }
}
