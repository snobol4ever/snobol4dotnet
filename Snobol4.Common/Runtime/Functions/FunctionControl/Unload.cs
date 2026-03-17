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
            NativeLibrary.Free(nativeEntry.LibraryHandle);
            NativeContexts.Remove(fnameKey);
            FunctionTable.Remove(fnameKey);

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

        SystemStack.Push(StringVar.Null());
        PredicateSuccess();
    }
}
