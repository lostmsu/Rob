namespace RoboZZle.DesignTime;

public static class SampleProgram {
	static Program Create(Puzzle puzzle) {
		var result = new Program(puzzle);
		result.Functions[0]!.Commands[0] = new Command
			{ Action = new Movement(), Condition = Color.RED };
		return result;
	}

	public static Program Instance { get; } = Create(SamplePuzzle.Instance);
}