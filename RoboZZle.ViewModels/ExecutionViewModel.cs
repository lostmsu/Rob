namespace RoboZZle.ViewModels;

using System.Threading.Tasks;

/// <summary>
/// View model for the program execution.
/// Handles execution and contains current program and puzzle states
/// </summary>
public class ExecutionViewModel: ViewModelBase {
	/// <summary>
	/// Creates an ExecutionViewModel for a specified program and puzzle
	/// </summary>
	/// <param name="program">Program ViewModel</param>
	/// <param name="puzzle">Puzzle</param>
	/// <param name="sessionLog">Session log</param>
	public ExecutionViewModel(ProgramViewModel program, Puzzle puzzle,
	                          SessionLogWriter sessionLog) {
		if (program == null)
			throw new ArgumentNullException(nameof(program));
		if (puzzle == null)
			throw new ArgumentNullException(nameof(puzzle));

		this.sessionLog = sessionLog ?? throw new ArgumentNullException(nameof(sessionLog));
		this.executor = new(program.Program, puzzle);
		this.PuzzleState = new(this.executor.PuzzleState);
		this.ProgramState = new(program.Program);
		this.ProgramState.NextCommand = this.ProgramState[0, 0];

		this.executor.CellChanged += this.ExecutorOnCellChanged;
		this.executor.ProgramStateChanged += this.ExecutorOnProgramStateChanged;
		this.executor.RobotPositionChanged += this.ExecutorOnRobotPositionChanged;

		this.PlayCommand = new(this.Start) { IsEnabled = true };
		this.PauseCommand = new(this.Pause);
		this.StepCommand = new(() => {
			if (this.IsRunning)
				this.PauseInternal();

			this.Step();
			this.sessionLog.Log(Telemetry.Actions.StepCommand.Instance);
		}) { IsEnabled = true };
		this.StopCommand = new(() => {
			this.StopInternal();
			this.sessionLog.Log(Telemetry.Actions.StopCommand.Instance);
		}) { IsEnabled = true };

		if (this.executor.IsTerminated())
			this.StopInternal();
	}

	#region Commands

	/// <summary>
	/// Command, that starts program execution
	/// </summary>
	public CommandViewModel PlayCommand { get; }
	/// <summary>
	/// Command, that pauses program execution
	/// </summary>
	public CommandViewModel PauseCommand { get; }
	/// <summary>
	/// Command to make single program execution step
	/// </summary>
	public CommandViewModel StepCommand { get; }
	/// <summary>
	/// Command, that terminates execution
	/// </summary>
	public CommandViewModel StopCommand { get; }

	#endregion

	/// <summary>
	/// Occurs when program makes a step
	/// </summary>
	public event Action<ExecutionViewModel>? Stepped;
	/// <summary>
	/// Occurs, when program execution is terminated
	/// </summary>
	public event Action<ExecutionViewModel>? Finished;

	/// <summary>
	/// Checks if victory is achieved
	/// </summary>
	public bool IsVictory => this.executor.IsVictory();

	/// <summary>
	/// Current instruction pointer
	/// </summary>
	public InstructionPointer? CurrentInstruction => this.executor.ProgramState.CurrentInstruction;

	/// <summary>
	/// Current puzzle state
	/// </summary>
	public PuzzleStateViewModel PuzzleState { get; }
	/// <summary>
	/// Current program state
	/// </summary>
	public ProgramStateViewModel ProgramState { get; }
	/// <summary>
	/// Number of program steps executed.
	/// </summary>
	public int ProgramSteps => this.executor.Steps;

	/// <summary>
	/// Number of robot moves.
	/// </summary>
	public int RobotMoves => this.executor.Moves;

	/// <summary>
	/// Time it should take to make a step
	/// </summary>
	public TimeSpan StepTime {
		get => this.stepTime;
		set {
			if (value < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(this.StepTime));
			this.stepTime = value;
			this.OnPropertyChanged();
		}
	}

	#region Private data

	readonly SessionLogWriter sessionLog;
	readonly ProgramExecutor executor;

	bool isFinished;
	bool isRunning;
	public bool IsRunning {
		get => this.isRunning;
		private set {
			this.isRunning = value;
			this.OnPropertyChanged();
		}
	}

	TimeSpan stepTime = TimeSpan.FromMilliseconds(150);

	#endregion

	#region Private implementation

	void Pause() {
		if (!this.IsRunning)
			throw new InvalidOperationException();

		this.PauseInternal();

		this.sessionLog.Log(Telemetry.Actions.PauseCommand.Instance);
	}

	void PauseInternal() {
		this.IsRunning = false;
		CommandViewModel.Off(this.PauseCommand);
		CommandViewModel.On(this.PlayCommand, this.StepCommand, this.StopCommand);

		this.sessionLog.LogPlayEnd(this.ProgramSteps);
	}

	void StopInternal() {
		this.isFinished = true;
		if (this.isRunning)
			this.sessionLog.LogPlayEnd(this.ProgramSteps);
		this.IsRunning = false;
		CommandViewModel.Off(this.PauseCommand, this.PlayCommand, this.StepCommand,
		                     this.StopCommand);

		this.Finished?.Invoke(this);
	}

	/// <summary>
	/// Resume execution from current state
	/// </summary>
	async void Start() {
		if (this.IsRunning || this.isFinished)
			throw new InvalidOperationException();

		this.IsRunning = true;

		this.sessionLog.LogPlayStart(this.ProgramSteps);

		CommandViewModel.Off(this.PlayCommand);
		CommandViewModel.On(this.PauseCommand, this.StepCommand, this.StopCommand);

		while (this.IsRunning) {
			await Task.Delay(this.stepTime);

			if (this.IsRunning)
				this.Step();
		}
	}

	/// <summary>
	/// Performs single program step
	/// </summary>
	void Step() {
		// TODO: check, that the operation is valid for the current state
		this.executor.Step();

		this.OnPropertyChanged(nameof(this.ProgramSteps));
		this.OnPropertyChanged(nameof(this.RobotMoves));

		this.Stepped?.Invoke(this);

		if (this.executor.IsTerminated())
			this.StopInternal();
	}

	void ExecutorOnCellChanged(object sender, PositionedEventArgs args) {
		var viewModel = this.PuzzleState[args.X, args.Y];
		var model = this.executor.PuzzleState.Cells[args.X][args.Y];
		viewModel.Color = model.Color;
		viewModel.Star = model.Star;
	}

	void ExecutorOnRobotPositionChanged(object sender, EventArgs<MovementKind> movement) {
		var viewModel = this.PuzzleState.Robot;
		var model = this.executor.PuzzleState.Robot;
		viewModel.Direction = (int)model.Direction;
		viewModel.Position.X = model.X;
		viewModel.Position.Y = model.Y;
		viewModel.OnMovement(movement.Argument);
	}

	void ExecutorOnProgramStateChanged(object sender, EventArgs eventArgs) {
		this.ProgramState.NextCommand = this.GetNextCommand();
	}

	Command? GetNextCommand() {
		// TODO: walk through rets
		var instruction = this.executor.ProgramState.CurrentInstruction;
		if (instruction == null)
			return null;

		if (instruction.Function >= this.ProgramState.Program.Functions.Length)
			return null;
		var function = this.ProgramState.Program.Functions[instruction.Function];

		int command = instruction.Command;
		// skip empty commands
		while (command < function.Commands.Length) {
			if (function.Commands[command] == null)
				command++;
			else
				return function.Commands[command];
		}

		return null;
	}

	#endregion
}