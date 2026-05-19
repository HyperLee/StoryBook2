using StoryBook.Tests.Support;

namespace StoryBook.Tests.Unit;

public sealed class CompareScriptContractTests
{
    [Fact]
    public void Compare_script_keeps_selection_state_page_local_and_exposes_required_hooks()
    {
        string script = File.ReadAllText(Path.Combine(TestPaths.StoryBookRoot, "wwwroot", "js", "compare.js"));

        Assert.Contains("data-compare-first-select", script);
        Assert.Contains("data-compare-second-select", script);
        Assert.Contains("data-compare-clear-selection", script);
        Assert.Contains("data-compare-status", script);
        Assert.Contains("data-compare-duplicate-message", script);
        Assert.Contains("data-compare-one-selected-message", script);
        Assert.Contains("data-compare-table", script);
        Assert.Contains("data-compare-candidate", script);
        Assert.Contains("data-compare-preserve-state-on-theme-change", script);
        Assert.DoesNotContain("pushState", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("replaceState", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("location.search", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("localStorage.setItem", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sessionStorage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("document.cookie", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("fetch(", script, StringComparison.OrdinalIgnoreCase);
    }
}
