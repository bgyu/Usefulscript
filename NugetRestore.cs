using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

class NuGetRestorer
{
    private string projectFolder;
    private string outputFolder;
    private string artifactoryUrl;
    private string nugetPackagesFolder;
    private HashSet<(string, string)> downloadedPackages;
    private object lockObject = new object();
    private int maxWorkers;

    public NuGetRestorer(string projectFolder, string outputFolder, string artifactoryUrl, int maxWorkers = 5)
    {
        this.projectFolder = projectFolder;
        this.outputFolder = outputFolder;
        this.artifactoryUrl = artifactoryUrl;
        this.nugetPackagesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget/packages");
        this.downloadedPackages = new HashSet<(string, string)>();
        this.maxWorkers = maxWorkers;

        if (!Directory.Exists(this.nugetPackagesFolder))
        {
            Directory.CreateDirectory(this.nugetPackagesFolder);
        }
    }

    public List<PackageInfo> ReadAllPackages()
    {
        var allPackages = new List<PackageInfo>();

        foreach (var projectFile in Directory.GetFiles(projectFolder, "*.csproj"))
        {
            var packages = ReadPackageReferences(projectFile);
            allPackages.AddRange(packages);
        }

        return allPackages;
    }

    public List<PackageInfo> ReadPackageReferences(string projectFile)
    {
        var packages = new List<PackageInfo>();

        try
        {
            XDocument doc = XDocument.Load(projectFile);
            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

            foreach (var packageReference in doc.Descendants(ns + "PackageReference"))
            {
                var packageName = packageReference.Attribute("Include")?.Value;
                var packageVersion = packageReference.Attribute("Version")?.Value;
                packages.Add(new PackageInfo { Name = packageName, Version = packageVersion });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing {projectFile}: {ex.Message}");
        }

        return packages;
    }

    public async Task<string> DownloadPackageAsync(PackageInfo package)
    {
        var packageUrl = $"{artifactoryUrl}/{package.Name}/{package.Version}/{package.Name}.{package.Version}.nupkg";

        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetAsync(packageUrl);

            if (response.IsSuccessStatusCode)
            {
                var packageFilePath = Path.Combine(outputFolder, $"{package.Name}.{package.Version}.nupkg");

                using (var fileStream = File.Create(packageFilePath))
                {
                    await response.Content.CopyToAsync(fileStream);
                }

                return packageFilePath;
            }
            else
            {
                Console.WriteLine($"Failed to download package {package.Name} version {package.Version}. Status code: {response.StatusCode}");
                return null;
            }
        }
    }

    public void RestorePackage(PackageInfo package)
    {
        lock (lockObject)
        {
            if (!downloadedPackages.Contains((package.Name, package.Version)))
            {
                // Download the package
                var packageFilePath = DownloadPackageAsync(package).Result;

                if (!string.IsNullOrEmpty(packageFilePath))
                {
                    // Extract the package to the NuGet packages folder
                    using (var zip = System.IO.Compression.ZipFile.OpenRead(packageFilePath))
                    {
                        zip.ExtractToDirectory(nugetPackagesFolder, true);
                    }

                    // Cleanup: Remove the downloaded package file
                    File.Delete(packageFilePath);

                    // Update the set of downloaded packages
                    downloadedPackages.Add((package.Name, package.Version));
                }
            }
            else
            {
                Console.WriteLine($"Package {package.Name} version {package.Version} is already downloaded.");
            }
        }
    }

    public void RestorePackages()
    {
        var allPackages = ReadAllPackages();

        Parallel.ForEach(allPackages, new ParallelOptions { MaxDegreeOfParallelism = maxWorkers }, package =>
        {
            RestorePackage(package);
        });
    }
}

class PackageInfo
{
    public string Name { get; set; }
    public string Version { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: dotnet run projectFolder=<project_folder> outputFolder=<output_folder> artifactoryUrl=<artifactory_url> [maxWorkers=<max_workers>]");
            Environment.Exit(1);
        }

        Dictionary<string, string> arguments = args
            .Select(arg => arg.Split('='))
            .ToDictionary(arg => arg[0].ToLower(), arg => arg.Length > 1 ? arg[1] : "");

        string projectFolder = arguments.GetValueOrDefault("projectfolder", "");
        string outputFolder = arguments.GetValueOrDefault("outputfolder", "");
        string artifactoryUrl = arguments.GetValueOrDefault("artifactoryurl", "");
        int maxWorkers = int.Parse(arguments.GetValueOrDefault("maxworkers", "5"));

        if (string.IsNullOrEmpty(projectFolder) || string.IsNullOrEmpty(outputFolder) || string.IsNullOrEmpty(artifactoryUrl))
        {
            Console.WriteLine("Missing required arguments. Usage: dotnet run projectFolder=<project_folder> outputFolder=<output_folder> artifactoryUrl=<artifactory_url> [maxWorkers=<max_workers>]");
            Environment.Exit(1);
        }

        NuGetRestorer nugetRestorer = new NuGetRestorer(projectFolder, outputFolder, artifactoryUrl, maxWorkers);
        nugetRestorer.RestorePackages();
    }
}
