#addin nuget:?package=Cake.Yarn&version=0.4.6
#addin nuget:?package=Cake.Coverlet&version=2.4.2
#tool nuget:?package=ReportGenerator&version=4.5.8

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
    if (IsRunningOnWindows())
    {
        var opencover = GetFiles("./coverage/*.opencover.xml").Single();
        ReportGenerator(File(opencover.FullPath), Directory("./coverage"));
    }
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
