namespace RoboZZle.ViewModels;

public static class ApplicationStatus {
	public const string? NONE = null;
	public const string LOGGING_IN = "LoggingIn";
	public const string LOADING_CACHE = "Loading";
	public const string QUERYING_NEW_LEVELS = "QueryingNewLevels";
	public const string SAVING_CACHE = "SavingNewLevels";
	public const string SUBMITTING_SOLUTIONS = "SubmittingSolutions";
}