namespace RoboZZle.WinRT.Common.DataBinding;

using Windows.UI.Xaml.Data;

using Action = RoboZZle.Core.Action;

public sealed class PuzzleToCommandsConverter: IValueConverter {
	public bool ReverseActionOrder { get; set; }

	public object? Convert(object? value, Type targetType, object parameter, string language) {
		if (value is not Puzzle puzzle)
			return null;

		var conditions = puzzle.AvailableConditions;
		var actions = this.GetActions(puzzle);
		var commands = (from action in actions
		                from condition in conditions
		                select new Command {
			                Action = action,
			                Condition = condition,
		                }).ToArray();
		return from command in commands
		       group command by command.Condition
		       into grp
		       orderby grp.Key
		       select grp;
	}

	IEnumerable<Action> GetActions(Puzzle puzzle) {
		var result = new List<Action> {
			new Movement { Kind = MovementKind.MOVE },
			new Movement { Kind = MovementKind.TURN_LEFT },
			new Movement { Kind = MovementKind.TURN_RIGHT }
		};

		AddFunctionCalls(puzzle.SubLengths, result);

		AddPaintActions(puzzle.CommandSet, result);

		if (this.ReverseActionOrder)
			result.Reverse();

		return result;
	}

	static void AddFunctionCalls(int[] subLengths, ICollection<Action> result) {
		for (int i = 0; i < subLengths.Length; i++) {
			int length = subLengths[i];
			if (length > 0)
				result.Add(new Call { Function = i });
		}
	}

	static void AddPaintActions(CommandSet commandSet, ICollection<Action> result) {
		foreach (var color in Colors.All) {
			var paintCommand = color.PaintCommand();
			if (commandSet.HasFlag(paintCommand))
				result.Add(new Paint { Color = color });
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) {
		throw new NotSupportedException();
	}
}