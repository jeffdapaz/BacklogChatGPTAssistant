using JeffPires.BacklogChatGPTAssistant.Options;
using JeffPires.BacklogChatGPTAssistantShared.Utils;
using System;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace JeffPires.BacklogChatGPTAssistant.ToolWindows
{
    /// <summary>
    /// Represents a user control for command operations in the application and progress bar.
    /// </summary>
    public partial class ucCommands : UserControl
    {
        #region Events

        public event EventHandler onBtnGenerate_Clicked;

        #endregion Events

        #region Properties

        private OptionPageGridGeneral options;

        #endregion Properties

        #region Constructors

        public ucCommands(OptionPageGridGeneral options)
        {
            this.InitializeComponent();

            this.options = options;

            cboProjects.ItemsSource = AzureDevops.ListProjectsAsync().Result;
        }

        #endregion Constructors

        #region Event Handlers

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            //grdProgress.Visibility = Visibility.Visible;
            //btnGenerate.IsEnabled = false;

            
        }

        #endregion Event Handlers

        #region Methods

        

        #endregion Methods  
    }
}