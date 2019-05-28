using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using Tasks = System.Threading.Tasks;
using EnvDTE;
using System.Collections.Generic;

namespace SassConverter
{
    internal class CompilerService
    {
        public static bool ShouldCompile(string sassFilePath)
        {
            // File is not part of a project
            ProjectItem projectItem = VsHelpers.DTE.Solution.FindProjectItem(sassFilePath);

            if (projectItem == null || projectItem.ContainingProject == null)
                return false;

            return true;
        }

        public static List<string> ShouldCompileFiles(string sassFilePath)
        {
            sassFilePath = sassFilePath.ToLower();
            var legacyPath = "sass\\legacy";
            var directoryName = sassFilePath.Substring(0, sassFilePath.LastIndexOf("\\sass\\"))+"\\sass\\";
            if (sassFilePath.Contains(legacyPath) || sassFilePath.Contains("sass\\videsktop-black.scss") || sassFilePath.Contains("sass\\videsktop-classic.scss") || sassFilePath.Contains("sass\\videsktop-light.scss")) {
                return new List<string> {
                    Path.Combine(directoryName, "videsktop-black.scss"),
                    Path.Combine(directoryName, "videsktop-classic.scss"),
                    Path.Combine(directoryName, "videsktop-light.scss")
                };
            }
            if (sassFilePath.Contains("sass\\bootstrap") || sassFilePath.Contains("sass\\custom-cosmos") || sassFilePath.Contains("sass\\custom-vi") || sassFilePath.Contains("sass\\videsktop.scss")) {
             return new List<string>{ Path.Combine(directoryName,"videsktop.scss")};
            }
            return new List<string>();
        }

        public static async Tasks.Task Compile(string sassFilePath, string cssDirectoryName, NodeProcess node)
        {
            var compileFiles = ShouldCompileFiles(sassFilePath);
            var projectDirectoryPath = sassFilePath.Substring(0, sassFilePath.LastIndexOf("\\sass\\"));
            try
            {
                foreach (var sassFile in compileFiles)
                {
                    if (sassFile.IndexOf("videsktop-")>0)  // legacyCode
                    {
                        string cssFilePath = Path.ChangeExtension(sassFile, ".css").Replace("\\sass\\", "\\"+cssDirectoryName+"\\");
                        VsHelpers.CheckFileOutOfSourceControl(cssFilePath);
                        await node.ExecuteProcessLegacyAsync(projectDirectoryPath, sassFile);
                    }
                    else
                    {
                        string cssFilePath = Path.ChangeExtension(sassFile, ".css").Replace("\\sass\\", "\\"+cssDirectoryName+"\\");
                        string cssMapFilePath = cssFilePath + ".map";
                        VsHelpers.CheckFileOutOfSourceControl(cssFilePath);
                        VsHelpers.CheckFileOutOfSourceControl(cssMapFilePath);
                        await node.ExecuteProcessAsync(projectDirectoryPath, sassFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.EventLog("method Compile: " + ex.Message);
            }
        }

        // sass -l --style compact  ..\sass/videsktop.scss ..\css/jack.css
        // c:\dart-sass\sass.bat sass\videsktop.scss css\videsktop.css --style=compressed
    }
}
