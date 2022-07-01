using Microsoft.Win32;
using System;

namespace LogFilesServiceCompressor
{
    public static class Utilities
    {
        public static void ReadNetFrameworkVersion()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    LogHelper.Info($".NET Framework Version: {CheckFor45PlusVersion((int)ndpKey.GetValue("Release"))} - Release: {ndpKey.GetValue("Release")} ");
                    Console.WriteLine($".NET Framework Version: {CheckFor45PlusVersion((int)ndpKey.GetValue("Release"))} - Release: {ndpKey.GetValue("Release")} ");
                }
                else
                {
                    LogHelper.Info(".NET Framework Version 4.5 or later is not detected.");
                    Console.WriteLine(".NET Framework Version 4.5 or later is not detected.");
                }
            }

            // Checking the version using >= enables forward compatibility.
            string CheckFor45PlusVersion(int releaseKey)
            {
                if (releaseKey >= 528040)
                    return "4.8 or later";
                if (releaseKey >= 461808)
                    return "4.7.2";
                if (releaseKey >= 461308)
                    return "4.7.1";
                if (releaseKey >= 460798)
                    return "4.7";
                if (releaseKey >= 394802)
                    return "4.6.2";
                if (releaseKey >= 394254)
                    return "4.6.1";
                if (releaseKey >= 393295)
                    return "4.6";
                if (releaseKey >= 379893)
                    return "4.5.2";
                if (releaseKey >= 378675)
                    return "4.5.1";
                if (releaseKey >= 378389)
                    return "4.5";
                // This code should never execute. A non-null release key should mean
                // that 4.5 or later is installed.
                return "No 4.5 or later version detected";
            }
        }


        // this should populate ApplicationSettings object
        public static void ReadApplicationConfiguration()
        {

        }  
    }
    // Create a type to hold all the settings
    //public class ApplicationSettings
    //{ 

    //}
}
