using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Python.Runtime;

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
                // TODO figure out how to properly manage this cross-platform
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Runtime.PythonDLL = "C:/Program Files/Python38/python38.dll";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Runtime.PythonDLL = "libpython3.8.so";
                }

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
