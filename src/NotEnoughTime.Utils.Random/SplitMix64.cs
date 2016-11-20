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

using System.Runtime.CompilerServices;

namespace NotEnoughTime.Utils.Random
{
    // Ported from http://xoroshiro.di.unimi.it/splitmix64.c
    public sealed class SplitMix64 : Random64BitGenerator
    {
        private ulong mS;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SplitMix64(ulong state)
        {
            mS = state;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SplitMix64()
            : this(RandomDevice.NextULong())
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SplitMix64 Create(ulong state)
        {
            return new SplitMix64(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ulong Next64Bits()
        {
            var z = mS += 0x9E3779B97F4A7C15;
            z = (z ^ (z >> 30))*0xBF58476D1CE4E5B9;
            z = (z ^ (z >> 27))*0x94D049BB133111EB;
            return z ^ (z >> 31);
        }
    }
}