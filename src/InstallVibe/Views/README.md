# Views

This folder contains all XAML pages/views for the InstallVibe application.

## Folder Structure

- **Shell/** - Main application shell and navigation
  - MainWindow.xaml - Main application window
  - ShellPage.xaml - Navigation shell with NavigationView

- **Activation/** - Product key activation views
  - WelcomePage.xaml - First-run welcome page
  - ActivationPage.xaml - Product key entry
  - LicenseInfoPage.xaml - License status display

- **Guides/** - Guide management views
  - GuideListPage.xaml - Browse all guides
  - GuideDetailPage.xaml - View guide details and steps
  - GuideCategoriesPage.xaml - Browse by category
  - GuideEditorPage.xaml - Create/edit guides (Admin only)
  - StepEditorPage.xaml - Edit individual steps (Admin)

- **Progress/** - Progress tracking views
  - ProgressPage.xaml - View all progress/history
  - ActiveGuidePage.xaml - Current active guide session

- **Sync/** - Synchronization views
  - SyncPage.xaml - Manual sync controls
  - SyncStatusDialog.xaml - Sync progress dialog

- **Settings/** - Application settings views
  - SettingsPage.xaml - Main settings page
  - AccountSettingsPage.xaml - SharePoint account settings
  - CacheSettingsPage.xaml - Cache management
  - AboutPage.xaml - About/version info

- **Dialogs/** - Reusable dialog views
  - ConfirmationDialog.xaml - Generic confirmation
  - ErrorDialog.xaml - Error messages
  - ProductKeyDialog.xaml - Quick product key entry

## Creating New Views

Each view should have:
1. A .xaml file (UI markup)
2. A .xaml.cs code-behind file
3. A corresponding ViewModel in the ViewModels folder
