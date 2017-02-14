using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using Tasks = System.Threading.Tasks;

namespace JavaScriptPrettier
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [Guid(PackageGuids.guidPrettierPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class PrettierPackage : AsyncPackage
    {
        protected override async Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            //if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            //{
            //    PrettierCommand.Initialize(this, commandService);
            //}

            await base.InitializeAsync(cancellationToken, progress);
        }
    }
}
