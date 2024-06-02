namespace RoboZZle.WinRT.Views;

using System;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

public sealed partial class ProgramView: UserControl {
	public ProgramView() {
		this.InitializeComponent();

		this.CommandList.SelectionChanged += this.OnSelectionChanged;
	}

	#region Command property

	public static DependencyProperty CurrentCommandProperty { get; } =
		DependencyProperty.Register(
			nameof(CurrentCommand), propertyType: typeof(int), ownerType: typeof(ProgramView),
			typeMetadata: new PropertyMetadata(defaultValue: -1, OnCommandIndexChanged));

	public int CurrentCommand {
		get => (int)this.GetValue(CurrentCommandProperty);
		set => this.SetValue(CurrentCommandProperty, value);
	}

	static void OnCommandIndexChanged(DependencyObject dependencyObject,
	                                  DependencyPropertyChangedEventArgs
		                                  dependencyPropertyChangedEventArgs) {
		((ProgramView)dependencyObject).OnCommandIndexChanged(
			(int)dependencyPropertyChangedEventArgs.NewValue);
	}

	void OnCommandIndexChanged(int newIndex) {
		this.CommandList.SelectedIndex = newIndex;
		this.CommandChanged?.Invoke(this, EventArgs.Empty);
		if (newIndex < 0)
			return;
		this.CommandList.GotFocus -= this.OnCommandGotFocus;
		try {
			this.SetFocus(FocusState.Programmatic, newIndex);
		} finally {
			this.CommandList.GotFocus += this.OnCommandGotFocus;
		}
	}

	void OnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
		this.CurrentCommand = this.CommandList.SelectedIndex;

	public void WithSelectionLock(Action action) {
		this.CommandList.SelectionChanged -= this.OnSelectionChanged;
		try {
			action();
		} finally {
			this.CommandList.SelectionChanged += this.OnSelectionChanged;
		}
	}

	public event EventHandler? CommandChanged;

	#endregion

	void OnCommandGotFocus(object sender, RoutedEventArgs e) {
		if (e.OriginalSource is ListViewItem command &&
		    command.FocusState == FocusState.Keyboard) {
			command.IsSelected = true;
		}
	}

	public bool SetFocus(FocusState focusState, int? item = null) {
		if (this.CommandList.Items.Count == 0)
			return false;

		int index = Math.Max(0, item.GetValueOrDefault(0));
		this.CommandList.UpdateLayout();
		if (this.CommandList.ContainerFromIndex(index) is ListViewItem command)
			return command.Focus(focusState);

		return false;
	}

	public new event KeyEventHandler PreviewKeyDown {
		add => this.CommandList.KeyDown += value;
		remove => this.CommandList.KeyDown -= value;
	}
}