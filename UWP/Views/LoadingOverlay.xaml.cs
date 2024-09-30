namespace RoboZZle.WinRT.Views;

using Microsoft.UI.Xaml.Controls;

public sealed partial class LoadingOverlay: UserControl {
	public LoadingOverlay() {
		this.InitializeComponent();
	}

	bool autoHide = false;

	/// <summary>
	/// Shows overlay and schedules automatic hiding when application will become idle
	/// </summary>
	public void ShowAndAutoHide() {
		if (this.Visibility == Visibility.Visible)
			return;

		this.autoHide = true;
		this.Visibility = Visibility.Visible;
		this.Dispatcher.RunIdleAsync(_ => {
			if (this.autoHide)
				this.Visibility = Visibility.Collapsed;

			this.autoHide = false;
		});
	}
}