using Microsoft.VisualStudio.OLE.Interop;
using System;

namespace JavaScriptPrettier
{
    abstract class BaseCommand : IOleCommandTarget
    {
        public IOleCommandTarget Next { get; set; }

        public abstract int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);

        public abstract int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText);
    }
}
