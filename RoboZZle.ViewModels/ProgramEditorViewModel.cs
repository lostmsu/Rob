namespace RoboZZle.ViewModels;

using System.Threading.Tasks;
using System.Windows.Input;

using RoboZZle.Telemetry.Actions;

using RobotAction = RoboZZle.Core.Action;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Newtonsoft.Json;

/// <summary>
/// View-model of program editor
/// </summary>
public class ProgramEditorViewModel: ViewModelBase {
	readonly PuzzleViewModel puzzle;
	SessionLog sessionLog;
	SessionLogWriter sessionLogWriter;
	readonly IProgramHistory history;
	readonly IPuzzleTelemetry puzzleTelemetry;

	readonly CommandViewModel editPlay;
	readonly CommandViewModel editStep;
	readonly CommandViewModel closeVictoryScreen;

	public CommandViewModel NextCommand { get; }
	public CommandViewModel PrevCommand { get; }

	PuzzleStateViewModel puzzleState;
	ExecutionViewModel? execution;

	Program sessionStartProgram;
	Program? lastSolution;
	bool victoryScreen;

	int programLength;
	int currentCommandIndex;

	/// <summary>
	/// Creates instance of <see cref="ProgramEditorViewModel"/>
	/// </summary>
	public ProgramEditorViewModel(PuzzleViewModel puzzle, IProgramHistory history,
	                              IPuzzleTelemetry puzzleTelemetry) {
		this.puzzle = puzzle ?? throw new ArgumentNullException(nameof(puzzle));
		this.history = history ?? throw new ArgumentNullException(nameof(history));
		this.puzzleTelemetry =
			puzzleTelemetry ?? throw new ArgumentNullException(nameof(puzzleTelemetry));

		this.Program = new ProgramViewModel(history.CurrentProgram);
		this.ProgramLength = this.Program.Program.GetLength();

		this.puzzleState = new PuzzleStateViewModel(puzzle.Puzzle.InitialState);

		this.editPlay = new CommandViewModel(this.Start) { IsEnabled = true };
		this.editStep = new CommandViewModel(this.Step) { IsEnabled = true };
		this.closeVictoryScreen = new CommandViewModel(this.CloseVictoryScreen)
			{ IsEnabled = true };


		this.UndoCommand = new CommandViewModel(this.Undo);
		this.RedoCommand = new CommandViewModel(this.Redo);

		this.NextCommand = new CommandViewModel(this.MoveToNextCommand)
			{ IsEnabled = this.Program.Program.TotalSlots > 1 };
		this.PrevCommand = new CommandViewModel(this.MoveToPrevCommand) { IsEnabled = false };

		this.UpdateHistoryCommands();
		this.StartNewTelemetrySession();
	}

	public ApplicationViewModel? App { get; set; }

	/// <summary>
	/// Returns puzzle, which is being edited
	/// </summary>
	public Puzzle Puzzle => this.puzzle.Puzzle;
	/// <summary>
	/// Gets puzzle, which is being edited
	/// </summary>
	public PuzzleViewModel PuzzleViewModel => this.puzzle;

	#region Current Command

	/// <summary>
	/// Current command: selected command for editing mode, currently executing command for executing modes
	/// </summary>
	public int CurrentCommandIndex {
		get => this.IsExecuting
			? this.Execution.CurrentInstruction.ToGlobal(this.Program.Program)
			: this.currentCommandIndex;
		set {
			this.currentCommandIndex = value;
			this.UpdateMoveCommandStates();
			if (!this.IsExecuting) {
				this.OnPropertyChanged();
				this.OnPropertyChanged(nameof(this.CanChangeCurrentCommand));
			}
		}
	}

	void UpdateMoveCommandStates() {
		this.NextCommand.IsEnabled = this.CanMoveToNextCommand;
		this.PrevCommand.IsEnabled = this.CanMoveToPrevCommand;
	}

	bool CanMoveToNextCommand => !this.IsExecuting;
	bool CanMoveToPrevCommand => !this.IsExecuting && this.currentCommandIndex > 0;

	public void MoveToNextCommand() {
		if (this.IsExecuting)
			throw new InvalidOperationException();
		if (!this.CanMoveToNextCommand)
			return;

		if (this.currentCommandIndex < 0)
			this.CurrentCommandIndex = 0;
		else {
			var currentIndex =
				InstructionPointer.FromGlobal(this.Program.Program, this.currentCommandIndex);
			this.CurrentCommandIndex =
				currentIndex.Next(this.Program.Program).ToGlobal(this.Program.Program);
		}
	}

	public void MoveToPrevCommand() {
		if (this.IsExecuting)
			throw new InvalidOperationException();
		if (this.CanMoveToPrevCommand)
			this.CurrentCommandIndex--;
	}

	/// <summary>
	/// Replaces currently selected command with new one
	/// </summary>
	/// <param name="newCommand">New command</param>
	public void ChangeCurrentCommand(Command? newCommand) {
		if (this.IsExecuting)
			throw new InvalidOperationException("Can't change current command in executing mode");
		if (this.currentCommandIndex < 0)
			throw new InvalidOperationException(
				"Can't change current command - no command is selected");

		var currentIndex =
			InstructionPointer.FromGlobal(this.Program.Program, this.currentCommandIndex);
		if (newCommand?.Action == null)
			newCommand = null;

		this.sessionLogWriter.Log(new EditCommand {
			CommandOffset = currentIndex.Command,
			Function = currentIndex.Function,
			NewCommand = newCommand,
			OldCommand = Command.Clone(this.Program[currentIndex.Function, currentIndex.Command]),
		});

		this.Program[currentIndex.Function, currentIndex.Command] = Command.Clone(newCommand);
	}

	public void ChangeCurrentCommandCondition(Color? condition)
		=> this.UpdateCurrentCommand(command => command.With(condition));

	public void ChangeCurrentCommandAction(RobotAction? action)
		=> this.UpdateCurrentCommand(command => command.With(action));

	void UpdateCurrentCommand(Func<Command, Command?> update) {
		if (update == null)
			throw new ArgumentNullException(nameof(update));
		if (this.IsExecuting)
			throw new InvalidOperationException("Can't change current command in executing mode");
		if (this.currentCommandIndex < 0)
			throw new InvalidOperationException(
				"Can't change current command - no command is selected");

		var newCommand = update(this.CurrentCommand ?? new Command());
		this.ChangeCurrentCommand(newCommand);
	}

	public Command? CurrentCommand {
		get {
			if (this.currentCommandIndex < 0)
				throw new InvalidOperationException(
					"Can't get current command - no command is selected");

			var currentIndex =
				InstructionPointer.FromGlobal(this.Program.Program, this.currentCommandIndex);
			return Command.Clone(this.Program[currentIndex.Function, currentIndex.Command]);
		}
	}

	#endregion Current Command

	#region Commands

	/// <summary>
	/// Starts or continues program execution
	/// </summary>
	public ICommand PlayCommand => this.IsExecuting ? this.Execution.PlayCommand
		: this.IsEditing ? this.editPlay
		: CommandViewModel.Disabled;

	/// <summary>
	/// Performs one program step
	/// </summary>
	public ICommand StepCommand => this.IsExecuting ? this.Execution.StepCommand
		: this.IsEditing ? this.editStep
		: CommandViewModel.Disabled;

	/// <summary>
	/// Pauses program execution
	/// </summary>
	public ICommand PauseCommand =>
		this.IsExecuting ? this.Execution.PauseCommand : CommandViewModel.Disabled;

	/// <summary>
	/// Terminates program execution and transitions to editing mode
	/// </summary>
	public ICommand StopCommand => this.IsExecuting ? this.Execution.StopCommand
		: this.IsViewingStatistics ? this.closeVictoryScreen
		: CommandViewModel.Disabled;

	/// <summary>
	/// Undoes the last edit operation on program
	/// </summary>
	public CommandViewModel UndoCommand { get; }
	/// <summary>
	/// Redoes the last edit operation, which was undone
	/// </summary>
	public CommandViewModel RedoCommand { get; }

	#endregion

	/// <summary>
	/// Gets current puzzle state
	/// </summary>
	public PuzzleStateViewModel PuzzleState {
		get => this.puzzleState;
		set {
			this.puzzleState = value;
			this.OnPropertyChanged();
		}
	}

	#region Session

	DateTime? sessionStart;

	/// <summary>
	/// Indicates start of current puzzle solving session
	/// </summary>
	public void StartSession() {
		if (this.sessionStart != null)
			throw new InvalidOperationException("Session has already been started");

		this.sessionStart = GetTime();
		this.OnPropertyChanged(nameof(this.TimeSpent));
	}

	/// <summary>
	/// Updates time spent on current puzzle
	/// </summary>
	public void SessionTick() {
		if (this.sessionStart == null)
			throw new InvalidOperationException("Session was not strated");

		this.puzzleTelemetry.SolutionInProgress.QueueUpdate(this.sessionLog);

		this.puzzle.Local.Task.Result.TimeSpent = this.TimeSpent;
		this.sessionStart = GetTime();

		this.OnPropertyChanged(nameof(this.TimeSpent));
	}

	static DateTime GetTime() {
		return Truncate(DateTime.UtcNow);
	}

	static DateTime Truncate(DateTime date) {
		return new DateTime(date.Ticks - (date.Ticks % TimeSpan.TicksPerSecond), date.Kind);
	}

	TimeSpan CurrentSessionTime => (GetTime() - this.sessionStart).GetValueOrDefault();

	public TimeSpan TimeSpent {
		get {
			TimeSpan? previousRecorded = this.puzzle.Local.Task.Result.TimeSpent;
			return previousRecorded.HasValue
				? previousRecorded.Value + this.CurrentSessionTime
				: this.CurrentSessionTime;
		}
	}

	#endregion

	/// <summary>
	/// Gets current program length
	/// </summary>
	public int ProgramLength {
		get => this.programLength;
		private set {
			this.programLength = value;
			this.OnPropertyChanged();
		}
	}

	/// <summary>
	/// Adds current version to version history
	/// </summary>
	public void Checkpoint() {
		this.history.Add(this.Program.Program);

		this.ProgramLength = this.Program.Program.GetLength();

		this.UpdateHistoryCommands();

		this.puzzleTelemetry.SolutionInProgress.QueueUpdate(this.sessionLog);
	}

	/// <summary>
	/// Gets current program
	/// </summary>
	public ProgramViewModel Program { get; private set; }

	/// <summary>
	/// Occurs, when all stars are collected.
	/// </summary>
	public event EventHandler? Victory;

	/// <summary>
	/// Occurs when a warning is generated from the editor
	/// </summary>
	public event EventHandler<Exception>? Warning;

	/// <summary>
	/// Checks if current mode is editing mode
	/// </summary>
	public bool IsEditing => !this.victoryScreen && this.Execution == null;

	/// <summary>
	/// Checks if current mode is executing mode
	/// </summary>
	[MemberNotNullWhen(true, nameof(Execution))]
	public bool IsExecuting => !this.victoryScreen && this.Execution != null;

	/// <summary>
	/// Checks if it is possible to change current command
	/// </summary>
	public bool CanChangeCurrentCommand => this.IsEditing && this.currentCommandIndex >= 0;

	/// <summary>
	/// Checks if current mode is winner results mode
	/// </summary>
	public bool IsViewingStatistics => this.victoryScreen;

	public ExecutionViewModel? Execution {
		get => this.execution;
		private set {
			this.execution = value;
			this.OnPropertyChanged();
		}
	}

	/// <summary>
	/// Submits last correct solution to service. Requires App property to be set.
	/// </summary>
	public async Task SubmitLastSolution() {
		if (this.lastSolution == null)
			throw new InvalidOperationException("No solution is known");

		if (this.App == null)
			throw new InvalidOperationException("You must assign App property to use this method");

		await this.App.SubmitSolution(this.puzzle.ID, this.lastSolution);
	}

	#region Private implementation

	void Start() {
		this.ExecutionMode();

		if (this.Execution!.PlayCommand.IsEnabled) {
			this.Execution.PlayCommand.Execute(null);
		}
	}

	void Step() {
		this.ExecutionMode();

		if (this.Execution!.StepCommand.IsEnabled) {
			this.Execution.StepCommand.Execute(null);
		}
	}

	void CloseVictoryScreen() {
		this.Execution = null;
		this.victoryScreen = false;
		this.ModeChanged();
	}

	void Undo() {
		this.sessionLogWriter.Log(Telemetry.Actions.UndoCommand.Instance);

		var previousProgram = this.history.Undo();
		this.Program.Load(previousProgram);

		this.UpdateHistoryCommands();
	}

	void Redo() {
		this.sessionLogWriter.Log(Telemetry.Actions.RedoCommand.Instance);

		var nextProgram = this.history.Redo();
		this.Program.Load(nextProgram);

		this.UpdateHistoryCommands();
	}

	[MemberNotNull(nameof(Execution))]
	void ExecutionMode() {
		this.Execution =
			new ExecutionViewModel(this.Program, this.puzzle.Puzzle, this.sessionLogWriter);
		this.PuzzleState = this.Execution.PuzzleState;
		this.Program = this.Execution.ProgramState;
		this.ModeChanged();

		this.Execution.Finished += this.ExecutionFinished;
		this.Execution.Stepped += this.ExecutionStepped;
	}

	void ExecutionStepped(ExecutionViewModel _) {
		this.OnPropertyChanged(nameof(this.CurrentCommandIndex));
	}

	async void ExecutionFinished(ExecutionViewModel sender) {
		// TODO make variable interval
		await Task.Delay(2000);

		sender.Finished -= this.ExecutionFinished;
		sender.Stepped -= this.ExecutionStepped;

		this.ProgramLength = this.Program.Program.GetLength();

		if (sender.IsVictory) {
			await this.OnVictory();
		} else {
			this.Execution = null;
		}

		this.PuzzleState = new PuzzleStateViewModel(this.Puzzle.InitialState);
		this.Program = new ProgramViewModel(this.Program.Program);

		this.ModeChanged();
	}

	async Task OnVictory() {
		this.lastSolution = this.Program.Program.Clone();
		if (this.App != null) {
			var solution = new LocalSolution {
				Program = this.Program.Program.Clone(),
				PuzzleID = this.puzzle.ID
			};
			await this.App.Add(solution);
		}

		string serializedSolution = this.lastSolution.Encode(collapseEmptyCommands: false);
		await this.puzzleTelemetry.Victory(serializedSolution);
		this.TestSessionValidity(this.lastSolution);

		this.StartNewTelemetrySession();

		this.Victory?.Invoke(this, EventArgs.Empty);

		this.victoryScreen = true;
		this.ModeChanged();

		if (this.App?.HasCredentials == true)
			await this.SubmitSolution(this.App);

		this.SessionTick();
	}

	async Task SubmitSolution(ApplicationViewModel app) {
		try {
			await this.SubmitLastSolution();
		} catch (Exception e) {
			this.Warning?.Invoke(this, e);
		}

		//if (this.App.RobAiTelemetryEnabled != true)
		//	return;

		try {
			var telemetry = new TelemetryBag {
				Solutions = [
					new SolutionTelemetry {
						PuzzleID = this.puzzle.ID,
						Sessions = { this.sessionLog },
						Source = new TelemetrySource {
							Product = app.AppName,
							Version = app.AppVersion,
							IsTest = app.TestMode,
						},
						StartingProgram =
							this.sessionStartProgram.Encode(collapseEmptyCommands: false),
					}
				],
			};
			var client = new HttpClient {
				DefaultRequestHeaders = {
					Authorization = new AuthenticationHeaderValue(
						AuthenticationSchemes.Basic.ToString(),
						Convert.ToBase64String(
							Encoding.UTF8.GetBytes(
								$"{app.UserName}:{app.PasswordHash}"))),
				}
			};
			var content = new StringContent(JsonConvert.SerializeObject(telemetry), Encoding.UTF8,
			                                "application/json");
			var postResult = await client.PostAsync(
				"https://robtelemetry.azurewebsites.net/api/Telemetry",
				//"http://localhost:44617/api/Telemetry",
				content);
			if (postResult.IsSuccessStatusCode)
				DebugEx.WriteLine("successfully posted solution telemetry");
			else
				postResult.EnsureSuccessStatusCode();
		} catch (Exception e) {
			DebugEx.WriteLine($"failed to post solution telemetry: {e}");
		}
	}

	[Conditional("DEBUG")]
	private void TestSessionValidity(Program solution) {
		string actualProgram = this.sessionLog.Replay(this.sessionStartProgram)
		                           .Encode(collapseEmptyCommands: false);
		string expectedProgram = solution.Encode(collapseEmptyCommands: false);
		DebugEx.WriteLine(actualProgram == expectedProgram
			                  ? $"telemetry program match: {actualProgram}"
			                  : $"telemetry program expected: {expectedProgram}; actual: {actualProgram}");
	}

	[MemberNotNull(nameof(sessionLog), nameof(sessionLogWriter), nameof(sessionStartProgram))]
	void StartNewTelemetrySession() {
		this.sessionLog = new SessionLog { PuzzleID = this.puzzle.ID };
		this.sessionLogWriter = new SessionLogWriter(this.sessionLog);
		this.sessionStartProgram = this.history.CurrentProgram;
	}

	void ModeChanged() {
		this.OnPropertyChanged(nameof(this.PlayCommand));
		this.OnPropertyChanged(nameof(this.StepCommand));
		this.OnPropertyChanged(nameof(this.PauseCommand));
		this.OnPropertyChanged(nameof(this.StopCommand));
		this.OnPropertyChanged(nameof(this.CurrentCommandIndex));
		this.OnPropertyChanged(nameof(this.CanChangeCurrentCommand));
		this.OnPropertyChanged(nameof(this.Program));
		this.OnPropertyChanged(nameof(this.IsEditing));
		this.OnPropertyChanged(nameof(this.IsExecuting));
		this.OnPropertyChanged(nameof(this.IsViewingStatistics));

		this.UpdateMoveCommandStates();

		this.UpdateHistoryCommands();
	}

	void UpdateHistoryCommands() {
		this.UndoCommand.IsEnabled = this.IsEditing && this.history.CurrentVersion > 0;
		this.RedoCommand.IsEnabled =
			this.IsEditing && this.history.CurrentVersion < this.history.LatestVersion;
	}

	#endregion
}