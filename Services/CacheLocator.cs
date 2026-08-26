using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PanopticonAuditHistorySearch.Services
{
    public static class CacheLocator
    {
        private static readonly Regex Unsafe = new Regex("[^A-Za-z0-9._-]", RegexOptions.Compiled);

        public static string RootDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "MscrmTools", "XrmToolBox", "Panopticon");
            }
        }

        public static string DatabasePath(string organizationKey)
        {
            var safe = Unsafe.Replace(organizationKey ?? "unknown", "_");
            if (safe.Length > 80) safe = safe.Substring(0, 80);
            return Path.Combine(RootDirectory, safe + ".db");
        }

        public static void EnsureRoot()
        {
            Directory.CreateDirectory(RootDirectory);
        }

        public static long SizeOnDisk(string databasePath)
        {
            if (string.IsNullOrEmpty(databasePath)) return 0;
            var dir = Path.GetDirectoryName(databasePath);
            var name = Path.GetFileName(databasePath);
            if (!Directory.Exists(dir)) return 0;
            return Directory.GetFiles(dir, name + "*")
                .Select(f => new FileInfo(f).Length)
                .Sum();
        }

        public static void Delete(string databasePath)
        {
            var dir = Path.GetDirectoryName(databasePath);
            var name = Path.GetFileName(databasePath);
            if (!Directory.Exists(dir)) return;
            foreach (var file in Directory.GetFiles(dir, name + "*"))
                File.Delete(file);
        }
    }
}
