#!/bin/bash

# Check if the correct number of arguments are provided
if [ $# -lt 1 ] || [ $# -gt 2 ]; then
    echo "Usage: $0 <file_or_folder> [debug_symbols_folder]"
    exit 1
fi

file_or_folder="$1"
debug_symbols_folder="${2:-$(dirname "$file_or_folder")/debug_symbols}"

# Create debug_symbols_folder if it doesn't exist
mkdir -p "$debug_symbols_folder"

# Function to strip debug symbols
strip_debug_symbols() {
    file="$1"
    debug_symbols_folder="$2"
    
    # Check if the file has debug symbols
    if objdump -h "$file" 2>/dev/null | grep -q '.debug_info'; then
        # Extract debug symbols
        objcopy --only-keep-debug "$file" "$debug_symbols_folder/$(basename "$file").debug"

        # Strip debug symbols from the file
        objcopy --strip-debug --strip-unneeded "$file"

        # Link stripped file with debug symbols
        objcopy --add-gnu-debuglink="$debug_symbols_folder/$(basename "$file").debug" "$file"
        
        echo "Debug symbols stripped and stored for: $file"
    else
        echo "No debug symbols found in: $file"
    fi
}

# Process files or folders
if [ -d "$file_or_folder" ]; then
    # If it's a directory, process each file within
    for f in "$file_or_folder"/*; do
        if [ -f "$f" ]; then
            strip_debug_symbols "$f" "$debug_symbols_folder"
        fi
    done
elif [ -f "$file_or_folder" ]; then
    # If it's a file, directly process it
    strip_debug_symbols "$file_or_folder" "$debug_symbols_folder"
else
    echo "Invalid input: $file_or_folder is neither a file nor a directory."
    exit 1
fi

echo "Debug symbols stripped and stored in: $debug_symbols_folder"
