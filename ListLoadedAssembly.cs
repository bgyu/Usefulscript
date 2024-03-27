using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        // Get all loaded assemblies in the current application domain
        Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in loadedAssemblies)
        {
            // Get the full path of the assembly
            // Note: Assembly.Location might be empty for dynamically generated assemblies
            string location = assembly.Location;

            Console.WriteLine($"Assembly Name: {assembly.FullName}");
            if (!string.IsNullOrEmpty(location))
            {
                Console.WriteLine($"Location: {location}");
            }
            else
            {
                Console.WriteLine("Location: [Dynamic Assembly - No Path]");
            }
            Console.WriteLine(); // Blank line for better readability
        }
    }
}
