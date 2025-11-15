using CommunityToolkit.Mvvm.ComponentModel;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Models.Progress;

namespace InstallVibe.ViewModels.Dashboard;

/// <summary>
/// ViewModel for displaying a guide card in the dashboard.
/// Combines Guide data with Progress information and UI states.
/// </summary>
public partial class GuideCardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _guideId = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private int _estimatedMinutes = 0;

    [ObservableProperty]
    private int _totalSteps = 0;

    [ObservableProperty]
    private int _completedSteps = 0;

    [ObservableProperty]
    private int _percentComplete = 0;

    [ObservableProperty]
    private bool _isInProgress = false;

    [ObservableProperty]
    private bool _isCompleted = false;

    [ObservableProperty]
    private bool _isNew = false;

    [ObservableProperty]
    private bool _isPinned = false;

    [ObservableProperty]
    private string _currentStepTitle = string.Empty;

    [ObservableProperty]
    private DateTime? _lastAccessedDate;

    [ObservableProperty]
    private DateTime? _completedDate;

    [ObservableProperty]
    private DateTime _createdDate;

    /// <summary>
    /// Creates a GuideCardViewModel from a Guide.
    /// </summary>
    public static GuideCardViewModel FromGuide(Guide guide, GuideProgress? progress = null, bool isPinned = false)
    {
        var viewModel = new GuideCardViewModel
        {
            GuideId = guide.GuideId,
            Title = guide.Title,
            Description = guide.Description ?? string.Empty,
            Category = guide.Category,
            EstimatedMinutes = guide.EstimatedMinutes ?? 0,
            TotalSteps = guide.Steps?.Count ?? 0,
            CreatedDate = guide.LastModified, // Using LastModified as CreatedDate
            IsPinned = isPinned
        };

        // Calculate "new" status (created/modified within last 30 days)
        viewModel.IsNew = guide.LastModified > DateTime.UtcNow.AddDays(-30);

        if (progress != null)
        {
            // Calculate progress stats
            var totalSteps = progress.StepProgress.Count;
            var completedSteps = progress.StepProgress.Values.Count(s => s == StepStatus.Completed);

            viewModel.CompletedSteps = completedSteps;
            viewModel.PercentComplete = totalSteps > 0 ? (completedSteps * 100 / totalSteps) : 0;
            viewModel.IsCompleted = progress.CompletedDate.HasValue;
            viewModel.IsInProgress = !viewModel.IsCompleted && completedSteps > 0;
            viewModel.LastAccessedDate = progress.LastUpdated;
            viewModel.CompletedDate = progress.CompletedDate;

            // Get current step title
            if (!string.IsNullOrEmpty(progress.CurrentStepId) && guide.Steps != null)
            {
                var currentStep = guide.Steps.FirstOrDefault(s => s.StepId == progress.CurrentStepId);
                viewModel.CurrentStepTitle = currentStep?.Title ?? string.Empty;
            }
        }

        return viewModel;
    }

    /// <summary>
    /// Gets a display string for estimated time.
    /// </summary>
    public string EstimatedTimeDisplay =>
        EstimatedMinutes > 0 ? $"{EstimatedMinutes} min" : "Unknown";

    /// <summary>
    /// Gets a display string for progress status.
    /// </summary>
    public string ProgressDisplay
    {
        get
        {
            if (IsCompleted)
                return "Completed";
            if (IsInProgress)
                return $"{PercentComplete}% Complete";
            return "Not Started";
        }
    }

    /// <summary>
    /// Gets a display string for last accessed date.
    /// </summary>
    public string LastAccessedDisplay
    {
        get
        {
            if (!LastAccessedDate.HasValue)
                return string.Empty;

            var daysDiff = (DateTime.UtcNow - LastAccessedDate.Value).Days;

            if (daysDiff == 0)
                return "Today";
            if (daysDiff == 1)
                return "Yesterday";
            if (daysDiff < 7)
                return $"{daysDiff} days ago";
            if (daysDiff < 30)
                return $"{daysDiff / 7} weeks ago";

            return LastAccessedDate.Value.ToString("MMM d, yyyy");
        }
    }

    /// <summary>
    /// Gets a display string for completed date.
    /// </summary>
    public string CompletedDateDisplay
    {
        get
        {
            if (!CompletedDate.HasValue)
                return string.Empty;

            var daysDiff = (DateTime.UtcNow - CompletedDate.Value).Days;

            if (daysDiff == 0)
                return "Completed today";
            if (daysDiff == 1)
                return "Completed yesterday";
            if (daysDiff < 7)
                return $"Completed {daysDiff} days ago";

            return $"Completed {CompletedDate.Value:MMM d, yyyy}";
        }
    }
}
