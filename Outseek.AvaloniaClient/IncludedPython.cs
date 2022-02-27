using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using Python.Runtime;
using Dispatcher = Avalonia.Threading.Dispatcher;

namespace Outseek.AvaloniaClient;

public class IncludedPython
{
    private readonly string _venvPath;
    private readonly string _pipExecutablePath;

    private IncludedPython(string venvPath, string pipExecutablePath)
    {
        _venvPath = venvPath;
        _pipExecutablePath = pipExecutablePath;
    }

    public static Task<IncludedPython?> Create() =>
        Task.Run(() =>
        {
            string? pyLibPath = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                pyLibPath = SubprocessUtils.LocateDllOnWindows("python3?.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                pyLibPath = SubprocessUtils.LocateSharedLibraryOnLinux(new Regex(@"libpython3\.\d+\.so"));
            }

            if (pyLibPath == null)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    var messagebox = MessageBoxManager.GetMessageBoxStandardWindow(
                        "Could not locate python",
                        "No python3 installation could be located.\n" +
                        "A bunch of features that need python will not work.\n" +
                        "Please make sure some recent version of Python 3 is installed and on the PATH",
                        ButtonEnum.Ok, Icon.Error);
                    await messagebox.Show();
                });
                return null;
            }

            Runtime.PythonDLL = pyLibPath;
            string basePythonExecutablePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Join(Path.GetDirectoryName(pyLibPath), "python.exe")
                : Path.Join(Path.GetDirectoryName(pyLibPath), "python");

            Match pyVerMatch = new Regex(@"python3\.?(\d+)").Match(pyLibPath);
            if (!pyVerMatch.Success)
                throw new InvalidOperationException(
                    $"failed to extract minor python version from lib path {pyLibPath}");
            int pyMinorVersion = int.Parse(pyVerMatch.Groups[1].Value);

            string venvPath = Path.Join(API.Outseek.StorageDirectory, $"python-venv3{pyMinorVersion}");
            string pipExecutablePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Join(venvPath, "Scripts", "pip.exe")
                : Path.Join(venvPath, "bin", "pip");
            if (!Directory.Exists(venvPath))
            {
                try
                {
                    SubprocessUtils.Run(basePythonExecutablePath, "-m venv " + venvPath);
                }
                catch (NonzeroExitCodeException ex)
                {
                    Dispatcher.UIThread.Post(async () =>
                    {
                        var messagebox = MessageBoxManager.GetMessageBoxStandardWindow(
                            "Could not initialize python virtual environment (venv)",
                            "The application attempted to create a virtual environment (venv) to install modules into.\n" +
                            "However, that failed with the following output:\n" +
                            ex.Message,
                            ButtonEnum.Ok, Icon.Error);
                        await messagebox.Show();
                    });
                    return null;
                }
            }

            // Inspired by wiki: https://github.com/pythonnet/pythonnet/wiki/Using-Python.NET-with-Virtual-Environments
            // but adjusted for better robustness and crash-less-ness, see also https://github.com/pythonnet/pythonnet/issues/1478
            string origPath = Environment.GetEnvironmentVariable("PATH") ?? "";
            IEnumerable<string> pathWithVenv = origPath
                .Split(Path.PathSeparator)
                .Prepend(venvPath)
                // This is where the venv executables are stored, e.g. python or pip
                .Prepend(Path.Join(venvPath,
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Scripts" : "bin"))
                .Where(s => !string.IsNullOrEmpty(s));
            Environment.SetEnvironmentVariable("PATH",
                string.Join(Path.PathSeparator, pathWithVenv), EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONHOME", venvPath, EnvironmentVariableTarget.Process);
            string pythonPath = $"{venvPath}/Lib/site-packages{Path.PathSeparator}" +
                                $"{venvPath}/Lib{Path.PathSeparator}";
            Environment.SetEnvironmentVariable("PYTHONPATH", pythonPath, EnvironmentVariableTarget.Process);

            PythonEngine.PythonPath = PythonEngine.PythonPath + Path.PathSeparator + pythonPath;
            PythonEngine.PythonHome = venvPath;

            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();

            return new IncludedPython(venvPath, pipExecutablePath);
        });

    private string Pip(string command)
    {
        // calling the venv's pip executable directly with PYTHONHOME set causes issues like "ModuleNotFoundError: No module named 'encodings'",
        // as described here: https://discuss.python.org/t/fatal-python-error-init-fs-encoding-failed-to-get-the-python-codec-of-the-filesystem-encoding/3173/2
        // Therefore temporarily unset it. TODO this feels hacky, figure out how to properly use the venv
        string? pythonhome = Environment.GetEnvironmentVariable("PYTHONHOME");
        Environment.SetEnvironmentVariable("PYTHONHOME", null, EnvironmentVariableTarget.Process);
        string stdout = SubprocessUtils.Run(_pipExecutablePath, command);
        Environment.SetEnvironmentVariable("PYTHONHOME", pythonhome, EnvironmentVariableTarget.Process);
        return stdout;
    }

    public Task<dynamic?> GetModule(
        string importName, string pypiName, string? pipInstallName = null, bool updateToLatest = false) =>
        Task.Run((Func<dynamic?>) (() =>
        {
            using (Py.GIL())
            {
                string pipListOutput = Pip("list");
                // example output:
                // Package          Version
                // ---------------- ---------
                // certifi          2021.5.30
                // chardet          4.0.0
                // chat-downloader  0.0.8
                Dictionary<string, Version> installedModules =
                    new Regex(@"^(?<module>[^ ]+)\s+(?<version>[\d.]+)\s*$", RegexOptions.Multiline)
                        .Matches(pipListOutput)
                        .ToDictionary(
                            match => match.Groups["module"].Value,
                            match => Version.Parse(match.Groups["version"].Value));
                string installName = pipInstallName ?? pypiName;

                if (!installedModules.ContainsKey(pypiName))
                {
                    try
                    {
                        Console.WriteLine(Pip($"install {installName} --no-input --disable-pip-version-check"));
                        return Py.Import(importName);
                    }
                    catch (NonzeroExitCodeException ex)
                    {
                        Dispatcher.UIThread.Post(async () =>
                        {
                            var messagebox = MessageBoxManager.GetMessageBoxStandardWindow(
                                $"Could not install {importName} python module",
                                $"The module {pypiName} is not currently installed and was attempted to be installed.\n" +
                                "But the installation failed with the following error:\n" + ex.Message,
                                ButtonEnum.Ok, Icon.Error);
                            await messagebox.Show();
                        });
                        return null;
                    }
                }

                if (updateToLatest)
                {
                    try
                    {
                        Console.WriteLine(
                            Pip($"install -U {installName} --no-input --disable-pip-version-check"));
                    }
                    catch (NonzeroExitCodeException ex)
                    {
                        // Failing to update is not a fatal error, since we can keep using the existing one.
                        Console.Error.WriteLine(ex);
                    }
                }

                return Py.Import(importName);
            }
        }));
}
