namespace RoboZZle.ViewModels;

using System.Collections.ObjectModel;
using System.Collections.Specialized;

public class PuzzleListViewModel: ReadOnlyObservableCollection<PuzzleViewModel> {
	internal PuzzleListViewModel(): base([]) { }

	readonly Dictionary<int, int> lookup = new();

	public void Add(PuzzleViewModel puzzle) {
		this.lookup.Add(puzzle.ID, this.Items.Count);
		this.Items.Add(puzzle);
	}

	public ReadOnlyObservableCollection<PuzzleViewModel> Puzzles => this;

	public new PuzzleViewModel? this[int puzzleID]
		=> this.lookup.TryGetValue(puzzleID, out int index) ? this.Items[index] : null;

	internal void Update(int puzzleID) {
		int index = this.lookup[puzzleID];
		this.Items[index] = this.Items[index];
	}

	public void Reset() {
		this.OnCollectionChanged(
			new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}
}