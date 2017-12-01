using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace JavaScriptPrettier
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("TypeScript")]
    [ContentType("JavaScript")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal sealed class CommandRegistration : IVsTextViewCreationListener
    {
        [Import]
        private IVsEditorAdaptersFactoryService AdaptersFactory { get; set; }

        [Import]
        private ITextDocumentFactoryService DocumentService { get; set; }

        [Import]
        private ITextBufferUndoManagerProvider UndoProvider { get; set; }

        public async void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView view = AdaptersFactory.GetWpfTextView(textViewAdapter);

            if (!DocumentService.TryGetTextDocument(view.TextBuffer, out ITextDocument doc))
                return;

            ITextBufferUndoManager undoManager = UndoProvider.GetTextBufferUndoManager(view.TextBuffer);
            NodeProcess node = view.Properties.GetOrCreateSingletonProperty(() => new NodeProcess());

            AddCommandFilter(textViewAdapter, new PrettierCommand(view, undoManager, node, doc.Encoding, doc.FilePath));

            if (!node.IsReadyToExecute())
            {
                await Install(node);
            }
        }

        private static async System.Threading.Tasks.Task Install(NodeProcess node)
        {
            var statusbar = (IVsStatusbar)ServiceProvider.GlobalProvider.GetService(typeof(SVsStatusbar));

            statusbar.FreezeOutput(0);
            statusbar.SetText($"Installing {NodeProcess.Packages} npm module...");
            statusbar.FreezeOutput(1);

            bool success = await node.EnsurePackageInstalled();
            string status = success ? "Done" : "Failed";

            statusbar.FreezeOutput(0);
            statusbar.SetText($"Installing {NodeProcess.Packages} npm module... {status}");
            statusbar.FreezeOutput(1);
        }

        private void AddCommandFilter(IVsTextView textViewAdapter, BaseCommand command)
        {

            textViewAdapter.AddCommandFilter(command, out var next);
            command.Next = next;
        }
    }
}
