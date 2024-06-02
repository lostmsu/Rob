namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;

/// <summary>
/// Value converter that translates non-empty objects to <see cref="Visibility.Visible"/>,
/// and nulls and empty ones to <see cref="Visibility.Collapsed"/>.
/// </summary>
public sealed class ObjectToVisibilityConverter: IValueConverter {
	public object Convert(object? value, Type targetType, object parameter, string language) {
		if (value == null)
			return Visibility.Collapsed;

		if (value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
			return Visibility.Collapsed;

		return Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		throw new NotSupportedException();
	}
}