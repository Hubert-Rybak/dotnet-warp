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
            All = new List<string> {WindowsRid, LinuxRid, OsxRid};
        }

        public static string WindowsRid = "win-x64";
        
        public static string LinuxRid = "linux-x64";
        
        public static string OsxRid = "osx-x64";

        public static string CurrentRid()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsRid;
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

        public static void EnsureValid(string rid)
        {
            if (rid != null && !Rid.All.Contains(rid))
            {
                throw new Exception(
                    $"Supplied runtime {rid} is not valid. Available values: {string.Join(",", Rid.All)}");
            }
        }
    }
}