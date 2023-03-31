$FolderPath = $args[0]
$OutputPath = $args[1]

# Get the list of assemblies in the current app's bin folder
$Assemblies = Get-ChildItem -Path "$FolderPath\*.dll" -Exclude "*vshost*", "*Test*", "*Tests*"

# Get the list of referenced assemblies from the current app's assembly manifest
$References = [System.AppDomain]::CurrentDomain.GetAssemblies() | 
              Where-Object { $_.Location -like "$FolderPath\*" } | 
              ForEach-Object { $_.GetReferencedAssemblies() } | 
              Select-Object -Unique

# Filter the referenced assemblies to include only strongly named assemblies
$StrongNamedReferences = $References | 
                         Where-Object { $_.FullName -match "PublicKeyToken" }

# Generate the assembly binding XML
$AssemblyBinding = $StrongNamedReferences | ForEach-Object {
    $AssemblyName = $_.Name
    $PublicKeyToken = $_.GetPublicKeyToken() | ForEach-Object { $_.ToString("x2") }
    $OldVersion = $_.Version
    $NewVersion = $_.Version
    "<dependentAssembly>
        <assemblyIdentity name='$AssemblyName' publicKeyToken='$PublicKeyToken' culture='neutral' />
        <bindingRedirect oldVersion='$OldVersion' newVersion='$NewVersion' />
     </dependentAssembly>"
}

# Generate the app.config file with the assembly bindings
if ($AssemblyBinding) {
    $AppConfigContent = @"<?xml version='1.0' encoding='utf-8'?>
<configuration>
  <runtime>
    <assemblyBinding xmlns='urn:schemas-microsoft-com:asm.v1'>
      $AssemblyBinding
    </assemblyBinding>
  </runtime>
</configuration>
"@
    Set-Content -Path $OutputPath -Value $AppConfigContent
}


# Usage:
# .\GenerateAssemblyBindings.ps1 -FolderPath "C:\MyApp\bin\Debug" -OutputPath "C:\MyApp\bin\Debug\app.config"
