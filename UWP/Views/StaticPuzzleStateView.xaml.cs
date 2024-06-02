namespace RoboZZle.WinRT.Views;

using RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

public sealed partial class StaticPuzzleStateView: UserControl {
	static readonly IValueConverter AutoScaler = new PuzzleAutoScaler();

	public StaticPuzzleStateView() {
		this.InitializeComponent();

		for (int x = 0; x < Puzzle.WIDTH; x++)
		for (int y = 0; y < Puzzle.HEIGHT; y++) {
			var cellView = new CellViewSlim();
			this.cellViews[x, y] = cellView;
			cellView.AddTo(x, y, this.Canvas);
		}
	}

	readonly CellViewSlim[,] cellViews = new CellViewSlim[Puzzle.WIDTH, Puzzle.HEIGHT];

	#region Puzzle State property

	public PuzzleStateViewModel PuzzleState {
		get => (PuzzleStateViewModel)this.GetValue(PuzzleStateProperty);
		set => this.SetValue(PuzzleStateProperty, value);
	}

	public static DependencyProperty PuzzleStateProperty { get; } = DependencyProperty.Register(
		nameof(PuzzleState), propertyType: typeof(PuzzleStateViewModel),
		ownerType: typeof(StaticPuzzleStateView),
		typeMetadata: new PropertyMetadata(null, OnPuzzleStateChanged));

	void OnPuzzleStateChanged(DependencyPropertyChangedEventArgs args) {
		if (args.NewValue == null)
			return;

		var state = (PuzzleStateViewModel)args.NewValue;

		this.ApplyState(state);
	}

	void ApplyState(PuzzleStateViewModel state) {
		foreach (var cell in state.Cells) {
			var view = this.cellViews[cell.Position.X, cell.Position.Y];
			view.Color = cell.Color;
		}

		this.Robot.RenderTransform = new CompositeTransform {
			CenterX = CellView.CELL_SIZE_SCALE * 0.5,
			CenterY = CellView.CELL_SIZE_SCALE * 0.5,
			Rotation = state.Robot.Angle,
			TranslateX = CellView.CELL_SIZE_SCALE * state.Robot.Position.X,
			TranslateY = CellView.CELL_SIZE_SCALE * state.Robot.Position.Y,
		};

		this.AutoscaleTransform.Matrix =
			(Matrix)AutoScaler.Convert(state, typeof(Matrix), null, null);
	}

	static void OnPuzzleStateChanged(DependencyObject target,
	                                 DependencyPropertyChangedEventArgs args) {
		((StaticPuzzleStateView)target).OnPuzzleStateChanged(args);
	}

	#endregion
}