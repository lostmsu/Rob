namespace RoboZZle.WinRT.Common;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

/// <summary>
/// Typical implementation of Page that provides several important conveniences:
/// <list type="bullet">
/// <item>
/// <description>Application view state to visual state mapping</description>
/// </item>
/// <item>
/// <description>GoBack, GoForward, and GoHome event handlers</description>
/// </item>
/// <item>
/// <description>Mouse and keyboard shortcuts for navigation</description>
/// </item>
/// <item>
/// <description>State management for navigation and process lifetime management</description>
/// </item>
/// <item>
/// <description>A default view model</description>
/// </item>
/// </list>
/// </summary>
[Windows.Foundation.Metadata.WebHostHidden]
public class LayoutAwarePage : Page
{
	/// <summary>
	/// Identifies the <see cref="DefaultViewModel"/> dependency property.
	/// </summary>
	public static DependencyProperty DefaultViewModelProperty { get; } =
		DependencyProperty.Register("DefaultViewModel", typeof(IObservableMap<string, object>),
		                            ownerType: typeof(LayoutAwarePage), typeMetadata: null);

	List<Control>? layoutAwareControls;

	protected ApplicationViewModel App => (Application.Current as RoboZZleRt)!.ViewModel;

	/// <summary>
	/// Initializes a new instance of the <see cref="LayoutAwarePage"/> class.
	/// </summary>
	public LayoutAwarePage()
	{
		if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
			return;

		// Create an empty default view model
		this.DefaultViewModel = new ObservableDictionary<string, object>();

		// When this page is part of the visual tree make two changes:
		// 1) Map application view state to visual state for the page
		// 2) Handle keyboard and mouse navigation requests
		this.Loaded += (sender, e) =>
		{
			this.StartLayoutUpdates(sender, e);

			// Keyboard and mouse navigation only apply when occupying the entire window
			if (Math.Abs(this.ActualHeight - Window.Current.Bounds.Height) < 1 &&
			    Math.Abs(this.ActualWidth - Window.Current.Bounds.Width) < 1)
			{
				// Listen to the window directly so focus isn't required
				Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=
					this.CoreDispatcher_AcceleratorKeyActivated;
				Window.Current.CoreWindow.PointerPressed +=
					this.CoreWindow_PointerPressed;
			}
		};

		// Undo the same changes when the page is no longer visible
		this.Unloaded += (sender, e) =>
		{
			this.StopLayoutUpdates(sender, e);
			Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -=
				this.CoreDispatcher_AcceleratorKeyActivated;
			Window.Current.CoreWindow.PointerPressed -=
				this.CoreWindow_PointerPressed;
		};
	}

	/// <summary>
	/// An implementation of <see cref="IObservableMap&lt;String, Object&gt;"/> designed to be
	/// used as a trivial view model.
	/// </summary>
	protected IObservableMap<string, object> DefaultViewModel
	{
		get
		{
			return this.GetValue(DefaultViewModelProperty) as IObservableMap<string, object>;
		}

		set
		{
			this.SetValue(DefaultViewModelProperty, value);
		}
	}

	#region Navigation support

	/// <summary>
	/// Invoked as an event handler to navigate backward in the page's associated
	/// <see cref="Frame"/> until it reaches the top of the navigation stack.
	/// </summary>
	/// <param name="sender">Instance that triggered the event.</param>
	/// <param name="e">Event data describing the conditions that led to the event.</param>
	protected virtual void GoHome(object sender, RoutedEventArgs e)
	{
		// Use the navigation frame to return to the topmost page
		if (this.Frame != null)
		{
			while (this.Frame.CanGoBack)
				this.Frame.GoBack();
		}
	}

	/// <summary>
	/// Invoked as an event handler to navigate backward in the navigation stack
	/// associated with this page's <see cref="Frame"/>.
	/// </summary>
	/// <param name="sender">Instance that triggered the event.</param>
	/// <param name="e">Event data describing the conditions that led to the
	/// event.</param>
	protected virtual void GoBack(object sender, RoutedEventArgs? e)
	{
		// Use the navigation frame to return to the previous page
		if (this.Frame is { CanGoBack: true })
			this.Frame.GoBack();
	}

	/// <summary>
	/// Invoked as an event handler to navigate forward in the navigation stack
	/// associated with this page's <see cref="Frame"/>.
	/// </summary>
	/// <param name="sender">Instance that triggered the event.</param>
	/// <param name="e">Event data describing the conditions that led to the
	/// event.</param>
	protected virtual void GoForward(object sender, RoutedEventArgs? e)
	{
		// Use the navigation frame to move to the next page
		if (this.Frame is { CanGoForward: true })
			this.Frame.GoForward();
	}

	/// <summary>
	/// Invoked on every keystroke, including system keys such as Alt key combinations, when
	/// this page is active and occupies the entire window.  Used to detect keyboard navigation
	/// between pages even when the page itself doesn't have focus.
	/// </summary>
	/// <param name="sender">Instance that triggered the event.</param>
	/// <param name="args">Event data describing the conditions that led to the event.</param>
	void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender,
	                                            AcceleratorKeyEventArgs args)
	{
		var virtualKey = args.VirtualKey;

		// Only investigate further when Left, Right, or the dedicated Previous or Next keys
		// are pressed
		if ((args.EventType == CoreAcceleratorKeyEventType.SystemKeyDown ||
		     args.EventType == CoreAcceleratorKeyEventType.KeyDown) &&
		    (virtualKey == VirtualKey.Left || virtualKey == VirtualKey.Right ||
		     (int)virtualKey == 166 || (int)virtualKey == 167))
		{
			var coreWindow = Window.Current.CoreWindow;
			var downState = CoreVirtualKeyStates.Down;
			bool menuKey = (coreWindow.GetKeyState(VirtualKey.Menu) & downState) == downState;
			bool controlKey = (coreWindow.GetKeyState(VirtualKey.Control) & downState) == downState;
			bool shiftKey = (coreWindow.GetKeyState(VirtualKey.Shift) & downState) == downState;
			bool noModifiers = !menuKey && !controlKey && !shiftKey;
			bool onlyAlt = menuKey && !controlKey && !shiftKey;

			if (((int)virtualKey == 166 && noModifiers) ||
			    (virtualKey == VirtualKey.Left && onlyAlt))
			{
				// When the previous key or Alt+Left are pressed navigate back
				args.Handled = true;
				this.GoBack(this, new RoutedEventArgs());
			}
			else if (((int)virtualKey == 167 && noModifiers) ||
			         (virtualKey == VirtualKey.Right && onlyAlt))
			{
				// When the next key or Alt+Right are pressed navigate forward
				args.Handled = true;
				this.GoForward(this, new RoutedEventArgs());
			}
		}
	}

	/// <summary>
	/// Invoked on every mouse click, touch screen tap, or equivalent interaction when this
	/// page is active and occupies the entire window.  Used to detect browser-style next and
	/// previous mouse button clicks to navigate between pages.
	/// </summary>
	/// <param name="sender">Instance that triggered the event.</param>
	/// <param name="args">Event data describing the conditions that led to the event.</param>
	void CoreWindow_PointerPressed(CoreWindow sender,
	                               PointerEventArgs args)
	{
		var properties = args.CurrentPoint.Properties;

		// Ignore button chords with the left, right, and middle buttons
		if (properties.IsLeftButtonPressed || properties.IsRightButtonPressed ||
		    properties.IsMiddleButtonPressed)
			return;

		// If back or foward are pressed (but not both) navigate appropriately
		bool backPressed = properties.IsXButton1Pressed;
		bool forwardPressed = properties.IsXButton2Pressed;
		if (backPressed ^ forwardPressed)
		{
			args.Handled = true;
			if (backPressed)
				this.GoBack(this, new RoutedEventArgs());
			if (forwardPressed)
				this.GoForward(this, new RoutedEventArgs());
		}
	}

	#endregion

	#region Visual state switching

	/// <summary>
	/// Invoked as an event handler, typically on the <see cref="FrameworkElement.Loaded"/>
	/// event of a <see cref="Control"/> within the page, to indicate that the sender should
	/// start receiving visual state management changes that correspond to application view
	/// state changes.
	/// </summary>
	/// <param name="sender">Instance of <see cref="Control"/> that supports visual state
	/// management corresponding to view states.</param>
	/// <param name="e">Event data that describes how the request was made.</param>
	/// <remarks>The current view state will immediately be used to set the corresponding
	/// visual state when layout updates are requested.  A corresponding
	/// <see cref="FrameworkElement.Unloaded"/> event handler connected to
	/// <see cref="StopLayoutUpdates"/> is strongly encouraged.  Instances of
	/// <see cref="LayoutAwarePage"/> automatically invoke these handlers in their Loaded and
	/// Unloaded events.</remarks>
	/// <seealso cref="DetermineVisualState"/>
	/// <seealso cref="InvalidateVisualState"/>
	public void StartLayoutUpdates(object sender, RoutedEventArgs e)
	{
		var control = sender as Control;
		if (control == null)
			return;
		if (this.layoutAwareControls == null)
		{
			// Start listening to view state changes when there are controls interested in updates
			Window.Current.SizeChanged += this.WindowSizeChanged;
			this.layoutAwareControls = new List<Control>();
		}
		this.layoutAwareControls.Add(control);

		// Set the initial visual state of the control
		VisualStateManager.GoToState(control, this.DetermineVisualState(ApplicationView.Value), false);
	}

	void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
	{
		this.InvalidateVisualState();
		this.windowBounds = Window.Current.Bounds;
	}

	/// <summary>
	/// Invoked as an event handler, typically on the <see cref="FrameworkElement.Unloaded"/>
	/// event of a <see cref="Control"/>, to indicate that the sender should start receiving
	/// visual state management changes that correspond to application view state changes.
	/// </summary>
	/// <param name="sender">Instance of <see cref="Control"/> that supports visual state
	/// management corresponding to view states.</param>
	/// <param name="e">Event data that describes how the request was made.</param>
	/// <remarks>The current view state will immediately be used to set the corresponding
	/// visual state when layout updates are requested.</remarks>
	/// <seealso cref="StartLayoutUpdates"/>
	public void StopLayoutUpdates(object sender, RoutedEventArgs e)
	{
		var control = sender as Control;
		if (control == null || this.layoutAwareControls == null)
			return;
		this.layoutAwareControls.Remove(control);
		if (this.layoutAwareControls.Count == 0)
		{
			// Stop listening to view state changes when no controls are interested in updates
			this.layoutAwareControls = null;
			Window.Current.SizeChanged -= this.WindowSizeChanged;
		}
	}

	/// <summary>
	/// Translates <see cref="ApplicationViewState"/> values into strings for visual state
	/// management within the page.  The default implementation uses the names of enum values.
	/// Subclasses may override this method to control the mapping scheme used.
	/// </summary>
	/// <param name="viewState">View state for which a visual state is desired.</param>
	/// <returns>Visual state name used to drive the
	/// <see cref="VisualStateManager"/></returns>
	/// <seealso cref="InvalidateVisualState"/>
	protected virtual string DetermineVisualState(ApplicationViewState viewState)
	{
		return viewState.ToString();
	}

	/// <summary>
	/// Updates all controls that are listening for visual state changes with the correct
	/// visual state.
	/// </summary>
	/// <remarks>
	/// Typically used in conjunction with overriding <see cref="DetermineVisualState"/> to
	/// signal that a different value may be returned even though the view state has not
	/// changed.
	/// </remarks>
	public void InvalidateVisualState()
	{
		if (this.layoutAwareControls != null)
		{
			string visualState = this.DetermineVisualState(ApplicationView.Value);
			foreach (var layoutAwareControl in this.layoutAwareControls)
			{
				VisualStateManager.GoToState(layoutAwareControl, visualState, false);
			}
		}
	}

	#endregion

	#region Process lifetime management

	string pageKey;

	/// <summary>
	/// Invoked when this page is about to be displayed in a Frame.
	/// </summary>
	/// <param name="e">Event data that describes how this page was reached.  The Parameter
	/// property provides the group to be displayed.</param>
	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		this.windowBounds = Window.Current.Bounds;

		// Returning to a cached page through navigation shouldn't trigger state loading
		if (this.pageKey != null)
			return;

		var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
		this.pageKey = "Page-" + this.Frame.BackStackDepth;

		if (e.NavigationMode == NavigationMode.New)
		{
			// Clear existing state for forward navigation when adding a new page to the
			// navigation stack
			var nextPageKey = this.pageKey;
			int nextPageIndex = this.Frame.BackStackDepth;
			while (frameState.Remove(nextPageKey))
			{
				nextPageIndex++;
				nextPageKey = "Page-" + nextPageIndex;
			}

			// Pass the navigation parameter to the new page
			this.LoadState(e.Parameter, null);
		}
		else
		{
			// Pass the navigation parameter and preserved page state to the page, using
			// the same strategy for loading suspended state and recreating pages discarded
			// from cache
			this.LoadState(e.Parameter, (Dictionary<string, object>)frameState[this.pageKey]);
		}
	}

	/// <summary>
	/// Invoked when this page will no longer be displayed in a Frame.
	/// </summary>
	/// <param name="e">Event data that describes how this page was reached.  The Parameter
	/// property provides the group to be displayed.</param>
	protected override void OnNavigatedFrom(NavigationEventArgs e)
	{
		var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
		var pageState = new Dictionary<string, object>();
		this.SaveState(pageState);
		frameState[this.pageKey] = pageState;
	}

	/// <summary>
	/// Populates the page with content passed during navigation.  Any saved state is also
	/// provided when recreating a page from a prior session.
	/// </summary>
	/// <param name="navigationParameter">The parameter value passed to
	/// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested.
	/// </param>
	/// <param name="pageState">A dictionary of state preserved by this page during an earlier
	/// session.  This will be null the first time a page is visited.</param>
	protected virtual void LoadState(object navigationParameter, Dictionary<string, object> pageState)
	{
	}

	/// <summary>
	/// Preserves state associated with this page in case the application is suspended or the
	/// page is discarded from the navigation cache.  Values must conform to the serialization
	/// requirements of <see cref="SuspensionManager.SessionState"/>.
	/// </summary>
	/// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
	protected virtual void SaveState(Dictionary<string, object> pageState)
	{
	}

	#endregion

	#region Settings
	static readonly Guid SettingsId = new Guid("{5E395C19-6C25-4651-812C-363930A13AE0}");

	protected void ShowSettings()
	{
		// Create a Popup window which will contain our flyout.
		this.settingsPopup = new Popup {IsLightDismissEnabled = true, Width = SettingsWidth, Height = this.windowBounds.Height};
		this.settingsPopup.Closed += this.OnSettingsClosed;
		Window.Current.Activated += this.OnWindowActivated;

		// Add the proper animation for the panel.
		this.settingsPopup.ChildTransitions = new TransitionCollection {
			new PaneThemeTransition()
			{
				Edge = EdgeTransitionLocation.Right
			}
		};

		var mypane = new SettingsPopup {Width = SettingsWidth, Height = this.windowBounds.Height};

		// Place the SettingsFlyout inside our Popup window.
		this.settingsPopup.Child = mypane;

		// Let's define the location of our Popup.
		this.settingsPopup.SetValue(Canvas.LeftProperty, (this.windowBounds.Width - SettingsWidth));
		this.settingsPopup.SetValue(Canvas.TopProperty, 0);
		this.settingsPopup.IsOpen = true;
		mypane.ThemeCombo.UpdateLayout();
		mypane.ThemeCombo.Focus(FocusState.Keyboard);
	}

	void OnWindowActivated(object sender, WindowActivatedEventArgs e)
	{
		if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
		{
			this.settingsPopup.IsOpen = false;
		}
	}

	void OnSettingsClosed(object sender, object e)
	{
		Window.Current.Activated -= this.OnWindowActivated;
		this.settingsPopup.Closed -= this.OnSettingsClosed;
		this.settingsPopup = null;
	}

	Rect windowBounds;

	Popup settingsPopup;
	const double SettingsWidth = 400;
	#endregion
	/// <summary>
	/// Implementation of IObservableMap that supports reentrancy for use as a default view
	/// model.
	/// </summary>
	class ObservableDictionary<TK, TV> : IObservableMap<TK, TV>
	{
		class ObservableDictionaryChangedEventArgs : IMapChangedEventArgs<TK>
		{
			public ObservableDictionaryChangedEventArgs(CollectionChange change, TK key)
			{
				this.CollectionChange = change;
				this.Key = key;
			}

			public CollectionChange CollectionChange { get; private set; }
			public TK Key { get; private set; }
		}

		Dictionary<TK, TV> dictionary = new Dictionary<TK, TV>();
		public event MapChangedEventHandler<TK, TV> MapChanged;

		void InvokeMapChanged(CollectionChange change, TK key)
		{
			var eventHandler = this.MapChanged;
			if (eventHandler != null)
			{
				eventHandler(this, new ObservableDictionaryChangedEventArgs(change, key));
			}
		}

		public void Add(TK key, TV value)
		{
			this.dictionary.Add(key, value);
			this.InvokeMapChanged(CollectionChange.ItemInserted, key);
		}

		public void Add(KeyValuePair<TK, TV> item)
		{
			this.Add(item.Key, item.Value);
		}

		public bool Remove(TK key)
		{
			if (this.dictionary.Remove(key))
			{
				this.InvokeMapChanged(CollectionChange.ItemRemoved, key);
				return true;
			}
			return false;
		}

		public bool Remove(KeyValuePair<TK, TV> item)
		{
			TV currentValue;
			if (this.dictionary.TryGetValue(item.Key, out currentValue) &&
			    object.Equals(item.Value, currentValue) && this.dictionary.Remove(item.Key))
			{
				this.InvokeMapChanged(CollectionChange.ItemRemoved, item.Key);
				return true;
			}
			return false;
		}

		public TV this[TK key]
		{
			get
			{
				return this.dictionary[key];
			}
			set
			{
				this.dictionary[key] = value;
				this.InvokeMapChanged(CollectionChange.ItemChanged, key);
			}
		}

		public void Clear()
		{
			var priorKeys = this.dictionary.Keys.ToArray();
			this.dictionary.Clear();
			foreach (var key in priorKeys)
			{
				this.InvokeMapChanged(CollectionChange.ItemRemoved, key);
			}
		}

		public ICollection<TK> Keys => this.dictionary.Keys;

		public bool ContainsKey(TK key)
		{
			return this.dictionary.ContainsKey(key);
		}

		public bool TryGetValue(TK key, out TV value)
		{
			return this.dictionary.TryGetValue(key, out value);
		}

		public ICollection<TV> Values => this.dictionary.Values;

		public bool Contains(KeyValuePair<TK, TV> item)
		{
			return this.dictionary.Contains(item);
		}

		public int Count => this.dictionary.Count;

		public bool IsReadOnly => false;

		public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
		{
			return this.dictionary.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.dictionary.GetEnumerator();
		}

		public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
		{
			int arraySize = array.Length;
			foreach (var pair in this.dictionary)
			{
				if (arrayIndex >= arraySize)
					break;
				array[arrayIndex++] = pair;
			}
		}
	}
}