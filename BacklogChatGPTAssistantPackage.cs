using Community.VisualStudio.Toolkit;
using JeffPires.BacklogChatGPTAssistant.Commands;
using JeffPires.BacklogChatGPTAssistant.Options;
using JeffPires.BacklogChatGPTAssistant.ToolWindows;
using JeffPires.BacklogChatGPTAssistant.Utils;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace JeffPires.BacklogChatGPTAssistant
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.BacklogChatGPTAssistantString)]
    [ProvideOptionPage(typeof(OptionPageGridGeneral), "Backlog chatGPT Assistant", "General", 0, 0, true)]
    [ProvideProfile(typeof(OptionPageGridGeneral), "Backlog chatGPT Assistant", "General", 0, 0, true)]
    [ProvideToolWindow(typeof(TerminalWindow))]
    public sealed class BacklogChatGPTAssistantPackage : ToolkitPackage
    {
        /// <summary>
        /// Gets or sets the cancellation token source.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Gets the OptionPageGridGeneral object.
        /// </summary>
        public OptionPageGridGeneral OptionsGeneral
        {
            get
            {
                return (OptionPageGridGeneral)GetDialogPage(typeof(OptionPageGridGeneral));
            }
        }

        /// <summary>
        /// Initializes the terminal window commands.
        /// </summary>
        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Logger.Initialize(this, Constants.EXTENSION_NAME);

            await this.RegisterCommandsAsync();
            await TerminalWindowCommand.InitializeAsync(this);
        }
    }
}