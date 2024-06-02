namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

public sealed class LocalizationConverter: IValueConverter {
	readonly ResourceLoader resources = new();

	public object? Convert(object? value, Type targetType, object parameter, string language) {
		return value == null ? null : this.resources.GetString((string)value);
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		throw new NotSupportedException();
	}
}