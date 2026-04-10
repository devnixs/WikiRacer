using System.Xml.Linq;

namespace WikiRacer.ArchitectureTests;

public class ArchitectureDependencyTests
{
    [Fact]
    public void Domain_Should_Not_Reference_Transport_Or_Infrastructure_Assemblies()
    {
        var references = GetProjectReferences("backend/src/WikiRacer.Domain/WikiRacer.Domain.csproj");

        Assert.DoesNotContain("WikiRacer.Api", references);
        Assert.DoesNotContain("WikiRacer.Infrastructure", references);
        Assert.DoesNotContain("WikiRacer.Contracts", references);
    }

    [Fact]
    public void Application_Should_Only_Depend_On_Domain_From_Project_Assemblies()
    {
        var references = GetProjectReferences("backend/src/WikiRacer.Application/WikiRacer.Application.csproj");

        Assert.Contains("WikiRacer.Domain", references);
        Assert.DoesNotContain("WikiRacer.Api", references);
        Assert.DoesNotContain("WikiRacer.Infrastructure", references);
        Assert.DoesNotContain("WikiRacer.Contracts", references);
    }

    [Fact]
    public void Contracts_Should_Not_Reference_Other_Project_Assemblies()
    {
        var references = GetProjectReferences("backend/src/WikiRacer.Contracts/WikiRacer.Contracts.csproj");

        Assert.Empty(references);
    }

    [Fact]
    public void Infrastructure_Should_Not_Reference_Api_Or_Contracts()
    {
        var references = GetProjectReferences("backend/src/WikiRacer.Infrastructure/WikiRacer.Infrastructure.csproj");

        Assert.Contains("WikiRacer.Application", references);
        Assert.Contains("WikiRacer.Domain", references);
        Assert.DoesNotContain("WikiRacer.Api", references);
        Assert.DoesNotContain("WikiRacer.Contracts", references);
    }

    [Fact]
    public void Api_Should_Not_Reference_Domain_Directly()
    {
        var references = GetProjectReferences("backend/src/WikiRacer.Api/WikiRacer.Api.csproj");

        Assert.Contains("WikiRacer.Application", references);
        Assert.Contains("WikiRacer.Infrastructure", references);
        Assert.Contains("WikiRacer.Contracts", references);
        Assert.DoesNotContain("WikiRacer.Domain", references);
    }

    private static IReadOnlySet<string> GetProjectReferences(string projectPathFromRoot)
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectPath = Path.Combine(repositoryRoot.FullName, projectPathFromRoot);
        var project = XDocument.Load(projectPath);

        return project
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Select(include => include?.Replace('\\', Path.DirectorySeparatorChar))
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);
    }

    private static DirectoryInfo FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null && !File.Exists(Path.Combine(current.FullName, "WikiRacer.slnx")))
        {
            current = current.Parent;
        }

        return current ?? throw new DirectoryNotFoundException("Could not locate the repository root from the test output directory.");
    }
}
