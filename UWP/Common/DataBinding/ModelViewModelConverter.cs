namespace RoboZZle.WinRT.Common.DataBinding;

sealed class ModelViewModelConverter: IValueConverter {
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo _) {
		return value switch {
			null => null,
			PuzzleState puzzleState => new PuzzleStateViewModel(puzzleState),
			_ => throw new InvalidCastException(),
		};
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo _) {
		throw new NotImplementedException();
	}
}