param (
    [Parameter(Mandatory = $true)]
    [string]$AssemblyPath
)

# Load the target assembly
$Assembly = [System.Reflection.Assembly]::LoadFile($AssemblyPath)

# Get the full name of the target assembly
$AssemblyFullName = $Assembly.FullName

# Get the list of all the loaded assemblies
$LoadedAssemblies = [System.AppDomain]::CurrentDomain.GetAssemblies()

# Create a dictionary to store the dependent assemblies
$Dependencies = @{}

# Loop through all the loaded assemblies and find the ones that reference the target assembly
foreach ($LoadedAssembly in $LoadedAssemblies) {
    $ReferencedAssemblies = $LoadedAssembly.GetReferencedAssemblies()

    foreach ($ReferencedAssembly in $ReferencedAssemblies) {
        if ($ReferencedAssembly.FullName -eq $AssemblyFullName) {
            # Add the dependent assembly to the dictionary
            $Dependency = @{
                FullName = $ReferencedAssembly.FullName
                Version = $ReferencedAssembly.Version.ToString()
                PublicKeyToken = [System.BitConverter]::ToString($ReferencedAssembly.GetPublicKeyToken()).Replace("-", "")
                Location = $LoadedAssembly.Location
            }
            $Dependencies.Add($ReferencedAssembly.FullName, $Dependency)
        }
    }
}

# Sort the dictionary by key
$Dependencies = $Dependencies.GetEnumerator() | Sort-Object -Property Key

# Output the dependent assemblies
$Dependencies | Format-Table -AutoSize


# Usage: .\ListAssemblyDependencies.ps1 -AssemblyPath "C:\MyAssembly.dll"
