using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.Input;

namespace InstallVibe.Controls;

/// <summary>
/// Panel for resuming the last active guide.
/// </summary>
public sealed partial class ResumeGuidePanel : UserControl
{
    public ResumeGuidePanel()
    {
        this.InitializeComponent();
    }

    #region Dependency Properties

    public static readonly DependencyProperty HasLastGuideProperty =
        DependencyProperty.Register(
            nameof(HasLastGuide),
            typeof(bool),
            typeof(ResumeGuidePanel),
            new PropertyMetadata(false));

    public bool HasLastGuide
    {
        get => (bool)GetValue(HasLastGuideProperty);
        set => SetValue(HasLastGuideProperty, value);
    }

    public static readonly DependencyProperty LastGuideTitleProperty =
        DependencyProperty.Register(
            nameof(LastGuideTitle),
            typeof(string),
            typeof(ResumeGuidePanel),
            new PropertyMetadata(string.Empty));

    public string LastGuideTitle
    {
        get => (string)GetValue(LastGuideTitleProperty);
        set => SetValue(LastGuideTitleProperty, value);
    }

    public static readonly DependencyProperty LastGuideStepProperty =
        DependencyProperty.Register(
            nameof(LastGuideStep),
            typeof(int),
            typeof(ResumeGuidePanel),
            new PropertyMetadata(0));

    public int LastGuideStep
    {
        get => (int)GetValue(LastGuideStepProperty);
        set => SetValue(LastGuideStepProperty, value);
    }

    public static readonly DependencyProperty LastGuideTotalStepsProperty =
        DependencyProperty.Register(
            nameof(LastGuideTotalSteps),
            typeof(int),
            typeof(ResumeGuidePanel),
            new PropertyMetadata(0));

    public int LastGuideTotalSteps
    {
        get => (int)GetValue(LastGuideTotalStepsProperty);
        set => SetValue(LastGuideTotalStepsProperty, value);
    }

    public static readonly DependencyProperty LastGuideProgressProperty =
        DependencyProperty.Register(
            nameof(LastGuideProgress),
            typeof(double),
            typeof(ResumeGuidePanel),
            new PropertyMetadata(0.0));

    public double LastGuideProgress
    {
        get => (double)GetValue(LastGuideProgressProperty);
        set => SetValue(LastGuideProgressProperty, value);
    }

    public static readonly DependencyProperty ResumeCommandProperty =
        DependencyProperty.Register(
            nameof(ResumeCommand),
            typeof(IRelayCommand),
            typeof(ResumeGuidePanel),
            new PropertyMetadata(null));

    public IRelayCommand ResumeCommand
    {
        get => (IRelayCommand)GetValue(ResumeCommandProperty);
        set => SetValue(ResumeCommandProperty, value);
    }

    #endregion
}
