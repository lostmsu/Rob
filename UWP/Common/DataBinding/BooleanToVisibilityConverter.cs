namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;

/// <summary>
/// Value converter that translates true to <see cref="Visibility.Visible"/> and false to
/// <see cref="Visibility.Collapsed"/>.
/// </summary>
public sealed class BooleanToVisibilityConverter: IValueConverter {
	public object Convert(object value, Type targetType, object parameter, string language) {
		return value is true ? Visibility.Visible : Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		return value is Visibility.Visible;
	}
}