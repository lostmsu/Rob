namespace RoboZZle.WinRT.Views;

using RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

sealed class CellViewSlim {
	readonly Rectangle cell = new() { Width = 19, Height = 19 };

	public void AddTo(int x, int y, Canvas canvas) {
		Canvas.SetLeft(this.cell, x * CellView.CELL_SIZE_SCALE);
		Canvas.SetTop(this.cell, y * CellView.CELL_SIZE_SCALE);
		canvas.Children.Add(this.cell);
	}

	public Color? Color {
		set => this.cell.Fill = ColorToBrushConverter.Instance.Convert(value);
	}
}