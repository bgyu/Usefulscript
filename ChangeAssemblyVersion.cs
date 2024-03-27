using Mono.Cecil;

var assemblyPath = @"YourAssemblyPath.dll";
var newVersion = new Version("1.0.0.2");

var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);
assemblyDefinition.Name.Version = newVersion;

// Modify the AssemblyFileVersion attribute
var customAttribute = assemblyDefinition.CustomAttributes
    .FirstOrDefault(a => a.AttributeType.Name == "AssemblyFileVersionAttribute");
if (customAttribute != null)
{
    customAttribute.ConstructorArguments.Clear();
    customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(assemblyDefinition.MainModule.TypeSystem.String, newVersion.ToString()));
}

assemblyDefinition.Write(@"YourNewAssemblyPath.dll");
