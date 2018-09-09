using System;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace JavaScriptPrettier
{
    internal sealed class RunningDocTableEventsHandler : IVsRunningDocTableEvents3
    {
        private readonly PrettierPackage _package;

        public RunningDocTableEventsHandler(PrettierPackage package)
        {
            _package = package;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;
        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;
        public int OnAfterSave(uint docCookie) => VSConstants.S_OK;
        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs) => VSConstants.S_OK;
        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame) => VSConstants.S_OK;
        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame) => VSConstants.S_OK;
        public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew) => VSConstants.S_OK;

        public int OnBeforeSave(uint docCookie)
        {
            if (_package.optionPage.FormatOnSave)
            {
                RunningDocumentInfo docInfo = _package._runningDocTable.GetDocumentInfo(docCookie);
                Document doc = _package._dte.Documents.OfType<Document>().SingleOrDefault(x => x.FullName == docInfo.Moniker);

                if (doc != null)
                {
                    IVsTextView vsTextView = GetIVsTextView(doc.FullName);
                    if (vsTextView == null)
                    {
                        return VSConstants.S_OK;
                    }

                    IWpfTextView wpfTextView = GetWpfTextView(vsTextView);
                    if (wpfTextView == null)
                    {
                        return VSConstants.S_OK;
                    }

                    PrettierCommand cmd;
                    if (wpfTextView.Properties.TryGetProperty<PrettierCommand>("prettierCommand", out cmd))
                    {
                        ThreadHelper.JoinableTaskFactory.Run(() => cmd.MakePrettierAsync());
                    }
                }
            }
            return VSConstants.S_OK;
        }

        private IVsTextView GetIVsTextView(string filePath)
        {
            return VsShellUtilities.IsDocumentOpen(_package._serviceProvider, filePath, Guid.Empty, out var uiHierarchy, out uint itemId, out var windowFrame)
                ? VsShellUtilities.GetTextView(windowFrame) : null;
        }

        private static IWpfTextView GetWpfTextView(IVsTextView vTextView)
        {
            IWpfTextView view = null;
            IVsUserData userData = (IVsUserData)vTextView;

            if (userData != null)
            {
                Guid guidViewHost = Microsoft.VisualStudio.Editor.DefGuidList.guidIWpfTextViewHost;
                userData.GetData(ref guidViewHost, out var holder);
                IWpfTextViewHost viewHost = (IWpfTextViewHost)holder;
                view = viewHost.TextView;
            }

            return view;
        }
    }
}
