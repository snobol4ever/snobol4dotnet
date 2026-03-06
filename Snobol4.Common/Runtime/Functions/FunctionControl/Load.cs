using System.Reflection;
using System.Runtime.Loader;

namespace Snobol4.Common;

//"load first argument is not a string" /* 137 */,
//"load first argument is null" /* 138 */,
//"load second argument is not a string" /* 136 */,
//"load does not implement IExternalLibrary" /* 142 */,
//"load function caused input error during load" /* 143 */,

public partial class Executive
{
    /// <summary>
    /// AssemblyLoadContext that resolves dependencies (e.g. FSharp.Core) from
    /// the same directory as the loaded DLL, then falls back to the default context.
    /// Snobol4.Common is always resolved from the shared context to prevent
    /// interface-type mismatches when casting to IExternalLibrary.
    /// </summary>
    private sealed class PluginLoadContext(string pluginPath) : AssemblyLoadContext(null, isCollectible: true)
    {
        private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // Always use the shared Snobol4.Common so IExternalLibrary types match
            if (assemblyName.Name == "Snobol4.Common")
                return null;

            // Resolve other dependencies from the plugin's own directory
            var path = _resolver.ResolveAssemblyToPath(assemblyName);
            if (path != null) return LoadFromAssemblyPath(path);

            // Fall back to default context
            return null;
        }
    }

    /// <summary>
    /// Keyed by fully-resolved absolute DLL path so two DLLs with the
    /// same filename in different directories don't collide.
    /// Also stores the IExternalLibrary instance so Unload() can call it.
    /// </summary>
    internal Dictionary<string, (AssemblyLoadContext Context, IExternalLibrary Library)> ActiveContexts = [];

    internal void LoadExternalFunction(List<Var> arguments)
    {
        // arg 0 = DLL path, arg 1 = fully-qualified class name
        if (!arguments[0].Convert(VarType.STRING, out _, out var pathObj, this))
        {
            LogRuntimeException(137);
            return;
        }

        if (!arguments[1].Convert(VarType.STRING, out _, out var nameObj, this))
        {
            LogRuntimeException(136);
            return;
        }

        var rawPath = (string)pathObj;
        var className = (string)nameObj;

        if (string.IsNullOrEmpty(rawPath))
        {
            LogRuntimeException(138);
            return;
        }

        // Resolve relative paths against the directory of the source file
        var resolvedPath = Path.IsPathRooted(rawPath)
            ? rawPath
            : Path.GetFullPath(rawPath,
                Parent.FilesToCompile.Count > 0
                    ? Path.GetDirectoryName(Parent.FilesToCompile[^1]) ?? Directory.GetCurrentDirectory()
                    : Directory.GetCurrentDirectory());

        // Idempotent: already loaded — succeed silently
        if (ActiveContexts.ContainsKey(resolvedPath))
        {
            SystemStack.Push(StringVar.Null());
            PredicateSuccess();
            return;
        }

        try
        {
            var loadContext = new PluginLoadContext(resolvedPath);
            var assembly = loadContext.LoadFromAssemblyPath(resolvedPath);
            var instance = assembly.CreateInstance(className) as IExternalLibrary;

            if (instance == null)
            {
                loadContext.Unload();
                LogRuntimeException(142);   // does not implement IExternalLibrary
                return;
            }

            instance.Init(this);
            ActiveContexts[resolvedPath] = (loadContext, instance);

            // Push null string result so callers can branch :S(ok)F(fail)
            SystemStack.Push(StringVar.Null());
            PredicateSuccess();
        }
        catch (Exception)
        {
            LogRuntimeException(143);   // I/O or reflection error during load
        }
    }
}
