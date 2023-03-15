[CmdletBinding()]
param(
    [Parameter(Mandatory=$true,Position=0)]
    [string]$FolderPath,
    [Parameter(Mandatory=$true,Position=1)]
    [string]$JsonFilePath,
    [Parameter()]
    [switch]$Save
)

# Get all files in the folder with a .dll extension
$Files = Get-ChildItem -Path $FolderPath -Filter *.dll

# Create an empty array to store the assembly information
$AssemblyInfo = @()

foreach ($File in $Files) {
    try {
        # Try to get the version, public token, and target framework information for the assembly
        $Assembly = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($File.FullName)
        
        # $FullPathFile =[IO.Path]::Combine($FolderPath, $File)
        # $AssemblyFile = "$FolderPath\$File"
        # Write-Host "Loading Assembly $AssemblyFile "
        # $Assembly = [System.Reflection.Assembly]::LoadFrom($AssemblyFile)
        
        $Version = $Assembly.GetName().Version.ToString()
        $PublicKeyToken = $Assembly.GetName().GetPublicKeyToken()
        $PublicKeyTokenHex = if ($PublicKeyToken) { [System.BitConverter]::ToString($PublicKeyToken).Replace('-','') } else { '' }
        # $TargetFramework = $Assembly.GetCustomAttributesData() | Where-Object { $_.AttributeType.Name -eq "TargetFrameworkAttribute" } | Select-Object -ExpandProperty ConstructorArguments | Select-Object -First 1
        # $IsNetFrameworkAssembly = $AssemblyData.ImageRuntimeVersion.StartsWith("v")
        $ImageRuntimeVersion = $Assembly.ImageRuntimeVersion

        # Add the assembly information to the array
        $AssemblyInfo += @{
            Name = $File.Name
            Version = $Version
            PublicKeyToken = $PublicKeyTokenHex
            TargetFramework = $ImageRuntimeVersion
        }
    } catch [System.BadImageFormatException] {
        # Ignore native DLLs that throw a BadImageFormatException
        Write-Warning "$($File.Name) is not a .NET assembly and will be ignored"
    } catch {
        # Ignore any other exceptions that might occur
        Write-Warning "Failed to get assembly information for $($File.Name): $_"
    }
}

# Filter out any non-.NET assemblies
$Assemblies = $AssemblyInfo | Where-Object { $_.TargetFramework -like "v*" }

if ($Save) {
    # Save the assembly information to a JSON file
    $Assemblies | ConvertTo-Json | Out-File -Encoding utf8 -FilePath $JsonFilePath
    Write-Host "Assembly information saved to $JsonFilePath"
} else {
    # Save the assembly information to a temporary JSON file
    # $TempFilePath = [System.IO.Path]::GetTempFileName() + '.json'
    $TempFilePath = $JsonFilePath + '.tmp'
    $Assemblies | ConvertTo-Json | Out-File -Encoding utf8 -FilePath $TempFilePath
    # Write-Host "Assembly information saved to $TempFilePath"

    # Compare the assembly information to the JSON file
    $BaseInfo = Get-Content -Path $JsonFilePath -Raw | ConvertFrom-Json
    $CmpInfo =  Get-Content -Path $TempFilePath -Raw | ConvertFrom-Json
    $Diff = Compare-Object -ReferenceObject $BaseInfo -DifferenceObject $CmpInfo -Property Name,Version,PublicKeyToken,TargetFramework -PassThru

    # Delete the temporary file
    Remove-Item $TempFilePath
    
    if ($Diff) {
        # Print the differences
        $Diff | Format-Table -AutoSize
    } else {
        Write-Host "All assemblies match the base information in $JsonFilePath"
    }
}
