using System.Runtime.InteropServices;

namespace Snobol4.Common;

//"unload argument is not a string" /* 201 */,
//"unload argument is not natural variable name" /* 201 */,  // Error 202 per SPITBOL spec for bad fname

public partial class Executive
{
    // Natural-variable-name pattern: letter/digit/underscore, starting with letter
    // (SNOBOL4 identifiers). Used for Error 202 check on spec-path UNLOAD(fname).
    private static bool IsNaturalVariableName(string s) =>
        s.Length > 0 && char.IsLetter(s[0]) && s.All(c => char.IsLetterOrDigit(c) || c == '_');

    internal void UnloadExternalFunction(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.STRING, out _, out var argObj, this))
        {
            LogRuntimeException(201);
            return;
        }

        var arg = (string)argObj;

        if (string.IsNullOrEmpty(arg))
        {
            LogRuntimeException(201);
            return;
        }

        var fnameKey = Parent.FoldCase(arg);

        // ── Spec path: UNLOAD(fname) — arg is a function name ────────────
        // Error 201: if the arg looks like an identifier (no path separators or dots)
        // but is NOT a valid natural variable name, that's a spec error.
        var looksLikePath = arg.Contains(Path.DirectorySeparatorChar)
                         || arg.Contains(Path.AltDirectorySeparatorChar)
                         || arg.Contains('.');
        if (!looksLikePath && !IsNaturalVariableName(arg))
        {
            LogRuntimeException(201);
            return;
        }

        if (NativeContexts.TryGetValue(fnameKey, out var nativeEntry))
        {
            // xncbp: fire shutdown callback if one was registered and not yet fired.
            FireNativeCallback(nativeEntry);
            nativeEntry.FreeXndta();
            NativeLibrary.Free(nativeEntry.LibraryHandle);
            NativeContexts.Remove(fnameKey);
            FunctionTable.Remove(fnameKey);

            SystemStack.Push(StringVar.Null());
            PredicateSuccess();
            return;
        }

        // ── Reflect path: UNLOAD(fname) — registered via auto-prototype ──
        if (DotNetReflectContexts.TryGetValue(fnameKey, out var reflectEntry))
        {
            FunctionTable.Remove(fnameKey);
            DotNetReflectContexts.Remove(fnameKey);

            // Step 4: decrement ref-count; release shared ALC only when it hits zero.
            var dllPath = reflectEntry.ResolvedPath;
            var count   = DllRefCounts.GetValueOrDefault(dllPath) - 1;
            if (count <= 0)
            {
                DllRefCounts.Remove(dllPath);
                if (DllSharedContexts.TryGetValue(dllPath, out var sharedCtx))
                {
                    DllSharedContexts.Remove(dllPath);
                    sharedCtx.Unload();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
            }
            else
            {
                DllRefCounts[dllPath] = count;
            }

            SystemStack.Push(StringVar.Null());
            PredicateSuccess();
            return;
        }

        // ── .NET-native path: UNLOAD(path) ───────────────────────────────
        // Resolve path the same way Load does
        var resolvedPath = Path.IsPathRooted(arg)
            ? arg
            : Path.GetFullPath(arg,
                Parent.FilesToCompile.Count > 0
                    ? Path.GetDirectoryName(Parent.FilesToCompile[^1]) ?? Directory.GetCurrentDirectory()
                    : Directory.GetCurrentDirectory());

        // ── Reflect path: path-based UNLOAD — sweep all fnames for this DLL ──
        var reflectKeys = DotNetReflectContexts
            .Where(kv => kv.Value.ResolvedPath == resolvedPath)
            .Select(kv => kv.Key)
            .ToList();
        if (reflectKeys.Count > 0)
        {
            foreach (var key in reflectKeys)
            {
                FunctionTable.Remove(key);
                DotNetReflectContexts.Remove(key);
            }
            var count = DllRefCounts.GetValueOrDefault(resolvedPath) - reflectKeys.Count;
            if (count <= 0)
            {
                DllRefCounts.Remove(resolvedPath);
                if (DllSharedContexts.TryGetValue(resolvedPath, out var sharedCtx))
                {
                    DllSharedContexts.Remove(resolvedPath);
                    sharedCtx.Unload();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
            }
            else
            {
                DllRefCounts[resolvedPath] = count;
            }
            SystemStack.Push(StringVar.Null());
            PredicateSuccess();
            return;
        }

        if (!ActiveContexts.TryGetValue(resolvedPath, out var entry))
        {
            // Not loaded under either path — idempotent success
            SystemStack.Push(StringVar.Null());
            PredicateSuccess();
            return;
        }

        // Give the library a chance to de-register functions / release resources
        entry.Library.Unload();

        entry.Context.Unload();
        ActiveContexts.Remove(resolvedPath);

        // Collectible ALCs are not unloaded until the GC reclaims all references.
        // Force a full collect so the assembly is truly gone before we return.
        // This matters in test suites where a subsequent NativeLibrary.Load in the
        // same process can race against a lingering FSharp.Core or native handle.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        SystemStack.Push(StringVar.Null());
        PredicateSuccess();
    }
}
