using System;
using System.Runtime.InteropServices;
using DotnetPack.Exceptions;

namespace DotnetPack
{
    internal static class Platform
    {
        public static Value Current()
        {
            if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
            {
                throw new DotnetPackException("DotnetPack only supports x64 architectures.");
            }
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Value.Windows;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Value.Linux;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Value.MacOs;
            }

            throw new Exception("Unknown platform.");
        }
        
        internal enum Value
        {
            Windows,
            Linux,
            MacOs
        }
    }
}