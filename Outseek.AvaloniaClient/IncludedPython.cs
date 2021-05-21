using System;
using System.Threading.Tasks;
using Python.Included;
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
            Task.Run(async () =>
            {
                await Installer.SetupPython();
                Installer.TryInstallPip();
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
                return new IncludedPython();
            });

        public Task<dynamic> GetModule(string moduleName, string pipInstallName) =>
            Task.Run((Func<dynamic>) (() =>
            {
                using (Py.GIL())
                {
                    if (!Installer.IsModuleInstalled(moduleName))
                        Installer.PipInstallModule(pipInstallName);
                    return Py.Import(moduleName);
                }
            }));
    }
}
