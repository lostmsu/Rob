namespace RoboZZle.WinRT.Common.DataBinding;

public sealed class TimeSpanToTwoPartStringConverter: IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		var time = (TimeSpan?)value;
		if (targetType != typeof(string))
			throw new ArgumentException("Value must be a System.String", nameof(targetType));
		if (time == null)
			return null;

		return time.Value >= new TimeSpan(hours: 1, minutes: 0, seconds: 0)
			? $"{(int)time.Value.TotalHours:D2}h {time.Value.Minutes:D2}m"
			: $"{time.Value.Minutes:D2}m {time.Value.Seconds:D2}s";
	}

	object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter,
	                                    CultureInfo culture) {
		throw new NotSupportedException();
	}
}