namespace RoboZZle.ViewModels.Empty;

using RoboZZle.WebService;

public static class EmptyPuzzle {
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
			"#..##..##..##..#",
			"#..##..##..##..#",
			"#..##..##..##..#",
			"#..##..##..##..#",
			"#..##..##..##..#",
			"#..##..##..##..#",
			"#..##..##..##..#",
			"#..##..##..##..#",
			"#..##..##..##..#",
			"#..##..##..##..#",
			"#..##..##..##..#",
			"#..##..##..##..#",
		],
	};

	#endregion

	public static Puzzle Instance { get; } = Create();
	public static Program EmptyProgram { get; } = new(Instance);

	public static IProgramHistory EmptyHistory { get; } = new EmptyHistory(EmptyProgram);

	static Puzzle Create() {
		var puzzle = new Puzzle {
			Title = "",
			About = "",
			Difficulty = 0,
			ID = 0,
			CommandSet = CommandSet.PAINT_ALL,
			InitialState = new PuzzleState {
				Robot = new Robot { Direction = Direction.Left, X = 4, Y = 4 },
			},
			SubLengths = [1],
		};

		Converters.ConvertCells(LevelInfo, puzzle.InitialState.Cells);
		return puzzle;
	}
}