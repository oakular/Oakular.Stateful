using System.Collections.Immutable;

namespace Oakular.Stateful;

/// <summary>
/// The state manager of <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class State<T> : IObservable<T>
{
    private IImmutableList<IObserver<T>> _observers = ImmutableList.Create<IObserver<T>>();
    private T _state;

    ///
    public State(T initialState) => _state = initialState;

    /// <summary>
    /// Dispatches an action to modify the state <typeparamref name="T"/>.
    /// </summary>
    /// <param name="action">Action to mutate the state.</param>
    public void Dispatch(Mutator<T> action)
    {
        try
        {
            _state = action(_state);

            foreach (var observer in _observers)
            {
                observer.OnNext(_state);
            }
        }
        catch (Exception ex)
        {
            foreach (var observer in _observers)
            {
                observer.OnError(ex);
            }

            throw;
        }
    }

    /// <summary>
    /// Exposes the underlying state <typeparamref name="T"/> for access.
    /// </summary>
    /// <remarks>
    /// This method should <b>not</b> be used to modify the state of <typeparamref name="T"/>.
    /// Any modifications made here will not propagate to subscribers.
    /// To modify state, use <see cref="Dispatch(Mutator{T})"/>.
    /// </remarks>
    /// <typeparam name="R"></typeparam>
    /// <param name="selector"></param>
    /// <returns>A new state instance.</returns>
    public State<R> Select<R>(Func<T, R> selector) => new(selector(_state));

    /// <summary>
    /// Subscribe the <paramref name="observer"/> to state mutations.
    /// </summary>
    /// <param name="observer"></param>
    /// <returns>An <see cref="IDisposable"/> that can be disposed to unsubscribe <paramref name="observer"/>.</returns>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        _observers = _observers.Add(observer);
        return new Unsubscriber(this, observer);
    }

    readonly struct Unsubscriber : IDisposable
    {
        private readonly State<T> store;
        private readonly IObserver<T> observer;

        internal Unsubscriber(State<T> store, IObserver<T> observer)
        {
            this.store = store;
            this.observer = observer;
        }

        public void Dispose()
        {
            if (store._observers.Contains(observer))
            {
                store._observers = store._observers.Remove(observer);
            }
        }
    }
}

/// <summary>
/// Defines a mutating action on <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The state type.</typeparam>
/// <param name="state">The current state.</param>
/// <returns></returns>
public delegate T Mutator<T>(T state);