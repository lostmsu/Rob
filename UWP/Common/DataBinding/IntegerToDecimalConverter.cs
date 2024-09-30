namespace RoboZZle.WinRT.Common.DataBinding;

public sealed class IntegerToDecimalConverter: IValueConverter {
	public int Scale { get; set; }

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		int? intValue = (int?)value;
		if (intValue == null)
			return null;

		decimal result = intValue.Value;
		for (int i = 0; i < Math.Abs(this.Scale); i++) {
			result = this.Scale < 0 ? result / 10 : result * 10;
		}

		return result;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}