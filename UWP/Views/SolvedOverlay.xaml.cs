namespace RoboZZle.WinRT.Views;

using Windows.UI.Xaml.Controls;

public sealed partial class SolvedOverlay: UserControl {
	public SolvedOverlay() {
		this.InitializeComponent();
	}

	public void Victory() {
		this.Visibility = Visibility.Visible;
	}
}