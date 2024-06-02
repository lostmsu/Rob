namespace RoboZZle.ViewModels;

using System;
using System.Windows.Input;

/// <summary>
/// Represents executable command
/// </summary>
public class CommandViewModel: ICommand {
	readonly Action action;
	bool isEnabled;

	/// <summary>
	/// Creates new instance of CommandViewModel
	/// </summary>
	/// <param name="action">Action to perform, when command is executed</param>
	public CommandViewModel(Action action) {
		this.action = action ?? throw new ArgumentNullException(nameof(action));
	}

	/// <summary>
	/// Get or set, of command is enabled
	/// </summary>
	public bool IsEnabled {
		get => this.isEnabled;
		set {
			if (value == this.isEnabled)
				return;

			this.isEnabled = value;

			this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	/// <summary>
	/// Fired, when IsEnabled changed
	/// </summary>
	public event EventHandler? CanExecuteChanged;

	/// <summary>
	/// Executes command
	/// </summary>
	/// <param name="parameter">Not used</param>
	public void Execute(object? parameter) => this.action();

	/// <summary>
	/// Checks if command can be executed
	/// </summary>
	/// <param name="parameter">Not used</param>
	/// <returns>True, if command can be executed, otherwise false.</returns>
	public bool CanExecute(object? parameter) => this.isEnabled;

	public static CommandViewModel Disabled { get; } = new(() => { });

	/// <summary>
	/// Turns specified commands on
	/// </summary>
	/// <param name="commands">Commands to turn on</param>
	public static void On(params CommandViewModel[] commands) {
		foreach (var command in commands) {
			command.IsEnabled = true;
		}
	}

	/// <summary>
	/// Turns specified commands off
	/// </summary>
	/// <param name="commands">Commands to turn off</param>
	public static void Off(params CommandViewModel[] commands) {
		foreach (var command in commands) {
			command.IsEnabled = false;
		}
	}
}