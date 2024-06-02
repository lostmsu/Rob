namespace RoboZZle.WinRT.Imported;

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;

/// <summary>
/// Simple implementation of the <see cref="ICollectionViewEx"/> interface,
/// which extends the standard WinRT definition of the <see cref="ICollectionView"/>
/// interface to add sorting and filtering.
/// </summary>
/// <remarks>
/// Here's an example that shows how to use the <see cref="ListCollectionView"/> class:
/// <code>
/// // create a simple list
/// var list = new List&lt;Rect&gt;();
/// for (int i = 0; i &lt; 200; i++)
///   list.Add(new Rect(i, i, i, i));
///
/// // create collection view based on list
/// var cv = new ListCollectionView();
/// cv.Source = list;
///
/// // apply filter
/// cv.Filter = (item) =&gt; { return ((Rect)item).X % 2 == 0; };
///
/// // apply sort
/// cv.SortDescriptions.Add(new SortDescription("Width", ListSortDirection.Descending));
///
/// // show data on grid
/// mygrid.ItemsSource = cv;
/// </code>
/// </remarks>
public sealed class ListCollectionView:
	ICollectionViewEx,
	IEditableCollectionView,
	INotifyPropertyChanged,
	IComparer<object?> {
	//------------------------------------------------------------------------------------

	#region ** fields

	object? source; // original data source
	IList? sourceList; // original data source as list
	Type? itemType; // type of item in the source collection
	INotifyCollectionChanged? sourceNcc; // listen to changes in the source
	// view exposed to consumers
	readonly List<object?> view = []; // filtered/sorted data source
	readonly ObservableCollection<SortDescription> sort = []; // sorting parameters
	readonly Dictionary<string, PropertyInfo> sortProps = new(); // PropertyInfo dictionary used while sorting
	Predicate<object>? filter; // filter
	int index; // cursor position
	int updating; // suspend notifications

	#endregion

	//------------------------------------------------------------------------------------

	#region ** ctor

	public ListCollectionView(object? source) {
		// sort descriptor collection
		this.sort.CollectionChanged += this.OnSortCollectionChanged;

		// hook up data source
		this.Source = source;
	}

	public ListCollectionView(): this(null) { }

	#endregion

	//------------------------------------------------------------------------------------

	#region ** object model

	/// <summary>
	/// Gets or sets the collection from which to create the view.
	/// </summary>
	public object? Source {
		get => this.source;
		set {
			if (this.source == value)
				return;

			// save new source
			this.source = value;

			// save new source as list (so we can add/remove etc)
			this.sourceList = value as IList;

			// get the type of object in the collection
			this.itemType = this.GetItemType();

			// listen to changes in the source
			if (this.sourceNcc != null) {
				this.sourceNcc.CollectionChanged -= this.OnSourceCollectionChanged;
			}

			this.sourceNcc = this.source as INotifyCollectionChanged;
			if (this.sourceNcc != null) {
				this.sourceNcc.CollectionChanged += this.OnSourceCollectionChanged;
			}

			// refresh our view
			this.HandleSourceChanged();

			// inform listeners
			this.OnPropertyChanged("Source");
		}
	}

	/// <summary>
	/// Update the view from the current source, using the current filter and sort settings.
	/// </summary>
	public void Refresh() {
		this.HandleSourceChanged();
	}

	/// <summary>
	/// Raises the <see cref="CurrentChanging"/> event.
	/// </summary>
	void OnCurrentChanging(CurrentChangingEventArgs e) {
		if (this.updating <= 0) {
			this.CurrentChanging?.Invoke(this, e);
		}
	}

	/// <summary>
	/// Raises the <see cref="CurrentChanged"/> event.
	/// </summary>
	void OnCurrentChanged(object e) {
		if (this.updating <= 0) {
			this.CurrentChanged?.Invoke(this, e);
			this.OnPropertyChanged("CurrentItem");
		}
	}

	/// <summary>
	/// Raises the <see cref="VectorChanged"/> event.
	/// </summary>
	void OnVectorChanged(IVectorChangedEventArgs e) {
		if (this.IsAddingNew || this.IsEditingItem) {
			throw new NotSupportedException(
				"Cannot change collection while adding or editing items.");
		}

		if (this.updating <= 0) {
			this.VectorChanged?.Invoke(this, e);
			this.OnPropertyChanged("Count");
		}
	}

	/// <summary>
	/// Enters a defer cycle that you can use to merge changes to the view and delay
	/// automatic refresh.
	/// </summary>
	public IDisposable DeferRefresh() {
		return new DeferNotifications(this);
	}

	#endregion

	//------------------------------------------------------------------------------------

	#region ** event handlers

	// the original source has changed, update our source list
	void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
		if (this.updating <= 0) {
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				if (e.NewItems.Count == 1) {
					this.HandleItemAdded(e.NewStartingIndex, e.NewItems[0]);
				} else {
					this.HandleSourceChanged();
				}

				break;
			case NotifyCollectionChangedAction.Remove:
				if (e.OldItems.Count == 1) {
					this.HandleItemRemoved(e.OldStartingIndex, e.OldItems[0]);
				} else {
					this.HandleSourceChanged();
				}

				break;
			case NotifyCollectionChangedAction.Move:
			case NotifyCollectionChangedAction.Replace:
			case NotifyCollectionChangedAction.Reset:
				this.HandleSourceChanged();
				break;
			default:
				throw new Exception("Unrecognized collection change notification: " +
				                    e.Action.ToString());
			}
		}
	}

	// sort changed, refresh view
	void OnSortCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
		if (this.updating <= 0) {
			this.HandleSourceChanged();
		}
	}

	#endregion

	//------------------------------------------------------------------------------------

	#region ** implementation

	// add item to view
	void HandleItemAdded(int index, object item) {
		// if the new item is filtered out of view, no work
		if (this.filter != null && !this.filter(item)) {
			return;
		}

		// compute insert index
		if (this.sort.Count > 0) {
			// sorted: insert at sort position
			this.sortProps.Clear();
			index = this.view.BinarySearch(item, this);
			if (index < 0)
				index = ~index;
		} else if (this.filter != null) {
			// if the source is not a list (e.g. enum), then do a full refresh
			if (this.sourceList == null) {
				this.HandleSourceChanged();
				return;
			}

			// find insert index
			// count invisible items below the insertion point and
			// subtract from the number of items in the view
			// (counting from the bottom is more efficient for the
			// most common case which is appending to the source collection)
			int visibleBelowIndex = 0;
			for (int i = index; i < this.sourceList.Count; i++) {
				if (!this.filter(this.sourceList[i])) {
					visibleBelowIndex++;
				}
			}

			index = this.view.Count - visibleBelowIndex;
		}

		// add item to view
		this.view.Insert(index, item);

		// keep selection on the same item
		if (index <= this.index) {
			this.index++;
		}

		// notify listeners
		var e = new VectorChangedEventArgs(CollectionChange.ItemInserted, index, item);
		this.OnVectorChanged(e);
	}

	// remove item from view
	void HandleItemRemoved(int index, object item) {
		// check if the item is in the view
		if (this.filter != null && !this.filter(item)) {
			return;
		}

		// compute index into view
		if (index < 0 || index >= this.view.Count || !object.Equals(this.view[index], item)) {
			index = this.view.IndexOf(item);
		}

		if (index < 0) {
			return;
		}

		// remove item from view
		this.view.RemoveAt(index);

		// keep selection on the same item
		if (index <= this.index) {
			this.index--;
		}

		// notify listeners
		var e = new VectorChangedEventArgs(CollectionChange.ItemRemoved, index, item);
		this.OnVectorChanged(e);
	}

	// update view after changes other than add/remove an item
	void HandleSourceChanged() {
		// release sort property PropertyInfo dictionary
		this.sortProps.Clear();

		// keep selection if possible
		object? currentItem = this.CurrentItem;

		// re-create view
		this.view.Clear();
		if (this.Source is IEnumerable ie) {
			foreach (object? item in ie) {
				if (this.filter == null || this.filter(item)) {
					if (this.sort.Count > 0) {
						int index = this.view.BinarySearch(item, this);
						if (index < 0)
							index = ~index;
						this.view.Insert(index, item);
					} else {
						this.view.Add(item);
					}
				}
			}
		}

		// release sort property PropertyInfo dictionary
		this.sortProps.Clear();

		// notify listeners
		this.OnVectorChanged(VectorChangedEventArgs.Reset);

		// restore selection if possible
		this.MoveCurrentTo(currentItem);
	}

	// update view after an item changes (apply filter/sort if necessary)
	void HandleItemChanged(object item) {
		// apply filter/sort after edits
		bool refresh = this.filter != null && !this.filter(item);

		if (this.sort.Count > 0) {
			// find sorted index for this object
			this.sortProps.Clear();
			int newIndex = this.view.BinarySearch(item, this);
			if (newIndex < 0)
				newIndex = ~newIndex;

			// item moved within the collection
			if (newIndex >= this.view.Count || this.view[newIndex] != item) {
				refresh = true;
			}
		}

		if (refresh) {
			this.HandleSourceChanged();
		}
	}

	// move the cursor to a new position
	bool MoveCurrentToIndex(int index) {
		// invalid?
		if (index < -1 || index >= this.view.Count) {
			return false;
		}

		// no change?
		if (index == this.index) {
			return false;
		}

		// fire changing
		var e = new CurrentChangingEventArgs();
		this.OnCurrentChanging(e);
		if (e.Cancel) {
			return false;
		}

		// change and fire changed
		this.index = index;
		this.OnCurrentChanged(null);
		return true;
	}

	// get the type of item in the source collection
	Type? GetItemType() {
		if (this.source == null)
			return null;

		Type? itemType = null;
		var type = this.source.GetType();
		var args = type.GenericTypeArguments;
		if (args.Length == 1) {
			itemType = args[0];
		} else if (this.sourceList != null && this.sourceList.Count > 0) {
			object? item = this.sourceList[0];
			itemType = item.GetType();
		}

		return itemType;
	}

	#endregion

	//------------------------------------------------------------------------------------

	#region ** nested classes

	/// <summary>
	/// Class that handles deferring notifications while the view is modified.
	/// </summary>
	class DeferNotifications: IDisposable {
		readonly ListCollectionView view;
		readonly object? currentItem;

		internal DeferNotifications(ListCollectionView view) {
			this.view = view;
			this.currentItem = this.view.CurrentItem;
			this.view.updating++;
		}

		public void Dispose() {
			this.view.MoveCurrentTo(this.currentItem);
			this.view.updating--;
			this.view.Refresh();
		}
	}

	/// <summary>
	/// Class that implements IVectorChangedEventArgs so we can fire VectorChanged events.
	/// </summary>
	class VectorChangedEventArgs: IVectorChangedEventArgs {
		public static VectorChangedEventArgs Reset { get; } = new(CollectionChange.Reset);

		public VectorChangedEventArgs(CollectionChange cc, int index = -1, object? item = null) {
			this.CollectionChange = cc;
			this.Index = (uint)index;
		}

		public CollectionChange CollectionChange { get; } = CollectionChange.Reset;

		public uint Index { get; }
	}

	#endregion

	//------------------------------------------------------------------------------------

	#region ** IC1CollectionView

	public bool CanFilter => true;

	public Predicate<object>? Filter {
		get => this.filter;
		set {
			if (this.filter != value) {
				this.filter = value;
				this.Refresh();
			}
		}
	}
	public bool CanGroup => false;
	public IList<object>? GroupDescriptions => null;
	public bool CanSort => true;
	public IList<SortDescription> SortDescriptions => this.sort;
	public IEnumerable? SourceCollection => this.source as IEnumerable;

	#endregion

	//------------------------------------------------------------------------------------

	#region ** ICollectionView

	/// <summary>
	/// Occurs after the current item has changed.
	/// </summary>
	public event EventHandler<object>? CurrentChanged;
	/// <summary>
	/// Occurs before the current item changes.
	/// </summary>
	public event CurrentChangingEventHandler? CurrentChanging;
	/// <summary>
	/// Occurs when the view collection changes.
	/// </summary>
	public event VectorChangedEventHandler<object>? VectorChanged;
	/// <summary>
	/// Gets a colletion of top level groups.
	/// </summary>
	public IObservableVector<object>? CollectionGroups => null;

	/// <summary>
	/// Gets the current item in the view.
	/// </summary>
	public object? CurrentItem {
		get => this.index > -1 && this.index < this.view.Count ? this.view[this.index] : null;
		set => this.MoveCurrentTo(value);
	}
	/// <summary>
	/// Gets the ordinal position of the current item in the view.
	/// </summary>
	public int CurrentPosition => this.index;

	public bool IsCurrentAfterLast => this.index >= this.view.Count;
	public bool IsCurrentBeforeFirst => this.index < 0;
	public bool MoveCurrentToFirst() => this.MoveCurrentToIndex(0);
	public bool MoveCurrentToLast() => this.MoveCurrentToIndex(this.view.Count - 1);
	public bool MoveCurrentToNext() => this.MoveCurrentToIndex(this.index + 1);
	public bool MoveCurrentToPosition(int index) => this.MoveCurrentToIndex(index);

	public bool MoveCurrentToPrevious() {
		return this.MoveCurrentToIndex(this.index - 1);
	}

	public int IndexOf(object? item) {
		return this.view.IndexOf(item);
	}

	public bool MoveCurrentTo(object? item) =>
		item == this.CurrentItem || this.MoveCurrentToIndex(this.IndexOf(item));

	// async operations not supported
	public bool HasMoreItems => false;

	public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) {
		throw new NotSupportedException();
	}

	#region list operations

	public bool IsReadOnly => this.sourceList == null || this.sourceList.IsReadOnly;

	public void Add(object item) {
		this.CheckReadOnly();
		this.sourceList.Add(item);
	}

	public void Insert(int index, object item) {
		this.CheckReadOnly();
		if (this.sort.Count > 0 || this.filter != null) {
			throw new Exception("Cannot insert items into sorted or filtered views.");
		}

		this.sourceList.Insert(index, item);
	}

	public void RemoveAt(int index) {
		this.Remove(this.view[index]);
	}

	public bool Remove(object item) {
		this.CheckReadOnly();
		this.sourceList.Remove(item);
		return true;
	}

	public void Clear() {
		this.CheckReadOnly();
		this.sourceList.Clear();
	}

	void CheckReadOnly() {
		if (this.IsReadOnly) {
			throw new Exception("The source collection cannot be modified.");
		}
	}

	public object this[int index] {
		get => this.view[index];
		set => this.view[index] = value;
	}
	public bool Contains(object item) => this.view.Contains(item);
	public void CopyTo(object[] array, int arrayIndex) => this.view.CopyTo(array, arrayIndex);
	public int Count => this.view.Count;
	public IEnumerator<object> GetEnumerator() => this.view.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => this.view.GetEnumerator();

	#endregion

	#endregion

	//------------------------------------------------------------------------------------

	#region ** IEditableCollectionView

	object? addItem;
	object? editItem;

	public bool CanCancelEdit => true;
	public object? CurrentEditItem => this.editItem;
	public bool IsEditingItem => this.editItem != null;

	public void EditItem(object item) {
		if (item is IEditableObject ieo && ieo != this.editItem) {
			ieo.BeginEdit();
		}

		this.editItem = item;
	}

	public void CancelEdit() {
		if (this.editItem is IEditableObject ieo) {
			ieo.CancelEdit();
		}

		this.editItem = null;
	}

	public void CommitEdit() {
		if (this.editItem != null) {
			// finish editing item
			object item = this.editItem;
			var ieo = item as IEditableObject;
			if (ieo != null) {
				ieo.EndEdit();
			}

			this.editItem = null;

			// apply filter/sort after edits
			this.HandleItemChanged(item);
		}
	}

	public bool CanAddNew => !this.IsReadOnly && this.itemType != null;

	public object? AddNew() {
		this.addItem = null;
		if (this.itemType == null)
			return null;

		this.addItem = Activator.CreateInstance(this.itemType);
		if (this.addItem != null) {
			this.Add(this.addItem);
		}

		return this.addItem;
	}

	public void CancelNew() {
		if (this.addItem != null) {
			this.Remove(this.addItem);
			this.addItem = null;
		}
	}

	public void CommitNew() {
		if (this.addItem == null)
			return;

		object item = this.addItem;
		this.addItem = null;
		this.HandleItemChanged(item);
	}

	public bool CanRemove => !this.IsReadOnly;
	public object? CurrentAddItem => this.addItem;
	public bool IsAddingNew => this.addItem != null;

	#endregion

	//------------------------------------------------------------------------------------

	#region ** INotifyPropertyChanged

	public event PropertyChangedEventHandler? PropertyChanged;

	void OnPropertyChanged(string propName) {
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
	}

	#endregion

	//------------------------------------------------------------------------------------

	#region ** IComparer<object>

	int IComparer<object?>.Compare(object? x, object? y) {
		// get property descriptors (once)
		if (this.sortProps.Count == 0) {
			var typeInfo = x.GetType().GetTypeInfo();
			foreach (var sd in this.sort) {
				this.sortProps[sd.PropertyName] = typeInfo.GetDeclaredProperty(sd.PropertyName);
			}
		}

		// compare two items
		foreach (var sd in this.sort) {
			var pi = this.sortProps[sd.PropertyName];
			var cx = pi.GetValue(x) as IComparable;
			var cy = pi.GetValue(y) as IComparable;

			try {
				int cmp =
					cx == cy ? 0 :
					cx == null ? -1 :
					cy == null ? +1 :
					cx.CompareTo(cy);

				if (cmp != 0) {
					return sd.Direction == ListSortDirection.Ascending ? +cmp : -cmp;
				}
			} catch {
				System.Diagnostics.Debug.WriteLine("comparison failed...");
			}
		}

		return 0;
	}

	#endregion
}