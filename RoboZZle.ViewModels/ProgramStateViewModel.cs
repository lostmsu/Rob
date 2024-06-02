namespace RoboZZle.ViewModels;

/// <summary>
/// View model for program execution state
/// </summary>
public class ProgramStateViewModel: ProgramViewModel {
	[Obsolete]
	public ProgramStateViewModel() { }

	/// <summary>
	/// Creates a view model for a specified program in its initial state
	/// </summary>
	/// <param name="program">Model program</param>
	public ProgramStateViewModel(Program program): base(program) { }

	Command? nextCommand;
	/// <summary>
	/// Command to be executed next
	/// </summary>
	public Command? NextCommand {
		get => this.nextCommand;
		internal set {
			this.nextCommand = value;
			this.OnPropertyChanged();
		}
	}
}