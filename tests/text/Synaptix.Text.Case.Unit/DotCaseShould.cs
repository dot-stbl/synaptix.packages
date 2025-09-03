using FluentAssertions;

namespace Synaptix.Text.Case.Unit;

public class StringExtensionsToDotCase
{
    [Theory]
    [InlineData(null)]
    public void ReturnArgumentNullException(string? source = null)
    {
        Action act = () => source.ToDotCase();
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("api/users/32/someActionToDo?param=%a%")]
    [InlineData("Api/Users/32/SomeActionToDo?Param=%A%")]
    [InlineData("api/users/32/some-action-to-do?param=%a%")]
    [InlineData("api/users/32/Some-Action-To-Do?Param=%a%")]
    [InlineData("api/users/32/some_action_to_do?param=%a%")]
    [InlineData("api/users/32/Some_Action_to_do?param=%a%")]
    [InlineData("api/users/32/some.action.to.do?param=%a%")]
    public void ReturnDotCaseUrl(string source)
    {
        const string expectedResult = "api/users/32/some.action.to.do?param=%a%";
        source.ToDotCase().Should().Be(expectedResult);
    }
}