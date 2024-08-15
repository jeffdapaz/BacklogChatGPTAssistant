﻿using JeffPires.BacklogChatGPTAssistant.Options;
using JeffPires.BacklogChatGPTAssistant.Utils;
using JeffPires.BacklogChatGPTAssistantShared.Models;
using JeffPires.BacklogChatGPTAssistantShared.Utils;
using System;
using System.Linq;
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

        private readonly OptionPageGridGeneral options;
        private readonly bool controlStarted = false;

        #endregion Properties

        #region Constructors

        public ucCommands(OptionPageGridGeneral options)
        {
            this.InitializeComponent();

            this.options = options;

            cboProjects.SelectionChanged += CboProjects_SelectionChanged;
            cboIterationPaths.SelectionChanged += CboIterationPaths_SelectionChanged;
            cboInitialLevel.SelectionChanged += CboInitialLevel_SelectionChanged;
            cboInitialLevel.SelectedIndex = 0;

            System.Collections.Generic.List<WorkItemType> workItemTypes = Enum.GetValues(typeof(WorkItemType)).Cast<WorkItemType>().Reverse().ToList();
            cboInitialLevel.ItemsSource = workItemTypes.Select(wi => wi.GetStringValue()).ToList();

            cboProjects.ItemsSource = AzureDevops.ListProjects();
            cboProjects.DisplayMemberPath = nameof(Project.Name);

            controlStarted = true;
        }

        #endregion Constructors

        #region Event Handlers

        private async void CboProjects_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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

        private void CboIterationPaths_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!controlStarted)
            {
                return;
            }

            LoadParentWorkItems();

            cboInitialLevel.IsEnabled = true;
            gbStartFrom.IsEnabled = true;
            optNewWorkItem.IsChecked = true;
        }

        private void CboInitialLevel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!controlStarted)
            {
                return;
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


        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            //grdProgress.Visibility = Visibility.Visible;
            //btnGenerate.IsEnabled = false;


        }

        #endregion Event Handlers

        #region Methods

        private async System.Threading.Tasks.Task LoadParentWorkItems()
        {
            try
            {
                cboParentWorkItem.ItemsSource = await AzureDevops.ListWorkItemsAsync(((Project)cboProjects.SelectedItem).Name,
                                                                                     cboIterationPaths.SelectedValue.ToString(),
                                                                                     EnumHelper.GetEnumFromStringValue<WorkItemType>(cboInitialLevel.SelectedValue.ToString()));

                cboParentWorkItem.DisplayMemberPath = nameof(WorkItemBase.Title);
                cboParentWorkItem.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

                MessageBox.Show(ex.Message, Constants.EXTENSION_NAME, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion Methods  
    }
}