namespace RoboZZle.DesignTime;

using System.Collections.ObjectModel;

public class SamplePuzzleCollection: ObservableCollection<PuzzleViewModel> {
	public SamplePuzzleCollection() {
		for (int i = 0; i < 40; i++) {
			this.Add(new SamplePuzzleViewModel());
		}
	}
}