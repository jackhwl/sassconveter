using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace SassConverter
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("SCSS")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal sealed class Command : IVsTextViewCreationListener
    {
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
        }
        private async void DocumentSaved(object sender, TextDocumentFileActionEventArgs e)
        {
            if (e.FileActionType != FileActionTypes.ContentSavedToDisk)
                return;

            if (_node != null && CompilerService.ShouldCompile(e.FilePath) && _node.IsReadyToExecute())
            {
                //
                await CompilerService.Compile(e.FilePath, "css", _node);
            }
        }
    }
}
