using System.Reflection;
using System.Resources;
using System.Windows;


// This attribute specifies the culture that the resources are designed for.
// The resources are typically embedded in the assembly in the culture-specific satellite assemblies.
// This attribute tells the build process that the resources are designed to be culture-neutral.
//[assembly: NeutralResourcesLanguage("en-GB", UltimateResourceFallbackLocation.MainAssembly)]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page,
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page,
                                              // app, or any theme specific resource dictionaries)
)]
