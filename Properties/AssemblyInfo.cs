using System.Reflection;
using System.Runtime.InteropServices;
using Rhino.PlugIns;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("RhinoCNC Production Suite")]
[assembly: AssemblyDescription("Material Management and Element Organization for CNC Manufacturing")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("RhinoCNC")]
[assembly: AssemblyProduct("RhinoCNC Production Suite")]
[assembly: AssemblyCopyright("Copyright © 2024")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Rhino Plugin Attributes
[assembly: PlugInDescription(DescriptionType.Address, "")]
[assembly: PlugInDescription(DescriptionType.Country, "")]
[assembly: PlugInDescription(DescriptionType.Email, "")]
[assembly: PlugInDescription(DescriptionType.Phone, "")]
[assembly: PlugInDescription(DescriptionType.Fax, "")]
[assembly: PlugInDescription(DescriptionType.Organization, "RhinoCNC")]
[assembly: PlugInDescription(DescriptionType.UpdateUrl, "")]
[assembly: PlugInDescription(DescriptionType.WebSite, "")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
// This will also be the Guid of the Rhino plug-in
[assembly: Guid("A1B2C3D4-E5F6-4789-A012-345678901234")]

// Rhino plugin-specific assembly attributes
[assembly: System.Reflection.AssemblyMetadata("PluginId", "A1B2C3D4-E5F6-4789-A012-345678901234")] 