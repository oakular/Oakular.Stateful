using FluentAssertions;

namespace Oakular.Stateful.Tests;

public class SubscribeTests
{
    public class Observer : IObserver<IList<string>>
    {
        public IList<string> Value { get; private set; }

        public void OnCompleted() => throw new NotImplementedException();

        public void OnError(Exception error) => throw new NotImplementedException();

        public void OnNext(IList<string> value) => Value = value;
    }

    [Fact(DisplayName = "State change is propagated when subscribing")]
    public void SubscribingCausesStateChangeToPropagate()
    {
        var observer = new Observer();
        var sut = new State<IList<string>>(Array.Empty<string>());

        sut.Subscribe(observer);
        sut.Dispatch(_ => _.Append("test").ToList());

        sut.Select(_ => _.Should().BeEquivalentTo(observer.Value));
    }
}