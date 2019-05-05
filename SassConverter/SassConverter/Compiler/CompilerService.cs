using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using Tasks = System.Threading.Tasks;
using EnvDTE;

namespace SassConverter
{
    internal class CompilerService
    {
        public static bool ShouldCompile(string sassFilePath)
        {
            //// File name starts with a underscore
            //if (Path.GetFileName(sassFilePath).StartsWith("_", StringComparison.Ordinal))
            //    return false;

            // File is not part of a project
            ProjectItem projectItem = VsHelpers.DTE.Solution.FindProjectItem(sassFilePath);

            if (projectItem == null || projectItem.ContainingProject == null)
                return false;

            //// A comment with "nocompile" is found
            //string less = File.ReadAllText(sassFilePath);

            //if (less.IndexOf("nocompile", StringComparison.OrdinalIgnoreCase) > -1)
            //    return false;

            return true;
        }

        public static async Tasks.Task Compile(string sassFilePath, NodeProcess node)
        {
            string css = await node.ExecuteProcess(sassFilePath);
            string cssFilePath = Path.ChangeExtension(sassFilePath, ".css");

            bool exist = File.Exists(cssFilePath);

            if (exist)
            {
                string oldCss = File.ReadAllText(cssFilePath);

                if (oldCss == css)
                    return;
            }

            VsHelpers.CheckFileOutOfSourceControl(cssFilePath);
            File.WriteAllText(cssFilePath, css);
            //VsHelpers.AddNestedFile(sassFilePath, cssFilePath);
        }

        //public static async Tasks.Task Install(NodeProcess node)
        //{
        //    var statusbar = (IVsStatusbar)ServiceProvider.GlobalProvider.GetService(typeof(SVsStatusbar));

        //    statusbar.FreezeOutput(0);
        //    statusbar.SetText($"Installing {NodeProcess.Packages} npm modules...");
        //    statusbar.FreezeOutput(1);

        //    bool success = await node.EnsurePackageInstalled();
        //    string status = success ? "Done" : "Failed";

        //    statusbar.FreezeOutput(0);
        //    statusbar.SetText($"Installing {NodeProcess.Packages} npm modules... {status}");
        //    statusbar.FreezeOutput(1);
        //}
    }
}
