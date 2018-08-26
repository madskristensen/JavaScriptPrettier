using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace JavaScriptPrettier
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [Guid(PackageGuids.guidPrettierPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionPageGrid),
        "Prettier", "General", 0, 0, true)]
    [ProvideAutoLoad(cmdUiContextGuid: VSConstants.UICONTEXT.NotBuildingAndNotDebugging_string, flags: PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class PrettierPackage : AsyncPackage
    {
        internal DTE2 _dte;

        internal RunningDocumentTable _runningDocTable;
        internal OptionPageGrid optionPage;

        internal ServiceProvider _serviceProvider;

        public PrettierPackage()
        {
        }

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            _dte = ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE)) as DTE2;
            _serviceProvider = new ServiceProvider((IServiceProvider)_dte);
            _runningDocTable = new RunningDocumentTable(_serviceProvider);

            _runningDocTable.Advise(new RunningDocTableEventsHandler(this));

            optionPage = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));

            await base.InitializeAsync(cancellationToken, progress);
        }
    }

    public class OptionPageGrid : DialogPage
    {
        [Category("Prettier")]
        [DisplayName("Run On Save ")]
        [Description("Run Pretter whenever a file is saved")]
        public bool RunOnSave { get; set; }
    }
}
