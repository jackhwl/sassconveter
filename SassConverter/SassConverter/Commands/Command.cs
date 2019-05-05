using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Utilities;

namespace SassConverter
{
    /// <summary>
    /// Command handler
    /// </summary>
    ///     
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("SCSS")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal sealed class Command : IVsTextViewCreationListener
    {
        ///// <summary>
        ///// Command ID.
        ///// </summary>
        //public const int CommandId = 0x0100;

        ///// <summary>
        ///// Command menu group (command set GUID).
        ///// </summary>
        //public static readonly Guid CommandSet = new Guid("cfdb5ccc-bed2-4310-8fb6-7550d6c550c6");

        ///// <summary>
        ///// VS Package that provides this command, not null.
        ///// </summary>
        //private readonly AsyncPackage package;

        ///// <summary>
        ///// Initializes a new instance of the <see cref="Command"/> class.
        ///// Adds our command handlers for menu (commands must exist in the command table file)
        ///// </summary>
        ///// <param name="package">Owner package, not null.</param>
        ///// <param name="commandService">Command service to add command to, not null.</param>
        //private Command(AsyncPackage package, OleMenuCommandService commandService)
        //{
        //    this.package = package ?? throw new ArgumentNullException(nameof(package));
        //    commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

        //    var menuCommandID = new CommandID(CommandSet, CommandId);
        //    var menuItem = new MenuCommand(this.Execute, menuCommandID);
        //    commandService.AddCommand(menuItem);
        //}

        ///// <summary>
        ///// Gets the instance of the command.
        ///// </summary>
        //public static Command Instance
        //{
        //    get;
        //    private set;
        //}

        ///// <summary>
        ///// Gets the service provider from the owner package.
        ///// </summary>
        //private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        //{
        //    get
        //    {
        //        return this.package;
        //    }
        //}

        ///// <summary>
        ///// Initializes the singleton instance of the command.
        ///// </summary>
        ///// <param name="package">Owner package, not null.</param>
        //public static async Task InitializeAsync(AsyncPackage package)
        //{
        //    // Switch to the main thread - the call to AddCommand in Command's constructor requires
        //    // the UI thread.
        //    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

        //    OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
        //    Instance = new Command(package, commandService);
        //}
        private NodeProcess _node;

        [Import]
        private IVsEditorAdaptersFactoryService AdaptersFactory { get; set; }

        [Import]
        private ITextDocumentFactoryService DocumentService { get; set; }
        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView view = AdaptersFactory.GetWpfTextView(textViewAdapter);

            if (!DocumentService.TryGetTextDocument(view.TextBuffer, out ITextDocument doc))
                return;

            doc.FileActionOccurred += DocumentSaved;

            _node = view.Properties.GetOrCreateSingletonProperty(() => new NodeProcess());

            //if (!_node.IsReadyToExecute())
            //{
            //    await CompilerService.Install(_node);
            //}
        }
        private async void DocumentSaved(object sender, TextDocumentFileActionEventArgs e)
        {
            if (e.FileActionType != FileActionTypes.ContentSavedToDisk)
                return;

            if (_node != null && CompilerService.ShouldCompile(e.FilePath) && _node.IsReadyToExecute())
            {
                await CompilerService.Compile(e.FilePath, _node);
            }
        }
        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        //private void Execute(object sender, EventArgs e)
        //{
        //    ThreadHelper.ThrowIfNotOnUIThread();
        //    string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
        //    string title = "Command";
        //    var args = @"C:\inetpub\wwwroot\viDesktopDev\sass\videsktop.scss C:\inetpub\wwwroot\viDesktopDev\css\jack.css";

        //    Process p = new Process();
        //    p.StartInfo = new ProcessStartInfo("sass", args);
        //    p.StartInfo.WorkingDirectory = @"C:\Program Files\Chrome";
        //    p.StartInfo.CreateNoWindow = true;
        //    //p.StartInfo.UseShellExecute = false;
        //    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        //    //p.StartInfo.UseShellExecute = false;
        //    p.Start();

        //    //Process.Start("sass", args);

        //}
    }
}
