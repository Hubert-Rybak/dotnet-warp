using System;
using System.Collections.Generic;

namespace DotnetPack
{
    internal static class Validator
    {

        public static void EnsureValid(Program.Options options)
        {
            if (!Rid.All.Contains(options.Runtime))
            {
                throw new Exception(
                    $"Supplied runtime {options.Runtime} is not valid. Available values: {string.Join(",", Rid.All)}");
            }
        }
    }
}