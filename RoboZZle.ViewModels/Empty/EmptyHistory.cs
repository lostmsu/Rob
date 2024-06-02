namespace RoboZZle.ViewModels.Empty;

class EmptyHistory: List<Program>, IProgramHistory {
	public EmptyHistory(Program program) {
		this.CurrentProgram = program ?? throw new ArgumentNullException(nameof(program));
		this.Add(program);
	}

	public Program CurrentProgram { get; }

	public int CurrentVersion => 0;

	public int LatestVersion => 0;

	public Program Redo() => throw new NotSupportedException();

	public void Save() => throw new NotSupportedException();

	public Program Undo() => throw new NotSupportedException();
}