import subprocess
import argparse
import os

class MSTestRunner:
    def __init__(self, assembly_path, parallel_count, result_path):
        self.assembly_path = assembly_path
        self.parallel_count = parallel_count
        self.result_path = result_path

    def list_tests(self):
        list_tests_command = f"vstest.console.exe {self.assembly_path} /listtests"
        process = subprocess.Popen(list_tests_command, stdout=subprocess.PIPE, shell=True)
        stdout, _ = process.communicate()
        return stdout.decode()

    def run_tests(self):
        run_tests_command = f"vstest.console.exe {self.assembly_path} /logger:trx /Parallel:{self.parallel_count} /ResultsDirectory:{self.result_path}"
        subprocess.run(run_tests_command, shell=True)

def main():
    parser = argparse.ArgumentParser(description="Run MSTest framework tests in parallel.")
    parser.add_argument("assembly_path", help="Path to the test assembly.")
    parser.add_argument("parallel_count", type=int, help="Number of parallel test execution threads.")
    parser.add_argument("result_path", help="Path to store test result files.")

    args = parser.parse_args()

    if not os.path.exists(args.assembly_path):
        print("Error: The specified assembly path does not exist.")
        return

    if not os.path.exists(args.result_path):
        os.makedirs(args.result_path)

    runner = MSTestRunner(args.assembly_path, args.parallel_count, args.result_path)

    # List the tests
    print("Listing available tests:")
    test_list = runner.list_tests()
    print(test_list)

    # Run the tests in parallel
    print("Running tests in parallel...")
    runner.run_tests()

if __name__ == "__main__":
    main()
