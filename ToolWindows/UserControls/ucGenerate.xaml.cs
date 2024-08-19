using JeffPires.BacklogChatGPTAssistant.Models;
using JeffPires.BacklogChatGPTAssistant.Options;
using JeffPires.BacklogChatGPTAssistant.Utils;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using static JeffPires.BacklogChatGPTAssistant.Models.WorkItemBase;
using JsonSerializer = System.Text.Json.JsonSerializer;
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

        private readonly OptionPageGridGeneral options;
        private readonly bool controlStarted = false;
        private CancellationTokenSource cancellationTokenSource;

        #endregion Properties

        #region Constructors

        public ucCommands(OptionPageGridGeneral options)
        {
            this.InitializeComponent();

            this.options = options;

            cboProjects.SelectionChanged += cboProjects_SelectionChanged;
            cboIterationPaths.SelectionChanged += cboIterationPaths_SelectionChanged;
            cboInitialLevel.SelectionChanged += cboInitialLevel_SelectionChanged;
            cboInitialLevel.SelectedIndex = 0;

            List<WorkItemType> workItemTypes = Enum.GetValues(typeof(WorkItemType)).Cast<WorkItemType>().Reverse().ToList();
            cboInitialLevel.ItemsSource = workItemTypes.Select(wi => wi.GetStringValue()).ToList();

            cboProjects.ItemsSource = AzureDevops.ListProjects();
            cboProjects.DisplayMemberPath = nameof(Project.Name);

            controlStarted = true;
        }

        #endregion Constructors

        #region Event Handlers

        private async void cboProjects_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                cboIterationPaths.ItemsSource = await AzureDevops.ListInterationPathsAsync(((Project)cboProjects.SelectedItem).Name);
                cboIterationPaths.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

                MessageBox.Show(ex.Message, Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void cboIterationPaths_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!controlStarted)
            {
                return;
            }

            LoadParentWorkItems();

            cboInitialLevel.IsEnabled = true;
            gbStartFrom.IsEnabled = true;
            optNewWorkItem.IsChecked = true;
            btnGenerate.IsEnabled = true;
        }

        private void cboInitialLevel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!controlStarted)
            {
                return;
            }

            WorkItemType initialLevelSelected = GetSelectedInitialLevelWorkItemType();

            if (initialLevelSelected == WorkItemType.Epic)
            {
                gbStartFrom.Visibility = Visibility.Collapsed;
            }
            else
            {
                gbStartFrom.Visibility = Visibility.Visible;
            }

            if (initialLevelSelected != WorkItemType.Task)
            {
                chkGenerateChildren.Visibility = Visibility.Visible;
            }
            else
            {
                chkGenerateChildren.Visibility = Visibility.Collapsed;
            }

            if (initialLevelSelected == WorkItemType.ProductBacklogItem || initialLevelSelected == WorkItemType.Task)
            {
                spEstimateProjectHours.Visibility = Visibility.Visible;
                imgEstimateProjectHours.Visibility = Visibility.Visible;
            }
            else
            {
                spEstimateProjectHours.Visibility = Visibility.Collapsed;
                imgEstimateProjectHours.Visibility = Visibility.Collapsed;
            }

            LoadParentWorkItems();
        }

        private void optNewWorkItem_Checked(object sender, RoutedEventArgs e)
        {
            if (!controlStarted)
            {
                return;
            }

            grdParentWorkItem.Visibility = Visibility.Collapsed;
        }

        private void optExistingWorkItem_Checked(object sender, RoutedEventArgs e)
        {
            grdParentWorkItem.Visibility = Visibility.Visible;
        }

        private void txtEstimateProjectHours_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private async void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnGenerate.IsEnabled = false;
                btnStop.IsEnabled = true;

                grdProgress.Visibility = Visibility.Visible;
                txtProgress.Visibility = Visibility.Visible;

                await CreateBacklogItemsAsync();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

                MessageBox.Show(ex.Message, Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                ResetPage();
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            ResetPage();
        }

        #endregion Event Handlers

        #region Methods

        private async System.Threading.Tasks.Task LoadParentWorkItems()
        {
            try
            {
                cboParentWorkItem.ItemsSource = null;

                if (cboIterationPaths.SelectedValue == null)
                {
                    return;
                }

                WorkItemType selectedInitialLevel = GetSelectedInitialLevelWorkItemType();

                if (selectedInitialLevel == WorkItemType.Task)
                {
                    selectedInitialLevel = WorkItemType.ProductBacklogItem;
                }
                else if (selectedInitialLevel == WorkItemType.ProductBacklogItem)
                {
                    selectedInitialLevel = WorkItemType.Feature;
                }
                else if (selectedInitialLevel == WorkItemType.Feature)
                {
                    selectedInitialLevel = WorkItemType.Epic;
                }
                else
                {
                    return;
                }

                cboParentWorkItem.ItemsSource = await AzureDevops.ListWorkItemsAsync(((Project)cboProjects.SelectedItem).Name, cboIterationPaths.SelectedValue.ToString(), selectedInitialLevel);

                cboParentWorkItem.DisplayMemberPath = nameof(WorkItemBase.Title);
                cboParentWorkItem.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

                MessageBox.Show(ex.Message, Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async System.Threading.Tasks.Task CreateBacklogItemsAsync()
        {
            List<string> systemMessages = [];

            if (!string.IsNullOrWhiteSpace(options.InstructionDefault))
            {
                systemMessages.Add(options.InstructionDefault);
            }

            WorkItemType initialLevelSelected = GetSelectedInitialLevelWorkItemType();

            if (!string.IsNullOrWhiteSpace(options.InstructionWorkItemType))
            {
                systemMessages.Add($"{options.InstructionWorkItemType} {initialLevelSelected.GetStringValue()}");
            }

            if (initialLevelSelected != WorkItemType.Epic && optExistingWorkItem.IsChecked.Value)
            {
                systemMessages.Add($"{options.InstructionParentWork} Parent item:{Environment.NewLine}{JsonSerializer.Serialize(cboParentWorkItem.SelectedValue)}");
            }

            if (!string.IsNullOrWhiteSpace(options.InstructionEstimatedHours) && int.TryParse(txtEstimateProjectHours.Text, out int estimateProjectHours))
            {
                if (estimateProjectHours > 0)
                {
                    systemMessages.Add($"{options.InstructionEstimatedHours} {estimateProjectHours}");
                }
            }

            if (!string.IsNullOrWhiteSpace(options.InstructionChildren) && chkGenerateChildren.IsChecked.Value && initialLevelSelected != WorkItemType.Task)
            {
                systemMessages.Add(options.InstructionChildren);
            }

            systemMessages.Add($"Ensure the response is in the following json schema:{Environment.NewLine}{CreateAWorkItemAsExampleAsJson()}.");

            JSchemaGenerator generator = new();

            JSchema schema = generator.Generate(typeof(WorkItemBase));

            cancellationTokenSource = new CancellationTokenSource();

            string response = await ChatGPT.GetResponseAsync(options, systemMessages, txtInstructions.Text, null, schema, nameof(WorkItemBase), cancellationTokenSource.Token);

            response = TextFormat.RemoveLanguageIdentifier(response);

            JToken token = JToken.Parse(response);

            List<WorkItemBase> backlogItemmGenerated;

            try
            {
                if (token.Type == JTokenType.Object)
                {
                    backlogItemmGenerated = [JsonSerializer.Deserialize<WorkItemBase>(response)];
                }
                else
                {
                    backlogItemmGenerated = JsonSerializer.Deserialize<List<WorkItemBase>>(response);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

                MessageBox.Show("OpenAI invalid response.", Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private WorkItemType GetSelectedInitialLevelWorkItemType()
        {
            return EnumHelper.GetEnumFromStringValue<WorkItemType>(cboInitialLevel.SelectedValue.ToString()); ;
        }

        private void ResetPage()
        {
            cancellationTokenSource.Cancel();

            btnGenerate.IsEnabled = true;
            btnStop.IsEnabled = false;

            grdProgress.Visibility = Visibility.Collapsed;
            txtProgress.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Creates an example work item and serializes it to JSON format.
        /// </summary>
        /// <returns>
        /// A JSON string representation of the example work item.
        /// </returns>
        private string CreateAWorkItemAsExampleAsJson()
        {
            WorkItemBase workItem = new()
            {
                Id = 0,
                ParentId = 0,
                Type = WorkItemType.ProductBacklogItem,
                Title = "Example Work Item",
                Description = "This is an example work item.",
                AcceptanceCriteria = "This is an example of an Acceptance Criteria",
                RemainingWork = 0,
                Children = []
            };

            return JsonSerializer.Serialize(workItem);
        }

        #endregion Methods  
    }
}