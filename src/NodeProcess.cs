using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JavaScriptPrettier
{
    internal class NodeProcess
    {
        private PrettierPackage _package;
        private string _installDir;
        private string _executable;

        private string _packages
        {
            get
            {
                return $"prettier@{_package.optionPage.EmbeddedVersion}";
            }
        }

        public bool IsInstalling
        {
            get;
            private set;
        }

        public bool IsReadyToExecute()
        {
            return File.Exists(_executable);
        }

        public NodeProcess(PrettierPackage package)
        {
            _package = package;
        }

        public async Task<bool> EnsurePackageInstalledAsync()
        {
            // These values are refreshed on each run to ensure they match the Prettier version
            // value currently in settings (OptionPageGrid.EmbeddedVersion).
            _installDir = Path.Combine(Path.GetTempPath(), Vsix.Name, _packages.GetHashCode().ToString());
            _executable = Path.Combine(_installDir, "node_modules\\.bin\\prettier.cmd");

            if (IsInstalling)
                return false;

            if (IsReadyToExecute())
                return true;

            IsInstalling = true;

            try
            {
                return await InstallEmbeddedPrettierAsync();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            finally
            {
                IsInstalling = false;
            }
        }

        public async Task<string> ExecuteProcessAsync(string input, Encoding encoding,
            string filePath)
        {
            if (!await EnsurePackageInstalledAsync())
                return null;

            string executable = FindPrettierExecutable(filePath);
            if (executable == null)
            {
                Logger.Log("No local prettier found. Falling back to plugin version");

                executable = _executable;
            }

            string command = $"/c \"\"{executable}\" --stdin-filepath \"{filePath}\" --stdin\"";

            var start = new ProcessStartInfo("cmd", command)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = encoding,
            };

            ModifyPathVariable(start);

            try
            {
                using (var proc = Process.Start(start))
                {
                    using (var stream = new StreamWriter(proc.StandardInput.BaseStream, encoding))
                    {
                        await stream.WriteAsync(input);
                    }

                    string output = await proc.StandardOutput.ReadToEndAsync();
                    string error = await proc.StandardError.ReadToEndAsync();

                    if (!string.IsNullOrEmpty(error))
                        Logger.Log(error);

                    proc.WaitForExit();
                    return output;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }
        }

        private async Task<bool> InstallEmbeddedPrettierAsync()
        {
            if (!Directory.Exists(_installDir))
                Directory.CreateDirectory(_installDir);

            Logger.Log($"npm init -y & npm install {_packages} (this can take a few minutes)");

            var start = new ProcessStartInfo("cmd", $"/c npm init -y & npm install {_packages}")
            {
                WorkingDirectory = _installDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            ModifyPathVariable(start);

            using (var proc = Process.Start(start))
            {
                string output = await proc.StandardOutput.ReadToEndAsync();
                string error = await proc.StandardError.ReadToEndAsync();

                if (!string.IsNullOrEmpty(output))
                    Logger.Log(output);

                if (!string.IsNullOrEmpty(error))
                    Logger.Log(error);

                if (
                    error.Contains("code ETARGET")
                    && !_package.optionPage.EmbeddedVersion.Equals(_package.optionPage._prettierFallbackVersion)
                )
                {
                    _package.optionPage.EmbeddedVersion = _package.optionPage._prettierFallbackVersion;
                    return await InstallEmbeddedPrettierAsync();
                }

                proc.WaitForExit();
                return proc.ExitCode == 0;
            }

        }

        private string FindPrettierExecutable(string filePath)
        {
            string currentDir = filePath;

            while ((currentDir = Path.GetDirectoryName(currentDir)) != null)
            {
                if (File.Exists(Path.Combine(currentDir, "package.json")))
                {
                    string executable = Path.Combine(currentDir, @"node_modules\.bin\prettier.cmd");
                    if (File.Exists(executable))
                    {
                        Logger.Log($"Using prettier from {executable}");
                        return executable;
                    }
                }
                // If not, move a level up and try again.
            }

            return null;
        }

        private static void ModifyPathVariable(ProcessStartInfo start)
        {
            string path = start.EnvironmentVariables["PATH"];

            var process = Process.GetCurrentProcess();
            string ideDir = Path.GetDirectoryName(process.MainModule.FileName);

            if (Directory.Exists(ideDir))
            {
                string parent = Directory.GetParent(ideDir).Parent.FullName;

                string rc2Preview1Path = new DirectoryInfo(Path.Combine(parent, @"Web\External")).FullName;

                if (Directory.Exists(rc2Preview1Path))
                {
                    path += ";" + rc2Preview1Path;
                }
                else
                {
                    path += ";" + Path.Combine(ideDir, @"Extensions\Microsoft\Web Tools\External");
                    path += ";" + Path.Combine(ideDir, @"Extensions\Microsoft\Web Tools\External\git");
                }
            }

            start.EnvironmentVariables["PATH"] = path;
        }
    }
}
