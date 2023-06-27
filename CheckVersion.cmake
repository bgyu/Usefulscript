function(Check_Version VERSION_NAME)
    if(NOT DEFINED ${VERSION_NAME})
        if(NOT DEFINED ENV{${VERSION_NAME}})
            message(FATAL_ERROR "${VERSION_NAME} is not provided!")
        else()
            set(${VERSION_NAME} $ENV{${VERSION_NAME}})
        endif()
    endif()

    # Check if the version is empty or consists of only whitespace characters
    string(STRIP ${VERSION_NAME} ${VERSION_NAME})
    if("${${VERSION_NAME}}" STREQUAL "")
        message(FATAL_ERROR "${VERSION_NAME} is empty or consists of only whitespace characters!")
    else()
        message(STATUS "${VERSION_NAME}=${${VERSION_NAME}}")
    endif()
endfunction()

# Call Check_Version for MAJOR_VERSION and MINOR_VERSION
Check_Version(MAJOR_VERSION)
Check_Version(MINOR_VERSION)

# Use the versions in target_compile_definitions
target_compile_definitions(Hello PRIVATE MAJOR_VERSION=${MAJOR_VERSION} MINOR_VERSION=${MINOR_VERSION})
