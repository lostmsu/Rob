namespace RoboZZle.DesignTime;

using RoboZZle.WebService;

public static class SamplePuzzle {
	#region Item data

	static readonly LevelInfo2 LevelInfo = new() {
		Colors = [
			// ReSharper disable StringLiteralTypo
			"RGBRGBRGBRGBRGBR",
			"GBRGBRGBRGBRGBRG",
			"BRGBRGBRGBRGBRGB",
			"RGBRGBRGBRGBRGBR",
			"GBRGBRGBRGBRGBRG",
			"BRGBRGBRRRRRRRGB",
			"RGBRGBRGBRGBRGBR",
			"GBRGBRGBRGBRGBRG",
			"BRGBRGBRGBRGBRGB",
			"RGBRGBRGBRGBRGBR",
			"GBRGBRGBRGBRGBRG",
			"BRGBRGBRGBRGBRRR",
			// ReSharper restore StringLiteralTypo
		],
		Items = [
			"#..*#..*#..*#..*",
			"#..*#..*#..*#..*",
			"#..*#..*#..*#..*",
			"#..*#..*#..*#..*",
			"#..*#..*#..*#..*",
			"#..*#..*#..*#..*",
			"#..*#..*#..*#..*",
			"#..*#..*#..*#..*",
			"#..*#..*#..*#..*",
			"#..*#..*#..*#..*",
			"#..*#..*#..*#..*",
			"#..*#..*#..*#..*",
		],
	};

	#endregion

	public static Puzzle Instance { get; } = Create();
	public static Program EmptyProgram { get; } = new(Instance);
	public static ProgramHistory EmptyHistory { get; } = new(null, Instance) { EmptyProgram };

	static Puzzle Create() {
		var puzzle = new Puzzle {
			Title = "Sample",
			About = "Sample puzzle description. Not too long",
			Difficulty = 42,
			ID = 42,
			CommandSet = CommandSet.PAINT_ALL,
			InitialState = new PuzzleState {
				Robot = new Robot { Direction = Direction.Left, X = 4, Y = 4 },
			},
			SubLengths = [5, 4, 10, 0, 1],
		};

		Converters.ConvertCells(LevelInfo, puzzle.InitialState.Cells);
		return puzzle;
	}
}