namespace RoboZZle.ViewModels;

using System.Collections;
using System.Collections.ObjectModel;

public sealed class ScopedDictionary<TValue>: IDictionary<string, TValue> {
	readonly string scope;
	readonly IDictionary<string, TValue> parent;

	bool IsKeyInScope(string key) => key.StartsWith(this.scope, StringComparison.Ordinal);

	public ICollection<string> Keys =>
		new ReadOnlyCollection<string>(this.parent.Keys.Where(this.IsKeyInScope).ToArray());

	public ICollection<TValue> Values =>
		new ReadOnlyCollection<TValue>(
			this.parent.Where(pair => this.IsKeyInScope(pair.Key))
			    .Select(pair => pair.Value)
			    .ToArray()
		);

	public int Count =>
		this.parent.Keys.Count(this.IsKeyInScope);

	public bool IsReadOnly => this.parent.IsReadOnly;

	public TValue this[string key] {
		get => this.parent[this.scope + key];
		set => this.parent[this.scope + key] = value;
	}

	public ScopedDictionary(string scope, IDictionary<string, TValue> parent) {
		if (string.IsNullOrEmpty(scope))
			throw new ArgumentNullException(nameof(scope));

		this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
		this.scope = scope;
	}

	public void Add(string key, TValue value) => this.parent.Add(this.scope + key, value);

	public bool ContainsKey(string key) => this.parent.ContainsKey(this.scope + key);

	public bool Remove(string key) => this.parent.Remove(this.scope + key);

	public bool TryGetValue(string key, out TValue value)
		=> this.parent.TryGetValue(this.scope + key, out value);

	public void Add(KeyValuePair<string, TValue> item) {
		var scopedItem = new KeyValuePair<string, TValue>(this.scope + item.Key, item.Value);
		this.parent.Add(scopedItem);
	}

	public void Clear() {
		foreach (string key in this.parent.Keys.Where(this.IsKeyInScope).ToArray()) {
			this.parent.Remove(key);
		}
	}

	public bool Contains(KeyValuePair<string, TValue> item) {
		var scopedItem = new KeyValuePair<string, TValue>(this.scope + item.Key, item.Value);
		return this.parent.Contains(scopedItem);
	}

	public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex) {
		throw new NotImplementedException();
	}

	public bool Remove(KeyValuePair<string, TValue> item) {
		var scopedItem = new KeyValuePair<string, TValue>(this.scope + item.Key, item.Value);
		return this.parent.Remove(scopedItem);
	}

	public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator() {
		return this.parent.Where(pair => this.IsKeyInScope(pair.Key))
		           .Select(pair => new KeyValuePair<string, TValue>(
			                   pair.Key.Substring(this.scope.Length), pair.Value))
		           .GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return this.GetEnumerator();
	}
}