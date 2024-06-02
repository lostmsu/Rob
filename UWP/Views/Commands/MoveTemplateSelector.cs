namespace RoboZZle.WinRT.Views.Commands;

using Windows.UI.Xaml.Controls;

class MoveTemplateSelector: DataTemplateSelector {
	public DataTemplate? Forward { get; set; }
	public DataTemplate? TurnLeft { get; set; }
	public DataTemplate? TurnRight { get; set; }

	protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) {
		if (item is not Command command)
			return null;

		var movementKind = ((Movement)command.Action!).Kind;

		return movementKind switch {
			MovementKind.MOVE => this.Forward,
			MovementKind.TURN_LEFT => this.TurnLeft,
			MovementKind.TURN_RIGHT => this.TurnRight,
			_ => throw new NotImplementedException(movementKind.ToString()),
		};
	}
}