namespace RoboZZle.WinRT.Common.DataBinding;

public class FormatConverter: IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		ArgumentNullException.ThrowIfNull(parameter);

		if (parameter is not string format)
			throw new ArgumentException("Converter parameter must be of type string",
			                            nameof(parameter));

		if (targetType != typeof(string))
			throw new ArgumentException("The only supported type is string", nameof(targetType));

		return string.Format(format, value);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}