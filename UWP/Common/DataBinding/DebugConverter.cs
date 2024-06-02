namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;

sealed class DebugConverter: IValueConverter {
	public object Convert(object value, Type targetType, object parameter, string language) {
		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		return value;
	}
}