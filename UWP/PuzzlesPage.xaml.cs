namespace RoboZZle.WinRT;

using System.Threading.Tasks;

using Windows.ApplicationModel.Core;

using RoboZZle.WinRT.Common;
using RoboZZle.WinRT.Imported;

using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using Microsoft.HockeyApp;
using Microsoft.Xbox.Services.System;

using static Windows.System.VirtualKey;

using Application = Windows.UI.Xaml.Application;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class PuzzlesPage: LayoutAwarePage {
	public PuzzlesPage() {
		this.InitializeComponent();
		this.xboxSilentSignInTask = XboxTrySilentSignIn();
	}

	readonly ResourceLoader resources = new();
	readonly Progress<double> progress = new();

	/// <summary>
	/// Invoked when this page is about to be displayed in a Frame.
	/// </summary>
	/// <param name="e">Event data that describes how this page was reached.  The Parameter
	/// property is typically used to configure the page.</param>
	protected override async void OnNavigatedTo(NavigationEventArgs e) {
		base.OnNavigatedTo(e);

		if (this.DataContext != null)
			return;

		Task<bool> signedIn = this.XboxSignIn();
		this.DataContext = this.App;

		this.progress.ProgressChanged += this.UpdateProgress;
		this.App.Progress = this.progress;

		if (!await signedIn)
			return;
		this.InitiateOnlineSuggestions();

		this.LoadingOverlay.ShowAndAutoHide();

		this.App.Filter.TitlePart = e.Parameter as string;

		var robozzle = (RoboZZleRt)Application.Current;
		this.CampaignCollection.Tag = robozzle.Campaign;
		this.PuzzlesView.ItemsSource = robozzle.Campaign;

		await this.App.ShowReadyTask;

		this.AllCollection.Tag = robozzle.PuzzleCollectionView;
		this.AllCollection.IsEnabled = true;

		await this.App.InitializationTask;

		DebugEx.WriteLine("application initialized");

		// TODO: replace with state management in ApplicationViewModel
		this.OnPuzzleListLoadingFinished();

		this.Dispatcher.RunIdleAsync(_ => DebugEx.WriteLine("app is idle"));
	}

	async void InitiateOnlineSuggestions() {
		if (!this.App.AiTelemetrySuggested) {
			var yesCommand = new UICommand(label: this.resources.GetString("AI_Agree"));
			var noCommand = new UICommand(label: this.resources.GetString("AI_Decline"));
			string title = this.resources.GetString("AI_OfferTitle");
			string content = this.resources.GetString("AI_Agreement");
			var dialog = new MessageDialog(content, title) { Commands = { yesCommand, noCommand } };
			IUICommand selectedCommand = await dialog.ShowAsync();
			this.App.AiTelemetryEnabled = selectedCommand == yesCommand;
			this.App.AiTelemetrySuggested = true;
		}

		if (this.App is { AiTelemetryEnabled: true, HasCredentials: false }) {
			await new MessageDialog(this.resources.GetString("AI_MustLogin")).ShowAsync();
			this.ShowSettings();
		}
	}

	void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
		this.App.Filter.TitlePart = sender.Text;
	}

	protected override void OnNavigatedFrom(NavigationEventArgs e) {
		this.progress.ProgressChanged -= this.UpdateProgress;

		base.OnNavigatedFrom(e);
	}

	void UpdateProgress(object sender, double progressValue) {
		this.StatusProgress.Value = progressValue;
	}

	async void OnPuzzleListLoadingFinished() {
		this.StatusProgress.Visibility = Visibility.Collapsed;
		this.StatusText.Text = "";
		var app = (RoboZZleRt)Application.Current;
		if (this.PuzzlesView.Items?.Count > 0) {
			this.PuzzlesView.SelectedIndex = 0;
			this.PuzzlesView.UpdateLayout();
			if (this.PuzzlesView.ContainerFromItem(this.PuzzlesView.SelectedItem) is GridViewItem a)
				a.Focus(FocusState.Keyboard);
		} else if (app.PuzzleCollectionView.Count > 0) {
			this.AllCollection.IsChecked = true;
		} else {
			await new MessageDialog(this.resources.GetString("No_Internet")).ShowAsync();
		}
	}

	void OnPuzzleClick(object sender, ItemClickEventArgs puzzleArgs) {
		var puzzle = (PuzzleViewModel)puzzleArgs.ClickedItem;
		RoboZZleRt.Navigate(this, typeof(ProgramEditor), puzzle);
	}

	void PuzzleCollectionChecked(object sender, RoutedEventArgs e) {
		if (this.PuzzlesView == null)
			return;

		this.LoadingOverlay.ShowAndAutoHide();

		var collection = (ListCollectionView)((Control)sender).Tag;
		this.PuzzlesView.ItemsSource = collection;
		if (collection.Count > 0) {
			this.PuzzlesView.ScrollIntoView(collection[0]);
			this.PuzzlesView.Focus(FocusState.Keyboard);
		}
	}

	void ShowLoadingOverlay(object sender, RoutedEventArgs e) {
		this.LoadingOverlay.ShowAndAutoHide();
	}

	void ShowSettings_Click(object sender, RoutedEventArgs e) {
		this.ShowSettings();
	}

	protected override void OnKeyDown(KeyRoutedEventArgs e) {
		if (e.Handled)
			return;

		switch (e.OriginalKey) {
		case GamepadX:
		case S when !(e.OriginalSource is TextBox):
			this.App.Filter.HideSolved = !this.App.Filter.HideSolved;
			e.Handled = true;
			break;
		case GamepadY:
		case U when !(e.OriginalSource is TextBox):
			this.App.Filter.HideUnpopular = !this.App.Filter.HideUnpopular;
			e.Handled = true;
			break;
		}

		base.OnKeyDown(e);
	}

	readonly Task xboxSilentSignInTask;

	static async Task XboxTrySilentSignIn() {
		var user = new XboxLiveUser();
		try {
			await user.SignInSilentlyAsync(CoreApplication.MainView.CoreWindow.Dispatcher);
		} catch (Exception) {
			return;
		}

		var app = (RoboZZleRt)Application.Current;
		app.SetXboxUser(user);
	}

	async Task<bool> XboxSignIn() {
		await this.xboxSilentSignInTask;
		if (!string.IsNullOrEmpty(this.App.Social.ProfilePicture))
			return true;

		var user = new XboxLiveUser();
		SignInResult? signInResult = null;
		while (signInResult is not { Status: SignInStatus.Success }) {
			try {
				signInResult = await user.SignInAsync(this.Dispatcher);
				if (signInResult.Status == SignInStatus.UserCancel) {
					Application.Current.Exit();
					return false;
				}
			} catch (Exception e) {
				HockeyClient.Current.TrackException(e);
				Application.Current.Exit();
				return false;
			}
		}

		var app = (RoboZZleRt)Application.Current;
		app.SetXboxUser(user);
		return true;
	}
}