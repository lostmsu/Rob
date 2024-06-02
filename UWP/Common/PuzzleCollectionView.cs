namespace RoboZZle.WinRT.Common;

using RoboZZle.WinRT.Imported;

sealed class PuzzleCollectionView {
	public PuzzleCollectionView(PuzzleListViewModel puzzleList, FilterViewModel filter) {
		if (puzzleList == null)
			throw new ArgumentNullException(nameof(puzzleList));

		this.View = new ListCollectionView(puzzleList.Puzzles) {
			Filter = filter.Filter,
			SortDescriptions = {
				new SortDescription("HasDifficulty", ListSortDirection.Descending),
				new SortDescription("Difficulty", ListSortDirection.Ascending),
			},
		};

		filter.PropertyChanged += this.PuzzleListOnPropertyChanged;
	}

	void PuzzleListOnPropertyChanged(object sender,
	                                 PropertyChangedEventArgs propertyChangedEventArgs) {
		if (propertyChangedEventArgs.PropertyName == nameof(FilterViewModel.Filter)) {
			var filter = (FilterViewModel)sender;
			this.View.Filter = filter.Filter;
		}
	}

	public ListCollectionView View { get; }
}