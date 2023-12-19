#!/bin/bash

# Default values
folder="."
config="Release"
build_skydotnet=false
parallel_count=10

# Function to print help
print_help() {
    echo "Usage: $0 [-f <folder>] [-c <configuration>] [-b] [-p <count>]"
    echo "Options:"
    echo "  -f, --folder <folder>       Specify the folder name (default: current folder)"
    echo "  -c, --configuration <type>  Specify the configuration type (Debug or Release, default: Release)"
    echo "  -b, --build-skydotnet      Run a special command if present"
    echo "  -p, --parallel <count>      Specify the parallel process count (default: 10)"
    echo "  -h, --help                 Display this help message"
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
        -b|--build-skydotnet)
            build_skydotnet=true
            ;;
        -p|--parallel)
            parallel_count="$2"
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

# Get the full path of the folder
folder=$(realpath "$folder")

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

# Check if parallel_count is a positive number
if ! [[ "$parallel_count" =~ ^[1-9][0-9]*$ ]]; then
    echo "Error: Parallel count must be a positive number."
    print_help
fi

# Replace "user" with "sgdvr" in the folder name
folder=$(echo "$folder" | sed 's/user/sgdvr/')

# Change to the specified folder
cd "$folder" || exit 1

# Run some command with the configuration
echo "Running command in folder '$folder' with configuration '$config'"

# Check and run a special command if build_skydotnet is true
if [ "$build_skydotnet" = true ]; then
    echo "Running special command for build-skydotnet"
    # Replace the following line with the actual command you want to run for build-skydotnet
    # For example: ./build-skydotnet-command
fi

# Replace the following line with the actual command you want to run
# For example: ./your_command --config "$config"

# Print the parallel count
echo "Parallel count: $parallel_count"
