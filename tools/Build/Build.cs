using System.IO.Compression;

return BuildRunner.Execute(args, build =>
{
	var dotNetBuildSettings = new DotNetBuildSettings
	{
		NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
	};

	build.AddDotNetTargets(dotNetBuildSettings);

	build.Target("package")
		.Describe("Creates NuGet packages and release zips")
		.Does(() =>
		{
			var releasePath = Path.Combine(Environment.CurrentDirectory, "release");
			var frameworkPublishPath = Path.Combine(releasePath, "Facility.LanguageServer");
			var windowsPublishPath = Path.Combine(releasePath, "Facility.LanguageServer-win-x64");

			Directory.CreateDirectory(releasePath);
			Publish(frameworkPublishPath);
			Publish(windowsPublishPath, "--runtime", "win-x64", "-p:PublishSingleFile=true", "--self-contained", "false");
			Zip(frameworkPublishPath, Path.Combine(releasePath, "Facility.LanguageServer.zip"));
			Zip(windowsPublishPath, Path.Combine(releasePath, "Facility.LanguageServer-win-x64.zip"));
		});

	void Publish(string outputPath, params string[] args)
	{
		if (Directory.Exists(outputPath))
			Directory.Delete(outputPath, recursive: true);

		RunDotNet(new[] { "publish", "--configuration", dotNetBuildSettings.GetConfiguration(), "--no-build", "--output", outputPath, "src/Facility.LanguageServer/Facility.LanguageServer.csproj" }.Concat(args));
	}

	static void Zip(string sourceDirectory, string zipPath)
	{
		if (File.Exists(zipPath))
			File.Delete(zipPath);

		ZipFile.CreateFromDirectory(sourceDirectory, zipPath);
	}
});
