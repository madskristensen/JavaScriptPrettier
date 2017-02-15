using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

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

            string ext = Path.GetExtension(doc.FilePath);

            if (!Constants.FileExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                return;

            ITextBufferUndoManager undoManager = UndoProvider.GetTextBufferUndoManager(view.TextBuffer);

            AddCommandFilter(textViewAdapter, new PrettierCommand(view, undoManager));

            if (!NodeProcess.IsReadyToExecute())
            {
                await Install();
            }
        }

        private static async System.Threading.Tasks.Task Install()
        {
            var statusbar = (IVsStatusbar)ServiceProvider.GlobalProvider.GetService(typeof(SVsStatusbar));
            object icon = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Synch;

            statusbar.FreezeOutput(0);
            statusbar.SetText("Installing prettier npm module...");
            statusbar.Animation(1, icon);
            statusbar.FreezeOutput(1);

            bool success = await NodeProcess.EnsurePackageInstalled();
            string status = success ? "Done" : "Failed";

            statusbar.FreezeOutput(0);
            statusbar.SetText($"Installing prettier npm module... {status}");
            statusbar.Animation(0, icon);
            statusbar.FreezeOutput(1);
        }

        private void AddCommandFilter(IVsTextView textViewAdapter, BaseCommand command)
        {

            textViewAdapter.AddCommandFilter(command, out var next);
            command.Next = next;
        }
    }
}
