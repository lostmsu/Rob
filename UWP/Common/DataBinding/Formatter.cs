namespace RoboZZle.WinRT.Common.DataBinding;

using System.Globalization;

public sealed class Formatter: IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (parameter is not string format)
			throw new ArgumentNullException(paramName: nameof(parameter),
			                                "Format parameter must be specified");

		return value is IFormattable formattable
			? formattable.ToString(format, CultureInfo.CurrentUICulture)
			: null;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotSupportedException();
	}
}