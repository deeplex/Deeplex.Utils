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
using System.Security.Cryptography;

namespace Deeplex.Utils.Random
{
    public static class RandomDevice
    {
        private static readonly RandomNumberGenerator Generator
            = RandomNumberGenerator.Create();

        public static uint NextUInt()
        {
            var raw = NextBytes(4);
            return raw[0]
                   | ((uint) raw[1] << 8)
                   | ((uint) raw[2] << 16)
                   | ((uint) raw[3] << 24);
        }

        public static ulong NextULong()
        {
            var raw = NextBytes(8);
            return raw[0]
                   | ((ulong) raw[1] << 8)
                   | ((ulong) raw[2] << 16)
                   | ((ulong) raw[3] << 24)
                   | ((ulong) raw[4] << 32)
                   | ((ulong) raw[5] << 40)
                   | ((ulong) raw[6] << 48)
                   | ((ulong) raw[7] << 56);
        }

        public static void NextBytes(byte[] data)
            => Generator.GetBytes(data);

        public static byte[] NextBytes(int numBytes)
        {
            var dest = new byte[numBytes];
            NextBytes(dest);
            return dest;
        }
    }
}