using System;
using GlavKod.Nuke.Components;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;
using Serilog;

class Build : NukeBuild
{
    static readonly int DegreeOfParallelism = Environment.ProcessorCount;

    [Parameter]
    public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;

    [GitRepository] readonly GitRepository GitRepository = null!;

    [Parameter] readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter] readonly string NugetApiKey;
    
    [Parameter] readonly string NugetSource = "https://api.nuget.org/v3/index.json";

    /// <summary>
    /// Образ .NET SDK, в котором собираются и упаковываются пакеты. Версия задана явно, потому что
    /// source-генератор компилируется против Roslyn из <c>Microsoft.CodeAnalysis.CSharp</c>
    /// и не загружается компилятором более старой версии (CS9057).
    /// </summary>
    [Parameter] readonly string DotNetSdkImage = "mcr.microsoft.com/dotnet/sdk:10.0.400";

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

    /// <summary>
    /// Собирает и упаковывает решение внутри контейнера, а готовые <c>.nupkg</c> выгружает
    /// в <see cref="ArtifactsDirectory" />. Компиляция вынесена из агента в контейнер, чтобы
    /// версия .NET SDK была задана явно и не зависела от того, что установлено на агенте.
    /// </summary>
    Target DotNetPack => x =>
    {
        return x
               .Executes(() =>
               {
                   TeamCity?.StartProgress(nameof(DotNetPack));

                   Log.Information("Cleaning artifacts directory");
                   ArtifactsDirectory.CreateOrCleanDirectory();

                   Log.Information("Packing in {DotNetSdkImage}", DotNetSdkImage);

                   // BuildKit нужен для `--output` и кеш-mount в Dockerfile. В Docker 23 и новее он
                   // включён по умолчанию, переменная страхует от более старого демона на агенте.
                   DockerTasks.DockerBuild(x => x.SetProcessEnvironmentVariable("DOCKER_BUILDKIT", "1")
                                                 .SetPath(RootDirectory)
                                                 .SetFile(RootDirectory / "Dockerfile")
                                                 .SetTarget("artifacts")
                                                 .SetOutput($"type=local,dest={ArtifactsDirectory}")
                                                 .SetProgress(DockerProgressType.plain)
                                                 .AddBuildArg($"DOTNET_SDK_IMAGE={DotNetSdkImage}")
                                                 .AddBuildArg($"CONFIGURATION={Configuration}")
                                                 .AddBuildArg($"PACKAGE_VERSION={BuildVersion.NuGetVersion}")
                                                 .AddBuildArg("PACKAGE_AUTHORS=GlavKod")
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
