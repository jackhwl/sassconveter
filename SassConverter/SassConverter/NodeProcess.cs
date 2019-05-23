﻿using System;
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

        private static string _installDir = @"C:\Ruby193\bin"; // Path.Combine(Path.GetTempPath(), Vsix.Name.Replace(" ", ""), Packages.GetHashCode().ToString());
        private static string _executable = Path.Combine(_installDir, "sass.bat");

        //public bool IsInstalling
        //{
        //    get;
        //    private set;
        //}

        public bool IsReadyToExecute()
        {
            return File.Exists(_executable);
        }

        public bool EnsurePackageInstalled()
        {
            return IsReadyToExecute();
            //if (IsInstalling)
            //    return false;

            //if (IsReadyToExecute())
            //    return true;

            //bool success = await Task.Run(() =>
            //{
            //    IsInstalling = true;

            //    try
            //    {
            //        if (!Directory.Exists(_installDir))
            //            Directory.CreateDirectory(_installDir);

            //        var start = new ProcessStartInfo("cmd", $"/c npm install {Packages} --no-optional")
            //        {
            //            WorkingDirectory = _installDir,
            //            UseShellExecute = false,
            //            RedirectStandardOutput = true,
            //            CreateNoWindow = true,
            //        };

            //        ModifyPathVariable(start);

            //        using (var proc = Process.Start(start))
            //        {
            //            proc.WaitForExit();
            //            return proc.ExitCode == 0;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.Log(ex);
            //        return false;
            //    }
            //    finally
            //    {
            //        IsInstalling = false;
            //    }
            //});

            //return success;
        }

        public async Task<string> ExecuteProcess(string filePath)
        {
            if (!EnsurePackageInstalled())
                return null;

            string fileName = Path.GetFileName(filePath);
            var option = " -l --style compact ";
            var start = new ProcessStartInfo("cmd", $"/c \"\"{_executable}\" {option} \"{filePath}\"\"")
            {
                WorkingDirectory = Path.GetDirectoryName(filePath),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            ModifyPathVariable(start);

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

                    Logger.Log("total 1:" + lines.ToString());
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
