using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Outseek.AvaloniaClient
{
    public class NonzeroExitCodeException : IOException
    {
        public NonzeroExitCodeException(string? message) : base(message)
        {
        }
    }

    public static class SubprocessUtils
    {
        public static string? LocateDllOnWindows(string dllPattern)
        {
            try
            {
                string output = Run("CMD.exe", "/C where " + dllPattern);
                return output.Split('\n').Select(s => s.Trim()).FirstOrDefault();
            }
            catch (NonzeroExitCodeException)
            {
                return null;
            }
        }

        public static string? LocateSharedLibraryOnLinux(Regex soRegex)
        {
            string ldCache = Run("ldconfig", "--print-cache");
            Match match = soRegex.Match(ldCache);
            return match.Success ? match.Value : null;
        }

        public static string Run(string executable, string arguments)
        {
            Process p = new()
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = executable,
                    Arguments = arguments,
                }
            };
            string stdout = "";
            string stderr = "";
            p.OutputDataReceived += (_, e) => stdout += e.Data + '\n';
            p.ErrorDataReceived += (_, e) => stderr += e.Data + '\n';
            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                throw new NonzeroExitCodeException(stderr);
            }
            return stdout;
        }
    }
}
