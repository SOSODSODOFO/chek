using System;
using Microsoft.Win32;

namespace ChekApp
{
    public static class RegistryHelper
    {
        public static bool KeyExists(string path)
        {
            try
            {
                var (rootName, subKey) = SplitRoot(path);
                var root = GetRoot(rootName);
                if (root == null || subKey == null)
                {
                    return false;
                }

                using var key = root.OpenSubKey(subKey);
                return key != null;
            }
            catch
            {
                return false;
            }
        }

        private static (string? Root, string? SubKey) SplitRoot(string path)
        {
            var index = path.IndexOf('\\');
            if (index < 0)
            {
                return (null, null);
            }

            var root = path[..index];
            var subKey = path[(index + 1)..];
            return (root, subKey);
        }

        private static RegistryKey? GetRoot(string? rootName)
        {
            return rootName switch
            {
                "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                "HKEY_CURRENT_USER" => Registry.CurrentUser,
                "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                "HKEY_USERS" => Registry.Users,
                "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
                "Software" => Registry.CurrentUser.OpenSubKey("Software"),
                "SYSTEM" => Registry.LocalMachine.OpenSubKey("SYSTEM"),
                _ => null
            };
        }
    }
}
