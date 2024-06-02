namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;

class FormatConverter: IValueConverter {
	public object Convert(object value, Type targetType, object parameter, string language) {
		string? format = parameter as string;
		if (parameter == null)
			throw new ArgumentNullException(nameof(parameter));

		if (format == null)
			throw new ArgumentException("Converter parameter must be of type string",
			                            nameof(parameter));

		if (targetType != typeof(string))
			throw new ArgumentException("The only supported type is string", nameof(targetType));

		return string.Format(format, value);
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		throw new NotImplementedException();
	}
}