using JeffPires.BacklogChatGPTAssistant.Options;
using JeffPires.BacklogChatGPTAssistant.Utils;
using JeffPires.BacklogChatGPTAssistantShared.Utils;
using Microsoft.VisualStudio.Shell;
using System;
using System.Windows;
using System.Windows.Controls;
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

                ucCommands commands = new(options);

                Grid.SetRow(commands, 1);
                Grid.SetColumn(commands, 1);
                Grid.SetColumnSpan(commands, 3);

                grdControls.Children.Add(commands);

                controlStarted = true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

                MessageBox.Show(ex.Message, Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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

        #endregion Methods        
    }
}