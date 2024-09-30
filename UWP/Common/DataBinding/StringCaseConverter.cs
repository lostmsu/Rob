namespace RoboZZle.WinRT.Common.DataBinding;

public sealed class StringCaseConverter: IValueConverter {
	public bool Capitalize { get; set; } = true;

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		string? originalString = value?.ToString();
		if (originalString == null)
			return null;

		return this.Capitalize ? originalString.ToUpper() : originalString.ToLower();
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotSupportedException();
	}
}