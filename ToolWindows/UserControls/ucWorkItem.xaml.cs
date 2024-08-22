using JeffPires.BacklogChatGPTAssistant.Models;
using JeffPires.BacklogChatGPTAssistant.Utils;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace JeffPires.BacklogChatGPTAssistant.ToolWindows
{
    /// <summary>
    /// User control containing the template to show the Backlog Items
    /// </summary>
    public partial class ucWorkItem : UserControl
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ucWorkItem()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the PreviewTextInput event for the txtEstimateProjectHours control, restricting input to numeric characters only.
        /// </summary>
        private void txtRemainingWork_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void btnImprove_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtImprove.Text))
            {
                MessageBox.Show("Please, write the suggestions to improve.", Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);

                return;
            }

            var button = sender as Button;

            WorkItemResult workItem = button.DataContext as WorkItemResult; 


        }

        #endregion Event Handlers

        #region Methods



        #endregion Methods                  
    }
}