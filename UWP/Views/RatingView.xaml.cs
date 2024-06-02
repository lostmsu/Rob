namespace RoboZZle.WinRT.Views;

using System.Collections.ObjectModel;

using Windows.UI.Xaml.Controls;

public sealed partial class RatingView: UserControl {
	public RatingView() {
		this.InitializeComponent();
	}

	public ObservableCollection<object> RatingObjects { get; }
		= new(Enumerable.Range(0, 5).Cast<object>());

	#region Rating property implementation

	public static DependencyProperty RatingProperty { get; } = DependencyProperty.Register(
		nameof(Rating), propertyType: typeof(int), ownerType: typeof(RatingView),
		// NaN is a dirty trick to have initial command value invalid
		typeMetadata: new PropertyMetadata(5, OnRatingChanged));

	public int Rating {
		get => (int)this.GetValue(RatingProperty);
		set {
			if (value is < 0 or > 5)
				throw new ArgumentOutOfRangeException(nameof(value));

			this.SetValue(RatingProperty, value);
		}
	}

	static void OnRatingChanged(DependencyObject dependencyObject,
	                            DependencyPropertyChangedEventArgs
		                            dependencyPropertyChangedEventArgs) {
		((RatingView)dependencyObject).OnRatingChanged();
	}

	void OnRatingChanged() {
		while (this.Rating < this.RatingObjects.Count) {
			this.RatingObjects.RemoveAt(this.RatingObjects.Count - 1);
		}

		while (this.Rating > this.RatingObjects.Count) {
			this.RatingObjects.Add(new object());
		}
	}

	#endregion
}