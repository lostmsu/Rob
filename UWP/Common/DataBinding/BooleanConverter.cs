namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;

sealed class BooleanConverter: IValueConverter {
	public object? False { get; set; }
	public object? True { get; set; }
	public object? Null { get; set; }

	public object? Convert(object? value, Type targetType, object parameter, string language) {
		if (value == null)
			return this.Null;

		if (value is not bool boolean)
			throw new ArgumentException("value");

		return boolean ? this.True : this.False;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		throw new NotSupportedException();
	}
}