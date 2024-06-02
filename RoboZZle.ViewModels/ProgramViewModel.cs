namespace RoboZZle.ViewModels;

using System.Collections.ObjectModel;

/// <summary>
/// View model for program
/// </summary>
public class ProgramViewModel: ViewModelBase {
	/// <summary>
	/// Returns underlying model
	/// </summary>
	public Program Program { get; }

	[Obsolete]
	public ProgramViewModel(): this(null) { }

	/// <summary>
	/// Creates new instance of ProgramViewModel for specified program
	/// </summary>
	/// <param name="program">Program model</param>
	public ProgramViewModel(Program program) {
		this.Program = program ?? throw new ArgumentNullException(nameof(program));

		var functions = new ObservableCollection<FunctionViewModel>(
			program.Functions.Select((function, index) => new FunctionViewModel(function, index))
		);
		this.Functions = new ReadOnlyObservableCollection<FunctionViewModel>(functions);
	}

	/// <summary>
	/// Loads new program
	/// </summary>
	/// <param name="program">Program to load</param>
	public void Load(Program program) {
		if (program == null)
			throw new ArgumentNullException(nameof(program));
		if (program.Functions.Length != this.Functions.Count)
			throw new ArgumentException("program");

		for (int func = 0; func < program.Functions.Length; func++) {
			var function = this.Functions[func];
			var newFunction = program.Functions[func];
			if (function.Commands.Count != newFunction.Commands.Length)
				throw new ArgumentException("program");

			for (int instruction = 0; instruction < function.Commands.Count; instruction++) {
				if (function.Commands[instruction] != newFunction.Commands[instruction])
					function.InternalCommands[instruction] = newFunction.Commands[instruction];
			}
		}
	}

	/// <summary>
	/// Collection of functions
	/// </summary>
	public ReadOnlyObservableCollection<FunctionViewModel> Functions { get; }

	/// <summary>
	/// Gets or sets specified command
	/// </summary>
	/// <param name="function">Index of function</param>
	/// <param name="instruction">Index of command inside function</param>
	public Command? this[int function, int instruction] {
		get => this.Functions[function].Commands[instruction];
		set {
			var model = this.Program.Functions[function];
			if (model is null)
				throw new ArgumentException($"Function {function} is disallowed",
				                            paramName: nameof(function));
			this.Functions[function].InternalCommands[instruction] = value;
			model.Commands[instruction] = value;
		}
	}

	/// <summary>
	/// Gets <see cref="Command"/> at specified global index
	/// </summary>
	/// <param name="globalIndex">Global command index in this program</param>
	public Command? this[int globalIndex] {
		get {
			var pointer = InstructionPointer.FromGlobal(this.Program, globalIndex);
			return pointer == null ? null : this[pointer.Function, pointer.Command];
		}
	}

	/// <summary>
	/// Gets index of the specified command
	/// </summary>
	/// <param name="command"><see cref="Command"/> to find index for</param>
	public InstructionPointer? GetCommandIndex(Command command) {
		for (int fun = 0; fun < this.Functions.Count; fun++) {
			var function = this.Functions[fun];
			int index = function.Commands.IndexOf(command);
			if (index >= 0)
				return new InstructionPointer { Command = index, Function = fun };
		}

		return null;
	}
}