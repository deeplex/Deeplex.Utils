// Copyright © 2016 Henrik Steffen Gaßmann
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace NotEnoughTime.Utils.Random
{
    public class UniformRandomIntDistribution : IRandomIntDistribution
    {
        private int mLowerBound = int.MinValue;
        private int mUpperBound = int.MaxValue;

        public int LowerBound
        {
            get { return mLowerBound; }
            set
            {
                if (value > mUpperBound)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"Tried to set the LowerBoundary to a value higher than the UpperBound ({value} > {mUpperBound})");
                mLowerBound = value;
            }
        }

        public int UpperBound
        {
            get { return mUpperBound; }
            set
            {
                if (value < mLowerBound)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"Tried to set the LowerBoundary to a value higher than the UpperBound ({value} > {mLowerBound})");
                mUpperBound = value;
            }
        }

        private uint Delta => (uint) (UpperBound - LowerBound);

        public IUniformRandomBitGenerator Generator { get; set; }

        public int NextInt()
            => NextInt(Generator);

        public int NextInt(IUniformRandomBitGenerator generator)
            => (int) Adjust(Generator, Delta) + LowerBound;

        public static int Next(IUniformRandomBitGenerator generator, int lowerBound, int upperBound)
            => (int) Adjust(generator, (uint) (upperBound - lowerBound)) + lowerBound;

        private static uint Adjust(IUniformRandomBitGenerator generator, uint upperBoundary)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));
            if (upperBoundary == 0)
                return 0;
            if (upperBoundary == uint.MaxValue)
                return generator.Next32Bits();

            ++upperBoundary;
            var numBuckets = ulong.MaxValue/upperBoundary;
            var limit = numBuckets*upperBoundary;
            ulong sample;
            do
            {
                sample = generator.Next64Bits();
            } while (sample >= limit);
            return (uint) (sample/numBuckets);
        }
    }
}