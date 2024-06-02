namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;

/// <summary>
/// Value converter that translates true and false to double values
/// </summary>
public sealed class BooleanToDoubleConverter: IValueConverter {
	public double False { get; set; } = 0;
	public double True { get; set; } = 1;

	public object Convert(object value, Type targetType, object parameter, string language) {
		return value is true ? this.True : this.False;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		throw new NotSupportedException();
	}
}