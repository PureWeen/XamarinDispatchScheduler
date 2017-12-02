//////////////////////////////////////////////////////////////////////
// ADDINS
//////////////////////////////////////////////////////////////////////

#addin "Cake.FileHelpers"
#addin "Cake.Coveralls"
#addin "Cake.PinNuGetDependency"

//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////

#tool "GitReleaseManager"
#tool "GitVersion.CommandLine"
#tool "coveralls.io"
#tool "OpenCover"
#tool "ReportGenerator"
#tool nuget:?package=vswhere

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

// Should MSBuild & GitLink treat any errors as warnings?
var treatWarningsAsErrors = false;
var githubUrl = "https://github.com/PureWeen/XamarinDispatchScheduler";
var msBuildPath = VSWhereLatest().CombineWithFilePath("./MSBuild/15.0/Bin/MSBuild.exe");

// Version
var gitVersion = GitVersion();
var majorMinorPatch = gitVersion.MajorMinorPatch;
var informationalVersion = gitVersion.InformationalVersion;
var nugetVersion = gitVersion.NuGetVersion;
var buildVersion = gitVersion.FullBuildMetaData;

// Artifacts
var artifactDirectory = "./artifacts/";
var packageWhitelist = new[] { "XamarinDispatchScheduler" }; 

// Macros
Action Abort = () => { throw new Exception("a non-recoverable fatal error occurred."); };

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup((context) =>
{
    Information("Building version {0} of XamarinDispatchScheduler. Nuget Version {2}", informationalVersion, nugetVersion);
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
            .WithTarget("restore;build;pack")
            .WithProperty("PackageOutputPath",  MakeAbsolute(Directory(artifactDirectory)).ToString())
            .WithProperty("TreatWarningsAsErrors", treatWarningsAsErrors.ToString())
            .SetConfiguration("Release")          
            // Due to https://github.com/NuGet/Home/issues/4790 and https://github.com/NuGet/Home/issues/4337 we
            // have to pass a version explicitly
            .WithProperty("Version", nugetVersion.ToString())
            .SetVerbosity(Verbosity.Minimal)
            .SetNodeReuse(false));
			 
    };

    build("./XamarinDispatcher.sln");
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
    // only pin whitelisted packages.
    foreach(var package in packageWhitelist)
    {
        // only pin the package which was created during this build run.
        var packagePath = artifactDirectory + File(string.Concat(package, ".", nugetVersion, ".nupkg"));

        // see https://github.com/cake-contrib/Cake.PinNuGetDependency
        PinNuGetDependency(packagePath, "XamarinDispatchScheduler");
    }
});

Task("PublishPackages")
    .IsDependentOn("Package")
    .WithCriteria(() => !local)
    .WithCriteria(() => !isPullRequest)
    .WithCriteria(() => isRepository)
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
});

Task("CreateRelease")
    .IsDependentOn("RunUnitTests")
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
    .IsDependentOn("RunUnitTests")
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
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("CreateRelease")
    .IsDependentOn("PublishPackages")
    .IsDependentOn("PublishRelease")
    .Does (() =>
{

});


//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
