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
using System.Runtime.CompilerServices;

namespace Deeplex.Utils.Random
{
    // Ported from http://xoroshiro.di.unimi.it/xoroshiro128plus.c
    public sealed class XoroShiro128Plus : Random64BitGenerator
    {
        private ulong mS1;
        private ulong mS2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private XoroShiro128Plus(ulong s1, ulong s2)
        {
            mS1 = s1;
            mS2 = s2;
            Next64Bits();
        }

        private XoroShiro128Plus(SplitMix64 tempGen)
            : this(tempGen.Next64Bits(), tempGen.Next64Bits())
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XoroShiro128Plus()
            : this(RandomDevice.NextULong(), RandomDevice.NextULong())
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XoroShiro128Plus Create(ulong seed)
        {
            return new XoroShiro128Plus(SplitMix64.Create(seed));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XoroShiro128Plus Create(ulong state1, ulong state2)
        {
            return new XoroShiro128Plus(state1, state2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ulong Next64Bits()
        {
            var s1 = mS1;
            var s2 = mS2;
            var result = s1 + s2;

            s2 ^= s1;
            mS1 = RotateLeft(s1, 55) ^ s2 ^ (s2 << 14); // a, b
            mS2 = RotateLeft(s2, 36); // c

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong RotateLeft(ulong x, int num)
            => (x << num) | (x >> (64 - num));
    }
}