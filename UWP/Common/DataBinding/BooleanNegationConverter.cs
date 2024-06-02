namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;

/// <summary>
/// Value converter that translates true to false and vice versa.
/// </summary>
sealed class BooleanNegationConverter: IValueConverter {
	public object Convert(object? value, Type targetType, object parameter, string language) {
		return value is not true;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		return value is not true;
	}
}