namespace RoboZZle.WinRT.Common;

public static class VisibilityExtensions {
	public static Visibility Negate(this Visibility visibility)
		=> visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
}