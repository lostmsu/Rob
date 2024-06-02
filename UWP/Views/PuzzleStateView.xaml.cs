namespace RoboZZle.WinRT.Views;

using RoboZZle.WinRT.Common.Views;

using Windows.UI.Xaml.Controls;

public sealed partial class PuzzleStateView: UserControl {
	public PuzzleStateView() {
		this.InitializeComponent();

		this.EnsureCells();
	}

	void EnsureCells() {
		for (int x = 0; x < Puzzle.WIDTH; x++)
		for (int y = 0; y < Puzzle.HEIGHT; y++) {
			var cellView = new CellView();
			Canvas.SetLeft(cellView, x * CellView.CELL_SIZE_SCALE);
			Canvas.SetTop(cellView, y * CellView.CELL_SIZE_SCALE);
			this.cellViews[x, y] = cellView;
			this.Canvas.Children.Add(this.cellViews[x, y]);
		}
	}

	readonly CellView[,] cellViews = new CellView[Puzzle.WIDTH, Puzzle.HEIGHT];

	public RobotViewModel? RobotViewModel => this.Robot.DataContext as RobotViewModel;

	#region Animation Duration property

	public Duration StepDuration {
		get => (Duration)this.GetValue(StepDurationProperty);
		set => this.SetValue(StepDurationProperty, value);
	}

	public static DependencyProperty StepDurationProperty { get; } = DependencyProperty.Register(
		nameof(StepDuration), propertyType: typeof(Duration), ownerType: typeof(PuzzleStateView),
		typeMetadata: new PropertyMetadata(defaultValue: new Duration(new(0, 0, 0, 0, 150))));

	#endregion

	#region Puzzle State property

	public PuzzleStateViewModel PuzzleState {
		get => (PuzzleStateViewModel)this.GetValue(PuzzleStateProperty);
		set => this.SetValue(PuzzleStateProperty, value);
	}

	public static DependencyProperty PuzzleStateProperty { get; } = DependencyProperty.Register(
		nameof(PuzzleState), propertyType: typeof(PuzzleStateViewModel),
		ownerType: typeof(PuzzleStateView),
		typeMetadata: new PropertyMetadata(null, OnPuzzleStateChanged));

	void OnPuzzleStateChanged(DependencyPropertyChangedEventArgs args) {
		if (args.NewValue == null)
			return;

		var state = (PuzzleStateViewModel)args.NewValue;
		foreach (var cell in state.Cells) {
			this.cellViews[cell.Position.X, cell.Position.Y].DataContext = cell;
		}

		this.Robot.DataContext = state.Robot;
		foreach (Animator animator in new[] { "XAnimator", "YAnimator", "AngleAnimator" }
			         .Select(name => this.Robot.Resources[name])) {
			animator.Animate = true;
		}
	}

	static void OnPuzzleStateChanged(DependencyObject target,
	                                 DependencyPropertyChangedEventArgs args) {
		((PuzzleStateView)target).OnPuzzleStateChanged(args);
	}

	#endregion
}