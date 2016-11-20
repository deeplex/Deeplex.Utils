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

namespace NotEnoughTime.Utils.Random
{
    public abstract class Random64BitGenerator : IUniformRandomBitGenerator
    {
        public uint Next32Bits()
        {
            return (uint) Next64Bits();
        }

        public abstract ulong Next64Bits();

        public void NextBytes(byte[] buffer)
        {
            var i = 0;
            for (var boundary = buffer.Length - 7; i < boundary; i += 8)
            {
                var sample = Next64Bits();
                buffer[i] = (byte) sample;
                buffer[++i] = (byte) (sample >> 8);
                buffer[++i] = (byte) (sample >> 16);
                buffer[++i] = (byte) (sample >> 24);
                buffer[++i] = (byte) (sample >> 32);
                buffer[++i] = (byte) (sample >> 40);
                buffer[++i] = (byte) (sample >> 48);
                buffer[++i] = (byte) (sample >> 56);
            }
            if (i < buffer.Length)
            {
                var sample = Next64Bits();
                for (var shift = 0; i < buffer.Length; shift += 8)
                    buffer[i] = (byte) (sample >> shift);
            }
        }
    }
}