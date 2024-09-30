namespace RoboZZle.WinRT.Common.DataBinding;

public sealed class DebugConverter: IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return value;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		return value;
	}
}