import os
import sys
import xml.etree.ElementTree as ET
import requests
from zipfile import ZipFile
from concurrent.futures import ThreadPoolExecutor, as_completed
from multiprocessing import Manager, Lock, Process, Value

class NuGetRestorer:
    def __init__(self, project_folder, output_folder, artifactory_url, max_workers=5):
        self.project_folder = project_folder
        self.output_folder = output_folder
        self.artifactory_url = artifactory_url
        self.nuget_packages_folder = os.path.expanduser('~/.nuget/packages')
        self.process_lock = Lock()
        self.max_workers = max_workers
        self.package_status = Manager().dict()

        if not os.path.exists(self.nuget_packages_folder):
            os.makedirs(self.nuget_packages_folder)

    def read_all_packages(self, project_file):
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

    def package_exists(self, package):
        package_name = package['name']
        package_version = package['version']
        package_file_path = os.path.join(self.nuget_packages_folder, package_name, package_version, f'{package_name}.{package_version}.nupkg')
        return os.path.exists(package_file_path)

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

        with self.process_lock:
            # Check if the package is being restored by another process
            if self.package_status.get((package_name, package_version), False):
                print(f"Package {package_name} version {package_version} is already being restored by another process.")
                return

            # Mark the package as being restored
            self.package_status[(package_name, package_version)] = True

        # Check if the package already exists in the package folder
        if self.package_exists(package):
            print(f"Package {package_name} version {package_version} already exists in the package folder. Skipping download and restore.")
        else:
            # Download the package
            package_file_path = self.download_package(package)

            if package_file_path:
                # Extract the package to the NuGet packages folder
                with ZipFile(package_file_path, 'r') as zip_ref:
                    zip_ref.extractall(self.nuget_packages_folder)

                # Cleanup: Remove the downloaded package file
                os.remove(package_file_path)

        with self.process_lock:
            # Mark the package as restored
            self.package_status[(package_name, package_version)] = False

    def restore_packages_in_project(self, project_file):
        all_packages = self.read_all_packages(project_file)

        with ThreadPoolExecutor(max_workers=self.max_workers) as executor:
            futures = [executor.submit(self.restore_package, package) for package in all_packages]

            # Wait for all tasks to complete
            for future in as_completed(futures):
                future.result()

    def restore_packages(self):
        processes = []

        for project_file in os.listdir(self.project_folder):
            if project_file.endswith('.csproj'):
                project_path = os.path.join(self.project_folder, project_file)
                process = Process(target=self.restore_packages_in_project, args=(project_path,))
                processes.append(process)
                process.start()

        for process in processes:
            process.join()

if __name__ == "__main__":
    if len(sys.argv) != 4:
        print("Usage: python script.py <project_folder> <output_folder> <artifactory_url>")
        sys.exit(1)

    project_folder = sys.argv[1]
    output_folder = sys.argv[2]
    artifactory_url = sys.argv[3]

    nuget_restorer = NuGetRestorer(project_folder, output_folder, artifactory_url)
    nuget_restorer.restore_packages()
