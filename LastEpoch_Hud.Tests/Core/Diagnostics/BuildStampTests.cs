using LastEpoch_Hud.Scripts.Core.Diagnostics;

namespace LastEpoch_Hud.Tests.Core.Diagnostics;

/// <summary>The build line written to the log at startup.</summary>
public sealed class BuildStampTests
{
    private const string BuiltAt = "2026-10-04 10:30";

    [Fact]
    public void Format_CleanCommit_ReturnsCommitAndTime()
    {
        Assert.Equal("Build 0bd3576 (2026-10-04 10:30)", BuildStamp.Format("0bd3576", false, BuiltAt));
    }

    [Fact]
    public void Format_DirtyCommit_AddsDirtySuffix()
    {
        Assert.Equal("Build 0bd3576+dirty (2026-10-04 10:30)", BuildStamp.Format("0bd3576", true, BuiltAt));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Format_EmptyCommit_ReturnsUnknown(string commit)
    {
        Assert.Equal("Build unknown (2026-10-04 10:30)", BuildStamp.Format(commit, false, BuiltAt));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Format_EmptyCommitAndDirty_IgnoresDirtyFlag(string commit)
    {
        Assert.Equal("Build unknown (2026-10-04 10:30)", BuildStamp.Format(commit, true, BuiltAt));
    }
}
