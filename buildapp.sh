#!/bin/bash

# Default values
folder="."
config="Release"

# Function to print help
print_help() {
    echo "Usage: $0 [-f <folder>] [-c <configuration>]"
    echo "Options:"
    echo "  -f, --folder <folder>       Specify the folder name (default: current folder)"
    echo "  -c, --configuration <type>  Specify the configuration type (Debug or Release, default: Release)"
    echo "  -h, --help                  Display this help message"
    exit 1
}

# Process command-line options
while [[ "$#" -gt 0 ]]; do
    case $1 in
        -f|--folder)
            folder="$2"
            shift
            ;;
        -c|--configuration)
            config="$2"
            shift
            ;;
        -h|--help)
            print_help
            ;;
        *)
            echo "Error: Unknown parameter '$1'"
            print_help
            ;;
    esac
    shift
done

# Check if the folder exists
if [ ! -d "$folder" ]; then
    echo "Error: Folder '$folder' does not exist."
    print_help
fi

# Check if the configuration is valid
if [ "$config" != "Debug" ] && [ "$config" != "Release" ]; then
    echo "Error: Invalid configuration type. Must be 'Debug' or 'Release'."
    print_help
fi

# Change to the specified folder
cd "$folder" || exit 1

# Run some command with the configuration
echo "Running command in folder '$folder' with configuration '$config'"
# Replace the following line with the actual command you want to run
# For example: ./your_command --config "$config"
