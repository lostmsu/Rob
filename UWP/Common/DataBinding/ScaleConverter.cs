namespace RoboZZle.WinRT.Common.DataBinding;

public sealed class ScaleConverter: IValueConverter {
	public double Scale { get; set; } = 1;

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (value == null)
			return null;

		return System.Convert.ToDouble(value) * this.Scale;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}