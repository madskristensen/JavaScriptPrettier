using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JavaScriptPrettier
{
    internal class NpmInstaller
    {
        private static string _installDir = Path.Combine(Path.GetTempPath(), Vsix.Name, Constants.NpmPackageVersion);

        public static bool IsInstalling
        {
            get;
            private set;
        }

        public static bool IsInstalled()
        {
            return File.Exists(Path.Combine(_installDir, "node_modules\\.bin\\prettier.cmd"));
        }

        public static async Task<bool> EnsurePackageInstalled()
        {
            if (IsInstalling)
                return false;

            if (IsInstalled())
                return true;

            bool success = await Task.Run(() =>
             {
                 try
                 {
                     Directory.CreateDirectory(_installDir);

                     var start = new ProcessStartInfo("cmd", $"/c npm install {Constants.NpmPackageName}@{Constants.NpmPackageVersion}")
                     {
                         WorkingDirectory = _installDir,
                         UseShellExecute = false,
                         RedirectStandardOutput = true,
                         CreateNoWindow = true,
                     };

                     ModifyPathVariable(start);

                     using (var proc = Process.Start(start))
                     {
                         proc.WaitForExit();
                     }
                 }
                 catch (Exception ex)
                 {
                     Logger.Log(ex);
                     return false;
                 }

                 return true;
             });

            return success;
        }

        internal static async Task<string> Execute(string input)
        {
            if (!await EnsurePackageInstalled())
                return null;

            var start = new ProcessStartInfo("cmd", $"/c prettier --stdin")
            {
                WorkingDirectory = _installDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
            };

            ModifyPathVariable(start);

            try
            {
                var sb = new StringBuilder();

                using (var proc = Process.Start(start))
                {
                    using (StreamWriter stream = proc.StandardInput)
                    {
                        await stream.WriteAsync(input);
                    }

                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string line = await proc.StandardOutput.ReadLineAsync();
                        sb.AppendLine(line);
                    }

                    proc.WaitForExit();
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }
        }

        public static void ModifyPathVariable(ProcessStartInfo start)
        {
            string path = ".\\node_modules\\.bin" + ";" + start.EnvironmentVariables["PATH"];

            var process = Process.GetCurrentProcess();
            string ideDir = Path.GetDirectoryName(process.MainModule.FileName);

            if (Directory.Exists(ideDir))
            {
                string parent = Directory.GetParent(ideDir).Parent.FullName;

                string rc2Preview1Path = new DirectoryInfo(Path.Combine(parent, @"Web\External")).FullName;

                if (Directory.Exists(rc2Preview1Path))
                {
                    path += ";" + rc2Preview1Path;
                    path += ";" + rc2Preview1Path + "\\git";
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
