namespace RoboZZle.ViewModels;

using System.ComponentModel;
using System.Runtime.CompilerServices;

/// <summary>
/// Base class for all view models
/// </summary>
public abstract class ViewModelBase: INotifyPropertyChanged {
	protected virtual void OnPropertyChanged([CallerMemberName] string name = null!) {
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

	/// <summary>
	/// Occurs, when observable property is changed
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;
}