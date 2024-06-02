namespace RoboZZle.ViewModels;

using System.Collections;

public sealed class InMemorySettings: IDictionary<string, object?> {
	readonly Dictionary<string, object?> values = [];

	public object? this[string key] {
		get => this.values.TryGetValue(key, out object? value) ? value : null;
		set => this.values[key] = value;
	}

	public ICollection<string> Keys => this.values.Keys;

	public ICollection<object?> Values => this.values.Values;

	public int Count => this.values.Count;

	public bool IsReadOnly => ((IDictionary<string, object?>)this.values).IsReadOnly;

	public void Add(string key, object? value) => this.values.Add(key, value);

	public bool ContainsKey(string key) => this.values.ContainsKey(key);

	public bool Remove(string key) => this.values.Remove(key);

	public bool TryGetValue(string key, out object? value) =>
		this.values.TryGetValue(key, out value);

	public void Add(KeyValuePair<string, object?> item) =>
		((IDictionary<string, object?>)this.values).Add(item);

	public void Clear() => this.values.Clear();

	public bool Contains(KeyValuePair<string, object?> item) =>
		((IDictionary<string, object?>)this.values).Contains(item);

	public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) =>
		((IDictionary<string, object?>)this.values).CopyTo(array, arrayIndex);

	public bool Remove(KeyValuePair<string, object?> item) =>
		((IDictionary<string, object?>)this.values).Remove(item);

	public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() =>
		this.values.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => this.values.GetEnumerator();
}