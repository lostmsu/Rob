namespace RoboZZle.WinRT;

using System.IO;

using Microsoft.Xbox.Services.System;

using Windows.UI.Xaml.Controls;

using Microsoft.Xbox.Services;
using Microsoft.Xbox.Services.Social;

public partial class RoboZZleRt {
	#region Navigation

	public static void Navigate(Page page, Type pageType, object? parameter = null) {
#if WINDOWS_PHONE
		string uri = string.Format("/{0}.xaml", pageType.Name);
		page.NavigationService.Navigate(new Uri(uri, UriKind.Relative));
#else
		bool success = Frame.Navigate(pageType, parameter);
		if (!success)
			throw new Exception("Navigation failed");
#endif
	}

	public static bool CanGoBack(Page page) {
#if WINDOWS_PHONE
		return page.NavigationService.CanGoBack;
#else
		return Frame.CanGoBack;
#endif
	}

	public static void GoBack(Page page) {
#if WINDOWS_PHONE
		page.NavigationService.GoBack();
#else
		Frame.GoBack();
#endif
	}

	#endregion Navigation

	static Frame Frame => (Frame)Window.Current.Content;

	internal async void SetXboxUser(XboxLiveUser user) {
		if (!user.IsSignedIn)
			return;
		var context = new XboxLiveContext(user);
		try {
			var profile = await context.ProfileService.GetUserProfileAsync(user.XboxUserId);
			this.ViewModel.Social.ProfilePicture =
				profile.GameDisplayPictureResizeUri + "&w=128";
		} catch (FileNotFoundException) {
			// it's file if we can't load user gamepic
		}
	}
}