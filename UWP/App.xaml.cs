namespace RoboZZle.WinRT;

using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Microsoft.HockeyApp;

using PCLStorage;

using RoboZZle.Core;
using RoboZZle.Offline;
using RoboZZle.ViewModels;
using RoboZZle.WinRT.Common;
using RoboZZle.WinRT.Imported;

using CreationCollisionOption = Windows.Storage.CreationCollisionOption;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
sealed partial class RoboZZleRt: Application {
	/// <summary>
	/// Initializes the singleton application object.  This is the first line of authored code
	/// executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public RoboZZleRt() {
		this.InitializeComponent();
		HockeyClient.Current.Configure("ab1a3e164bb64e4cb6a52c3c121e8153");
		this.Suspending += this.OnSuspending;
		this.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
		this.RequestedTheme = this.UseLightTheme ? ApplicationTheme.Light : ApplicationTheme.Dark;
	}

	public bool UseLightTheme {
		get {
			bool useLight =
				ApplicationData.Current.LocalSettings.Values.TryGetValue("Light", out object light)
			 && light is true;
			return useLight;
		}
		set => ApplicationData.Current.LocalSettings.Values["Light"] = value;
	}

	/// <summary>
	/// Invoked when the application is launched normally by the end user.  Other entry points
	/// will be used such as when the application is launched to open a specific file.
	/// </summary>
	/// <param name="e">Details about the launch request and process.</param>
	protected override async void OnLaunched(LaunchActivatedEventArgs e) {
		if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
			Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SetDesiredBoundsMode
				(Windows.UI.ViewManagement.ApplicationViewBoundsMode.UseCoreWindow);

		if (this.ViewModel == null) {
			await this.CreateViewModel();
			this.BeginInitializeViewModel();
		}

		Frame? rootFrame = Window.Current.Content as Frame;

		// Do not repeat app initialization when the Window already has content,
		// just ensure that the window is active
		if (rootFrame == null) {
			// Create a Frame to act as the navigation context and navigate to the first page
			rootFrame = new Frame();

			rootFrame.NavigationFailed += this.OnNavigationFailed;

			if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
				//TODO: Load state from previously suspended application
			}

			// Place the frame in the current Window
			Window.Current.Content = rootFrame;
		}

		if (e.PrelaunchActivated == false) {
			if (rootFrame.Content == null) {
				// When the navigation stack isn't restored navigate to the first page,
				// configuring the new page by passing required information as a navigation
				// parameter
				if (!rootFrame.Navigate(typeof(PuzzlesPage), e.Arguments))
					throw new Exception("Failed to create initial page");

				DebugEx.WriteLine("initial Navigate call succeeded");
			}

			// Ensure the current window is active
			Window.Current.Activate();
		}
	}

	readonly ResourceLoader resources = new();

	async void BeginInitializeViewModel() {
		this.PuzzleCollectionView =
			new PuzzleCollectionView(this.ViewModel.Puzzles, this.ViewModel.Filter).View;
		this.Campaign = new PuzzleCollectionView(this.ViewModel.Campaign, this.ViewModel.Filter)
			.View;

		DebugEx.WriteLine("puzzle collection view initialized");

		this.ViewModel.Filter.PropertyChanged += this.OnFilterChanged;
		await this.ViewModel.Initialize();
		this.ViewModel.Warning += async (_, messageKey) => {
			await new MessageDialog(this.resources.GetString(messageKey)).ShowAsync();
		};
	}

	async Task CreateViewModel() {
		DebugEx.WriteLine("preparing App ViewModel");

		var sha1 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
		IFolder levelCacheFolder =
			await GetOrCreateFolder("LevelCache", ApplicationData.Current.LocalCacheFolder);
		IFolder localSolutionsFolder = await GetLocalSolutionsFolder();
		var ver = Package.Current.Id.Version;
		this.ViewModel = new ApplicationViewModel(ApplicationData.Current.RoamingSettings.Values,
		                                          LocalSolutions.Open(localSolutionsFolder),
		                                          levelCacheFolder) {
			Sha1 = password => {
				var bytes =
					CryptographicBuffer.ConvertStringToBinary(password, BinaryStringEncoding.Utf8);
				var hashed = sha1.HashData(bytes);
				return hashed.ToArray();
			},
			AppName = $"{Package.Current.Id.Name}-{AnalyticsInfo.VersionInfo.DeviceFamily}",
			AppVersion =
				FormattableString.Invariant($"{ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}"),
			TestMode = Package.Current.IsDevelopmentMode,
		};

		DebugEx.WriteLine("App ViewModel created");
	}

	public static Task<IFolder> GetLocalSolutionsFolder() =>
		GetOrCreateFolder("Solutions", ApplicationData.Current.LocalFolder);

	public static async Task<IFolder> GetOrCreateFolder(string path, StorageFolder parent) {
		StorageFolder systemFolder =
			await parent.CreateFolderAsync(path, CreationCollisionOption.OpenIfExists);
		return new FileSystemFolder(systemFolder.Path);
	}

	void OnFilterChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
		if (propertyChangedEventArgs.PropertyName != nameof(FilterViewModel.Filter))
			return;

		this.PuzzleCollectionView.Filter = this.ViewModel.Filter.Filter;
		this.Campaign.Filter = this.ViewModel.Filter.Filter;
	}

	public ApplicationViewModel ViewModel { get; private set; }

	public ListCollectionView PuzzleCollectionView { get; private set; }
	public ListCollectionView Campaign { get; private set; }

	/// <summary>
	/// Invoked when application execution is being suspended.  Application state is saved
	/// without knowing whether the application will be terminated or resumed with the contents
	/// of memory still intact.
	/// </summary>
	/// <param name="sender">The source of the suspend request.</param>
	/// <param name="e">Details about the suspend request.</param>
	private void OnSuspending(object sender, SuspendingEventArgs e) {
		var deferral = e.SuspendingOperation.GetDeferral();
		//TODO: Save application state and stop any background activity
		deferral.Complete();
	}

	protected override async void OnSearchActivated(SearchActivatedEventArgs args) {
		// TODO: Register the Windows.ApplicationModel.Search.SearchPane.GetForCurrentView().QuerySubmitted
		// event in OnWindowCreated to speed up searches once the application is already running

		// If the Window isn't already using Frame navigation, insert our own Frame
		var previousContent = Window.Current.Content;

		// If the app does not contain a top-level frame, it is possible that this
		// is the initial launch of the app. Typically this method and OnLaunched
		// in App.xaml.cs can call a common method.
		if (previousContent is not Frame frame) {
			// Create a Frame to act as the navigation context and associate it with
			// a SuspensionManager key
			frame = new Frame();
			SuspensionManager.RegisterFrame(frame, "AppFrame");

			if (args.PreviousExecutionState == ApplicationExecutionState.Terminated) {
				// Restore the saved session state only when appropriate
				try {
					await SuspensionManager.RestoreAsync();
				} catch (SuspensionManagerException) {
					// Something went wrong restoring state.
					// Assume there is no state and continue
				}
			}
		}

		frame.Navigate(typeof(PuzzlesPage), args.QueryText);
		Window.Current.Content = frame;

		// Ensure the current window is active
		Window.Current.Activate();
	}

	/// <summary>
	/// Invoked when Navigation to a certain page fails
	/// </summary>
	/// <param name="sender">The Frame which failed navigation</param>
	/// <param name="e">Details about the navigation failure</param>
	void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
		throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
	}
}