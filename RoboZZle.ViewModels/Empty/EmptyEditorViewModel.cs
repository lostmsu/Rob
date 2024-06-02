namespace RoboZZle.ViewModels.Empty;

/// <summary>
/// Represents an emtpy instance of <see cref="ProgramEditorViewModel"/>
/// </summary>
public sealed class EmptyEditorViewModel: ProgramEditorViewModel {
	/// <summary>
	/// Creates new instance of <see cref="EmptyEditorViewModel"/>
	/// </summary>
	public EmptyEditorViewModel()
		: base(EmptyPuzzleViewModel.Instance, EmptyPuzzle.EmptyHistory,
		       new SamplePuzzleTelemetry()) { }
}