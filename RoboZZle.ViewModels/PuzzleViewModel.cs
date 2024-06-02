namespace RoboZZle.ViewModels;

using System.Threading.Tasks;

using LostTech.App;

public class PuzzleViewModel: ViewModelBase {
	/// <summary>
	/// Creates new instance of PuzzleViewModel
	/// </summary>
	/// <param name="puzzleID">ID of the puzzle to load</param>
	/// <param name="loader">Puzzle loader function</param>
	public PuzzleViewModel(int puzzleID, Func<int, Puzzle> loader) {
		this.ID = puzzleID;
		this.loader = loader ?? throw new ArgumentNullException(nameof(loader));
	}

	readonly Func<int, Puzzle> loader;
	Puzzle? task;
	int? best;
	BindableTask<LocalPuzzleViewModel>? local;

	/// <summary>
	/// ID of the puzzle
	/// </summary>
	public int ID { get; }

	/// <summary>
	/// Current best known solution for the puzzle
	/// </summary>
	public int? BestSolution {
		get => this.best;
		set {
			if (value == this.best)
				return;

			this.best = value;
			this.OnPropertyChanged();
			this.OnPropertyChanged(nameof(this.HasSolution));
		}
	}

	public bool HasSolution => this.best != null;

	public int Popularity =>
		checked((this.Puzzle.Liked + 1) * 100 / (this.Puzzle.Liked + this.Puzzle.Disliked + 1));

	public int? Difficulty => this.Puzzle.Difficulty;

	public bool HasDifficulty => this.Puzzle.Difficulty != null;

	protected virtual TaskScheduler GetTaskScheduler() =>
		TaskScheduler.FromCurrentSynchronizationContext();

	/// <summary>
	/// Returns puzzle, when it becomes available
	/// </summary>
	public Puzzle Puzzle {
		get {
			if (this.task != null)
				return this.task;

			this.task = this.loader(this.ID);

			return this.task;
		}
	}

	public BindableTask<LocalPuzzleViewModel>? Local {
		get => this.local;
		set {
			this.local = value;
			this.OnPropertyChanged();
		}
	}
}