//           Copyright © 2017 Henrik S. Gaßmann
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Deeplex.Utils.NativeLoader
{
    internal static class PlatformApi
    {
        internal static bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        internal static bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        internal static bool IsOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static void ThrowLastWin32Error()
        {
            if (IsWindows)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
            }
        }

        public static Exception GetLastWin32ErrorAsException()
        {
            try
            {
                ThrowLastWin32Error();
            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }

        internal static bool TryLoadNativeLibrary(string filename, out IntPtr handle)
        {
            if (IsWindows)
            {
                handle = Windows.LoadLibraryEx(filename, IntPtr.Zero, 0);
                //Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
            }
            else if (IsLinux)
            {
                handle = Linux.dlopen(filename, DlOpenFlags.Lazy | DlOpenFlags.DeepBind);
            }
            else if (IsOSX)
            {
                handle = OSX.dlopen(filename, DlOpenFlags.Lazy | DlOpenFlags.DeepBind);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            return handle != IntPtr.Zero;
        }

        internal static IntPtr LoadNativeLibrary(string filename)
        {
            if (!TryLoadNativeLibrary(filename, out IntPtr handle))
            {
                var winExc = GetLastWin32ErrorAsException();
                throw new DllNotFoundException($"Failed to load native library {filename}.", winExc);
            }
            return handle;
        }

        internal static bool TryGetSymbolAddress(IntPtr handle, string symbol, out IntPtr functionPointer)
        {
            if (IsWindows)
            {
                functionPointer = Windows.GetProcAddress(handle, symbol);
                //Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
            }
            else if (IsLinux)
            {
                functionPointer = Linux.dlsym(handle, symbol);
            }
            else if (IsOSX)
            {
                functionPointer = OSX.dlsym(handle, symbol);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            return handle != IntPtr.Zero;
        }

        internal static bool CloseLibrary(IntPtr handle)
        {
            if (IsWindows)
            {
                return Windows.FreeLibrary(handle) != 0;
                //Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
            }
            else if (IsLinux)
            {
                return Linux.dlclose(handle) == 0;
            }
            else if (IsOSX)
            {
                return OSX.dlclose(handle) == 0;
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        internal static class Windows
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr LoadLibraryEx(string filename, IntPtr reserved, LoadLibraryExFlags flags);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern int GetModuleHandleEx(GetModuleHandleExFlags flags, string lpModuleName, ref IntPtr handle);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern int FreeLibrary(IntPtr hModule);
        }

        internal static class OSX
        {
            [DllImport("libSystem.dylib", CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr dlopen(string filename, DlOpenFlags flags);

            [DllImport("libSystem.dylib", CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport("libSystem.dylib", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int dlclose(IntPtr handle);
        }

        internal static class Linux
        {
            [DllImport("libcoreclr.so", CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr dlopen(string filename, DlOpenFlags flags);

            [DllImport("libcoreclr.so", CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport("libcoreclr.so", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int dlclose(IntPtr handle);
        }

        [Flags]
        internal enum GetModuleHandleExFlags : uint
        {
            None              = 0x00_00_00_00,
            Pin               = 0x00_00_00_01,
            UnchangedRefCount = 0x00_00_00_02,
            FromAddress       = 0x00_00_00_04,
        }

        [Flags]
        internal enum LoadLibraryExFlags : uint
        {
            None                            = 0x00_00_00_00,
            DontResolveDllReferences        = 0x00_00_00_01,
            LoadIgnoreCodeAuthzLevel        = 0x00_00_00_10,
            LoadLibraryAsDatafile           = 0x00_00_00_02,
            LoadLibraryAsDatafileExclusive  = 0x00_00_00_40,
            LoadLibraryAsImageResource      = 0x00_00_00_20,
            LoadLibrarySearchApplicationDir = 0x00_00_02_00,
            LoadLibrarySearchDefaultDirs    = 0x00_00_10_00,
            LoadLibrarySearchDllLoadDir     = 0x00_00_01_00,
            LoadLibrarySearchSystem32       = 0x00_00_08_00,
            LoadLibrarySearchUserDirs       = 0x00_00_04_00,
            LoadWithAlteredSearchPath       = 0x00_00_00_08,
        }

        [Flags]
        internal enum DlOpenFlags : uint
        {
            Local    = 0x00_00_00_00,
            Lazy     = 0x00_00_00_01,
            Now      = 0x00_00_00_02,
            NoLoad   = 0x00_00_00_04,
            DeepBind = 0x00_00_00_08,
            Global   = 0x00_00_01_00,
            NoDelete = 0x00_00_10_00,

        }
    }
}