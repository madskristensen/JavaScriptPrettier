using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace JavaScriptPrettier
{
    internal sealed class PrettierCommand : BaseCommand
    {
        private Guid _commandGroup = PackageGuids.guidPrettierPackageCmdSet;
        private const uint _commandId = PackageIds.PrettierCommandId;

        private IWpfTextView _view;
        private ITextBufferUndoManager _undoManager;

        public PrettierCommand(IWpfTextView view, ITextBufferUndoManager undoManager)
        {
            _view = view;
            _undoManager = undoManager;
        }

        public override int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == _commandGroup && nCmdID == _commandId)
            {
                if (!NpmInstaller.IsInstalling)
                {
                    ThreadHelper.JoinableTaskFactory.RunAsync(Run);
                }

                return VSConstants.S_OK;
            }

            return Next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private async Task<bool> Run()
        {
            string input = _view.TextBuffer.CurrentSnapshot.GetText();
            string output = await NpmInstaller.Execute(input);

            if (string.IsNullOrEmpty(output))
                return false;

            using (ITextEdit edit = _view.TextBuffer.CreateEdit())
            using (ITextUndoTransaction undo = _undoManager.TextBufferUndoHistory.CreateTransaction("Prettify"))
            {
                edit.Replace(0, _view.TextBuffer.CurrentSnapshot.Length, output);
                edit.Apply();
                undo.Complete();
            }

            return true;
        }

        public override int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == _commandGroup && prgCmds[0].cmdID == _commandId)
            {
                if (NpmInstaller.IsInstalled())
                {
                    SetText(pCmdText, "Make Prettier");
                    prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_ENABLED | (uint)OLECMDF.OLECMDF_SUPPORTED;
                }
                else
                {
                    SetText(pCmdText, "Make Prettier (installing npm module...)");
                    prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
                }

                return VSConstants.S_OK;
            }

            return Next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public static void SetText(IntPtr pCmdTextInt, string text)
        {
            var pCmdText = (OLECMDTEXT)Marshal.PtrToStructure(pCmdTextInt, typeof(OLECMDTEXT));
            char[] menuText = text.ToCharArray();

            // Get the offset to the rgsz param.  This is where we will stuff our text
            IntPtr offset = Marshal.OffsetOf(typeof(OLECMDTEXT), "rgwz");
            IntPtr offsetToCwActual = Marshal.OffsetOf(typeof(OLECMDTEXT), "cwActual");

            // The max chars we copy is our string, or one less than the buffer size,
            // since we need a null at the end.
            int maxChars = Math.Min((int)pCmdText.cwBuf - 1, menuText.Length);

            Marshal.Copy(menuText, 0, (IntPtr)((long)pCmdTextInt + (long)offset), maxChars);

            // append a null character
            Marshal.WriteInt16((IntPtr)((long)pCmdTextInt + (long)offset + maxChars * 2), 0);

            // write out the length +1 for the null char
            Marshal.WriteInt32((IntPtr)((long)pCmdTextInt + (long)offsetToCwActual), maxChars + 1);
        }

    }
}