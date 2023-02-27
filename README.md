# Oakular.Stateful

![Nuget](https://img.shields.io/nuget/v/Oakular.Stateful?label=NuGet&logo=Nuget&style=flat-square) [![Build](https://github.com/oakular/Oakular.Stateful/actions/workflows/package.yml/badge.svg)](https://github.com/oakular/Oakular.Stateful/actions/workflows/package.yml)

A reduxesque implementation of state management.

## How to use

Stateful is composed solely of the `State<T>` type, which can be scoped at various levels of your environment: you can register it through DI, or set it as a property at the highest level you require if your app is using a component stack. For example, in a MAUI application you can register global state inside the `App` class:

```csharp
public partial class App : Application
{
    public static State<int> CounterState = new(0);

    public static State<ImageViewModel> ImageState = new(default);
}
```

Or, through DI:
```csharp
services.AddSingleton(new State<int>(0));
```

### Subscribing to changes
Consumers can subscribe to changes by implementing `IObserver<T>` and passing themselves to `State<T>.Subscribe`.
```csharp
sealed class CounterPage : IObserver<IEnumerable<ImageViewModel>>
{
    public CounterPage()
    {
        App.CounterState.Subscribe(this);
    }
}
```

`IObserver<T>` forces implementation of `OnNext` and `OnError`, which `State<T>` will invoke for all subscribers when the underlying state changes:

```csharp
public void OnNext(int value)
{
    // do something with the value received.
}

public void OnError(Exception ex)
{
    // handle the error received.
}
```

### Mutations
Modifying state is done through `Dispatch` only. It is important that `Dispatch` is used because it will propagate changes to all subscribers. `Select` can be used to modify the underlying state, but will not cause propagation. Unfortunately, because of the complexity of copying C# reference type, it is very difficult to constrain entirely the mutation of `T`, yet still allow access to its value.

```csharp
// Will propagate to subscribers
App.CounterState.Dispatch(_ => _ + 1);

// Will not propagate to subscribers
App.CounterState.Select(_ => _ + 1);
```
