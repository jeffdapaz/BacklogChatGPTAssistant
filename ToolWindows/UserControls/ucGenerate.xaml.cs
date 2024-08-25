using JeffPires.BacklogChatGPTAssistant.Models;
using JeffPires.BacklogChatGPTAssistant.Options;
using JeffPires.BacklogChatGPTAssistant.Utils;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using JsonSerializer = System.Text.Json.JsonSerializer;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace JeffPires.BacklogChatGPTAssistant.ToolWindows
{
    /// <summary>
    /// User control responsible to generate the Backlog Items
    /// </summary>
    public partial class ucGenerate : UserControl
    {
        #region Events

        /// <summary>
        /// Represents a delegate that is called when work items are generated.
        /// </summary>
        /// <param name="result">The result of the generated work items.</param>
        public delegate void WorkItemsGeneratedDelegate(GenerateResult result);
        public event WorkItemsGeneratedDelegate WorkItemsGenerated;

        #endregion Events

        #region Properties

        private readonly OptionPageGridGeneral options;
        private readonly bool controlStarted = false;
        private CancellationTokenSource cancellationTokenSource;
        private Dictionary<string, string> selectedFiles = [];

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ucGenerate"/> class.
        /// Sets up the UI components and event handlers for project and iteration selection.
        /// </summary>
        /// <param name="options">The options for the general settings of the command user control.</param>
        public ucGenerate(OptionPageGridGeneral options)
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
            cboProjects.DisplayMemberPath = nameof(AzureDevopsProject.Name);

            controlStarted = true;
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the selection change event for the project combo box.
        /// Asynchronously retrieves and populates the iteration paths based on the selected project.
        /// If an error occurs during the retrieval, it logs the exception and displays a warning message.
        /// </summary>
        private async void cboProjects_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                cboIterationPaths.ItemsSource = await AzureDevops.ListInterationPathsAsync(((AzureDevopsProject)cboProjects.SelectedItem).Name);
                cboIterationPaths.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

                MessageBox.Show(ex.Message, Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Handles the selection change event for the iteration paths combo box.
        /// If the control has started, it loads the parent work items and enables
        /// related UI elements for further interaction.
        /// </summary>
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

        /// <summary>
        /// Handles the selection change event for the initial level combo box.
        /// Updates the visibility of UI elements based on the selected work item type.
        /// </summary>
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

            if (chkGenerateChildren.IsChecked.Value || initialLevelSelected == WorkItemType.ProductBacklogItem || initialLevelSelected == WorkItemType.Task)
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

        /// <summary>
        /// Handles the Checked event for the optNewWorkItem control.
        /// Collapses the parent work item grid if the control has started.
        /// </summary>
        private void optNewWorkItem_Checked(object sender, RoutedEventArgs e)
        {
            if (!controlStarted)
            {
                return;
            }

            grdParentWorkItem.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Handles the Checked event for the optExistingWorkItem control.
        /// Sets the visibility of the grdParentWorkItem to Visible.
        /// </summary>
        private void optExistingWorkItem_Checked(object sender, RoutedEventArgs e)
        {
            grdParentWorkItem.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the PreviewTextInput event for the txtEstimateProjectHours control,
        /// restricting input to numeric characters only.
        /// </summary>
        private void txtEstimateProjectHours_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Handles the click event for the checkbox to generate children work items.
        /// Toggles the visibility of the project hours estimation panel based on the checkbox state.
        /// </summary>
        private void chkGenerateChildren_Click(object sender, RoutedEventArgs e)
        {
            spEstimateProjectHours.Visibility = chkGenerateChildren.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
            imgEstimateProjectHours.Visibility = spEstimateProjectHours.Visibility;
        }

        /// <summary>
        /// Handles the click event for the generate button. 
        /// Disables the generate button and enables the stop button while showing progress indicators. 
        /// Asynchronously creates backlog items and invokes the WorkItemsGenerated event if any items are generated. 
        /// Catches and logs exceptions, displaying a warning message to the user, and resets the page state in the end.
        /// </summary>
        private async void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnGenerate.IsEnabled = false;
                btnStop.IsEnabled = true;

                grdProgress.Visibility = Visibility.Visible;
                txtProgress.Visibility = Visibility.Visible;

                GenerateResult result = await CreateBacklogItemsAsync();

                if (result.GeneratedWorkItems.Any())
                {
                    WorkItemsGenerated?.Invoke(result);
                }
            }
            catch (OperationCanceledException ex)
            {
                Logger.Log(ex);
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

        /// <summary>
        /// Event handler for the Stop button click event. 
        /// Resets the page to its initial state.
        /// </summary>
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            ResetPage();
        }

        /// <summary>
        /// Handles the MouseDown event for the btnAddFiles button. 
        /// Opens a file dialog allowing the user to select multiple Word or PDF files. 
        /// If files are selected, adds their paths to the lvFilesList and makes it visible.
        /// </summary>
        private void btnAddFiles_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = true,
                Filter = "All Files (*.*)|*.*|Word (*.docx)|*.docx|PDF (*.pdf)|*.pdf",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (openFileDialog.FileNames.Count(f => Path.GetExtension(f) != ".docx" && Path.GetExtension(f) != ".pdf") > 0)
                {
                    MessageBox.Show("You can only select docx or PDF files.", Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                foreach (string filename in openFileDialog.FileNames)
                {
                    selectedFiles.Add(filename, Path.GetFileName(filename));

                    lvFilesList.Items.Add(Path.GetFileName(filename));
                }

                lvFilesList.Visibility = Visibility.Visible;
                exFilesList.IsExpanded = true;
            }
        }

        /// <summary>
        /// Handles the MouseDown event for the btnRemoveFile button.
        /// Removes the specified file from the lvFilesList and collapses the list if empty.
        /// </summary>
        private void btnRemoveFile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image button = sender as Image;

            string fileToRemove = button.DataContext as string;

            lvFilesList.Items.Remove(fileToRemove);

            foreach (KeyValuePair<string, string> selectedFile in selectedFiles)
            {
                if (selectedFile.Value.Equals(fileToRemove))
                {
                    selectedFiles.Remove(selectedFile.Key);
                    break;
                }
            }

            if (lvFilesList.Items.Count == 0)
            {
                lvFilesList.Visibility = Visibility.Collapsed;
            }
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Asynchronously loads parent work items based on the selected iteration path and work item type.
        /// It updates the combo box with the retrieved work items and handles any exceptions that may occur.
        /// </summary>
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

                cboParentWorkItem.ItemsSource = await AzureDevops.ListWorkItemsAsync(((AzureDevopsProject)cboProjects.SelectedItem).Name, cboIterationPaths.SelectedValue.ToString(), selectedInitialLevel);

                cboParentWorkItem.DisplayMemberPath = nameof(WorkItem.Title);
                cboParentWorkItem.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

                MessageBox.Show(ex.Message, Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Asynchronously creates backlog items based on user selections and options, 
        /// generating a response from an AI service and processing the result into a 
        /// structured format.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation, containing a 
        /// <see cref="GenerateResult"/> object with the selected project, iteration path, 
        /// and generated work items.
        /// </returns>
        private async System.Threading.Tasks.Task<GenerateResult> CreateBacklogItemsAsync()
        {
            GenerateResult result = new()
            {
                SelectedProject = (AzureDevopsProject)cboProjects.SelectedItem,
                SelectedIterationPath = cboIterationPaths.SelectedValue.ToString(),
                GeneratedWorkItems = []
            };

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
                result.ExistentWorkItem = (WorkItem)cboParentWorkItem.SelectedValue;

                systemMessages.Add($"{options.InstructionParentWork} Parent item:{Environment.NewLine}{JsonSerializer.Serialize(result.ExistentWorkItem)}");
            }

            if (spEstimateProjectHours.Visibility == Visibility.Visible &&
                !string.IsNullOrWhiteSpace(options.InstructionEstimatedHours) &&
                int.TryParse(txtEstimateProjectHours.Text, out int estimateProjectHours) &&
                estimateProjectHours > 0)
            {
                systemMessages.Add($"{options.InstructionEstimatedHours} {estimateProjectHours}");
            }

            if (!string.IsNullOrWhiteSpace(options.InstructionChildren) && chkGenerateChildren.IsChecked.Value && initialLevelSelected != WorkItemType.Task)
            {
                systemMessages.Add(options.InstructionChildren);
            }

            systemMessages.Add(string.Format(Constants.COMMAND_JSON_SCHEMA, Environment.NewLine + CreateAWorkItemAsExampleAsJson()));

            JsonSchema schema = JsonSchema.FromType<List<WorkItem>>();

            cancellationTokenSource = new CancellationTokenSource();

            string response = await ChatGPT.GetResponseAsync(options, systemMessages, txtInstructions.Text, null, schema, nameof(WorkItem), cancellationTokenSource.Token);

            response = TextFormat.RemoveLanguageIdentifier(response);

            try
            {
                JToken token = JToken.Parse(response);

                if (token.Type == JTokenType.Object)
                {
                    WorkItem workItemGenerated = JsonSerializer.Deserialize<WorkItem>(response);

                    if (result.ExistentWorkItem != null && result.ExistentWorkItem.Id == workItemGenerated.Id)
                    {
                        result.GeneratedWorkItems.AddRange(workItemGenerated.Children);
                    }
                    else
                    {
                        result.GeneratedWorkItems.Add(workItemGenerated);
                    }
                }
                else
                {
                    result.GeneratedWorkItems = JsonSerializer.Deserialize<List<WorkItem>>(response);
                }

                SetWorkItemsType(result);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

                if (ex is not OperationCanceledException)
                {
                    MessageBox.Show("OpenAI invalid response. Please try again.", Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves the selected initial level work item type from the combo box.
        /// </summary>
        /// <returns>
        /// The corresponding WorkItemType based on the selected value from the combo box.
        /// </returns>
        private WorkItemType GetSelectedInitialLevelWorkItemType()
        {
            return EnumHelper.GetEnumFromStringValue<WorkItemType>(cboInitialLevel.SelectedValue.ToString()); ;
        }

        /// <summary>
        /// Resets the page by canceling any ongoing operations, 
        /// enabling the generate button, disabling the stop button, 
        /// and hiding the progress indicators.
        /// </summary>
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
            List<WorkItem> workItems =
            [
                new()
                {
                    Id = 0,
                    ParentId = 0,
                    Type = WorkItemType.ProductBacklogItem,
                    Title = "Example Work Item",
                    Description = "This is an example work item.",
                    AcceptanceCriteria = "This is an example of an Acceptance Criteria",
                    RemainingWork = 0,
                    Children = []
                }
            ];

            return JsonSerializer.Serialize(workItems);
        }

        /// <summary>
        /// Sets the type of work items for a given set of generated work items, 
        /// assigning a selected initial level type to each parent and its children.
        /// </summary>
        /// <param name="workItems">The generated work items to set types for.</param>
        private void SetWorkItemsType(GenerateResult workItems)
        {
            WorkItemType selectedInitialLevel = GetSelectedInitialLevelWorkItemType();

            foreach (WorkItem parentWorkItem in workItems.GeneratedWorkItems)
            {
                parentWorkItem.Type = selectedInitialLevel;

                foreach (WorkItem childWorkItem in parentWorkItem.Children)
                {
                    SetWorkItemType(childWorkItem, parentWorkItem.Type);
                }
            }
        }

        /// <summary>
        /// Sets the work item type based on the parent work item type and recursively updates child work items.
        /// </summary>
        /// <param name="workItem">The work item to update.</param>
        /// <param name="parentType">The type of the parent work item.</param>
        private void SetWorkItemType(WorkItem workItem, WorkItemType parentType)
        {
            workItem.Type = parentType switch
            {
                WorkItemType.Epic => WorkItemType.Feature,
                WorkItemType.Feature => WorkItemType.ProductBacklogItem,
                _ => WorkItemType.Task
            };

            if (workItem.Children != null && workItem.Children.Count > 0)
            {
                foreach (WorkItem childWorkItem in workItem.Children)
                {
                    SetWorkItemType(childWorkItem, workItem.Type);
                }
            }
        }

        #endregion Methods  
    }
}