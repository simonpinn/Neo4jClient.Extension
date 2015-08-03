using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


[assembly: AssemblyCopyright("Copyright © 2014")]
[assembly: AssemblyVersion("0.1.2.6")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyInformationalVersion("0.1.2.6-beta-tx")]    // trigger pre release package
#else
    [assembly: AssemblyConfiguration("Release")]
#endif

[assembly: ComVisible(false)]