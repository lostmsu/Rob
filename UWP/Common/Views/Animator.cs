namespace RoboZZle.WinRT.Common.Views;

using Windows.UI.Xaml.Media.Animation;

public sealed class Animator: DependencyObject {
	public Storyboard? Storyboard { get; set; }

	#region Source

	public object Source {
		get => this.GetValue(SourceProperty);
		set => this.SetValue(SourceProperty, value);
	}

	public static DependencyProperty SourceProperty { get; } = DependencyProperty.Register(
		nameof(Source), typeof(object), typeof(Animator),
		new PropertyMetadata(null, SourceValueChanged));

	void OnSourceValueChanged(DependencyPropertyChangedEventArgs args) {
		if (this.Storyboard == null)
			return;

		foreach (var timeline in this.Storyboard.Children.Cast<DoubleAnimation>()) {
			timeline.From = (double?)args.OldValue;
			timeline.To = (double?)args.NewValue;
		}

		this.Storyboard.Begin();
		if (!this.Animate)
			this.Storyboard.SkipToFill();
	}

	static void SourceValueChanged(DependencyObject animatorObject,
	                               DependencyPropertyChangedEventArgs args) {
		((Animator)animatorObject).OnSourceValueChanged(args);
	}

	#endregion

	#region Animate

	public bool Animate {
		get => (bool)this.GetValue(AnimateProperty);
		set => this.SetValue(AnimateProperty, value);
	}

	public static DependencyProperty AnimateProperty { get; } = DependencyProperty.Register(
		nameof(Animate), typeof(bool), typeof(Animator),
		new PropertyMetadata(true, AnimateChanged));

	void OnAnimateChanged(DependencyPropertyChangedEventArgs args) {
		if (args.NewValue == args.OldValue)
			return;

		if (false.Equals(args.NewValue))
			this.Storyboard?.SkipToFill();
	}

	static void AnimateChanged(DependencyObject animatorObject,
	                           DependencyPropertyChangedEventArgs args) {
		((Animator)animatorObject).OnAnimateChanged(args);
	}

	#endregion
}