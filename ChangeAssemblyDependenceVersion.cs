using Mono.Cecil;
using System;
using System.Linq;

public class AssemblyModifier
{
    public static void ChangeDependencyVersion(string assemblyPath, string dependencyName, string newVersion)
    {
        // Load the assembly
        var assembly = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters { ReadWrite = true });

        // Find the assembly reference
        var reference = assembly.MainModule.AssemblyReferences.FirstOrDefault(r => r.Name == dependencyName);
        if (reference != null)
        {
            // Update the version
            reference.Version = new Version(newVersion);

            // Save the modified assembly
            assembly.Write(); // Writes the changes back to the same file
            Console.WriteLine($"Updated {dependencyName} to version {newVersion}");
        }
        else
        {
            Console.WriteLine($"Assembly reference '{dependencyName}' not found.");
        }
    }
}
