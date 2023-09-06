import os
import subprocess
import argparse
from multiprocessing import Pool

def list_dlls_and_exes(folder):
    dlls_and_exes = []
    for root, _, files in os.walk(folder):
        for file in files:
            if file.lower().endswith(('.dll', '.exe')):
                dlls_and_exes.append(os.path.join(root, file))
    return dlls_and_exes

def run_command(file_path, command):
    full_command = f"{command} {file_path}"
    subprocess.run(full_command, shell=True)

def main():
    parser = argparse.ArgumentParser(description="Run a command in parallel against DLL and EXE files.")
    parser.add_argument("folder_path", help="Path to the folder containing DLL and EXE files.")
    parser.add_argument("command", help="Command to run for each file.")
    parser.add_argument("--num_processes", type=int, default=4, help="Number of parallel processes (default: 4).")
    args = parser.parse_args()

    folder_path = args.folder_path
    command = args.command
    num_processes = args.num_processes

    if not os.path.isdir(folder_path):
        print("Error: The specified folder does not exist.")
        return

    files_to_process = list_dlls_and_exes(folder_path)

    with Pool(num_processes) as pool:
        pool.starmap(run_command, [(file_path, command) for file_path in files_to_process])

if __name__ == "__main__":
    main()
