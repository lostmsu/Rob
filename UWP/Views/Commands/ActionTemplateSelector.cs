namespace RoboZZle.WinRT.Views.Commands;

using Windows.UI.Xaml.Controls;

class ActionTemplateSelector: DataTemplateSelector {
	public DataTemplate? MoveTemplate { get; set; }
	public DataTemplate? CallTemplate { get; set; }
	public DataTemplate? PaintTemplate { get; set; }
	public DataTemplate? NullTemplate { get; set; }

	protected override DataTemplate? SelectTemplateCore(object? item, DependencyObject container) {
		if (item == null)
			return this.NullTemplate;

		var action = ((Command)item).Action;

		return action switch {
			null => this.NullTemplate,
			Call => this.CallTemplate,
			Movement => this.MoveTemplate,
			Paint => this.PaintTemplate,
			_ => throw new NotSupportedException(),
		};
	}
}