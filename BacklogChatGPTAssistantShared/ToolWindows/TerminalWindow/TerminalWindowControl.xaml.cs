using JeffPires.BacklogChatGPTAssistant.Options;
using Microsoft.VisualStudio.Shell;
using System.Threading;
using UserControl = System.Windows.Controls.UserControl;

namespace JeffPires.BacklogChatGPTAssistant.ToolWindows
{
    /// <summary>
    /// Interaction logic for TerminalWindowControl.
    /// </summary>
    public partial class TerminalWindowControl : UserControl
    {
        #region Properties

        private OptionPageGridGeneral options;
        private Package package;
        private readonly bool firstIteration;
        private readonly bool responseStarted;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly bool removeCodeTagsFromOpenAIResponses;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalWindowControl"/> class.
        /// </summary>
        public TerminalWindowControl()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Event Handlers



        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Starts the control with the given options and package.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="package">The package.</param>
        public void StartControl(OptionPageGridGeneral options, Package package)
        {
            this.options = options;
            this.package = package;
        }

        #endregion Methods        
    }
}