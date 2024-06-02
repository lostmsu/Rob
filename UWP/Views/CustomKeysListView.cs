namespace RoboZZle.WinRT.Views;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using static Windows.System.VirtualKey;

public class CustomKeysListView: ListView {
	protected override void OnKeyDown(KeyRoutedEventArgs e) {
		switch (e.OriginalKey) {
		// on XBox they behave like Page Up/Down, which I want to replace
		case GamepadRightTrigger:
		case GamepadLeftTrigger:
			return;
		}

		base.OnKeyDown(e);
	}
}