namespace RoboZZle.DesignTime;

public class SamplePuzzleViewModel: PuzzleViewModel {
	public SamplePuzzleViewModel(): base(SamplePuzzle.Instance.ID, _ => SamplePuzzle.Instance) {
		this.BestSolution = 32;
	}
}