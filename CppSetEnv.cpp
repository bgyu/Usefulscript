#include <iostream>
#include <string>

#if defined(_WIN32) || defined(_WIN64)
#include <windows.h>
#else
#include <cstdlib>
#endif

bool setEnvironmentVariable(const std::string& key, const std::string& value) {
#if defined(_WIN32) || defined(_WIN64) // Windows
    if (SetEnvironmentVariable(key.c_str(), value.c_str()) == 0) {
        // Failed to set the environment variable
        return false;
    }
    return true;
#else // Unix-based systems (Linux and macOS)
    if (setenv(key.c_str(), value.c_str(), 1) != 0) {
        // Failed to set the environment variable
        return false;
    }
    return true;
#endif
}

int main() {
    // Example usage:
    if (setEnvironmentVariable("MY_VARIABLE", "my_value")) {
        std::cout << "Environment variable set successfully." << std::endl;
    } else {
        std::cerr << "Failed to set the environment variable." << std::endl;
    }

    return 0;
}
