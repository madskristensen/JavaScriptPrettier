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
    [ProvideOptionPage(typeof(OptionPageGrid), "Prettier", "General", 0, 0, true)]
    [ProvideAutoLoad(cmdUiContextGuid: VSConstants.UICONTEXT.NotBuildingAndNotDebugging_string, flags: PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class PrettierPackage : AsyncPackage
    {
        internal static NodeProcess _node;

        internal DTE2 _dte;
        internal RunningDocumentTable _runningDocTable;
        internal OptionPageGrid optionPage;
        internal ServiceProvider _serviceProvider;

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            _dte = ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE)) as DTE2;
            _serviceProvider = new ServiceProvider((IServiceProvider)_dte);
            _runningDocTable = new RunningDocumentTable(_serviceProvider);
            _runningDocTable.Advise(new RunningDocTableEventsHandler(this));

            optionPage = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
            _node = new NodeProcess(this);

            if (!_node.IsReadyToExecute())
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _node.EnsurePackageInstalledAsync().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            await base.InitializeAsync(cancellationToken, progress);
        }
    }

    public class OptionPageGrid : DialogPage
    {
        [Category("Prettier")]
        [DisplayName("Format On Save")]
        [Description("Run Prettier whenever a file is saved")]
        public bool FormatOnSave { get; set; }

        // Keep in sync with message below until interpolated strings
        // can be used in the Description.
        internal readonly string _prettierFallbackVersion = "2.2.1";

        [Category("Prettier")]
        [DisplayName("Prettier version for embedded usage")]
        [Description("This extension downloads its own install of Prettier to run if " +
            "Prettier is not installed via npm in your local JavaScript project. " +
            "If the version entered cannot be found, version 2.2.1 will be used as a fallback. ")]
        public string EmbeddedVersion { get; set; } = "1.12.1";
    }
}
