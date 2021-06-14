using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Outseek.AvaloniaClient
{
    public static class LibraryUtils
    {
        public static string? LocateDllOnWindows(string dllPattern)
        {
            Process p = new()
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "CMD.exe",
                    Arguments = "/C where " + dllPattern
                }
            };
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return p.ExitCode == 0
                ? output.Split('\n').Select(s => s.Trim()).FirstOrDefault()
                : null;
        }

        public static string? LocateSharedLibraryOnLinux(Regex soRegex)
        {
            Process p = new()
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "ldconfig",
                    Arguments = "--print-cache"
                }
            };
            p.Start();
            string ldCache = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            Match match = soRegex.Match(ldCache);
            return match.Success ? match.Value : null;
        }
    }
}
