using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels.Admin;

namespace InstallVibe.Views.Admin;

/// <summary>
/// Admin page for managing guides.
/// </summary>
public sealed partial class AdminEditorPage : Page
{
    public AdminEditorViewModel ViewModel { get; }

    public AdminEditorPage()
    {
        ViewModel = App.GetService<AdminEditorViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
