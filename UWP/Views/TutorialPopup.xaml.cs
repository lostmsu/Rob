namespace RoboZZle.WinRT.Views;

using Microsoft.UI.Xaml.Controls;

public sealed partial class TutorialPopup {
	public TutorialPopup() {
		this.InitializeComponent();
	}

	public bool SetFocus(FocusState focusState) {
		if (this.Videos.Items.Count == 0)
			return false;

		this.Videos.UpdateLayout();
		if (this.Videos.ContainerFromIndex(0) is ListViewItem video)
			return video.Focus(focusState);

		return false;
	}

	void Tutorials_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
		if (this.XboxMap != null)
			this.XboxMap.Visibility = this.Videos.SelectedItem == this.XboxKeyMapItem
				? Visibility.Visible
				: Visibility.Collapsed;
	}
}