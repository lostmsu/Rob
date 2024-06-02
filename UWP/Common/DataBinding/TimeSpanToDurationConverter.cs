namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;

class TimeSpanToDurationConverter: IValueConverter {
	public object? Convert(object? value, Type targetType, object parameter, string language) {
		var time = (TimeSpan?)value;
		if (targetType != typeof(Duration))
			throw new ArgumentException($"Value must be a {typeof(Duration)}", nameof(targetType));
		if (time == null)
			return null;

		return new Duration(time.Value);
	}

	object IValueConverter.ConvertBack(object value, Type targetType, object parameter,
	                                   string language) {
		throw new NotSupportedException();
	}
}