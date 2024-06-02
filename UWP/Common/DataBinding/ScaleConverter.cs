namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;

sealed class ScaleConverter: IValueConverter {
	public double Scale { get; set; } = 1;

	public object? Convert(object? value, Type targetType, object parameter, string language) {
		if (value == null)
			return null;

		return System.Convert.ToDouble(value) * this.Scale;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		throw new NotImplementedException();
	}
}