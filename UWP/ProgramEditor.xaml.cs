namespace RoboZZle.WinRT;

using System.Diagnostics;

using Windows.Storage;
using Windows.System;

using RoboZZle.WinRT.Common;

using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using PCLStorage;

using System.Windows.Input;

using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;

using static System.FormattableString;

using static Windows.System.VirtualKey;

using static RoboZZle.Core.Color;

using Windows.ApplicationModel.UserActivities;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Navigation;

using RoboZZle.Offline;

/// <summary>
/// A basic page that provides characteristics common to most applications.
/// </summary>
public sealed partial class ProgramEditor: LayoutAwarePage {
	public ProgramEditor() {
		this.InitializeComponent();

		this.ContentGrid.Visibility = Visibility.Collapsed;
		this.PlayEditControls.Visibility = Visibility.Collapsed;
		this.TutorialPopup.Visibility = Visibility.Collapsed;
		this.TutorialToggle.Click += this.ToggleTutorial;
	}

	void ToggleTutorial(object sender, RoutedEventArgs args) {
		this.TutorialPopup.Visibility = this.TutorialPopup.Visibility.Negate();
		if (this.TutorialPopup.Visibility == Visibility.Visible)
			this.TutorialPopupContent.SetFocus(FocusState.Keyboard);
	}

	#region Load/Save state

	/// <summary>
	/// Populates the page with content passed during navigation.  Any saved state is also
	/// provided when recreating a page from a prior session.
	/// </summary>
	/// <param name="navigationParameter">The parameter value passed to
	/// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested.
	/// </param>
	/// <param name="pageState">A dictionary of state preserved by this page during an earlier
	/// session.  This will be null the first time a page is visited.</param>
	protected override async void LoadState(object navigationParameter,
	                                        Dictionary<string, object> pageState) {
		if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") {
			ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
		}

		var puzzle = (PuzzleViewModel)navigationParameter;
		IFolder solutionsFolder = await RoboZZleRt.GetLocalSolutionsFolder();
		var history = await ProgramHistory.Get(solutionsFolder, puzzle.Puzzle);
		IFolder telemetryFolder =
			await RoboZZleRt.GetOrCreateFolder("Telemetry",
			                                   ApplicationData.Current.TemporaryFolder);
		var telemetry = new TelemetryStorage(telemetryFolder);
		var puzzleTelemetry =
			await telemetry.GetOrStartPuzzleTelemetry(puzzle.ID,
			                                          history.CurrentProgram.Encode(false));
		this.DataContext = new ProgramEditorViewModel(puzzle, history, puzzleTelemetry);
		this.ViewModel.Warning += this.Warning;
		this.ViewModel.PropertyChanged += this.ViewModel_PropertyChanged;
		this.ViewModel.App = this.App;
		if (!this.App.SeenTutorial.GetValueOrDefault(false)) {
			this.App.SeenTutorial = true;
			this.TutorialPopup.Visibility = Visibility.Visible;
			this.TutorialPopupContent.SetFocus(FocusState.Keyboard);
		}

		this.ProgramView.CommandChanged += this.ProgramViewOnCommandChanged;
		this.CommandSetView.CommandChanged += this.CommandSetViewOnCommandChanged;
		Debug.Assert(!this.ViewModel.IsExecuting);

		this.Loading.Visibility = Visibility.Collapsed;
		this.ContentGrid.Visibility = this.PlayEditControls.Visibility = Visibility.Visible;
		if (this.TitleSeparator != null)
			this.TitleSeparator.Text = " - ";

		this.ViewModel.StartSession();
		this.ProgramView.SetFocus(FocusState.Keyboard);
		Window.Current.CoreWindow.KeyDown += this.CoreWindow_KeyDown;
		Window.Current.CoreWindow.KeyUp += this.CoreWindow_KeyUp;

		SystemNavigationManager.GetForCurrentView().BackRequested += this.OnSystemNavigationGoBack;

		if (ApiInformation.IsTypePresent(
			    "Windows.ApplicationModel.UserActivities.UserActivityChannel"))
			this.BeginUserActivityHistoryEntry();
	}

	void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
		switch (e.PropertyName) {
		case nameof(this.ViewModel.Execution):
			this.UpdateViewModelFastForward();
			break;
		}
	}

	async void Warning(object sender, Exception e) {
		// TODO: better error display
		var dialog = new MessageDialog(e.Message, "Error: " + e.GetType());
		await dialog.ShowAsync();
	}

	/// <summary>
	/// Preserves state associated with this page in case the application is suspended or the
	/// page is discarded from the navigation cache.  Values must conform to the serialization
	/// requirements of <see cref="SuspensionManager.SessionState"/>.
	/// </summary>
	/// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
	protected override void SaveState(Dictionary<string, object> pageState) {
		SystemNavigationManager.GetForCurrentView().BackRequested -= this.OnSystemNavigationGoBack;
		Window.Current.CoreWindow.KeyDown -= this.CoreWindow_KeyDown;
		Window.Current.CoreWindow.KeyUp -= this.CoreWindow_KeyUp;
		DebugEx.WriteLine("saveState for ProgramEditor");
#warning Ensure this is called on deactivating/minimizing
		this.ViewModel.SessionTick();
		this.ViewModel.Warning -= this.Warning;
		this.ViewModel.PropertyChanged -= this.ViewModel_PropertyChanged;
		if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") {
			ApplicationView.GetForCurrentView().ExitFullScreenMode();
		}
	}

	void OnSystemNavigationGoBack(object sender, BackRequestedEventArgs e) {
		this.GoBack(this, null);
		e.Handled = true;
	}

	protected override void OnNavigatedFrom(NavigationEventArgs e) {
		Debug.WriteLineIf(this.currentActivity != null, "ending activity session");
		this.currentActivity?.Dispose();

		base.OnNavigatedFrom(e);
	}

	#endregion

	#region Editing

	void ProgramViewOnCommandChanged(object sender, EventArgs eventArgs) {
		this.CommandSetView.CommandChanged -= this.CommandSetViewOnCommandChanged;
		this.CommandSetView.Command = this.ViewModel.Program[this.ProgramView.CurrentCommand];
		this.CommandSetView.CommandChanged += this.CommandSetViewOnCommandChanged;
	}

	void CommandSetViewOnCommandChanged(object sender, EventArgs eventArgs) {
		this.ProgramView.WithSelectionLock(() => {
			this.ViewModel.ChangeCurrentCommand(this.CommandSetView.Command);
			this.ViewModel.MoveToNextCommand();
		});

		this.ViewModel.Checkpoint();
	}

	#endregion

	public ProgramEditorViewModel ViewModel => (ProgramEditorViewModel)this.DataContext;

	void SelfOnKeyDown(object sender, KeyRoutedEventArgs e) {
		if (e.Handled)
			return;

		switch (e.Key) {
		case Escape:
			if (this.TutorialPopup.Visibility == Visibility.Visible) {
				this.TutorialPopup.Visibility = Visibility.Collapsed;
				e.Handled = true;
			} else if (this.ViewModel.IsExecuting) {
				e.Handled = ExecuteIfPossible(this.ViewModel.StopCommand);
			} else {
				this.GoBack(this, e);
				e.Handled = true;
			}

			break;

		case Space:
			e.Handled = ExecuteIfPossible(this.ViewModel.PlayCommand);
			break;
		}

		switch (e.OriginalKey) {
		case GamepadLeftShoulder:
		case GamepadRightShoulder:
			this.ToggleTutorial(this.ProgramView, new RoutedEventArgs());
			break;
		case GamepadView:
		case P:
			e.Handled = ExecuteIfPossible(this.ViewModel.PlayCommand) ||
			            ExecuteIfPossible(this.ViewModel.PauseCommand);
			break;
		case GamepadMenu:
		case S:
			if (ExecuteIfPossible(this.ViewModel.StopCommand)) {
				this.ProgramView.SetFocus(FocusState.Keyboard);
				e.Handled = true;
			}

			break;
		}
	}

	static bool ExecuteIfPossible(ICommand command) {
		if (command.CanExecute(null)) {
			command.Execute(null);
			return true;
		}

		return false;
	}

	void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args) {
		if (args.Handled && args.VirtualKey != GamepadA)
			// GamepadA has to be forcibly handled here because otherwise framework consumes it on Xbox
			return;
		var focusedItem = FocusManager.GetFocusedElement() as ListViewItem;
		if (focusedItem != null && !IsParent(this.ProgramView, focusedItem))
			return;

		args.Handled = true;
		switch (args.VirtualKey) {
		case GamepadA when this.ViewModel.CanChangeCurrentCommand && this.HasColor(GREEN)
		 && focusedItem != null && IsParent(this.ProgramView, focusedItem):
			int commandIndex = this.ViewModel.CurrentCommandIndex;
			// this resets selection
			this.ChangeCurrentCommandCondition(GREEN);
			// so it must be restored to original position
			this.ProgramView.SetFocus(FocusState.Keyboard, commandIndex);
			break;
		case GamepadX:
			this.UpdateViewModelFastForward();
			break;
		case GamepadY:
			args.Handled = ExecuteIfPossible(this.ViewModel.StepCommand);
			break;
		case GamepadLeftTrigger:
			break;
		case GamepadRightTrigger:
			break;
		default:
			args.Handled = false;
			break;
		}
	}

	void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args) {
		if (args.Handled)
			return;

		args.Handled = true;
		switch (args.VirtualKey) {
		case GamepadX:
			this.UpdateViewModelFastForward();
			break;
		default:
			args.Handled = false;
			break;
		}
	}

	static bool IsParent(DependencyObject parent, DependencyObject child) {
		DependencyObject actualParent = VisualTreeHelper.GetParent(child);
		if (actualParent == parent)
			return true;
		if (actualParent == null || actualParent == child)
			return false;
		return IsParent(parent, actualParent);
	}

	bool HasColor(Color color) => this.ViewModel.Puzzle.AvailableConditions.Contains(color);

	void ChangeCurrentCommandCondition(Color? condition) {
		this.ViewModel.ChangeCurrentCommandCondition(condition);
		this.ViewModel.Checkpoint();
	}

	void ChangeCurrentCommandAction(Core.Action? action) {
		this.ViewModel.ChangeCurrentCommandAction(action);
		this.ViewModel.Checkpoint();
	}

	void ProgramView_OnKeyDown(object sender, KeyRoutedEventArgs e) {
		if (e.Handled)
			return;

		if (!this.ViewModel.CanChangeCurrentCommand)
			return;

		int commandIndex = this.ViewModel.CurrentCommandIndex;

		bool SupportsCommand(CommandSet command) =>
			this.ViewModel.Puzzle.CommandSet.HasFlag(command);

		e.Handled = true;
		bool resetFocus = true;
		switch (e.OriginalKey) {
		case GamepadB when this.HasColor(RED):
		case R when this.HasColor(RED):
			this.ChangeCurrentCommandCondition(RED);
			break;
		case GamepadX when this.HasColor(BLUE):
		case B when this.HasColor(BLUE):
			this.ChangeCurrentCommandCondition(BLUE);
			break;
		case GamepadA when this.HasColor(GREEN):
		case G when this.HasColor(GREEN):
			this.ChangeCurrentCommandCondition(GREEN);
			break;
		case GamepadY:
		case Space:
		case N:
		case Back:
			this.ChangeCurrentCommandCondition(null);
			break;

		case GamepadRightThumbstickUp:
		case NumberPad8:
		case W:
			this.ChangeCurrentCommandAction(new Movement { Kind = MovementKind.MOVE });
			break;
		case GamepadRightThumbstickLeft:
		case NumberPad4:
		case A:
			this.ChangeCurrentCommandAction(new Movement { Kind = MovementKind.TURN_LEFT });
			break;
		case GamepadRightThumbstickRight:
		case NumberPad6:
		case D:
			this.ChangeCurrentCommandAction(new Movement { Kind = MovementKind.TURN_RIGHT });
			break;
		case GamepadRightThumbstickDown:
		case Delete:
			this.ChangeCurrentCommandAction(null);
			break;

		case GamepadDPadLeft when SupportsCommand(CommandSet.PAINT_BLUE):
			this.ChangeCurrentCommandAction(new Paint { Color = BLUE });
			break;
		case GamepadDPadDown when SupportsCommand(CommandSet.PAINT_GREEN):
			this.ChangeCurrentCommandAction(new Paint { Color = GREEN });
			break;
		case GamepadDPadRight when SupportsCommand(CommandSet.PAINT_RED):
			this.ChangeCurrentCommandAction(new Paint { Color = RED });
			break;


		case GamepadLeftTrigger:
		case HYPHEN:
		case OPEN_BRACKET:
			this.AddFunctionIndex(-1);
			break;
		case GamepadRightTrigger:
		case PLUS:
		case CLOSE_BRACKET:
			this.AddFunctionIndex(+1);
			break;

		case GamepadLeftThumbstickUp:
			this.Start.Focus(FocusState.Programmatic);
			resetFocus = false;
			break;

		default:
			e.Handled = false;
			resetFocus = false;
			break;
		}

		switch (e.OriginalKey) {
		case GamepadDPadLeft:
		case GamepadDPadRight:
		case GamepadDPadUp:
		case GamepadDPadDown:
		case GamepadB:
		case GamepadA:
		case GamepadX:
			e.Handled = true;
			break;
		}

		if (resetFocus)
			this.ProgramView.SetFocus(FocusState.Keyboard, commandIndex);
	}

	const VirtualKey HYPHEN = (VirtualKey)189;
	const VirtualKey PLUS = (VirtualKey)187;
	const VirtualKey OPEN_BRACKET = (VirtualKey)219;
	const VirtualKey CLOSE_BRACKET = (VirtualKey)221;

	void AddFunctionIndex(int delta) {
		var currentCommand = this.ViewModel.CurrentCommand ?? new Command { Action = null };
		int function = (currentCommand.Action as Call)?.Function ?? (delta > 0 ? -1 : 0);
		do {
			function += delta + this.ViewModel.Puzzle.SubLengths.Length;
			function %= this.ViewModel.Puzzle.SubLengths.Length;
		} while (this.ViewModel.Puzzle.SubLengths[function] == 0);

		this.ChangeCurrentCommandAction(new Call { Function = function });
	}

	#region Fast Forward

	int fastForward;
	readonly ISet<uint> capturedPointers = new SortedSet<uint>();

	void FastForward_PointerPressed(object sender, PointerRoutedEventArgs e) {
		if (this.capturedPointers.Add(e.Pointer.PointerId))
			this.fastForward++;
		((UIElement)sender).CapturePointer(e.Pointer);
		this.UpdateViewModelFastForward();
	}

	void FastForward_PointerReleased(object sender, PointerRoutedEventArgs e) {
		if (this.capturedPointers.Remove(e.Pointer.PointerId))
			this.fastForward--;
		((UIElement)sender).ReleasePointerCapture(e.Pointer);
		this.UpdateViewModelFastForward();
	}

	void FastForward_PointerCanceled(object sender, PointerRoutedEventArgs e) {
		if (this.capturedPointers.Remove(e.Pointer.PointerId))
			this.fastForward--;
		((UIElement)sender).CapturePointer(e.Pointer);
		this.UpdateViewModelFastForward();
	}

	void UpdateViewModelFastForward() {
		if (this.ViewModel.Execution != null) {
			bool keyPressed = Window.Current.CoreWindow.GetKeyState(GamepadX)
			                        .HasFlag(CoreVirtualKeyStates.Down);
			bool pointerPressed = this.fastForward > 0;
			this.ViewModel.Execution.StepTime =
				TimeSpan.FromMilliseconds(pointerPressed || keyPressed ? 50 : 150);
		}
	}

	#endregion

	void PlayEditControls_OnKeyDown(object sender, KeyRoutedEventArgs e) {
		if (e.Handled)
			return;

		var focusedButton = (AppBarButton)FocusManager.GetFocusedElement();
		AppBarButton[] enabledButtons = this.PlayEditControls.PrimaryCommands.OfType<AppBarButton>()
		                                    .Where(button => button.IsEnabled).ToArray();
		int focusIndex = Array.IndexOf(enabledButtons, focusedButton);

		e.Handled = true;

		switch (e.OriginalKey) {
		case GamepadLeftThumbstickLeft when focusIndex <= 0:
			this.BackButton.Focus(FocusState.Keyboard);
			break;
		case GamepadLeftThumbstickRight when focusIndex == enabledButtons.Length - 1:
			break;
		case GamepadLeftThumbstickLeft:
			focusIndex--;
			enabledButtons[focusIndex].Focus(FocusState.Keyboard);
			break;
		case GamepadLeftThumbstickRight:
			focusIndex++;
			enabledButtons[focusIndex].Focus(FocusState.Keyboard);
			break;
		default:
			e.Handled = false;
			break;
		}
	}

	void BackButton_OnKeyDown(object sender, KeyRoutedEventArgs e) {
		if (e.Handled)
			return;

		e.Handled = true;
		switch (e.OriginalKey) {
		case GamepadLeftThumbstickRight:
			var focus = this.PlayEditControls.PrimaryCommands.OfType<AppBarButton>()
			                .FirstOrDefault(button => button.IsEnabled);
			focus?.Focus(FocusState.Keyboard);
			break;
		default:
			e.Handled = false;
			break;
		}
	}

	UserActivitySession? currentActivity;

	async void BeginUserActivityHistoryEntry() {
		int puzzleID = this.ViewModel.Puzzle.ID;
		var channel = UserActivityChannel.GetDefault();
		var activity =
			await channel.GetOrCreateUserActivityAsync(Invariant($"puzzle?id={puzzleID}"));
		activity.VisualElements.DisplayText = this.ViewModel.Puzzle.Title;
		activity.ActivationUri = new Uri(Invariant($"rob-game://puzzle?id={puzzleID}"));
		await activity.SaveAsync();

		this.currentActivity?.Dispose();
		this.currentActivity = activity.CreateSession();
		Debug.WriteLine("Started activity session!");
	}
}