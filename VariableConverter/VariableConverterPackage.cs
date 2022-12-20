global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;

namespace VariableConverter
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.VariableConverterString)]
    [ProvideOptionPage(typeof(OptionsProvider.ConfigOptions), "变量转换", "配置", 0, 0, true)]
    [ProvideProfile(typeof(OptionsProvider.ConfigOptions), "变量转换", "配置", 0, 0, true)]
    public sealed class VariableConverterPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await ChineseToEnglishCommand.InitializeAsync(this);
            await ChineseToPinYinCommand.InitializeAsync(this);
        }
    }
}