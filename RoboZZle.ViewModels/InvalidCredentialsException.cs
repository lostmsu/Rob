namespace RoboZZle.ViewModels;

public class InvalidCredentialsException(string message): Exception(message) {
	/// <summary>
	/// Generated, when user-provided credentials for online are not accepted
	/// </summary>
	public InvalidCredentialsException(): this(DefaultMessage) { }

	static string DefaultMessage => "Invalid user name or password";
}