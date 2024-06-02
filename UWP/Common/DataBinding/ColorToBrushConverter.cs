namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

using Color = RoboZZle.Core.Color;
using WinColor = Windows.UI.Color;

public sealed class ColorToBrushConverter: IValueConverter {
	static readonly SolidColorBrush RedDefault = new(WinColor.FromArgb(0xFF, 0xFF, 0x10, 0x44));
	static readonly SolidColorBrush GreenDefault = new(WinColor.FromArgb(0xFF, 0x85, 0xD1, 0x00));
	static readonly SolidColorBrush BlueDefault = new(WinColor.FromArgb(0xFF, 0x00, 0xCC, 0xDB));

	public Brush? Colored {
		set => this.Red = this.Green = this.Blue = value;
	}
	public Brush? Red { get; set; } = RedDefault;
	public Brush? Green { get; set; } = GreenDefault;
	public Brush? Blue { get; set; } = BlueDefault;
	public Brush? Neutral { get; set; }

	public static ColorToBrushConverter Instance { get; } = new();

	public object? Convert(object value, Type targetType, object parameter, string language) {
		return this.Convert((Color?)value);
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		throw new NotSupportedException();
	}

	public Brush? Convert(Color? color) {
		if (color == null)
			return this.Neutral;

		return color.Value switch {
			Color.BLUE => this.Blue,
			Color.GREEN => this.Green,
			Color.RED => this.Red,
			_ => throw new NotSupportedException()
		};
	}
}