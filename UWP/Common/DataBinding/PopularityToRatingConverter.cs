namespace RoboZZle.WinRT.Common.DataBinding;

public sealed class PopularityToRatingConverter: IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (value == null)
			return null;

		return System.Convert.ToInt32(value) / 20;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}