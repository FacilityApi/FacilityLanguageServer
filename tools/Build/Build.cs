using System.IO.Compression;

return BuildRunner.Execute(args, build =>
{
	var dotNetBuildSettings = new DotNetBuildSettings
	{
		NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
		PackageSettings = new DotNetPackageSettings
		{
			PushTagOnPublish = x => $"nuget.{x.Version}",
		},
	};

	build.AddDotNetTargets(dotNetBuildSettings);

	build.Target("package")
		.Describe("Creates NuGet packages and release zips")
		.Does(() =>
		{
			var releasePath = Path.Combine(Environment.CurrentDirectory, "release");
			var frameworkPublishPath = Path.Combine(releasePath, "Facility.LanguageServer");

			Directory.CreateDirectory(releasePath);
			Publish(frameworkPublishPath);
			Zip(frameworkPublishPath, Path.Combine(releasePath, "Facility.LanguageServer.zip"));
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
