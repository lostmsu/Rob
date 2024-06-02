namespace RoboZZle.ViewModels;

using System.Globalization;
using System.ServiceModel;
using System.Threading.Tasks;

using PCLStorage;

using RoboZZle.WebService;

/// <summary>
/// View-model of application as whole
/// </summary>
public class ApplicationViewModel: ViewModelBase {
	public ApplicationViewModel(IDictionary<string, object?> roamingSettings,
	                            Task<ILocalSolutions> localSolutions,
	                            IFolder cacheFolder) {
		if (cacheFolder == null)
			throw new ArgumentNullException(nameof(cacheFolder));

		this.RoamingSettings =
			roamingSettings ?? throw new ArgumentNullException(nameof(roamingSettings));
		this.puzzles = new PuzzleListViewModel();
		this.campaign = new PuzzleListViewModel();
		this.levelCache = new LevelCache(cacheFolder);
		this.service = new RoboZZleService(this.levelCache);
		this.Filter = new FilterViewModel(roamingSettings);
		this.Progress = new Progress<double>();

		this.localSolutions = localSolutions;
	}

	readonly LevelCache levelCache;
	readonly RoboZZleService service;
	readonly Task<ILocalSolutions> localSolutions;
	readonly PuzzleListViewModel puzzles;
	readonly PuzzleListViewModel campaign;

	Task<Dictionary<int, int>?> onlineSolutions;
	string? currentAction = ApplicationStatus.NONE;
	readonly TaskCompletionSource<bool> readinessCompletionSource = new();

	public Task ShowReadyTask { get; private set; }
	public Task InitializationTask { get; private set; }

	/// <summary>
	/// Function to compute SHA1 of string. Required for logging in
	/// </summary>
	public Func<string, byte[]>? Sha1 { get; init; }

	/// <summary>
	/// Dictionary to store roaming settings
	/// </summary>
	public IDictionary<string, object?> RoamingSettings { get; }

	public SocialViewModel Social { get; } = new();

	/// <summary>
	/// Username for online service
	/// </summary>
	public string? UserName {
		get => this.RoamingSettings["userName"] as string;
		private set => this.RoamingSettings["userName"] = value;
	}

	/// <summary>
	/// PasswordHash for online service
	/// </summary>
	public string? PasswordHash {
		get => this.RoamingSettings["passwordHash"] as string;
		private set => this.RoamingSettings["passwordHash"] = value;
	}

	/// <summary>
	/// Should we submit puzzle solutions to AI project
	/// </summary>
	public bool AiTelemetryEnabled {
		get => this.RoamingSettings[nameof(this.AiTelemetryEnabled)] as bool? ?? false;
		set {
			this.RoamingSettings[nameof(this.AiTelemetryEnabled)] = (bool?)value;
			this.OnPropertyChanged();
		}
	}

	public bool AiTelemetrySuggested {
		get => this.RoamingSettings[nameof(this.AiTelemetrySuggested)] as bool? ?? false;
		set {
			this.RoamingSettings[nameof(this.AiTelemetrySuggested)] = (bool?)value;
			this.OnPropertyChanged();
		}
	}

	/// <summary>
	/// <c>true</c>, if user have seen tutorial. <c>false</c> otherwise.
	/// </summary>
	public bool? SeenTutorial {
		get => (bool?)this.RoamingSettings[nameof(this.SeenTutorial)];
		set {
			this.RoamingSettings[nameof(this.SeenTutorial)] = value;
			this.OnPropertyChanged();
		}
	}

	readonly TelemetrySource telemetrySource = new() {
		Product = "Rob",
		Version = "unset",
	};

	public string AppName {
		get => this.telemetrySource.Product;
		set => this.telemetrySource.Product = value;
	}
	public string AppVersion {
		get => this.telemetrySource.Version;
		set => this.telemetrySource.Version = value;
	}

	public bool TestMode { get; set; } = true;

	public FilterViewModel Filter { get; private set; }

	/// <summary>
	/// Available puzzles
	/// </summary>
	public PuzzleListViewModel Puzzles => this.puzzles;

	/// <summary>
	/// Campaign puzzles
	/// </summary>
	public PuzzleListViewModel Campaign => this.campaign;

	/// <summary>
	/// Current action
	/// </summary>
	public string? CurrentAction {
		get => this.currentAction;
		protected set {
			this.currentAction = value;
			this.OnPropertyChanged();
		}
	}

	/// <summary>
	/// Through this object view-model will provide progress information on currect action
	/// </summary>
	public IProgress<double> Progress { get; set; }

	public event EventHandler<string> Warning;

	/// <summary>
	/// Initializes application.
	/// </summary>
	public async Task Initialize() {
		if (this.InitializationTask != null)
			throw new InvalidOperationException();

		this.ShowReadyTask = this.readinessCompletionSource.Task;
		this.InitializationTask = this.InitializeInternal();

		await this.InitializationTask;
	}

	public string HashPassword(string password) {
		if (this.Sha1 == null)
			throw new InvalidOperationException("You must set PasswordHasher first");

		byte[] hashBytes = this.Sha1(password + "5A6fKpgSnXoMpxbcHcb7");
		return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
	}

	/// <summary>
	/// Performs login attempt. Stores user name and passwordHash, if it was successful.
	/// </summary>
	/// <param name="userName">User name to log in with</param>
	/// <param name="passwordHash">PasswordHash to log in with</param>
	public async Task Login(string userName, string passwordHash) {
		var attempt = this.service.Login(userName, passwordHash);
		var result = await attempt;
		if (result == null)
			throw new InvalidCredentialsException();
		this.UserName = userName;
		this.PasswordHash = passwordHash;

		await this.InitializationTask;

		this.onlineSolutions = attempt;

#pragma warning disable 4014 // do not wait for those calls
		this.DisplayRemoteSolutions();
		this.SubmitSolutions();
#pragma warning restore 4014
	}

	/// <summary>
	/// Attempts to register new user in online service. Stores name and password hash, if attempt was successful.
	/// </summary>
	/// <param name="userName">New user name</param>
	/// <param name="passwordHash">New user password hash</param>
	/// <param name="email">User's email address</param>
	public async Task Register(string userName, string passwordHash, string email) {
		int[] localSolved = (await this.localSolutions)
		                    .Solutions.Select(s => s.PuzzleID).Distinct()
		                    .ToArray();
		string? attempt = await this.service.Register(userName, passwordHash, email, localSolved);
		if (attempt != null)
			throw new InvalidCredentialsException(attempt);

		this.UserName = userName;
		this.PasswordHash = passwordHash;
	}

	internal async Task Add(LocalSolution solution) {
		(await this.localSolutions).Add(solution);

		var puzzleViewModel = this.puzzles[solution.PuzzleID];
		if (puzzleViewModel == null)
			return;

		int currentSolutionLength = puzzleViewModel.BestSolution.GetValueOrDefault(int.MaxValue);
		int newSolutionLength = solution.Program.GetLength();
		puzzleViewModel.BestSolution = Math.Min(currentSolutionLength, newSolutionLength);
	}

	internal async Task SubmitSolution(int puzzleID, Program program) {
		if (program == null)
			throw new ArgumentNullException(nameof(program));

		try {
			await this.service.SubmitSolution(puzzleID, program);
		} catch (EndpointNotFoundException e) when (e.InnerException?.HResult ==
		                                            unchecked((int)0x80072ee7)) {
			this.OnWarning("SubmitFailed_NoInternet");
		}
	}

	protected void OnWarning(string messageKey) =>
		this.Warning?.Invoke(
			this, messageKey ?? throw new ArgumentNullException(nameof(messageKey)));

	#region Initialization sequence

	async Task InitializeInternal() {
		DebugEx.WriteLine("starting initialization");

		this.FirstLogin();

		DebugEx.WriteLine("login request sent");

		this.CurrentAction = ApplicationStatus.LOADING_CACHE;

		await this.service.InitializeCache();

		DebugEx.WriteLine("cache initialized");

		const int FIRST_PUZZLE_ID = 14;
		if (this.service.GetPuzzle(FIRST_PUZZLE_ID) is null) {
			DebugEx.WriteLine("downloading levels from the archive");
			var archive = new LevelArchive();
			var levels = await archive.GetLevelsAsync();
			await this.levelCache.Save(levels.ToDictionary(l => l.Id, l => l));
			await this.service.InitializeCache();
			DebugEx.WriteLine("level archive downloaded and cached");
		}

		Task<int[]> newOnline = this.service.QueryNewPuzzles(this.Progress);

		DebugEx.WriteLine("new puzzles query sent");

		this.LoadLocalPuzzles();
		bool hasLocalPuzzles = this.puzzles.Count > 100;
		if (hasLocalPuzzles) {
			await this.LoadCampaign();
			this.readinessCompletionSource.TrySetResult(true);
		}

		DebugEx.WriteLine("local puzzles loaded");

		int[] newPuzzles = await this.LoadOnlinePuzzles(newOnline);

		if (!hasLocalPuzzles) {
			await this.LoadCampaign();
			this.readinessCompletionSource.TrySetResult(true);
		}

		if (newPuzzles.Length > 0) {
			this.CurrentAction = ApplicationStatus.SAVING_CACHE;
			await this.service.SaveCache();

			DebugEx.WriteLine("updated cache with new puzzles");
		}

		this.CurrentAction = ApplicationStatus.SUBMITTING_SOLUTIONS;

		await Task.WhenAll(this.DisplayLocalSolutions(), this.DisplayRemoteSolutions(),
		                   this.SubmitSolutions());
		lock (this.puzzles)
			this.puzzles.Reset();
	}

	async Task LoadCampaign() {
		int[] campaignPuzzleIDs = null;
		try {
			campaignPuzzleIDs = await this.service.GetCampaignPuzzleIDs();
		}
		// TODO: notify user
		catch (EndpointNotFoundException) { } catch (FaultException) { } catch
			(ServerTooBusyException) { } catch (CommunicationObjectAbortedException) { } catch
			(TimeoutException) { }

		if (campaignPuzzleIDs == null)
			return;

		lock (this.campaign)
		lock (this.puzzles) {
			foreach (int puzzleID in campaignPuzzleIDs) {
				var puzzleVM = this.puzzles[puzzleID];
				if (puzzleVM != null)
					this.campaign.Add(puzzleVM);
			}
		}
	}

	async Task DisplayLocalSolutions() {
		var locals = await this.localSolutions;

		DebugEx.WriteLine("rendering local solutions");

		foreach (var localSolution in locals.Solutions) {
			this.UpdateSolutionLength(localSolution.PuzzleID, localSolution.Program.GetLength());
		}

		DebugEx.WriteLine("local solutions rendered");
	}

	async Task DisplayRemoteSolutions() {
		Dictionary<int, int>? remote = null;
		try {
			remote = await this.onlineSolutions;

			DebugEx.WriteLine("login response and remote solutions arrived");
		}
		// TODO: notify user
		catch (EndpointNotFoundException) { } catch (FaultException) { } catch
			(ServerTooBusyException) { }

		if (remote == null)
			return;

		foreach (KeyValuePair<int, int> solution in remote) {
			this.UpdateSolutionLength(solution.Key, solution.Value);
		}

		DebugEx.WriteLine("remote solutions rendered");
	}

	void UpdateSolutionLength(int puzzleID, int solutionLength) {
		lock (this.campaign)
		lock (this.puzzles) {
			var puzzle = this.puzzles[puzzleID];
			if (puzzle == null)
				return;

			if (puzzle.BestSolution == null || puzzle.BestSolution.Value > solutionLength) {
				puzzle.BestSolution = solutionLength;
				if (this.campaign.Puzzles.Contains(puzzle))
					this.campaign.Update(puzzleID);
			}
		}
	}

	async Task SubmitSolutions() {
		Dictionary<int, int>? remote = null;

		try {
			await Task.WhenAll(this.onlineSolutions, this.localSolutions);
			remote = this.onlineSolutions.Result;
		}
		// TODO: notify user
		catch (EndpointNotFoundException) { } catch (FaultException) { } catch
			(ServerTooBusyException) { }

		if (remote == null)
			return;

		var submissions = new List<Task>();

		DebugEx.WriteLine("submitting new solutions");

		foreach (var localSolution in this.localSolutions.Result.Solutions) {
			if (!remote.TryGetValue(localSolution.PuzzleID, out int remoteLength))
				remoteLength = int.MaxValue;

			if (localSolution.Program.GetLength() >= remoteLength)
				continue;

			var submission =
				this.service.SubmitSolution(localSolution.PuzzleID, localSolution.Program);
			submissions.Add(submission);
		}

		await Task.WhenAll(submissions);

		DebugEx.WriteLine("submitted new solutions");
	}

	void FirstLogin() {
		this.service.UserName = this.UserName;
		this.service.PasswordHash = this.PasswordHash;

		try {
			this.onlineSolutions = this.HasCredentials
				? this.service.Login(this.UserName, this.PasswordHash)
				: Task.FromResult<Dictionary<int, int>?>(null);
			// TODO handle user changed password scenario
		} catch (EndpointNotFoundException) {
			this.onlineSolutions = Task.FromResult<Dictionary<int, int>?>(null);
		}
	}

	void LoadLocalPuzzles() {
		DebugEx.WriteLine("retrieving local puzzle IDs");

		int[] puzzleIDs = this.service.GetLocalPuzzleIDs();

		DebugEx.WriteLine("loading local puzzles");

		this.LoadPuzzles(puzzleIDs, this.Progress);
	}

	async Task<int[]> LoadOnlinePuzzles(Task<int[]> newOnline) {
		this.CurrentAction = ApplicationStatus.QUERYING_NEW_LEVELS;

		int[] newPuzzles = [];
		try {
			newPuzzles = await newOnline;
		}
		// TODO: notify user, that server is unavailable
		catch (EndpointNotFoundException) { } catch (ServerTooBusyException) { } catch
			(FaultException) { } catch (CommunicationObjectAbortedException) { } catch
			(TimeoutException) { }

		DebugEx.WriteLine("loading remote puzzles");
		this.LoadPuzzles(newPuzzles, this.Progress);
		DebugEx.WriteLine("remote puzzles loaded");
		return newPuzzles;
	}

	#endregion

	void LoadPuzzles(int[] puzzleIDs, IProgress<double>? progress) {
		progress?.Report(0);

		Array.Sort(puzzleIDs);

		lock (this.puzzles) {
			for (int i = 0; i < puzzleIDs.Length; i++) {
				int puzzleID = puzzleIDs[i];
				var puzzle = new PuzzleViewModel(puzzleID, this.service.GetPuzzle) {
					Local = Task.Factory.StartNew(() =>
						                              new LocalPuzzleViewModel(
							                              this.GetRoamingPuzzleData(puzzleID))),
				};
				this.puzzles.Add(puzzle);
				if (progress != null && (i & 31) == 0)
					progress.Report(i * 1.0 / puzzleIDs.Length);
			}
		}
	}

	ScopedDictionary<object?> GetRoamingPuzzleData(int puzzleID) {
		string scope = "puzzles." + puzzleID.ToString(CultureInfo.InvariantCulture) + ".";
		return new ScopedDictionary<object?>(scope, this.RoamingSettings);
	}

	public bool HasCredentials => !string.IsNullOrWhiteSpace(this.UserName) &&
	                              !string.IsNullOrWhiteSpace(this.PasswordHash);
}