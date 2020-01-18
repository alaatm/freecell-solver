#addin nuget:?package=Cake.Yarn&version=0.4.6
#addin nuget:?package=Cake.Coverlet&version=2.3.4
#addin nuget:?package=Cake.Coveralls&version=0.10.1
#addin nuget:?package=Cake.Json&version=4.0.0
#addin nuget:?package=Newtonsoft.Json&version=11.0.2
#tool nuget:?package=ReportGenerator&version=4.4.0
#tool nuget:?package=coveralls.net&version=1.0.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

Setup(context =>
{
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories("./dist/" + configuration);
    CleanDirectories("./src/**/bin/" + configuration);
    CleanDirectories("./src/**/obj");
    CleanDirectories("./coverage/**");
    if (FileExists("./lcov.info")) DeleteFile("./lcov.info");    
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreRestore(".");
});

Task("Build")
    .IsDependentOn("Restore")
    .IsDependentOn("VisualizerBuild")
    .Does(() =>
{
    DotNetCoreBuild(".", new DotNetCoreBuildSettings
    {
        NoRestore = true,
        OutputDirectory = Directory("./dist/" + configuration),
        Configuration = configuration,
    });
});

Task("VisualizerBuild")
    .Does(() =>
{
    Yarn.FromPath("./src/visualizer").Install();
    Yarn.FromPath("./src/visualizer").RunScript("build");
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest(".", new DotNetCoreTestSettings 
    { 
        NoRestore = true,
        NoBuild = true,
        OutputDirectory = Directory("./dist/" + configuration),
        Configuration = configuration,
    });
});

Task("Cover")
    .IsDependentOn("Build")
    .Does(() =>
{
    var testSettings = new DotNetCoreTestSettings 
    { 
        NoRestore = true,
        NoBuild = true,
        OutputDirectory = Directory("./dist/" + configuration),
        Configuration = configuration,
    };

    var coverletSettings = new CoverletSettings
    {
        CollectCoverage = true,
        CoverletOutputFormat = CoverletOutputFormat.lcov | CoverletOutputFormat.opencover,
        CoverletOutputDirectory = Directory("./coverage/"),
        CoverletOutputName = $"{DateTime.UtcNow.Ticks}",
        Exclude = new List<string> { "[*]FreeCellSolver.Entry.*", "[*]FreeCellSolver.Drawing.*" },
    };

    DotNetCoreTest(".", testSettings, coverletSettings);

    // Copy lcov file to root for code coverage highlight in vscode
    var lcov = GetFiles("./coverage/*.info").Single();
    CopyFile(lcov, "./lcov.info");

    // Generate coverage report
    var opencover = GetFiles("./coverage/*.opencover.xml").Single();
    ReportGenerator(File(opencover.FullPath), Directory("./coverage"));
});

Task("UploadCoverageReport")
    .IsDependentOn("Cover")
    .Does(() =>
{
    var githubActionsFlag = EnvironmentVariable("GITHUB_ACTIONS");
    if (githubActionsFlag != "true")
    {
        Warning("Task 'UploadCoverageReport' can only run when CI is executing in Github actions.");
        return;
    }

    var repoToken = EnvironmentVariable("GITHUB_TOKEN");
    var commit = EnvironmentVariable("GITHUB_SHA");
    var branch = EnvironmentVariable("GITHUB_REF");
    var eventName = EnvironmentVariable("GITHUB_EVENT_NAME");
    string jobId;

    if (eventName == "pull_request")
    {
        var event = FileReadText(EnvironmentVariable("GITHUB_EVENT_PATH"));
        var pr = ParseJson(event)["pr"];
        jobId = $"{commit}-PR-${pr}";
    }
    else
    {
        jobId = commit;
    }
    
    Information($"repoToken={repoToken}");
    Information($"commit={commit}");
    Information($"branch={branch}");
    Information($"eventName={eventName}");
    Information($"jobId={jobId}");
    return;

    var opencover = GetFiles("./coverage/*.opencover.xml").Single();
    CoverallsNet(opencover.FullPath, CoverallsNetReportType.OpenCover, new CoverallsNetSettings()
    {
        RepoToken = repoToken,
        CommitId = commit,
        CommitBranch = branch,
        JobId = jobId,
    });
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Test");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
