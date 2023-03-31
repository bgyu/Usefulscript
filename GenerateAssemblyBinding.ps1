[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$FolderPath,
    
    [string]$BindingOutputPath
)

# Get all of the strongly named .NET assemblies in the folder, ignoring native DLLs
$assemblies = Get-ChildItem -Path $FolderPath -Filter *.dll | Where-Object {
    try {
        [Reflection.AssemblyName]::GetAssemblyName($_.FullName) | Out-Null
        $true
    } catch {
        $false
    }
} | Where-Object { !($_.Name -like '*native*.dll') }

# Create an empty array to store the binding information
$bindingInfo = @()

# Loop through each assembly and generate binding information
foreach ($assembly in $assemblies) {
    $assemblyName = [Reflection.AssemblyName]::GetAssemblyName($assembly.FullName)
    $name = $assemblyName.Name
    $version = $assemblyName.Version
    $publicKeyToken = [BitConverter]::ToString($assemblyName.GetPublicKeyToken()).Replace("-","").ToLower()
    $culture = $assemblyName.CultureName
    
    $bindingInfo += "<dependentAssembly>"
    $bindingInfo += "    <assemblyIdentity name=`"$name`" publicKeyToken=`"$publicKeyToken`" culture=`"$culture`" />"
    $bindingInfo += "    <bindingRedirect oldVersion=`"0.0.0.0-$version`" newVersion=`"$version`" />"
    $bindingInfo += "</dependentAssembly>"
}

# Generate the app.config file with the binding information
$config = "<?xml version=`"1.0`" encoding=`"utf-8`" ?><configuration><runtime><assemblyBinding xmlns=`"urn:schemas-microsoft-com:asm.v1`">"
$config += $bindingInfo -join "`n"
$config += "</assemblyBinding></runtime></configuration>"

# Write the app.config file to disk
if ($BindingOutputPath) {
    $configPath = $BindingOutputPath
} else {
    $configPath = Join-Path -Path $FolderPath -ChildPath "app.config"
}

Set-Content -Path $configPath -Value $config
