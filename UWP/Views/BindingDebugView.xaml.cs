namespace RoboZZle.WinRT.Views;

using Windows.UI.Xaml.Controls;

public sealed partial class BindingDebugView: UserControl {
	public BindingDebugView() {
		this.InitializeComponent();
	}

	public static DependencyProperty DebugProperty { get; } = DependencyProperty.Register(
		name: nameof(Debug), propertyType: typeof(object), ownerType: typeof(BindingDebugView),
		typeMetadata: new PropertyMetadata(defaultValue: null, DebugChangedCallback));

	static void DebugChangedCallback(DependencyObject dependencyObject,
	                                 DependencyPropertyChangedEventArgs
		                                 dependencyPropertyChangedEventArgs) {
		GC.KeepAlive(Application.Current);
	}

	public object? Debug {
		get => this.GetValue(DebugProperty);
		set => this.SetValue(DebugProperty, value);
	}
}