using Synaptix.Analyzers;
using Synaptix.Analyzers.Tests;

public sealed class STBL0001Tests
{
    private static readonly Synaptix0001NoBannedSuffixesAnalyzer Analyzer = new();

    [Fact]
    public async Task FlagsClassWithDtoSuffix()
    {
        const string source = "public sealed class UserDto { }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0001", 0, 0, 0, 0, new[] { "UserDto", "Dto" }));
    }

    [Fact]
    public async Task DoesNotFlagNormalClassName()
    {
        const string source = "public sealed class UserProfile { }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Theory]
    [InlineData("Dto")]
    [InlineData("Model")]
    [InlineData("Impl")]
    [InlineData("Util")]
    [InlineData("Utility")]
    [InlineData("ViewModel")]
    public async Task FlagsAllBannedSuffixes(string suffix)
    {
        string source = $"public sealed class Foo{suffix} {{ }}";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0001", 0, 0, 0, 0, new[] { $"Foo{suffix}", suffix }));
    }

    [Fact]
    public async Task FlagsRecord()
    {
        const string source = "public sealed record CustomerDto(string Name);";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0001", 0, 0, 0, 0, new[] { "CustomerDto", "Dto" }));
    }

    [Fact]
    public async Task FlagsInterface()
    {
        const string source = "public interface IServiceUtil { }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0001", 0, 0, 0, 0, new[] { "IServiceUtil", "Util" }));
    }

    [Fact]
    public async Task AllowsRoleSpecificSuffixes()
    {
        const string source = "public sealed class UserSummary { }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task AllowsManagerSuffix()
    {
        const string source = "public sealed class ConnectionManager { }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task AllowsServiceSuffix()
    {
        const string source = "public sealed class PaymentService { }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }
}
