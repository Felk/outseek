using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using Python.Runtime;
using Dispatcher = Avalonia.Threading.Dispatcher;

namespace Outseek.AvaloniaClient
{
    public class IncludedPython
    {
        private IncludedPython()
        {
            // even though everything python related is done through static methods,
            // having an instances is an easy way to avoid accidentally trying to use python before it is initialized.
        }

        public static Task<IncludedPython> Create() =>
            Task.Run(() =>
            {
                string? pyLibPath = null;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    pyLibPath = LibraryUtils.LocateDllOnWindows("python3?.dll");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    pyLibPath = LibraryUtils.LocateSharedLibraryOnLinux(new Regex(@"libpython3\.\d+\.so"));
                }

                if (pyLibPath == null)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        var messagebox = MessageBoxManager.GetMessageBoxStandardWindow(
                            "Could not locate python",
                            "No python3 installation could be located.\n" +
                            "A bunch of features that need python will not work.\n" +
                            "Please make sure some recent version of Python 3 is installed and on the PATH",
                            ButtonEnum.Ok, Icon.Error);
                        messagebox.Show();
                    });
                    return;
                }

                Runtime.PythonDLL = pyLibPath;

                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }).ContinueWith(task =>
            {
                if (task.IsFaulted) Console.Error.WriteLine(task.Exception);
                return new IncludedPython();
            });

        public Task<dynamic> GetModule(string moduleName, string pipInstallName) =>
            Task.Run((Func<dynamic>) (() =>
            {
                using (Py.GIL())
                {
                    try
                    {
                        // TODO figure out how to properly manage this cross-platform
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            // if (!Installer.IsModuleInstalled(moduleName))
                                // Installer.PipInstallModule(pipInstallName);
                        }
                        return Py.Import(moduleName);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e);
                        throw;
                    }
                }
            }));
    }
}
