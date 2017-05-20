//           Copyright © 2017 Henrik S. Gaßmann
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using static NotEnoughTime.Utils.NativeLoader.PlatformApi;

namespace NotEnoughTime.Utils.NativeLoader
{
    public static class LoaderApi
    {
        private static readonly string ArchitectureDirectory;

        static LoaderApi()
        {

            switch (RuntimeInformation.ProcessArchitecture)
            {
            case Architecture.X86:
                ArchitectureDirectory = "x86";
                Debug.Assert(IntPtr.Size == 4, "Process architecture <-> IntPtr.Size mismatch");
                break;
            case Architecture.X64:
                ArchitectureDirectory = "x64";
                Debug.Assert(IntPtr.Size == 8, "Process architecture <-> IntPtr.Size mismatch");
                break;
            case Architecture.Arm:
                ArchitectureDirectory = "arm";
                break;
            case Architecture.Arm64:
                ArchitectureDirectory = "arm64";
                break;

            default:
                throw new ArgumentOutOfRangeException();
            }
        }

        public static bool IsNativeLibraryLoaded(string library)
        {
            if (IsWindows)
            {
                var handle = IntPtr.Zero;
                return Windows.GetModuleHandleEx(GetModuleHandleExFlags.UnchangedRefCount, library, ref handle) != 0
                       && handle != IntPtr.Zero;
            }
            else if (IsLinux)
            {
                var handle = Linux.dlopen(library, DlOpenFlags.NoLoad);
                var result = handle != IntPtr.Zero;
                if (result)
                {
                    Linux.dlclose(handle);
                }
                return result;
            }
            else if (IsOSX)
            {
                var handle = OSX.dlopen(library, DlOpenFlags.NoLoad);
                var result = handle != IntPtr.Zero;
                if (result)
                {
                    OSX.dlclose(handle);
                }
                return result;
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void PinDllImportLibrary(string library)
        {
#if NET46
            PinDllImportLibraryUnconditionally(library);
#endif
        }

        public static void PinDllImportLibraryUnconditionally(string library)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }
            if (!IsWindows)
            {
                throw new PlatformNotSupportedException();
            }

            // mimic LoadLibraryEx behaviour,
            // see https://github.com/dotnet/coreclr/issues/8902
            var dllName = Path.GetFileName(library);
            if (!dllName.Contains("."))
            {
                library += ".dll";
                dllName += ".dll";
            }

            string dllPath;
            if (!Path.IsPathRooted(library))
            {
                var appDirectory = AppContext.BaseDirectory;
                if (File.Exists(Path.Combine(appDirectory, library)))
                {
                    return;
                }
                dllPath = Path.Combine(appDirectory, ArchitectureDirectory, library);
            }
            else
            {
                dllPath = library;
            }

            if (Windows.LoadLibraryEx(dllPath, IntPtr.Zero, LoadLibraryExFlags.LoadWithAlteredSearchPath) == IntPtr.Zero)
            {
                var cause = GetLastWin32ErrorAsException();
                throw new DllNotFoundException($"Failed to load the native library \"{library}\".", cause);
            }

            // prevent accidental unloading
            var handle = IntPtr.Zero;
            if (Windows.GetModuleHandleEx(GetModuleHandleExFlags.Pin, dllName, ref handle) == 0)
            {
                var cause = GetLastWin32ErrorAsException();
                throw new DllNotFoundException($"Failed to pin the native library \"{library}\".", cause);
            }
        }
    }
}
