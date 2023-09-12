return BuildRunner.Execute(args, build =>
{
	var gitLogin = new GitLoginInfo("FacilityApiBot", Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD") ?? "");

	var dotNetBuildSettings = new DotNetBuildSettings
	{
		NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
		DocsSettings = new DotNetDocsSettings
		{
			GitLogin = gitLogin,
			GitAuthor = new GitAuthorInfo("FacilityApiBot", "facilityapi@gmail.com"),
			SourceCodeUrl = "https://github.com/FacilityApi/FacilityLanguageServer/tree/master/src",
		},
	};

	build.AddDotNetTargets(dotNetBuildSettings);

	build.Target("package")
		.Describe("Builds the publishable output")
		.ClearActions()
		.Does(() =>
		{
			RunDotNet("publish", "--configuration", "Release", "src/Facility.LanguageServer/Facility.LanguageServer.csproj");
			RunDotNet("publish", "--configuration", "Release", "--runtime", "win-x64", "-p:PublishSingleFile=true", "--self-contained", "false", "src/Facility.LanguageServer/Facility.LanguageServer.csproj");
		});
});
