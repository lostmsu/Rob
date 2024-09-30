namespace RoboZZle.WinRT.Common.DataBinding;

public sealed class LocalizationConverter: IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
#warning NOT IMPLEMENTED
		return value == null ? null : value;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotSupportedException();
	}
}