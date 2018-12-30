using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DotnetPack
{
    internal static class Rid
    {
        public static readonly List<string> All;

        static Rid()
        {
            All = new List<string> {WindowsRid64, WindowsRid86, LinuxRid, OsxRid};
        }

        public static string WindowsRid64 = "win-x64";

        public static string WindowsRid86 = "win-x86";

        public static string LinuxRid = "linux-x64";

        public static string OsxRid = "osx-x64";

        public static string Current()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                return WindowsRid64;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                RuntimeInformation.ProcessArchitecture == Architecture.X86)
            {
                return WindowsRid86;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return LinuxRid;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OsxRid;
            }

            throw new Exception("Unknown platform. Set it explicitly using -r flag.");
        }
    }
}