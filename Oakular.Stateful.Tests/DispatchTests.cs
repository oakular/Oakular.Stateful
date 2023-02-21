using FluentAssertions;

namespace Oakular.Stateful.Tests;

public class DispatchTests
{
    public static class Actions
    {
        public static Mutator<IList<string>> Append(string @string)
            => (IList<string> _)
            => _.Append(@string).ToList();
    }

    [Fact(DisplayName = "Dispatching an action modifies the state.")]
    public void DispatchingActionModifiesState()
    {
        var sut = new State<IList<string>>(Array.Empty<string>());
        sut.Dispatch(Actions.Append("test"));

        _ = from _ in sut
            select _.Single().Should().Be("test");
    }
}
