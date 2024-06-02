namespace RoboZZle.ViewModels;

using System.Collections.ObjectModel;

/// <summary>
/// View model for functions
/// </summary>
public class FunctionViewModel {
	/// <summary>
	/// Creates new instance of FunctionViewModel for specified function
	/// </summary>
	/// <param name="function">Function model</param>
	/// <param name="index">Function's index in the program</param>
	public FunctionViewModel(Function function, int index) {
		if (function == null)
			throw new ArgumentNullException(nameof(function));

		this.Index = index;

		if (function.Commands.Length == 0)
			return;

		this.InternalCommands = new(function.Commands);
		this.Commands = new(this.InternalCommands);
	}

	internal ObservableCollection<Command?> InternalCommands { get; }

	/// <summary>
	/// Collection of commands
	/// </summary>
	public ReadOnlyObservableCollection<Command?> Commands { get; }

	/// <summary>
	/// Index of the function inside program (1-based)
	/// </summary>
	public int Index { get; }
}