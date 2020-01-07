#addin nuget:?package=Cake.Coverlet&version=2.3.4
#tool nuget:?package=ReportGenerator&version=4.4.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

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
    CleanDirectories("./src/**/bin");
    CleanDirectories("./src/**/obj");
    CleanDirectories("./test/**/bin");
    CleanDirectories("./test/**/obj");
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
    .Does(() =>
{
	DotNetCoreBuild(".", new DotNetCoreBuildSettings
	{
        NoRestore = true,
		Configuration = configuration,
	});
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
	DotNetCoreTest(".", new DotNetCoreTestSettings 
	{ 
        NoRestore = true,
        NoBuild = true,
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
		Configuration = configuration,
	};

    var coverletSettings = new CoverletSettings {
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

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Test");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
