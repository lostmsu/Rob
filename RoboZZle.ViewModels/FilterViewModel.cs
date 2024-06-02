namespace RoboZZle.ViewModels;

public class FilterViewModel: ViewModelBase {
	public FilterViewModel(IDictionary<string, object?> settings) {
		this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

		object? hide;
		this.hideUnpopular = !settings.TryGetValue("hideUnpopular", out hide) || (bool)hide;
		this.hideSolved = settings.TryGetValue("hideSolved", out hide) && (bool)hide;
		this.ResetFilter();
	}

	readonly IDictionary<string, object?> settings;

	public bool HideSolved {
		get => this.hideSolved;
		set {
			this.settings["hideSolved"] = this.hideSolved = value;
			this.ResetFilter();
			this.OnPropertyChanged();
		}
	}

	public bool HideUnpopular {
		get => this.hideUnpopular;
		set {
			this.settings["hideUnpopular"] = this.hideUnpopular = value;
			this.ResetFilter();
			this.OnPropertyChanged();
		}
	}

	public string? TitlePart {
		get => this.titlePart;
		set {
			if (this.titlePart == value)
				return;

			this.titlePart = value;
			this.ResetFilter();
			this.OnPropertyChanged();
		}
	}

	public Predicate<object> Filter { get; private set; }

	bool hideSolved, hideUnpopular;
	string? titlePart;

	void ResetFilter() {
		// ReSharper disable LocalVariableHidesMember
		// need to capture variable values
		bool hideSolved = this.hideSolved;
		bool hideUnpopular = this.hideUnpopular;
		string? titlePart = this.titlePart;
		// ReSharper restore LocalVariableHidesMember
		this.Filter = obj => {
			var puzzleVM = (PuzzleViewModel)obj;
			return (!hideSolved || puzzleVM.BestSolution == null)
			    && (string.IsNullOrEmpty(titlePart) ||
			        puzzleVM.Puzzle.Title.IndexOf(titlePart,
			                                      StringComparison.CurrentCultureIgnoreCase) >= 0)
			    && (!hideUnpopular || puzzleVM is { Popularity: >= 80, HasDifficulty: true });
		};
		this.OnPropertyChanged(nameof(this.Filter));
	}
}