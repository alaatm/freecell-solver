#addin nuget:?package=Cake.Npm&version=1.0.0
#addin nuget:?package=Cake.Coverlet&version=2.5.4
#tool nuget:?package=ReportGenerator&version=4.8.7

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
    NpmInstall(new NpmInstallSettings { WorkingDirectory = "./src/visualizer" });
    NpmRunScript(new NpmRunScriptSettings { WorkingDirectory = "./src/visualizer", ScriptName = "build" });
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
