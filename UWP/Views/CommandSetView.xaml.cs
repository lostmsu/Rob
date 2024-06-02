namespace RoboZZle.WinRT.Views;

using Windows.UI.Xaml.Controls;

public sealed partial class CommandSetView: UserControl {
	public CommandSetView() {
		this.InitializeComponent();
	}

	#region Command property implementation

	public static DependencyProperty CommandProperty { get; } = DependencyProperty.Register(
		nameof(Command), propertyType: typeof(Command), ownerType: typeof(CommandSetView),
		// NaN is a dirty trick to have initial command value invalid
		typeMetadata: new PropertyMetadata(double.NaN, OnCommandChanged));

	public Command? Command {
		get => (Command?)this.GetValue(CommandProperty);
		set => this.SetValue(CommandProperty, value);
	}

	static void OnCommandChanged(DependencyObject dependencyObject,
	                             DependencyPropertyChangedEventArgs
		                             dependencyPropertyChangedEventArgs) {
		((CommandSetView)dependencyObject).OnCommandChanged();
	}

	void OnCommandChanged() => this.CommandChanged?.Invoke(this, EventArgs.Empty);

	public event EventHandler? CommandChanged;

	#endregion

	void OnCommandClick(object sender, RoutedEventArgs e) {
		this.Command = (Command)((Button)sender).DataContext;
	}

	void OnClearClick(object sender, RoutedEventArgs e) {
		this.Command = null;
	}
}