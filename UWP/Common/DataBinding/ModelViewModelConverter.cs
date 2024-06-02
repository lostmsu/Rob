namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;

sealed class ModelViewModelConverter: IValueConverter {
	public object? Convert(object? value, Type targetType, object parameter, string language) {
		return value switch {
			null => null,
			PuzzleState puzzleState => new PuzzleStateViewModel(puzzleState),
			_ => throw new InvalidCastException(),
		};
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		throw new NotImplementedException();
	}
}