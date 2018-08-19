//////////////////////////////////////////////////////////////////////
// ADDINS
//////////////////////////////////////////////////////////////////////

#addin "nuget:?package=Cake.FileHelpers&version=3.0.0"
#addin "nuget:?package=Cake.Coveralls&version=0.9.0"
#addin "nuget:?package=Cake.PinNuGetDependency&version=3.0.1"

//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////

#tool "nuget:?package=GitReleaseManager&version=0.7.1"
#tool "nuget:?package=coveralls.io&version=1.4.2"
#tool "nuget:?package=OpenCover&version=4.6.519"
#tool "nuget:?package=ReportGenerator&version=3.1.2"
#tool "nuget:?package=vswhere&version=2.5.2"
#tool "nuget:?package=xunit.runner.console&version=2.4.0"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
if (string.IsNullOrWhiteSpace(target))
{
    target = "Default";
}

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var treatWarningsAsErrors = false;

// Build configuration
var local = BuildSystem.IsLocalBuild;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;
var isRepository = StringComparer.OrdinalIgnoreCase.Equals("PureWeen/XamarinDispatchScheduler", AppVeyor.Environment.Repository.Name);

var isDevelopBranch = StringComparer.OrdinalIgnoreCase.Equals("develop", AppVeyor.Environment.Repository.Branch);
var isReleaseBranch = StringComparer.OrdinalIgnoreCase.Equals("master", AppVeyor.Environment.Repository.Branch);
var isTagged = AppVeyor.Environment.Repository.Tag.IsTag;

var githubOwner = "PureWeen";
var githubRepository = "XamarinDispatchScheduler";
var githubUrl = "https://github.com/PureWeen/XamarinDispatchScheduler";
var msBuildPath = VSWhereLatest().CombineWithFilePath("./MSBuild/15.0/Bin/MSBuild.exe");


// Version
var informationalVersion = EnvironmentVariable("GitAssemblyInformationalVersion");


// Artifacts
var artifactDirectory = "./artifacts/";
var packageWhitelist = new[] { "Xam.Reactive.DispatchScheduler" }; 

// Macros
Action Abort = () => { throw new Exception("a non-recoverable fatal error occurred."); };

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup((context) =>
{
	Information("Building version {0} of XamarinDispatchScheduler.", informationalVersion);
	CreateDirectory(artifactDirectory);
});

Teardown((context) =>
{
    // Executed AFTER the last task.
});



Task("Build")
    .Does (() =>
{
    Action<string> build = (solution) =>
    {
        Information("Building {0}", solution);


        MSBuild(solution, new MSBuildSettings() {
                ToolPath= msBuildPath
            }
            .WithTarget("build;pack")
             .WithProperty("PackageOutputPath",  MakeAbsolute(Directory(artifactDirectory)).ToString().Quote())
            .WithProperty("TreatWarningsAsErrors", treatWarningsAsErrors.ToString())
            .SetConfiguration("Release")
            .SetVerbosity(Verbosity.Minimal)
            .SetNodeReuse(false));
			 
    };

    build("./XamarinDispatchScheduler/XamarinDispatchScheduler.csproj");
});


Task("Package")
   .IsDependentOn("Build")
   .IsDependentOn("PinNuGetDependencies")
    .Does (() =>
{


});

Task("PinNuGetDependencies")
    .Does (() =>
{
	 var packages = GetFiles(artifactDirectory + "*.nupkg");
    foreach(var package in packages)
    {
        // only pin whitelisted packages.
        if(packageWhitelist.Any(p => package.GetFilename().ToString().StartsWith(p, StringComparison.OrdinalIgnoreCase)))
       
			// see https://github.com/cake-contrib/Cake.PinNuGetDependency
			PinNuGetDependency(package, "XamarinDispatchScheduler");
    }
});

/*Task("PublishPackages")
    .IsDependentOn("Package")
    .WithCriteria(() => !local)
    .WithCriteria(() => isDevelopBranch || isReleaseBranch)
    .Does (() =>
{
    if (isReleaseBranch && !isTagged)
    {
        Information("Packages will not be published as this release has not been tagged.");
        return;
    }

    // Resolve the API key.
    var apiKey = EnvironmentVariable("NUGET_APIKEY");
    if (string.IsNullOrEmpty(apiKey))
    {
        throw new Exception("The NUGET_APIKEY environment variable is not defined.");
    }

    var source = EnvironmentVariable("NUGET_SOURCE");
    if (string.IsNullOrEmpty(source))
    {
        throw new Exception("The NUGET_SOURCE environment variable is not defined.");
    }

    // only push whitelisted packages.
    foreach(var package in packageWhitelist)
    {
        // only push the package which was created during this build run.
        var packagePath = artifactDirectory + File(string.Concat(package, ".", nugetVersion, ".nupkg"));

        // Push the package.
        NuGetPush(packagePath, new NuGetPushSettings {
            Source = source,
            ApiKey = apiKey
        });
    }
});*/
/*
Task("CreateRelease")
    .IsDependentOn("Package")
    .WithCriteria(() => !local)
    .WithCriteria(() => !isPullRequest)
    .WithCriteria(() => isRepository)
    .WithCriteria(() => isReleaseBranch)
    .WithCriteria(() => !isTagged)
    .Does (() =>
{
    var username = EnvironmentVariable("GITHUB_USERNAME");
    if (string.IsNullOrEmpty(username))
    {
        throw new Exception("The GITHUB_USERNAME environment variable is not defined.");
    }

    var token = EnvironmentVariable("GITHUB_TOKEN");
    if (string.IsNullOrEmpty(token))
    {
        throw new Exception("The GITHUB_TOKEN environment variable is not defined.");
    }

    GitReleaseManagerCreate(username, token, githubOwner, githubRepository, new GitReleaseManagerCreateSettings {
        Milestone         = majorMinorPatch,
        Name              = majorMinorPatch,
        Prerelease        = true,
        TargetCommitish   = "master"
    });
});

Task("PublishRelease")
    .IsDependentOn("Package")
    .WithCriteria(() => !local)
    .WithCriteria(() => !isPullRequest)
    .WithCriteria(() => isRepository)
    .WithCriteria(() => isReleaseBranch)
    .WithCriteria(() => isTagged)
    .Does (() =>
{
    var username = EnvironmentVariable("GITHUB_USERNAME");
    if (string.IsNullOrEmpty(username))
    {
        throw new Exception("The GITHUB_USERNAME environment variable is not defined.");
    }

    var token = EnvironmentVariable("GITHUB_TOKEN");
    if (string.IsNullOrEmpty(token))
    {
        throw new Exception("The GITHUB_TOKEN environment variable is not defined.");
    }

    // only push whitelisted packages.
    foreach(var package in packageWhitelist)
    {
        // only push the package which was created during this build run.
        var packagePath = artifactDirectory + File(string.Concat(package, ".", nugetVersion, ".nupkg"));

        GitReleaseManagerAddAssets(username, token, githubOwner, githubRepository, majorMinorPatch, packagePath);
    }

    GitReleaseManagerClose(username, token, githubOwner, githubRepository, majorMinorPatch);
});*/

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Package")
   // .IsDependentOn("CreateRelease")
   // .IsDependentOn("PublishPackages")
  //  .IsDependentOn("PublishRelease")
    .Does (() =>
{

});


//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
