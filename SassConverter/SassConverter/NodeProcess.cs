using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassConverter
{
    internal class NodeProcess
    {
        public const string Packages = "less less-plugin-autoprefix less-plugin-csscomb";
        private static string _executable = Path.Combine(new OptionPageGrid().optionRubyFolder, "sass.bat");
        private static string _executableDartSass = Path.Combine(new OptionPageGrid().optionSassFolder, "sass.bat");

        public bool IsReadyToExecute()
        {
            if (File.Exists(_executable)) return true;
            Logger.Log("Rube file not found: " + _executable);
            return false;
        }
        public bool IsReadyToExecuteDartSass()
        {
            if (File.Exists(_executableDartSass)) return true;
            Logger.Log("Dart Sass file not found: " + _executableDartSass);
            return false;
        }

        public bool EnsurePackageInstalled()
        {
            return IsReadyToExecute();
        }

        public async Task<string> ExecuteProcessAsync(string projectDirectoryPath, string filePath)
        {
            if (!IsReadyToExecuteDartSass())
                return null;
            string fileName = "sass\\" + Path.GetFileName(filePath);
            string cssFileName = "css\\" + Path.GetFileNameWithoutExtension(filePath) + ".css";
            var start = new ProcessStartInfo("cmd", $"/c {_executableDartSass} {fileName} {cssFileName} --style=compressed")
            {
                WorkingDirectory = projectDirectoryPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            try
            {
                var sb = new StringBuilder();

                using (var proc = Process.Start(start))
                {
                    var lines = 0;
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string line = await proc.StandardOutput.ReadLineAsync();
                        sb.AppendLine(line);
                        lines++;
                    }
                    if (proc.ExitCode != 0)
                    {
                        Logger.Log("exited with status code " + proc.ExitCode);
                        Logger.Log(sb.ToString());
                        Logger.Log(lines.ToString());
                    }
                    else
                    {
                        Logger.Log(cssFileName + " generated.");
                    }
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }
        }

        public async Task<string> ExecuteProcessLegacyAsync(string projectDirectoryPath, string filePath)
        {
            if (!EnsurePackageInstalled())
                return null;

            string fileName = "sass\\" + Path.GetFileName(filePath);
            var start = new ProcessStartInfo("cmd", $"/c compass compile {fileName}")
            {
                WorkingDirectory = projectDirectoryPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            try
            {
                var sb = new StringBuilder();

                using (var proc = Process.Start(start))
                {
                    var lines = 0;
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string line = await proc.StandardOutput.ReadLineAsync();
                        sb.AppendLine(line);
                        lines++;
                    }
                    if (proc.ExitCode != 0)
                    {
                        Logger.Log("exited with status code " + proc.ExitCode);
                        Logger.Log(sb.ToString());
                        Logger.Log(lines.ToString());
                    }
                    else
                    {
                        Logger.Log(fileName + " generated.");
                    }

                    Logger.Log(sb.ToString());
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }
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
