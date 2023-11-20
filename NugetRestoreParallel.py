import os
import sys
import xml.etree.ElementTree as ET
import requests
from zipfile import ZipFile
from concurrent.futures import ThreadPoolExecutor
import threading

class NuGetRestorer:
    def __init__(self, project_folder, output_folder, artifactory_url, max_workers=5):
        self.project_folder = project_folder
        self.output_folder = output_folder
        self.artifactory_url = artifactory_url
        self.nuget_packages_folder = os.path.expanduser('~/.nuget/packages')
        self.downloaded_packages = set()
        self.lock = threading.Lock()
        self.max_workers = max_workers

        if not os.path.exists(self.nuget_packages_folder):
            os.makedirs(self.nuget_packages_folder)

    def read_all_packages(self):
        all_packages = []
        for project_file in os.listdir(self.project_folder):
            if project_file.endswith('.csproj'):
                project_path = os.path.join(self.project_folder, project_file)
                packages = self.read_package_references(project_path)
                all_packages.extend(packages)
        return all_packages

    def read_package_references(self, project_file):
        packages = []
        try:
            tree = ET.parse(project_file)
            root = tree.getroot()
            ns = {'ns': 'http://schemas.microsoft.com/developer/msbuild/2003'}

            for item_group in root.findall('.//ns:ItemGroup', ns):
                for package_reference in item_group.findall('ns:PackageReference', ns):
                    package_name = package_reference.get('Include')
                    package_version = package_reference.get('Version')
                    packages.append({'name': package_name, 'version': package_version})
        except ET.ParseError as e:
            print(f"Error parsing {project_file}: {e}")

        return packages

    def download_package(self, package):
        package_name = package['name']
        package_version = package['version']
        url = f'{self.artifactory_url}/{package_name}/{package_version}/{package_name}.{package_version}.nupkg'
        response = requests.get(url)

        if response.status_code == 200:
            package_file_path = os.path.join(self.output_folder, f'{package_name}.{package_version}.nupkg')
            with open(package_file_path, 'wb') as package_file:
                package_file.write(response.content)
            return package_file_path
        else:
            print(f"Failed to download package {package_name} version {package_version}. Status code: {response.status_code}")
            return None

    def restore_package(self, package):
        package_name = package['name']
        package_version = package['version']

        with self.lock:
            if (package_name, package_version) not in self.downloaded_packages:
                # Download the package
                package_file_path = self.download_package(package)

                if package_file_path:
                    # Extract the package to the NuGet packages folder
                    with ZipFile(package_file_path, 'r') as zip_ref:
                        zip_ref.extractall(self.nuget_packages_folder)

                    # Cleanup: Remove the downloaded package file
                    os.remove(package_file_path)

                    # Update the set of downloaded packages
                    self.downloaded_packages.add((package_name, package_version))
            else:
                print(f"Package {package_name} version {package_version} is already downloaded.")

    def restore_packages(self):
        all_packages = self.read_all_packages()

        with ThreadPoolExecutor(max_workers=self.max_workers) as executor:
            executor.map(self.restore_package, all_packages)

if __name__ == "__main__":
    if len(sys.argv) != 5:
        print("Usage: python script.py <project_folder> <output_folder> <artifactory_url> <max_workers>")
        sys.exit(1)

    project_folder = sys.argv[1]
    output_folder = sys.argv[2]
    artifactory_url = sys.argv[3]
    max_workers = int(sys.argv[4])

    nuget_restorer = NuGetRestorer(project_folder, output_folder, artifactory_url, max_workers)
    nuget_restorer.restore_packages()
