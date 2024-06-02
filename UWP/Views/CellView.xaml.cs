namespace RoboZZle.WinRT.Views;

using RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Controls;

public sealed partial class CellView: UserControl {
	public CellView() {
		this.InitializeComponent();
	}

	public const int CELL_SIZE_SCALE = 20;

	public Color? Color {
		set => this.Cell.Background = ColorToBrushConverter.Instance.Convert(value);
	}

	public bool HasStar {
		set => this.Star.Opacity = value ? 1 : 0;
	}
}