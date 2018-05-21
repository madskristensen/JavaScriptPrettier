using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace JavaScriptPrettier
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [Guid(PackageGuids.guidPrettierPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class PrettierPackage : AsyncPackage
    {

    }
}
