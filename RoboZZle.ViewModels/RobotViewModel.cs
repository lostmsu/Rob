namespace RoboZZle.ViewModels;

/// <summary>
/// View model for robot
/// </summary>
public class RobotViewModel: ViewModelBase {
	int direction;

	public RobotViewModel(Robot robot) {
		this.Position.X = robot.X;
		this.Position.Y = robot.Y;
		this.direction = (int)robot.Direction;
	}

	/// <summary>
	/// Occurs when robot moves as a result of program instruction execution
	/// </summary>
	public event EventHandler<EventArgs<MovementKind>>? Movement;

	/// <summary>
	/// Position on the field
	/// </summary>
	public PositionViewModel Position { get; } = new();

	/// <summary>
	/// Direction
	/// </summary>
	public int Direction {
		get => this.direction;
		set {
			this.direction = value;
			this.OnPropertyChanged();
			this.OnPropertyChanged(nameof(this.Angle));
		}
	}
	/// <summary>
	/// Rotation angle
	/// </summary>
	public double Angle => this.direction * 90;

	internal void OnMovement(MovementKind movement)
		=> this.Movement?.Invoke(this, movement.ToEventArgs());
}