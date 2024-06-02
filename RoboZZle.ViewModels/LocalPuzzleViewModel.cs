namespace RoboZZle.ViewModels;

public class LocalPuzzleViewModel: ViewModelBase {
	public LocalPuzzleViewModel(IDictionary<string, object?> localPuzzleData) {
		this.localPuzzleData = localPuzzleData
		                    ?? throw new ArgumentNullException(nameof(localPuzzleData));
	}

	readonly IDictionary<string, object?> localPuzzleData;

	public TimeSpan? TimeSpent {
		get {
			this.localPuzzleData.TryGetValue(nameof(this.TimeSpent), out object? timeSpent);
			return timeSpent as TimeSpan?;
		}
		set {
			this.localPuzzleData[nameof(this.TimeSpent)] = value;
			this.OnPropertyChanged();
		}
	}
}