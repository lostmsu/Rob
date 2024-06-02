namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

sealed class DifficultyToColorConverter: IValueConverter {
	public int EasyMedium { get; set; } = 40;
	public int MediumHard { get; set; } = 60;

	public int HardInsane { get; set; } = 75;

	public Brush? EasyForeground { get; set; }
	public Brush? MediumForeground { get; set; }
	public Brush? HardForeground { get; set; }
	public Brush? InsaneForeground { get; set; }

	public object? Convert(object? value, Type targetType, object parameter, string language) {
		if (value == null)
			return null;

		int difficulty = (int)value;
		if (difficulty < this.EasyMedium)
			return this.EasyForeground;
		if (difficulty < this.MediumHard)
			return this.MediumForeground;
		if (difficulty < this.HardInsane)
			return this.HardForeground;
		return this.InsaneForeground;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		throw new NotSupportedException();
	}
}