using JeffPires.BacklogChatGPTAssistant.Models;
using JeffPires.BacklogChatGPTAssistant.Options;
using JeffPires.BacklogChatGPTAssistant.Utils;
using Microsoft.VisualStudio.Shell;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
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
        private bool controlStarted = false;
        private GenerateResult generateResult;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalWindowControl"/> class.
        /// </summary>
        public TerminalWindowControl()
        {
            this.InitializeComponent();

            this.Loaded += TerminalWindowControl_Loaded;
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the Loaded event of the TerminalWindowControl. 
        /// Validates API key and Azure DevOps URL, initializes the control, and displays alerts if necessary.
        /// </summary>
        private void TerminalWindowControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(options.ApiKey) || string.IsNullOrWhiteSpace(options.AzureDevopsUrl))
                {
                    imgAlert.Visibility = Visibility.Visible;
                    lblAlert.Visibility = Visibility.Visible;

                    return;
                }
                else if (controlStarted)
                {
                    return;
                }

                imgAlert.Visibility = Visibility.Collapsed;
                lblAlert.Visibility = Visibility.Collapsed;

                AzureDevops.Login(options);

                InitializeWorkItemGeneratorControl();

                controlStarted = true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

                MessageBox.Show(ex.Message, Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Handles the generation of user controls when work items are generated.
        /// Initiates a fade-out animation for the user control and subscribes to the storyboard's completion event.
        /// </summary>
        private void GenerateUserControl_WorkItemsGenerated(GenerateResult result)
        {
            generateResult = result;

            ucGenerate generateUserControl = grdControls.Children.OfType<ucGenerate>().FirstOrDefault();

            Storyboard fadeOutStoryboard = (Storyboard)FindResource("FadeOutStoryboard");

            fadeOutStoryboard.Begin(generateUserControl);

            fadeOutStoryboard.Completed += FadeOutStoryboard_Completed;
        }

        /// <summary>
        /// Handles the completion of the fade-out storyboard, removes the current user control,
        /// and adds a new backlog items user control with a fade-in effect.
        /// </summary>
        private void FadeOutStoryboard_Completed(object sender, EventArgs e)
        {
            ucGenerate generateUserControl = grdControls.Children.OfType<ucGenerate>().FirstOrDefault();

            grdControls.Children.Remove(generateUserControl);

            ucBacklogItems backlogItemsUserControl = new(generateResult, options);

            backlogItemsUserControl.Canceled += BacklogItemsUserControl_Canceled;

            Grid.SetRow(backlogItemsUserControl, 1);
            Grid.SetColumn(backlogItemsUserControl, 1);
            Grid.SetColumnSpan(backlogItemsUserControl, 3);

            grdControls.Children.Add(backlogItemsUserControl);

            Storyboard fadeInStoryboard = (Storyboard)FindResource("FadeInStoryboard");

            fadeInStoryboard.Begin(backlogItemsUserControl);
        }

        /// <summary>
        /// Handles the cancellation of the backlog items user control, removing the backlog items control 
        /// from the grid and reinitializing the work item generator control.
        /// </summary>
        private void BacklogItemsUserControl_Canceled()
        {
            ucBacklogItems backlogItemsControl = grdControls.Children.OfType<ucBacklogItems>().FirstOrDefault();

            grdControls.Children.Remove(backlogItemsControl);

            InitializeWorkItemGeneratorControl();
        }

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

        /// <summary>
        /// Initializes the Work Item Generator control, sets up event handlers and adds the control to the grid layout.
        /// </summary>
        private void InitializeWorkItemGeneratorControl()
        {
            ucGenerate generateUserControl = new(options);

            generateUserControl.WorkItemsGenerated += GenerateUserControl_WorkItemsGenerated;

            Grid.SetRow(generateUserControl, 1);
            Grid.SetColumn(generateUserControl, 1);
            Grid.SetColumnSpan(generateUserControl, 3);

            grdControls.Children.Add(generateUserControl);
        }

        #endregion Methods        
    }
}