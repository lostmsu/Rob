namespace RoboZZle.ViewModels;

using System.Collections.ObjectModel;

/// <summary>
/// View model for puzzle state
/// </summary>
public class PuzzleStateViewModel: ViewModelBase {
	public PuzzleStateViewModel(PuzzleState puzzleState) {
		if (puzzleState == null)
			throw new ArgumentNullException(nameof(puzzleState));

		var cells = new ObservableCollection<CellViewModel>();
		this.cellViewModels = new CellViewModel[Puzzle.WIDTH, Puzzle.HEIGHT];
		for (int x = 0; x < Puzzle.WIDTH; x++)
		for (int y = 0; y < Puzzle.HEIGHT; y++) {
			var cellModel = puzzleState.Cells[x][y];
			var cell = new CellViewModel(x, y, cellModel);
			cells.Add(cell);
			this.cellViewModels[x, y] = cell;
		}

		this.Cells = new ReadOnlyObservableCollection<CellViewModel>(cells);
		this.Robot = new RobotViewModel(puzzleState.Robot);
		this.state = puzzleState;
	}

	readonly CellViewModel[,] cellViewModels;
	readonly PuzzleState state;

	/// <summary>
	/// Collection of cells
	/// </summary>
	public ReadOnlyObservableCollection<CellViewModel> Cells { get; private set; }
	/// <summary>
	/// Robot position and direction
	/// </summary>
	public RobotViewModel Robot { get; private set; }

	/// <summary>
	/// Fast accessor to cell view model by its position
	/// </summary>
	public CellViewModel this[int x, int y] => this.cellViewModels[x, y];

	public int Width => this.state.Width;
	public int Height => this.state.Height;
}