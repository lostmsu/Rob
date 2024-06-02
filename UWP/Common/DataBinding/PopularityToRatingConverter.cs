namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;

sealed class PopularityToRatingConverter: IValueConverter {
	public object? Convert(object? value, Type targetType, object parameter, string language) {
		if (value == null)
			return null;

		return System.Convert.ToInt32(value) / 20;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		throw new NotImplementedException();
	}
}