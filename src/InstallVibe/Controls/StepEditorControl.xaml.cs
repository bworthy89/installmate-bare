using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels.Editor;
using InstallVibe.Core.Models.Domain;
using System.Linq;

namespace InstallVibe.Controls;

/// <summary>
/// Control for editing a single step.
/// </summary>
public sealed partial class StepEditorControl : UserControl
{
    private StepEditorViewModel? _viewModel;
    private GuideEditorViewModel? _parentViewModel;

    public StepEditorControl()
    {
        this.InitializeComponent();
        this.DataContextChanged += StepEditorControl_DataContextChanged;
    }

    private void StepEditorControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        // Parent DataContext is GuideEditorViewModel
        if (DataContext is GuideEditorViewModel parentViewModel)
        {
            _parentViewModel = parentViewModel;

            // Create StepEditorViewModel
            _viewModel = App.GetService<StepEditorViewModel>();

            // Subscribe to selected step changes
            _parentViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(GuideEditorViewModel.SelectedStep))
                {
                    LoadSelectedStep();
                }
            };

            // Load initial step
            LoadSelectedStep();
        }
    }

    private void LoadSelectedStep()
    {
        if (_viewModel == null || _parentViewModel?.SelectedStep == null)
            return;

        // Load the selected step into the step editor view model
        _viewModel.LoadStep(_parentViewModel.SelectedStep);

        // Bind controls to view model properties
        StepTitleTextBox.Text = _viewModel.StepTitle;
        StepInstructionsTextBox.Text = _viewModel.StepInstructions;
        EstimatedMinutesNumberBox.Value = _viewModel.EstimatedMinutes;
        ValidationTypeComboBox.SelectedItem = _viewModel.ValidationType;
        ValidationRequiredCheckBox.IsChecked = _viewModel.ValidationRequired;
        ValidationScriptTextBox.Text = _viewModel.ValidationScript;
        SuccessCriteriaTextBox.Text = _viewModel.SuccessCriteria;

        // Bind collections
        CheckpointsItemsControl.ItemsSource = _viewModel.Checkpoints;
        ActionsItemsControl.ItemsSource = _viewModel.Actions;
        MediaControl.DataContext = _viewModel;

        // Update empty states
        UpdateCheckpointsEmptyState();
        UpdateActionsEmptyState();

        // Subscribe to collection changes
        _viewModel.Checkpoints.CollectionChanged += (s, e) => UpdateCheckpointsEmptyState();
        _viewModel.Actions.CollectionChanged += (s, e) => UpdateActionsEmptyState();

        // Subscribe to text changes
        StepTitleTextBox.TextChanged += (s, e) => _viewModel.StepTitle = StepTitleTextBox.Text;
        StepInstructionsTextBox.TextChanged += (s, e) => _viewModel.StepInstructions = StepInstructionsTextBox.Text;
        EstimatedMinutesNumberBox.ValueChanged += (s, e) => _viewModel.EstimatedMinutes = (int)EstimatedMinutesNumberBox.Value;
        ValidationTypeComboBox.SelectionChanged += (s, e) => _viewModel.ValidationType = ValidationTypeComboBox.SelectedItem as string ?? "Manual";
        ValidationRequiredCheckBox.Checked += (s, e) => _viewModel.ValidationRequired = true;
        ValidationRequiredCheckBox.Unchecked += (s, e) => _viewModel.ValidationRequired = false;
        ValidationScriptTextBox.TextChanged += (s, e) => _viewModel.ValidationScript = ValidationScriptTextBox.Text;
        SuccessCriteriaTextBox.TextChanged += (s, e) => _viewModel.SuccessCriteria = SuccessCriteriaTextBox.Text;
    }

    private void UpdateCheckpointsEmptyState()
    {
        if (_viewModel == null) return;

        CheckpointsEmptyState.Visibility = _viewModel.Checkpoints.Any()
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private void UpdateActionsEmptyState()
    {
        if (_viewModel == null) return;

        ActionsEmptyState.Visibility = _viewModel.Actions.Any()
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private void SaveStep_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null) return;

        // Save changes back to the step
        _viewModel.SaveChanges();

        // Mark parent as having unsaved changes
        if (_parentViewModel != null)
        {
            _parentViewModel.HasUnsavedChanges = true;
        }
    }

    private void DeleteCheckpoint_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Checkpoint checkpoint)
        {
            _viewModel?.DeleteCheckpointCommand.Execute(checkpoint);
        }
    }

    private void MoveCheckpointUp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Checkpoint checkpoint)
        {
            _viewModel?.MoveCheckpointUpCommand.Execute(checkpoint);
        }
    }

    private void MoveCheckpointDown_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Checkpoint checkpoint)
        {
            _viewModel?.MoveCheckpointDownCommand.Execute(checkpoint);
        }
    }

    private void DeleteAction_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is StepAction action)
        {
            _viewModel?.DeleteActionCommand.Execute(action);
        }
    }

    private void MoveActionUp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is StepAction action)
        {
            _viewModel?.MoveActionUpCommand.Execute(action);
        }
    }

    private void MoveActionDown_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is StepAction action)
        {
            _viewModel?.MoveActionDownCommand.Execute(action);
        }
    }
}
