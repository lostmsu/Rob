namespace RoboZZle.WinRT.Common.DataBinding;

/// <summary>
/// Value converter that translates true to false and vice versa.
/// </summary>
public sealed class BooleanNegationConverter: IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo _) {
		return value is not true;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo _) {
		return value is not true;
	}
}