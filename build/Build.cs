using System;
using GlavKod.Nuke.Components;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;

class Build : NukeBuild
{
    static readonly int DegreeOfParallelism = Environment.ProcessorCount;

    [Parameter]
    public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;

    [Solution(GenerateProjects = true)] readonly Solution Solution = null!;

    [GitRepository] readonly GitRepository GitRepository = null!;

    [Parameter] readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter] readonly string NugetApiKey;
    
    [Parameter] readonly string NugetSource = "https://api.nuget.org/v3/index.json";

    readonly TeamCity TeamCity = TeamCity.Instance;

    static AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    BuildVersion BuildVersion { get; set; } = null!;

    public static int Main() => Execute<Build>();

    protected override void OnBuildInitialized()
    {
        base.OnBuildInitialized();

        Log.Information("Current Branch: {GitBranch}", GitRepository.Branch);

        CalculateVersions();
    }

    void CalculateVersions()
    {
        Log.Information("Calculating Versions");

        var buildNumber = TeamCity?.BuildNumber ?? "0";

        BuildVersion = BuildVersion.Create(timeZoneId: TimeZone,
                                           gitRepo: GitRepository,
                                           buildNumber: buildNumber);

        Log.Information("BuildVersion: {@BuildVersion}", BuildVersion);
    }


    Target DotNetClean => x =>
    {
        return x
            .Executes(() =>
            {
                TeamCity?.StartProgress(nameof(DotNetClean));

                DotNetTasks.DotNetClean(x => x.SetProject(Solution)
                                              .SetConfiguration(Configuration));

                TeamCity?.FinishProgress(nameof(DotNetClean));
            });
    };

    Target DotNetRestore => x =>
    {
        return x
               .DependsOn(DotNetClean)
               .Executes(() =>
               {
                   TeamCity?.StartProgress(nameof(DotNetRestore));

                   DotNetTasks.DotNetRestore(x => x.SetProjectFile(Solution));

                   TeamCity?.FinishProgress(nameof(DotNetRestore));
               });
    };

    Target DotNetCompile => x =>
    {
        return x
               .DependsOn(DotNetRestore)
               .Executes(() =>
               {
                   TeamCity?.StartProgress(nameof(DotNetCompile));

                   DotNetTasks.DotNetBuild(x => x.SetProjectFile(Solution)
                                                 .SetConfiguration(Configuration));

                   TeamCity?.FinishProgress(nameof(DotNetCompile));
               });
    };

    Target DotNetPack => x =>
    {
        return x
               .DependsOn(DotNetCompile)
               .Executes(() =>
               {
                   TeamCity?.StartProgress(nameof(DotNetPack));

                   Log.Information("Cleaning artifacts directory");
                   ArtifactsDirectory.CreateOrCleanDirectory();

                   DotNetTasks.DotNetPack(x => x.SetConfiguration(Configuration)
                                                .SetNoBuild(true)
                                                .SetNoRestore(true)
                                                .SetProject(Solution.Path)
                                                .SetVersion(BuildVersion.NuGetVersion)
                                                .SetAuthors("GlavKod")
                                                .SetOutputDirectory(ArtifactsDirectory)
                                         );

                   TeamCity?.FinishProgress(nameof(DotNetPack));
               });
    };

    [PublicAPI]
    Target PushPackages => x =>
    {
        return x
               .DependsOn(DotNetPack)
               .Executes(() =>
               {
                   TeamCity?.StartProgress(nameof(PushPackages));
                   
                   DotNetTasks.DotNetNuGetPush(x => x.SetApiKey(NugetApiKey)
                                                     .SetSource(NugetSource)
                                                     .CombineWith(ArtifactsDirectory.GetFiles(), (s, path) =>
                                                     {
                                                         return s.SetTargetPath(path);
                                                     }), DegreeOfParallelism);

                   TeamCity?.FinishProgress(nameof(PushPackages));
               });
    };
}