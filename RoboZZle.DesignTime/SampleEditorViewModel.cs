namespace RoboZZle.DesignTime;

/// <summary>
/// Sample of <see cref="ProgramEditorViewModel"/>
/// </summary>
public sealed class SampleEditorViewModel: ProgramEditorViewModel {
	static readonly Dictionary<string, object> PuzzleData = new() {
		{ "TimeSpent", new TimeSpan(1, 2, 3, 4) },
	};

	/// <summary>
	/// Creates new instance of <see cref="SampleEditorViewModel"/>
	/// </summary>
	public SampleEditorViewModel()
		: base(new SamplePuzzleViewModel(), SamplePuzzle.EmptyHistory,
		       new SamplePuzzleTelemetry()) { }
}