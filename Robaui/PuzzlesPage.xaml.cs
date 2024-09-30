namespace Robaui;

using RoboZZle.WinRT.Imported;

public partial class PuzzlesPage: ContentPage {
	public PuzzlesPage() {
		this.InitializeComponent();
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs args) {
		base.OnNavigatedTo(args);

		var robozzle = (RoboZZleApp)Application.Current!;
		this.CampaignCollection.BindingContext = robozzle.Campaign;
		this.AllCollection.BindingContext = robozzle.PuzzleCollectionView;
		this.PuzzlesView.ItemsSource = robozzle.PuzzleCollectionView;
	}

	void OnCounterClicked(object sender, EventArgs e) {
		SemanticScreenReader.Announce("Hi");
	}

	//void OnPuzzleClick(object sender, ItemClickEventArgs puzzleArgs) {
	//	var puzzle = (PuzzleViewModel)puzzleArgs.ClickedItem;
	//	RoboZZleRt.Navigate(this, typeof(ProgramEditor), puzzle);
	//}

	void PuzzleCollectionChecked(object sender, CheckedChangedEventArgs e) {
		if (this.PuzzlesView == null)
			return;

		//this.LoadingOverlay.ShowAndAutoHide();

		var collection = (ListCollectionView)((RadioButton)sender).BindingContext;
		this.PuzzlesView.ItemsSource = collection;
		if (collection.Count > 0) {
			this.PuzzlesView.ScrollTo(collection[0], ScrollToPosition.Start, animated: true);
			this.PuzzlesView.Focus();
		}
	}

	ApplicationViewModel App => (ApplicationViewModel)this.BindingContext;
	void OnQuerySubmitted(object sender, TextChangedEventArgs args) {
		this.App.Filter.TitlePart = args.NewTextValue;
	}

	void ShowLoadingOverlay(object sender, EventArgs e) {
		//this.LoadingOverlay.ShowAndAutoHide();
	}

	void ShowSettings_Click(object sender, EventArgs e) {
		//this.ShowSettings();
	}
}

