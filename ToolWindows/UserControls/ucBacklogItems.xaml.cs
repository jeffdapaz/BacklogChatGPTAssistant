using JeffPires.BacklogChatGPTAssistant.Models;
using UserControl = System.Windows.Controls.UserControl;

namespace JeffPires.BacklogChatGPTAssistant.ToolWindows
{
    /// <summary>
    /// User control responsible to generate the Backlog Items
    /// </summary>
    public partial class ucBacklogItems : UserControl
    {
        #region Events

        /// <summary>
        /// Represents a delegate that is called when work items are generated.
        /// </summary>
        /// <param name="result">The result of the generated work items.</param>
        public delegate void WorkItemsSavedDelegate();
        public event WorkItemsSavedDelegate WorkItemsSaved;

        #endregion Events

        #region Properties

        private GenerateResult workItems;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ucGenerate"/> class.
        /// Sets up the UI components and event handlers for project and iteration selection.
        /// </summary>
        /// <param name="options">The options for the general settings of the command user control.</param>
        public ucBacklogItems(GenerateResult workItems)
        {
            this.InitializeComponent();

            this.workItems = workItems;

            trvWorkItems.ItemsSource = workItems.GeneratedWorkItems;
        }

        #endregion Constructors

        #region Event Handlers



        #endregion Event Handlers

        #region Methods



        #endregion Methods  
    }
}