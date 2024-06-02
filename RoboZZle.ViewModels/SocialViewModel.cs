namespace RoboZZle.ViewModels;

public class SocialViewModel: ViewModelBase {
	string? profilePicture;

	public string? ProfilePicture {
		get => this.profilePicture;
		set {
			this.profilePicture = value;
			this.OnPropertyChanged();
		}
	}
}