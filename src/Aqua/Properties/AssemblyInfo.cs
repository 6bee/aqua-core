using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Aqua")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("Aqua")]
[assembly: AssemblyCompany("Christof Senn")]
[assembly: AssemblyCopyright("Copyright © Christof Senn 2013-2016")]
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
//      Patch Number
//      Unused
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("3.1.0.0")]
[assembly: AssemblyFileVersion("3.1.0.0")]


#if NET
[assembly: InternalsVisibleTo("Aqua.Test")]
#endif

#if NET35
[assembly: InternalsVisibleTo("Aqua.NET35.Tests")]
#endif

#if SILVERLIGHT
[assembly: InternalsVisibleTo("System.Runtime.Serialization")]
#endif

#if WINDOWS_PHONE
[assembly: NeutralResourcesLanguageAttribute("en-US")]
#endif