namespace RoboZZle.WinRT;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

public sealed partial class SettingsPopup: UserControl {
	public SettingsPopup() {
		this.InitializeComponent();

		this.LoginBox.Text = this.App.UserName ?? "";
		this.ThemeCombo.SelectedItem = ((RoboZZleRt)Application.Current).UseLightTheme
			? this.LightTheme
			: this.DarkTheme;
	}

	ApplicationViewModel App => (Application.Current as RoboZZleRt)!.ViewModel;

	async void AttemptLoginRegister(object sender, RoutedEventArgs args) {
		if (!this.ValidateCredentials())
			return;

		this.BeginAccountAttempt();
		this.PasswordBox.Password = this.App.HashPassword(this.PasswordBox.Password);
		try {
			if (sender == this.LoginButton) {
				await this.App.Login(this.LoginBox.Text, this.PasswordBox.Password);
			} else {
				if (!this.ValidateEmail())
					return;
				await this.App.Register(this.LoginBox.Text, this.PasswordBox.Password,
				                        this.EmailBox.Text);
			}
		} catch (Exception e) {
			this.SetError(e.Message);
		}

		this.EndAccountAttempt();
	}

	void EndAccountAttempt() {
		On(this.LoginBox, this.PasswordBox, this.LoginButton, this.RegisterButton, this.EmailBox);
	}

	static void On(params Control[] controls) {
		foreach (var control in controls) {
			control.IsEnabled = true;
		}
	}

	static void Off(params Control[] controls) {
		foreach (var control in controls) {
			control.IsEnabled = false;
		}
	}

	void BeginAccountAttempt() {
		Off(this.LoginBox, this.PasswordBox, this.LoginButton, this.RegisterButton, this.EmailBox);
	}

	bool ValidateCredentials() {
		if (string.IsNullOrWhiteSpace(this.LoginBox.Text))
			this.SetError("Login must not be empty");
		else if (string.IsNullOrWhiteSpace(this.PasswordBox.Password))
			this.SetError("PasswordHash must not be empty");
		else {
			this.ResetError();
			return true;
		}

		return false;
	}

	bool ValidateEmail() {
		if (string.IsNullOrWhiteSpace(this.EmailBox.Text))
			this.SetError("Email must not be empty");
		else {
			this.ResetError();
			return true;
		}

		return false;
	}

	void ResetError() {
		this.AccountError.Opacity = 0;
	}

	void SetError(string error) {
		this.AccountError.Text = error;
		this.AccountError.Opacity = 1;
	}

	void CloseSettings(object sender, RoutedEventArgs e) {
		if (this.Parent is Popup parent) {
			parent.IsOpen = false;
		}
	}

	void ThemeChanged(object sender, SelectionChangedEventArgs e) {
		((RoboZZleRt)Application.Current).UseLightTheme = e.AddedItems.Contains(this.LightTheme);
	}
}