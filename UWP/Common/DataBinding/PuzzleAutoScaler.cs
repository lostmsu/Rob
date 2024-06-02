namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

class PuzzleAutoScaler: IValueConverter {
	public int Width { get; set; } = 320;
	public int Height { get; set; } = 240;

	public object? Convert(object? value, Type targetType, object parameter, string language) {
		if (value == null)
			return null;

		var puzzle = value as PuzzleStateViewModel;
		if (puzzle == null)
			throw new ArgumentException("Expected value of type " +
			                            typeof(PuzzleStateViewModel).FullName + ". Got: " +
			                            value.GetType().FullName);

		int startX = puzzle.Width;
		int endX = -1;
		int startY = puzzle.Height;
		int endY = -1;
		foreach (var cell in puzzle.Cells) {
			if (cell.Color.HasValue) {
				startX = Math.Min(startX, cell.Position.X);
				endX = Math.Max(endX, cell.Position.X);
				startY = Math.Min(startY, cell.Position.Y);
				endY = Math.Max(endY, cell.Position.Y);
			}
		}

		int width = endX - startX + 1;
		int height = endY - startY + 1;
		double scale = Math.Min(puzzle.Width * 1.0 / width, puzzle.Height * 1.0 / height);

		double offsetX = (-startX + 0.5 * (puzzle.Width / scale - width)) * (this.Width / 16.0) *
		                 scale;
		double offsetY = -startY * (this.Height / 12.0) * scale;

		var result = new Matrix {
			M11 = scale, M22 = scale,
			OffsetX = offsetX, OffsetY = offsetY
		};
		return result;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		throw new NotSupportedException();
	}
}