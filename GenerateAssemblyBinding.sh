#!/bin/bash

# Function to check if an assembly is strongly named
function is_strongly_named {
    local assembly_path=$1
    local assembly_name=$(basename "$assembly_path")
    local stdout=$(gacutil /l "$assembly_name" 2>&1)
    if [[ "$stdout" == *"The Global Assembly Cache contains the following assemblies"* ]]; then
        return 0
    else
        return 1
    fi
}

# Parse command line arguments
if [ "$#" -lt 1 ]; then
    echo "Usage: $0 <folder_path> [binding_output_path]"
    exit 1
fi

folder_path=$1
if [ ! -d "$folder_path" ]; then
    echo "Error: $folder_path is not a valid directory."
    exit 1
fi

if [ "$#" -eq 2 ]; then
    binding_output_path=$2
else
    binding_output_path="$folder_path/app.config"
fi

# Get all of the strongly named .NET assemblies in the folder, ignoring native DLLs
assemblies=($(find "$folder_path" -name "*.dll" | while read -r assembly_path; do
    if is_strongly_named "$assembly_path" && ! [[ "$assembly_path" == *"native"* ]]; then
        echo "$assembly_path"
    fi
done))

# Create an empty array to store the binding information
binding_info=()

# Loop through each assembly and generate binding information
for assembly_path in "${assemblies[@]}"; do
    assembly_name=$(basename "$assembly_path")
    assembly_info=$(mono "$MONO_ROOT/lib/mono/4.5/peapi.exe" -i "$assembly_path")
    name=$(echo "$assembly_info" | grep -oP "Name:\s+\K\w+")
    version=$(echo "$assembly_info" | grep -oP "Version:\s+\K[\d.]+")
    public_key=$(echo "$assembly_info" | grep -oP "Public Key:\s+\K([0-9a-fA-F]{2}:){15}[0-9a-fA-F]{2}")
    public_key_token=$(echo "$public_key" | sed 's/://g' | tr '[:upper:]' '[:lower:]')
    culture=$(echo "$assembly_info" | grep -oP "Culture:\s+\K\w+" | tr '[:upper:]' '[:lower:]')
    
    binding_info+=("<dependentAssembly>")
    binding_info+=("    <assemblyIdentity name=\"$name\" publicKeyToken=\"$public_key_token\" culture=\"$culture\" />")
    binding_info+=("    <bindingRedirect oldVersion=\"0.0.0.0-$version\" newVersion=\"$version\" />")
    binding_info+=("</dependentAssembly>")
done

# Generate the app.config file with the binding information
config="<?xml version=\"1.0\" encoding=\"utf-8\" ?><configuration><runtime><assemblyBinding xmlns=\"urn:schemas-microsoft-com:asm.v1\">"
config+=$(printf '%s\n' "${binding_info[@]}")
config+="</assemblyBinding></runtime></configuration>"

# Write the app.config file to disk
echo "$config" > "$binding_output_path"
