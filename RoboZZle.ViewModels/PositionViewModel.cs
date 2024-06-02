namespace RoboZZle.ViewModels;

public class PositionViewModel: ViewModelBase {
	int x, y;

	/// <summary>
	/// X-coordinate
	/// </summary>
	public int X {
		get => this.x;
		set {
			this.x = value;
			this.OnPropertyChanged();
		}
	}

	/// <summary>
	/// Y-coordinate
	/// </summary>
	public int Y {
		get => this.y;
		set {
			this.y = value;
			this.OnPropertyChanged();
		}
	}
}