namespace RoboZZle.WinRT.Common.DataBinding;

#if WINDOWS_UWP
class TimeSpanToDurationConverter: IValueConverter {
	public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
		var time = (TimeSpan?)value;
		if (targetType != typeof(Duration))
			throw new ArgumentException($"Value must be a {typeof(Duration)}", nameof(targetType));
		if (time == null)
			return null;

		return new Duration(time.Value);
	}

	object IValueConverter.ConvertBack(object? value, Type targetType, object? parameter,
	                                   CultureInfo culture) {
		throw new NotSupportedException();
	}
}
#endif
