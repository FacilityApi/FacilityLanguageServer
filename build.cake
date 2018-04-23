var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var solutionFileName = "FacilityLanguageServer.sln";

Task("Clean")
	.Does(() =>
	{
		CleanDirectories("src/**/bin");
		CleanDirectories("src/**/obj");
	});

Task("Build")
	.Does(() =>
	{
		DotNetCoreRestore(solutionFileName);
		DotNetCoreBuild(solutionFileName, new DotNetCoreBuildSettings { Configuration = configuration, ArgumentCustomization = args => args.Append("--verbosity normal") });
	});

Task("Rebuild")
	.IsDependentOn("Clean")
	.IsDependentOn("Build");

Task("Default")
	.IsDependentOn("Build");

RunTarget(target);
