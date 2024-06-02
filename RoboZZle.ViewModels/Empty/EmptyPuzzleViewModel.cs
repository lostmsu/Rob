namespace RoboZZle.ViewModels.Empty;

using System.Threading.Tasks;

using LostTech.App;

public sealed class EmptyPuzzleViewModel: PuzzleViewModel {
	EmptyPuzzleViewModel(): base(EmptyPuzzle.Instance.ID, _ => EmptyPuzzle.Instance) {
		var emptyLocalData = new Dictionary<string, object?>();
		var emptyLocalViewModel = new LocalPuzzleViewModel(emptyLocalData);
		this.Local = new BindableTask<LocalPuzzleViewModel>(Task.FromResult(emptyLocalViewModel));
	}

	public static EmptyPuzzleViewModel Instance { get; } = new();
	protected override TaskScheduler GetTaskScheduler() => TaskScheduler.Default;
}