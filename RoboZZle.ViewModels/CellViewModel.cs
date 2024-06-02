namespace RoboZZle.ViewModels;

/// <summary>
/// View model for puzzle cell
/// </summary>
public class CellViewModel: ViewModelBase {
	Color? color;
	bool star;

	public CellViewModel(int x, int y, PuzzleCell cellModel) {
		this.Position.X = x;
		this.Position.Y = y;
		this.color = cellModel.Color;
		this.star = cellModel.Star;
	}

	/// <summary>
	/// Cell position
	/// </summary>
	public PositionViewModel Position { get; } = new();

	/// <summary>
	/// Cell color
	/// </summary>
	public Color? Color {
		get => this.color;
		set {
			this.color = value;
			this.OnPropertyChanged();
		}
	}
	/// <summary>
	/// True if cell has a star
	/// </summary>
	public bool Star {
		get => this.star;
		set {
			this.star = value;
			this.OnPropertyChanged();
		}
	}
}