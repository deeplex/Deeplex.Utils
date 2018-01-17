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

namespace Deeplex.Utils.Random
{
    public class UniformRandomDoubleDistribution :
        IRandomDoubleDistribution
    {
        private const ulong mMask = 0x3FFul << 52;

        public IUniformRandomBitGenerator Generator { get; set; }

        public double NextDouble()
            => Next(Generator);

        public double NextDouble(IUniformRandomBitGenerator generator)
            => Next(generator);

        public static double Next(IUniformRandomBitGenerator generator)
        {
            var entropy = mMask | (generator.Next64Bits() >> 12);
            return BitConverter.Int64BitsToDouble((long) entropy) - 1.0;
        }
    }
}