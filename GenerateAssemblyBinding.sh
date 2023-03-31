#!/bin/bash

folder_path=$1
binding_output_path=$2

# Get all of the .NET assemblies in the folder, ignoring native DLLs
assemblies=$(find $folder_path -maxdepth 1 -name '*.dll' ! -name '*native*.dll')

# Create an empty array to store the binding information
binding_info=()

# Loop through each assembly and generate binding information
for assembly in $assemblies; do
  name=$(basename -s .dll $assembly)
  version=$(dotnet --info | awk '/Version/{print $2}')
  culture="neutral"
  
  binding_info+=("<dependentAssembly>")
  binding_info+=("  <assemblyIdentity name=\"$name\" publicKeyToken=\"null\" culture=\"$culture\" />")
  binding_info+=("  <bindingRedirect oldVersion=\"0.0.0.0-$version\" newVersion=\"$version\" />")
  binding_info+=("</dependentAssembly>")
done

# Generate the app.config file with the binding information
config="<?xml version=\"1.0\" encoding=\"utf-8\" ?><configuration><runtime><assemblyBinding xmlns=\"urn:schemas-microsoft-com:asm.v1\">"
config+="${binding_info[*]}"
config+="</assemblyBinding></runtime></configuration>"

# Write the app.config file to disk
if [ -z "$binding_output_path" ]; then
  config_path="$folder_path/app.config"
else
  config_path="$binding_output_path"
fi

echo "$config" > "$config_path"
