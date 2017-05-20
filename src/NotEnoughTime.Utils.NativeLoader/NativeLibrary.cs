//           Copyright © 2017 Henrik S. Gaßmann
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NotEnoughTime.Utils.NativeLoader
{
    public class NativeLibrary : SafeHandle
    {
        private readonly string library;

        internal NativeLibrary(ref IntPtr handle, string library, bool owning = true)
            : base(IntPtr.Zero, owning)
        {
            this.library = library ?? throw new ArgumentNullException(nameof(library));
            this.handle = handle;
            handle = IntPtr.Zero;
        }

        public static bool TryLoad(string library, out NativeLibrary handle)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }
            if (!PlatformApi.TryLoadNativeLibrary(library, out IntPtr nativeHandle))
            {
                handle = null;
                return false;
            }
            try
            {
                handle = new NativeLibrary(ref nativeHandle, library);
                return true;
            }
            catch (Exception) when (nativeHandle != IntPtr.Zero)
            {
                PlatformApi.CloseLibrary(nativeHandle);
                throw;
            }
        }

        public static NativeLibrary Load(string library)
        {
            var nativeHandle = PlatformApi.LoadNativeLibrary(library);
            try
            {
                return new NativeLibrary(ref nativeHandle, library);
            }
            catch (Exception) when (nativeHandle != IntPtr.Zero)
            {
                PlatformApi.CloseLibrary(nativeHandle);
                throw;
            }
        }

        public bool TryGetSymbolAddress(string symbol, out IntPtr address)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }
            if (IsInvalid)
            {
                throw new InvalidOperationException();
            }
            return PlatformApi.TryGetSymbolAddress(handle, symbol, out address);
        }

        public bool TryGetSymbol<T>(string symbol, out T function) where T : class
        {
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }
            if (IsInvalid)
            {
                throw new InvalidOperationException();
            }
#if NET46 && false // TryEmitPinvoke hasn't been implemented yet.
            return TryEmitPinvoke(symbol, out function);
#else
            function = TryGetSymbolAddress(symbol, out IntPtr fp)
                ? Marshal.GetDelegateForFunctionPointer<T>(fp)
                : null;
            return function != null;
#endif
        }

        public T GetSymbol<T>(string symbol) where T : class
        {
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }
            if (IsInvalid)
            {
                throw new InvalidOperationException();
            }
            if (!TryGetSymbol(symbol, out T function))
            {
                var winExc = PlatformApi.GetLastWin32ErrorAsException();
                throw new MissingMethodException($"The library \"{library}\" doesn't contain the symbol \"{symbol}\"", winExc);
            }
            return function;
        }


        /// <inheritdoc />
        protected override bool ReleaseHandle()
        {
            return PlatformApi.CloseLibrary(handle);
        }

        /// <inheritdoc />
        public override bool IsInvalid => handle == IntPtr.Zero;

#if NET46
        private bool TryEmitPinvoke<T>(string symbol, out T function)
        {
            throw new NotImplementedException();
        }
#endif
    }
}