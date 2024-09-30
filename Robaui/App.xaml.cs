namespace Robaui;

using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using PCLStorage;

using Robolonia;

using RoboZZle.Offline;
using RoboZZle.WinRT.Common;
using RoboZZle.WinRT.Imported;

using FileSystem = Microsoft.Maui.Storage.FileSystem;

public partial class App: Application {
	public ApplicationViewModel? ViewModel { get; private set; }
	public ListCollectionView PuzzleCollectionView { get; private set; }
	public ListCollectionView Campaign { get; private set; }

	public App() {
		this.InitializeComponent();

		this.MainPage = new AppShell {
			BindingContext = this.ViewModel = CreateViewModel(),
		};

		this.BeginInitializeViewModel();
	}

	async void BeginInitializeViewModel() {
		this.PuzzleCollectionView =
			new PuzzleCollectionView(this.ViewModel!.Puzzles, this.ViewModel.Filter).View;
		this.Campaign = new PuzzleCollectionView(this.ViewModel.Campaign, this.ViewModel.Filter)
			.View;

		DebugEx.WriteLine("puzzle collection view initialized");

		this.ViewModel.Filter.PropertyChanged += this.OnFilterChanged;
		await this.ViewModel.Initialize();
		this.ViewModel.Warning += async (_, messageKey) => {
#warning await new MessageDialog(this.resources.GetString(messageKey)).ShowAsync();
			DebugEx.WriteLine(messageKey);
		};
	}

	static ApplicationViewModel CreateViewModel() {
		DebugEx.WriteLine("preparing App ViewModel");
		var cacheDir = new DirectoryInfo(FileSystem.CacheDirectory);
		IFolder levelCacheFolder = GetOrCreateFolder("LevelCache", cacheDir);
		IFolder localSolutionsFolder = GetLocalSolutionsFolder();
		var ver = Assembly.GetExecutingAssembly().GetName().Version;
		var preferences = new PreferenceDictionary(Preferences.Default);
		var vm = new ApplicationViewModel(preferences,
												  LocalSolutions.Open(localSolutionsFolder),
												  levelCacheFolder) {
			Sha1 = password => {
				byte[] bytes = Encoding.UTF8.GetBytes(password);
				return SHA1.HashData(bytes);
			},
			AppName = $"Robolonia-{ver}",
			AppVersion = ver.ToString(),
#warning TestMode is not implemented in Robolonia
			TestMode = true,
		};

		DebugEx.WriteLine("App ViewModel created");

		return vm;
	}

	public static IFolder GetLocalSolutionsFolder() =>
		GetOrCreateFolder("Solutions", new(FileSystem.AppDataDirectory));

	public static IFolder GetOrCreateFolder(string path, DirectoryInfo parent) {
		var systemFolder = parent.CreateSubdirectory(path);
		return new FileSystemFolder(systemFolder.FullName);
	}

	void OnFilterChanged(object? sender, PropertyChangedEventArgs propertyChangedEventArgs) {
		if (propertyChangedEventArgs.PropertyName != nameof(FilterViewModel.Filter))
			return;

		this.PuzzleCollectionView.Filter = this.ViewModel!.Filter.Filter;
		this.Campaign.Filter = this.ViewModel.Filter.Filter;
	}
}
