using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace JavaScriptPrettier
{
    internal sealed class PrettierCommand : BaseCommand
    {
        private Guid _commandGroup = PackageGuids.guidPrettierPackageCmdSet;
        private const uint _commandId = PackageIds.PrettierCommandId;

        private IWpfTextView _view;
        private ITextBufferUndoManager _undoManager;
        private NodeProcess _node;
        private readonly Encoding _encoding;
        private readonly string _filePath;

        public PrettierCommand(IWpfTextView view, ITextBufferUndoManager undoManager, NodeProcess node, Encoding encoding, string filePath)
        {
            _view = view;
            _undoManager = undoManager;
            _node = node;
            _encoding = encoding;
            _filePath = filePath;
        }

        public override int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (pguidCmdGroup == _commandGroup && nCmdID == _commandId)
            {
                if (_node != null && _node.IsReadyToExecute())
                {
                    ThreadHelper.JoinableTaskFactory.RunAsync(MakePrettierAsync);
                }

                return VSConstants.S_OK;
            }

            return Next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public async Task<bool> MakePrettierAsync()
        {
            string input = _view.TextBuffer.CurrentSnapshot.GetText();
            string output = await _node.ExecuteProcessAsync(input, _encoding, _filePath);

            VirtualSnapshotPoint snapshotPoint = _view.Selection.ActivePoint;

            if (string.IsNullOrEmpty(output) || input == output)
                return false;

            using (ITextEdit edit = _view.TextBuffer.CreateEdit())
            using (ITextUndoTransaction undo = _undoManager.TextBufferUndoHistory.CreateTransaction("Make Prettier"))
            {
                edit.Replace(0, _view.TextBuffer.CurrentSnapshot.Length, output);
                edit.Apply();

                undo.Complete();
            }

            var newSnapshotPoint = new SnapshotPoint(_view.TextBuffer.CurrentSnapshot, snapshotPoint.Position.Position);
            _view.Caret.MoveTo(newSnapshotPoint);
            _view.ViewScroller.EnsureSpanVisible(new SnapshotSpan(newSnapshotPoint, 0));

            return true;
        }

        public override int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (pguidCmdGroup == _commandGroup && prgCmds[0].cmdID == _commandId)
            {
                if (_node != null)
                {
                    if (_node.IsReadyToExecute())
                    {
                        SetText(pCmdText, "Make Prettier");
                        prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_ENABLED | (uint)OLECMDF.OLECMDF_SUPPORTED;
                    }
                    else
                    {
                        SetText(pCmdText, "Make Prettier (installing npm modules...)");
                        prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
                    }
                }

                return VSConstants.S_OK;
            }

            return Next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        private static void SetText(IntPtr pCmdTextInt, string text)
        {
            try
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
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}