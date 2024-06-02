namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;

/// <summary>
/// Converts integers to <see cref="Duration"/> values
/// </summary>
sealed class IntegerToDurationConverter: IValueConverter {
	/// <summary>
	/// Time units in ticks. Default value is <see cref="TimeSpan.TicksPerMillisecond"/>.
	/// </summary>
	public long TimeUnitInTicks { get; set; } = TimeSpan.TicksPerMillisecond;

	public object Convert(object intValue, Type targetType, object parameter, string language) {
		if (targetType != typeof(Duration))
			throw new ArgumentException("targetType");
		long time = System.Convert.ToInt64(intValue);
		long ticks = checked(time * this.TimeUnitInTicks);
		return new Duration(new TimeSpan(ticks));
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		throw new NotImplementedException();
	}
}