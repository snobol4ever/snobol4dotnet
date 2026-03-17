using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Snobol4.Common;

//"load first argument is not a string" /* 137 */,
//"load first argument is null" /* 138 */,
//"load second argument is not a string" /* 136 */,
//"load first argument is missing a left paren" /* 139 */,
//"load first argument has null function name" /* 140 */,
//"load first argument is missing a right paren" /* 141 */,
//"load does not implement IExternalLibrary" /* 142 */,
//"load function caused input error during load" /* 143 */,

public partial class Executive
{
    // ── Prototype string ──────────────────────────────────────────────────

    /// <summary>
    /// Parsed result of a SPITBOL prototype string: 'FNAME(T1,T2,...)Tr'
    /// </summary>
    internal sealed record PrototypeInfo(
        string FunctionName,
        IReadOnlyList<string> ArgTypes,
        string ReturnType);

    /// <summary>
    /// Try to parse a SPITBOL prototype string of the form 'FNAME(T1,...,Tn)Tr'.
    /// Returns null and sets errorCode on failure; sets errorCode=0 on success.
    /// </summary>
    internal static PrototypeInfo? ParsePrototype(string s1, out int errorCode)
    {
        errorCode = 0;
        var lp = s1.IndexOf('(');
        if (lp < 0) { errorCode = 139; return null; }

        var fname = s1[..lp].Trim();
        if (fname.Length == 0) { errorCode = 140; return null; }

        var rp = s1.IndexOf(')', lp);
        if (rp < 0) { errorCode = 141; return null; }

        var argStr = s1[(lp + 1)..rp].Trim();
        var argTypes = argStr.Length == 0
            ? Array.Empty<string>()
            : argStr.Split(',').Select(t =>
            {
                var tok = t.Trim().ToUpperInvariant();
                // Known type keywords.  Anything else (including empty token,
                // "NOCONV", or an unknown keyword) → "NOCONV" (type code 0).
                return tok is "INTEGER" or "REAL" or "STRING" or "FILE" or "EXTERNAL"
                    ? tok : "NOCONV";
            }).ToArray();

        var returnType = s1[(rp + 1)..].Trim().ToUpperInvariant();
        return new PrototypeInfo(fname, argTypes, returnType);
    }

    // ── NativeLibrary tracking ────────────────────────────────────────────

    /// <summary>
    /// Entry for a spec-path (C-ABI) loaded function: the native library handle
    /// and a delegate wrapper that coerces SNOBOL4 Var arguments and return value.
    /// Keyed in NativeContexts by FNAME (folded).
    /// </summary>
    internal sealed class NativeEntry(IntPtr libraryHandle, string libraryPath, PrototypeInfo proto)
    {
        internal readonly IntPtr LibraryHandle = libraryHandle;
        internal readonly string LibraryPath    = libraryPath;
        internal readonly PrototypeInfo Proto   = proto;

        // ── XNBLK persistent state ────────────────────────────────────────
        // xndta: 32-long (256-byte) private storage block, pinned so the C
        // function receives a stable pointer across calls.  Allocated lazily
        // on the first call that requests it via EnsureXndta().
        // FirstCall mirrors the SPITBOL xn1st flag: true until the first
        // actual dispatch through CallNativeFunction.

        internal bool    FirstCall  = true;
        internal IntPtr  XnBlkData  = IntPtr.Zero;   // pointer into _xndtaPin

        // ── xncbp shutdown callback ───────────────────────────────────────
        // The C function may call snobol4_register_callback(fp) to register
        // a cleanup function that is invoked when the function is UNLOADed
        // or when the process exits.  CallbackFired prevents double-fire.
        internal IntPtr  CallbackPtr    = IntPtr.Zero;
        internal bool    CallbackFired  = false;

        private long[]?   _xndtaBuf;
        private GCHandle  _xndtaPin;

        /// <summary>
        /// Ensures the 32-long xndta buffer is allocated and pinned.
        /// Returns the stable native pointer to xndta[0].
        /// Safe to call multiple times — allocates only once.
        /// </summary>
        internal IntPtr EnsureXndta()
        {
            if (XnBlkData != IntPtr.Zero) return XnBlkData;
            _xndtaBuf  = new long[32];          // 256 bytes of private storage
            _xndtaPin  = GCHandle.Alloc(_xndtaBuf, GCHandleType.Pinned);
            XnBlkData  = _xndtaPin.AddrOfPinnedObject();
            return XnBlkData;
        }

        /// <summary>
        /// Frees the pinned xndta buffer.  Called from UnloadExternalFunction.
        /// </summary>
        internal void FreeXndta()
        {
            if (_xndtaPin.IsAllocated) _xndtaPin.Free();
            XnBlkData = IntPtr.Zero;
            _xndtaBuf = null;
        }
    }

    /// <summary>
    /// Keyed by folded FNAME. Used by UNLOAD(fname) spec path to release handles.
    /// </summary>
    internal Dictionary<string, NativeEntry> NativeContexts = [];

    // ── .NET-native tracking (existing) ───────────────────────────────────

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

    // ── LOAD dispatcher ───────────────────────────────────────────────────

    internal void LoadExternalFunction(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.STRING, out _, out var s1Obj, this))
        {
            LogRuntimeException(137);
            return;
        }

        var s1 = (string)s1Obj;

        if (string.IsNullOrEmpty(s1))
        {
            LogRuntimeException(138);
            return;
        }

        // Dispatcher: prototype string if s1 contains '(' — spec-compliant path.
        // Path-like s1 (no '(') — existing .NET-native path (net-load-dotnet).
        if (s1.Contains('('))
            LoadSpecPath(s1, arguments);
        else
            LoadDotNetPath(s1, arguments);
    }

    // ── Spec path (SPITBOL-compatible: prototype string) ─────────────────

    private void LoadSpecPath(string s1, List<Var> arguments)
    {
        var proto = ParsePrototype(s1, out var errorCode);
        if (proto == null)
        {
            LogRuntimeException(errorCode);
            return;
        }

        // s2: library filename (optional — SNOLIB search if omitted)
        string? rawLibPath = null;
        if (arguments.Count >= 2 && arguments[1] is not StringVar { Data: "" })
        {
            if (!arguments[1].Convert(VarType.STRING, out _, out var s2Obj, this))
            {
                LogRuntimeException(136);
                return;
            }
            rawLibPath = (string)s2Obj;
            if (rawLibPath.Length == 0) rawLibPath = null;
        }

        var fnameKey = Parent.FoldCase(proto.FunctionName);

        // Idempotent: already loaded under this FNAME
        if (NativeContexts.ContainsKey(fnameKey))
        {
            SystemStack.Push(StringVar.Null());
            PredicateSuccess();
            return;
        }

        // Resolve the library path (explicit or SNOLIB search)
        var resolvedLib = rawLibPath != null
            ? ResolveLibraryPath(rawLibPath)
            : SnolibSearch(proto.FunctionName);

        if (resolvedLib == null)
        {
            NonExceptionFailure();
            return;
        }

        try
        {
            var handle = NativeLibrary.Load(resolvedLib);
            var entry  = new NativeEntry(handle, resolvedLib, proto);
            NativeContexts[fnameKey] = entry;

            // If this library exports snobol4_rt_register, wire up the context
            // callback so snobol4_xndta() / snobol4_first_call() work inside it.
            TryRegisterRtCallback(handle);

            // Register a FunctionTableEntry that coerces args and dispatches
            // via NativeLibrary.GetExport at call time (lazy — avoids keeping
            // a typed delegate per signature).
            var argCount = proto.ArgTypes.Count;
            FunctionTable[fnameKey] = new FunctionTableEntry(
                this, fnameKey,
                args => CallNativeFunction(entry, args),
                argCount, false);

            SystemStack.Push(StringVar.Null());
            PredicateSuccess();
        }
        catch (Exception)
        {
            NonExceptionFailure();
        }
    }

    /// <summary>
    /// Resolve a library filename: absolute paths are used as-is; relative paths
    /// are searched against CWD then against directories in SNOLIB env var.
    /// </summary>
    private static string? ResolveLibraryPath(string rawPath)
    {
        if (Path.IsPathRooted(rawPath))
            return File.Exists(rawPath) ? rawPath : null;

        // Try CWD first
        var cwd = Path.GetFullPath(rawPath, Directory.GetCurrentDirectory());
        if (File.Exists(cwd)) return cwd;

        // SNOLIB search
        return SnolibSearch(rawPath);
    }

    /// <summary>
    /// Search SNOLIB env var directories for a library by base name.
    /// Tries the name as-is, then with platform-native extension (.so / .dll / .dylib).
    /// </summary>
    private static string? SnolibSearch(string nameOrPath)
    {
        var snolib = Environment.GetEnvironmentVariable("SNOLIB");
        if (string.IsNullOrEmpty(snolib)) return null;

        var extensions = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new[] { ".dll" }
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? new[] { ".dylib", ".so" }
                : new[] { ".so", ".dll" };

        var baseName = Path.GetFileName(nameOrPath);

        foreach (var dir in snolib.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            // Try exact name first
            var exact = Path.Combine(dir, baseName);
            if (File.Exists(exact)) return exact;

            // Try with each native extension if the name has none
            if (Path.GetExtension(baseName).Length == 0)
            {
                foreach (var ext in extensions)
                {
                    var candidate = Path.Combine(dir, baseName + ext);
                    if (File.Exists(candidate)) return candidate;
                }
            }
        }

        return null;
    }

    // ── Native call dispatch ──────────────────────────────────────────────

    // ── Native call marshal (Step 4) ─────────────────────────────────────
    //
    // Uses unsafe function pointers (the .NET 5+ way) rather than
    // Marshal.GetDelegateForFunctionPointer which does not support Cdecl
    // on .NET Core for NativeLibrary exports.
    //
    // Supported signatures: INTEGER/REAL/STRING return × arity 0–3.
    // Args are coerced per prototype ArgTypes before the call.

    // Thread-local: the NativeEntry currently being dispatched.
    // Set around every CallNativeFunction call so that snobol4_xndta() and
    // snobol4_first_call() in libsnobol4_rt can read the entry's xndta/FirstCall.
    [ThreadStatic] internal static NativeEntry? _currentNativeEntry;

    private unsafe void CallNativeFunction(NativeEntry entry, List<Var> args)
    {
        var proto = entry.Proto;
        var fnPtr = IntPtr.Zero;
        try { fnPtr = NativeLibrary.GetExport(entry.LibraryHandle, proto.FunctionName); }
        catch { NonExceptionFailure(); return; }

        var retType  = proto.ReturnType;
        var argTypes = proto.ArgTypes;
        var n        = argTypes.Count;

        // Coerce arguments
        var longs   = new long[n];
        var doubles = new double[n];
        var ptrs    = new IntPtr[n];
        var allocated = new List<IntPtr>();
        var noconvHandles = new List<GCHandle>();

        // Capture first-call state before we flip it; ensure xndta is ready.
        var isFirstCall = entry.FirstCall;
        entry.EnsureXndta();

        // Set thread-local so libsnobol4_rt shim helpers can reach this entry.
        var prevEntry = _currentNativeEntry;
        _currentNativeEntry = entry;

        try
        {
            for (var i = 0; i < n; i++)
            {
                var v = i < args.Count ? args[i] : new StringVar("");
                switch (argTypes[i])
                {
                    case "INTEGER":
                        v.Convert(VarType.INTEGER, out _, out var iv, this);
                        longs[i] = (long)iv;
                        break;
                    case "REAL":
                        v.Convert(VarType.REAL, out _, out var rv, this);
                        doubles[i] = (double)rv;
                        break;
                    case "NOCONV":
                        // ExternalVar fast-path: pass the raw native pointer directly —
                        // no GCHandle needed, the pointer is already an unmanaged address.
                        if (v is ExternalVar extVar)
                        {
                            ptrs[i] = extVar.Pointer;
                        }
                        else
                        {
                            // Pass the Var object itself as a pinned GCHandle pointer.
                            var gh = GCHandle.Alloc(v, GCHandleType.Pinned);
                            noconvHandles.Add(gh);
                            ptrs[i] = gh.AddrOfPinnedObject();
                        }
                        break;
                    default:
                        v.Convert(VarType.STRING, out _, out var sv, this);
                        var p = Marshal.StringToHGlobalAnsi((string)sv);
                        allocated.Add(p);
                        ptrs[i] = p;
                        break;
                }
            }

            void* fp = (void*)fnPtr;

            // Build a unified nativeArgs array where each slot holds the
            // coerced value as either long, double, or IntPtr.
            // We use IntPtr as the universal carrier and cast at call time.
            // Strategy: all args are passed as IntPtr (8 bytes on x64).
            // INTEGER args: cast long to IntPtr (fits on x64).
            // REAL args: we must use a separate double[] and dispatch on arg sig.
            // STRING args: already IntPtr.
            //
            // For simplicity and correctness, dispatch on the full (retType, argSig)
            // tuple using a helper that selects the right delegate type per slot.

            // Encode arg signature for dispatch selection
            // I=INTEGER, R=REAL, S=STRING/other, N=NOCONV (passes raw IntPtr like S)
            var argSig = string.Concat(argTypes.Select(t => t switch {
                "INTEGER" => "I", "REAL" => "R", "NOCONV" => "N", _ => "S" }));
            var retSig  = retType switch { "INTEGER" => "I", "REAL" => "R", "EXTERNAL" => "X", _ => "S" };

            Var resultVar = InvokeNative(fp, retSig, argSig, longs, doubles, ptrs);
            SystemStack.Push(resultVar);
            Failure = false;

            // Flip FirstCall after a successful dispatch.
            if (isFirstCall) entry.FirstCall = false;
        }
        catch
        {
            NonExceptionFailure();
        }
        finally
        {
            _currentNativeEntry = prevEntry;
            foreach (var p in allocated) Marshal.FreeHGlobal(p);
            foreach (var gh in noconvHandles) if (gh.IsAllocated) gh.Free();
        }
    }

    // ── libsnobol4_rt callback registration ──────────────────────────────────
    //
    // If a loaded native library exports snobol4_rt_register, we pass it TWO
    // function pointers:
    //   1. _RtGetContext  — reads xndta/first_call from the current NativeEntry.
    //   2. _RtSetCallback — stores a C shutdown callback into the current entry's
    //                       CallbackPtr (implements snobol4_register_callback).
    //
    // Both pointers are pinned delegates held for process lifetime.
    // Registration is idempotent — safe to call on every LOAD.

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void RtGetContextDelegate(IntPtr xndtaOut, IntPtr firstCallOut);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void RtSetCallbackDelegate(IntPtr callbackFn);

    // Pinned so the GC never moves them; held for process lifetime.
    private static readonly RtGetContextDelegate _rtGetContextDelegate = RtGetContext;
    private static readonly IntPtr _rtGetContextPtr =
        Marshal.GetFunctionPointerForDelegate(_rtGetContextDelegate);

    private static readonly RtSetCallbackDelegate _rtSetCallbackDelegate = RtSetCallback;
    private static readonly IntPtr _rtSetCallbackPtr =
        Marshal.GetFunctionPointerForDelegate(_rtSetCallbackDelegate);

    private static unsafe void RtGetContext(IntPtr xndtaOut, IntPtr firstCallOut)
    {
        var e = _currentNativeEntry;
        if (xndtaOut != IntPtr.Zero)
            *(long**)xndtaOut = e != null ? (long*)e.XnBlkData : null;
        if (firstCallOut != IntPtr.Zero)
            *(int*)firstCallOut = (e != null && e.FirstCall) ? 1 : 0;
    }

    private static void RtSetCallback(IntPtr callbackFn)
    {
        var e = _currentNativeEntry;
        if (e != null)
            e.CallbackPtr = callbackFn;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SnobolRtRegisterDelegate(IntPtr getContextFn, IntPtr setCallbackFn);

    private static unsafe void TryRegisterRtCallback(IntPtr libraryHandle)
    {
        if (!NativeLibrary.TryGetExport(libraryHandle, "snobol4_rt_register", out var procAddr))
            return;
        var register = Marshal.GetDelegateForFunctionPointer<SnobolRtRegisterDelegate>(procAddr);
        register(_rtGetContextPtr, _rtSetCallbackPtr);
    }

    // ── xncbp: fire shutdown callback ────────────────────────────────────────
    //
    // Called by UNLOAD (spec path) and by the ProcessExit handler.
    // The double-fire guard (CallbackFired) ensures the callback runs at most once
    // regardless of which path fires first.

    internal static unsafe void FireNativeCallback(NativeEntry entry)
    {
        if (entry.CallbackPtr == IntPtr.Zero) return;
        if (entry.CallbackFired) return;
        entry.CallbackFired = true;
        ((delegate* unmanaged[Cdecl]<void>)(void*)entry.CallbackPtr)();
    }

    internal void FireAllNativeCallbacks()
    {
        foreach (var entry in NativeContexts.Values)
            FireNativeCallback(entry);
    }

    // ── Native dispatch helper ───────────────────────────────────────────────
    //
    // Dispatches on retSig (I/R/S) × argSig (e.g. "II", "RS", "S", "") 
    // using unsafe function pointers with the correct parameter types per slot.
    // This avoids passing wrong types (e.g. long where double/IntPtr needed).
    //
    // Supported: arity 0–3, any mix of I/R/S args with I/R/S return.
    // Unsupported arity or sig falls through to NonExceptionFailure.

    private static unsafe Var InvokeNative(
        void* fp, string retSig, string argSig,
        long[] li, double[] ld, IntPtr[] lp)
    {
        // Select the correct typed call based on (retSig, argSig)
        // Each case uses the matching delegate* type so the ABI is correct.
        switch ((retSig, argSig))
        {
            // ── INTEGER return ────────────────────────────────────────────
            case ("I", ""):   return new IntegerVar(((delegate* unmanaged[Cdecl]<long>)fp)());
            case ("I", "I"):  return new IntegerVar(((delegate* unmanaged[Cdecl]<long,long>)fp)(li[0]));
            case ("I", "R"):  return new IntegerVar(((delegate* unmanaged[Cdecl]<double,long>)fp)(ld[0]));
            case ("I", "S"):  return new IntegerVar(((delegate* unmanaged[Cdecl]<IntPtr,long>)fp)(lp[0]));
            case ("I", "II"): return new IntegerVar(((delegate* unmanaged[Cdecl]<long,long,long>)fp)(li[0],li[1]));
            case ("I", "IR"): return new IntegerVar(((delegate* unmanaged[Cdecl]<long,double,long>)fp)(li[0],ld[1]));
            case ("I", "IS"): return new IntegerVar(((delegate* unmanaged[Cdecl]<long,IntPtr,long>)fp)(li[0],lp[1]));
            case ("I", "RI"): return new IntegerVar(((delegate* unmanaged[Cdecl]<double,long,long>)fp)(ld[0],li[1]));
            case ("I", "RR"): return new IntegerVar(((delegate* unmanaged[Cdecl]<double,double,long>)fp)(ld[0],ld[1]));
            case ("I", "RS"): return new IntegerVar(((delegate* unmanaged[Cdecl]<double,IntPtr,long>)fp)(ld[0],lp[1]));
            case ("I", "SI"): return new IntegerVar(((delegate* unmanaged[Cdecl]<IntPtr,long,long>)fp)(lp[0],li[1]));
            case ("I", "SR"): return new IntegerVar(((delegate* unmanaged[Cdecl]<IntPtr,double,long>)fp)(lp[0],ld[1]));
            case ("I", "SS"): return new IntegerVar(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr,long>)fp)(lp[0],lp[1]));
            // ── REAL return ───────────────────────────────────────────────
            case ("R", ""):   return new RealVar(((delegate* unmanaged[Cdecl]<double>)fp)());
            case ("R", "I"):  return new RealVar(((delegate* unmanaged[Cdecl]<long,double>)fp)(li[0]));
            case ("R", "R"):  return new RealVar(((delegate* unmanaged[Cdecl]<double,double>)fp)(ld[0]));
            case ("R", "S"):  return new RealVar(((delegate* unmanaged[Cdecl]<IntPtr,double>)fp)(lp[0]));
            case ("R", "II"): return new RealVar(((delegate* unmanaged[Cdecl]<long,long,double>)fp)(li[0],li[1]));
            case ("R", "IR"): return new RealVar(((delegate* unmanaged[Cdecl]<long,double,double>)fp)(li[0],ld[1]));
            case ("R", "IS"): return new RealVar(((delegate* unmanaged[Cdecl]<long,IntPtr,double>)fp)(li[0],lp[1]));
            case ("R", "RI"): return new RealVar(((delegate* unmanaged[Cdecl]<double,long,double>)fp)(ld[0],li[1]));
            case ("R", "RR"): return new RealVar(((delegate* unmanaged[Cdecl]<double,double,double>)fp)(ld[0],ld[1]));
            case ("R", "RS"): return new RealVar(((delegate* unmanaged[Cdecl]<double,IntPtr,double>)fp)(ld[0],lp[1]));
            case ("R", "SI"): return new RealVar(((delegate* unmanaged[Cdecl]<IntPtr,long,double>)fp)(lp[0],li[1]));
            case ("R", "SR"): return new RealVar(((delegate* unmanaged[Cdecl]<IntPtr,double,double>)fp)(lp[0],ld[1]));
            case ("R", "SS"): return new RealVar(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr,double>)fp)(lp[0],lp[1]));
            // ── STRING return ─────────────────────────────────────────────
            case ("S", ""):   return PtrToStr(((delegate* unmanaged[Cdecl]<IntPtr>)fp)());
            case ("S", "I"):  return PtrToStr(((delegate* unmanaged[Cdecl]<long,IntPtr>)fp)(li[0]));
            case ("S", "R"):  return PtrToStr(((delegate* unmanaged[Cdecl]<double,IntPtr>)fp)(ld[0]));
            case ("S", "S"):  return PtrToStr(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr>)fp)(lp[0]));
            case ("S", "II"): return PtrToStr(((delegate* unmanaged[Cdecl]<long,long,IntPtr>)fp)(li[0],li[1]));
            case ("S", "IR"): return PtrToStr(((delegate* unmanaged[Cdecl]<long,double,IntPtr>)fp)(li[0],ld[1]));
            case ("S", "IS"): return PtrToStr(((delegate* unmanaged[Cdecl]<long,IntPtr,IntPtr>)fp)(li[0],lp[1]));
            case ("S", "RI"): return PtrToStr(((delegate* unmanaged[Cdecl]<double,long,IntPtr>)fp)(ld[0],li[1]));
            case ("S", "RR"): return PtrToStr(((delegate* unmanaged[Cdecl]<double,double,IntPtr>)fp)(ld[0],ld[1]));
            case ("S", "RS"): return PtrToStr(((delegate* unmanaged[Cdecl]<double,IntPtr,IntPtr>)fp)(ld[0],lp[1]));
            case ("S", "SI"): return PtrToStr(((delegate* unmanaged[Cdecl]<IntPtr,long,IntPtr>)fp)(lp[0],li[1]));
            case ("S", "SR"): return PtrToStr(((delegate* unmanaged[Cdecl]<IntPtr,double,IntPtr>)fp)(lp[0],ld[1]));
            case ("S", "SS"): return PtrToStr(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr,IntPtr>)fp)(lp[0],lp[1]));
            // ── EXTERNAL return — opaque pointer wrapped in ExternalVar ──────
            case ("X", ""):   return new ExternalVar(((delegate* unmanaged[Cdecl]<IntPtr>)fp)());
            case ("X", "I"):  return new ExternalVar(((delegate* unmanaged[Cdecl]<long,IntPtr>)fp)(li[0]));
            case ("X", "R"):  return new ExternalVar(((delegate* unmanaged[Cdecl]<double,IntPtr>)fp)(ld[0]));
            case ("X", "S"):  return new ExternalVar(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr>)fp)(lp[0]));
            case ("X", "N"):  return new ExternalVar(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr>)fp)(lp[0]));
            case ("X", "II"): return new ExternalVar(((delegate* unmanaged[Cdecl]<long,long,IntPtr>)fp)(li[0],li[1]));
            case ("X", "IS"): return new ExternalVar(((delegate* unmanaged[Cdecl]<long,IntPtr,IntPtr>)fp)(li[0],lp[1]));
            case ("X", "SI"): return new ExternalVar(((delegate* unmanaged[Cdecl]<IntPtr,long,IntPtr>)fp)(lp[0],li[1]));
            case ("X", "SS"): return new ExternalVar(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr,IntPtr>)fp)(lp[0],lp[1]));
            case ("X", "NI"): return new ExternalVar(((delegate* unmanaged[Cdecl]<IntPtr,long,IntPtr>)fp)(lp[0],li[1]));
            case ("X", "NS"): return new ExternalVar(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr,IntPtr>)fp)(lp[0],lp[1]));
            default:
                // NOCONV single-arg cases (N slots share IntPtr ABI with S)
                // arity 1
                if (argSig is "N")
                    return retSig switch {
                        "I" => new IntegerVar(((delegate* unmanaged[Cdecl]<IntPtr,long>)fp)(lp[0])),
                        "R" => new RealVar(((delegate* unmanaged[Cdecl]<IntPtr,double>)fp)(lp[0])),
                        _   => PtrToStr(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr>)fp)(lp[0])),
                    };
                // arity 2 — one noconv slot
                if (argSig is "NI")
                    return retSig switch {
                        "I" => new IntegerVar(((delegate* unmanaged[Cdecl]<IntPtr,long,long>)fp)(lp[0],li[1])),
                        _   => PtrToStr(((delegate* unmanaged[Cdecl]<IntPtr,long,IntPtr>)fp)(lp[0],li[1])),
                    };
                if (argSig is "NS")
                    return retSig switch {
                        "I" => new IntegerVar(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr,long>)fp)(lp[0],lp[1])),
                        _   => PtrToStr(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr,IntPtr>)fp)(lp[0],lp[1])),
                    };
                if (argSig is "IN")
                    return retSig switch {
                        "I" => new IntegerVar(((delegate* unmanaged[Cdecl]<long,IntPtr,long>)fp)(li[0],lp[1])),
                        _   => PtrToStr(((delegate* unmanaged[Cdecl]<long,IntPtr,IntPtr>)fp)(li[0],lp[1])),
                    };
                if (argSig is "SN")
                    return retSig switch {
                        "I" => new IntegerVar(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr,long>)fp)(lp[0],lp[1])),
                        _   => PtrToStr(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr,IntPtr>)fp)(lp[0],lp[1])),
                    };
                // arity 2 — two noconv slots
                if (argSig is "NN")
                    return retSig switch {
                        "I" => new IntegerVar(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr,long>)fp)(lp[0],lp[1])),
                        _   => PtrToStr(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr,IntPtr>)fp)(lp[0],lp[1])),
                    };
                // arity 3 — common noconv patterns
                if (argSig is "NII")
                    return retSig switch {
                        "I" => new IntegerVar(((delegate* unmanaged[Cdecl]<IntPtr,long,long,long>)fp)(lp[0],li[1],li[2])),
                        _   => PtrToStr(((delegate* unmanaged[Cdecl]<IntPtr,long,long,IntPtr>)fp)(lp[0],li[1],li[2])),
                    };
                if (argSig is "NNI")
                    return retSig switch {
                        "I" => new IntegerVar(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr,long,long>)fp)(lp[0],lp[1],li[2])),
                        _   => PtrToStr(((delegate* unmanaged[Cdecl]<IntPtr,IntPtr,long,IntPtr>)fp)(lp[0],lp[1],li[2])),
                    };
                throw new NotSupportedException($"Native call sig ({retSig},{argSig}) arity>3 or unknown");
        }
    }

    private static StringVar PtrToStr(IntPtr p) =>
        new(p == IntPtr.Zero ? "" : Marshal.PtrToStringAnsi(p) ?? "");

    // ── F# union helpers (Step 8) ─────────────────────────────────────────

    /// <summary>
    /// Returns true if <paramref name="t"/> is an F# discriminated union type.
    /// Uses FSharp.Core FSharpType.IsUnion when available; falls back to duck-type.
    /// </summary>
    private static bool IsFSharpUnion(Type t)
    {
        try
        {
            var fsharpCoreAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "FSharp.Core");
            if (fsharpCoreAsm != null)
            {
                var fsharpType = fsharpCoreAsm.GetType("Microsoft.FSharp.Reflection.FSharpType");
                if (fsharpType != null)
                {
                    var isUnion = fsharpType.GetMethod("IsUnion",
                        BindingFlags.Public | BindingFlags.Static,
                        null, [typeof(Type), typeof(object)], null);
                    if (isUnion != null)
                    {
                        var result = isUnion.Invoke(null, [t, null]);
                        return result is true;
                    }
                }
            }
        }
        catch { /* fall through to duck-type */ }

        // Duck-type fallback: F# DUs have a "Tag" property and are abstract classes
        return t.IsClass && t.GetProperty("Tag") != null;
    }

    /// <summary>
    /// Converts an F# discriminated union value to a human-readable string:
    ///   zero fields  → "CaseName"
    ///   one field    → "CaseName value"
    ///   many fields  → "CaseName v1 v2 ..."
    /// Uses FSharp.Core FSharpValue.GetUnionFields when available.
    /// </summary>
    private static string FSharpUnionToString(object value, Type t)
    {
        try
        {
            var fsharpCoreAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "FSharp.Core");
            if (fsharpCoreAsm != null)
            {
                var fsharpValue = fsharpCoreAsm.GetType("Microsoft.FSharp.Reflection.FSharpValue");
                if (fsharpValue != null)
                {
                    // GetUnionFields(value, type, bindingFlags=null) → Tuple<UnionCaseInfo, object[]>
                    var getFields = fsharpValue.GetMethod("GetUnionFields",
                        BindingFlags.Public | BindingFlags.Static,
                        null, [typeof(object), typeof(Type), typeof(object)], null);
                    if (getFields != null)
                    {
                        var returned = getFields.Invoke(null, [value, t, null]);
                        if (returned != null)
                        {
                            // The return is a Tuple<UnionCaseInfo, object[]> — unpack via reflection
                            var returnedType = returned.GetType();
                            var caseInfo = returnedType.GetProperty("Item1")!.GetValue(returned)!;
                            var fields   = (object[])returnedType.GetProperty("Item2")!.GetValue(returned)!;
                            var caseName = (string)caseInfo.GetType().GetProperty("Name")!.GetValue(caseInfo)!;

                            if (fields.Length == 0)
                                return caseName;

                            var sb = new System.Text.StringBuilder(caseName);
                            foreach (var f in fields)
                            {
                                sb.Append(' ');
                                sb.Append(f?.ToString() ?? "");
                            }
                            return sb.ToString();
                        }
                    }
                }
            }
        }
        catch { /* fall through */ }

        return value.ToString() ?? "";
    }

    // ── .NET-native path (existing, unchanged) ────────────────────────────

    // ── .NET-reflect tracking (auto-prototype) ────────────────────────────

    /// <summary>
    /// Entry for a reflection-loaded function: records which DLL it came from
    /// so UNLOAD(fname) can decrement the ref-count and release the shared ALC
    /// when it reaches zero. Keyed by folded FNAME in DotNetReflectContexts.
    /// </summary>
    internal sealed class ReflectEntry(string resolvedPath)
    {
        internal readonly string ResolvedPath = resolvedPath;
    }

    /// <summary>
    /// Keyed by folded FNAME. Parallel to NativeContexts (spec path) and
    /// ActiveContexts (IExternalLibrary path). Used by UNLOAD(fname).
    /// </summary>
    internal Dictionary<string, ReflectEntry> DotNetReflectContexts = [];

    /// <summary>
    /// Step 4: one shared PluginLoadContext per resolved DLL path, shared across
    /// all FNAMEs loaded from the same assembly.
    /// </summary>
    internal Dictionary<string, AssemblyLoadContext> DllSharedContexts = [];

    /// <summary>
    /// Step 4: ref-count of how many FNAMEs are currently registered from each
    /// resolved DLL path. The ALC is unloaded when the count reaches zero.
    /// </summary>
    internal Dictionary<string, int> DllRefCounts = [];

    // ── .NET-native path (IExternalLibrary + auto-prototype) ─────────────

    private void LoadDotNetPath(string s1, List<Var> arguments)
    {
        // s1 = DLL path.  s2 = 'Namespace.Class' or 'Namespace.Class::MethodName'
        if (arguments.Count < 2 || !arguments[1].Convert(VarType.STRING, out _, out var nameObj, this))
        {
            LogRuntimeException(136);
            return;
        }

        var rawPath   = s1;
        var classSpec = (string)nameObj;   // e.g. "MyNs.Foo" or "MyNs.Foo::Bar"

        // Resolve relative paths against the directory of the source file
        var resolvedPath = Path.IsPathRooted(rawPath)
            ? rawPath
            : Path.GetFullPath(rawPath,
                Parent.FilesToCompile.Count > 0
                    ? Path.GetDirectoryName(Parent.FilesToCompile[^1]) ?? Directory.GetCurrentDirectory()
                    : Directory.GetCurrentDirectory());

        // Split 'Namespace.Class::MethodName' into className + optional explicitMethod
        string className;
        string? explicitMethod = null;
        var colonColon = classSpec.IndexOf("::", StringComparison.Ordinal);
        if (colonColon >= 0)
        {
            className      = classSpec[..colonColon];
            explicitMethod = classSpec[(colonColon + 2)..];
        }
        else
        {
            className = classSpec;
        }

        // Idempotent for IExternalLibrary path (keyed by resolved path)
        if (ActiveContexts.ContainsKey(resolvedPath) && explicitMethod == null)
        {
            SystemStack.Push(StringVar.Null());
            PredicateSuccess();
            return;
        }

        try
        {
            // Step 4: reuse shared ALC if this DLL is already loaded; create one if not.
            bool ownedNewContext = false;
            if (!DllSharedContexts.TryGetValue(resolvedPath, out var loadContext))
            {
                loadContext = new PluginLoadContext(resolvedPath);
                DllSharedContexts[resolvedPath] = loadContext;
                DllRefCounts[resolvedPath] = 0;
                ownedNewContext = true;
            }

            var assembly = loadContext.LoadFromAssemblyPath(resolvedPath);
            var type     = assembly.GetType(className);

            if (type == null)
            {
                if (ownedNewContext) { DllSharedContexts.Remove(resolvedPath); DllRefCounts.Remove(resolvedPath); loadContext.Unload(); }
                NonExceptionFailure();
                return;
            }

            // ── Fast path: IExternalLibrary ───────────────────────────────
            var instance = Activator.CreateInstance(type) as IExternalLibrary;
            if (instance != null)
            {
                // IExternalLibrary uses ActiveContexts (keyed by path), not the
                // shared-ALC machinery. Remove the shared entry we may have created.
                if (ownedNewContext) { DllSharedContexts.Remove(resolvedPath); DllRefCounts.Remove(resolvedPath); }

                // Snapshot keys before Init so we can identify newly-added entries.
                var keysBefore = new HashSet<string>(FunctionTable.Keys);
                instance.Init(this);

                // Step 4: if the library also implements IStatefulExternalLibrary,
                // wrap every newly-registered FunctionTableEntry with a one-shot
                // first-call guard that fires OnFirstCall(executive) exactly once.
                if (instance is IStatefulExternalLibrary stateful)
                {
                    var fired = false;
                    foreach (var key in FunctionTable.Keys.Where(k => !keysBefore.Contains(k)).ToList())
                    {
                        var original = FunctionTable[key];
                        FunctionTable[key] = new FunctionTableEntry(
                            this, key,
                            args =>
                            {
                                if (!fired) { fired = true; stateful.OnFirstCall(this); }
                                original.Handler(args);
                            },
                            original.ArgumentCount, false);
                    }
                }

                ActiveContexts[resolvedPath] = (loadContext, instance);
                SystemStack.Push(StringVar.Null());
                PredicateSuccess();
                return;
            }

            // ── Auto-prototype: reflect the class ─────────────────────────
            // Find candidate public methods (instance + static, exclude Object members)
            var candidates = type.GetMethods(
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.DeclaringType != typeof(object) && !m.IsSpecialName)
                .ToList();

            MethodInfo method;
            if (explicitMethod != null)
            {
                // Step 3: explicit ::MethodName binding
                var found = candidates.Where(m =>
                    string.Equals(m.Name, explicitMethod, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (found.Count == 0) { if (ownedNewContext) { DllSharedContexts.Remove(resolvedPath); DllRefCounts.Remove(resolvedPath); loadContext.Unload(); } NonExceptionFailure(); return; }
                if (found.Count > 1)  { if (ownedNewContext) { DllSharedContexts.Remove(resolvedPath); DllRefCounts.Remove(resolvedPath); loadContext.Unload(); } NonExceptionFailure(); return; }
                method = found[0];
            }
            else
            {
                // Step 2: auto-discover single unambiguous public method
                if (candidates.Count == 0) { if (ownedNewContext) { DllSharedContexts.Remove(resolvedPath); DllRefCounts.Remove(resolvedPath); loadContext.Unload(); } NonExceptionFailure(); return; }
                if (candidates.Count > 1)  { if (ownedNewContext) { DllSharedContexts.Remove(resolvedPath); DllRefCounts.Remove(resolvedPath); loadContext.Unload(); } NonExceptionFailure(); return; }
                method = candidates[0];
            }

            // SNOBOL4 function name = method name (folded)
            var fname    = method.Name;
            var fnameKey = Parent.FoldCase(fname);

            // Idempotent for reflect path (keyed by fname)
            if (DotNetReflectContexts.ContainsKey(fnameKey))
            {
                if (ownedNewContext) { DllSharedContexts.Remove(resolvedPath); DllRefCounts.Remove(resolvedPath); loadContext.Unload(); }
                SystemStack.Push(StringVar.Null());
                PredicateSuccess();
                return;
            }

            // Build the instance if method is not static
            object? target = method.IsStatic ? null : Activator.CreateInstance(type);

            // Map parameter count for FunctionTableEntry
            var parameters = method.GetParameters();
            var argCount   = parameters.Length;

            // Register FunctionTableEntry using reflection dispatch
            FunctionTable[fnameKey] = new FunctionTableEntry(
                this, fnameKey,
                args => CallReflectFunction(method, target, parameters, args),
                argCount, false);

            // Step 4: record entry (path only; ALC lives in DllSharedContexts)
            DotNetReflectContexts[fnameKey] = new ReflectEntry(resolvedPath);
            DllRefCounts[resolvedPath] = DllRefCounts.GetValueOrDefault(resolvedPath) + 1;

            SystemStack.Push(StringVar.Null());
            PredicateSuccess();
        }
        catch (Exception)
        {
            NonExceptionFailure();
        }
    }

    // ── Type mapping: .NET parameter/return → SNOBOL4 coercion ───────────

    /// <summary>
    /// Map a .NET Type to the SNOBOL4 VarType used for coercion.
    /// long/int/short/byte → INTEGER; double/float → REAL; everything else → STRING.
    /// </summary>
    private static VarType MapDotNetType(Type t)
    {
        if (t == typeof(long)   || t == typeof(int)   ||
            t == typeof(short)  || t == typeof(byte)  ||
            t == typeof(ulong)  || t == typeof(uint))
            return VarType.INTEGER;
        if (t == typeof(double) || t == typeof(float))
            return VarType.REAL;
        return VarType.STRING;
    }

    // ── Reflection call dispatch ──────────────────────────────────────────

    private void CallReflectFunction(
        MethodInfo method, object? target,
        ParameterInfo[] parameters, List<Var> args)
    {
        var callArgs = new object?[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var v       = i < args.Count ? args[i] : new StringVar("");
            var vt      = MapDotNetType(parameters[i].ParameterType);
            v.Convert(vt, out _, out var coerced, this);
            callArgs[i] = vt switch
            {
                VarType.INTEGER => (long)coerced,
                VarType.REAL    => (double)coerced,
                _               => (string)coerced,
            };
        }

        var raw = method.Invoke(target, callArgs);

        // Step 5: if the method returns Task or Task<T>, block until it completes.
        if (raw is Task task)
        {
            task.GetAwaiter().GetResult();   // blocks; rethrows on exception

            // Extract the result from Task<T> via reflection; plain Task → null.
            var taskType = raw.GetType();
            raw = taskType.IsGenericType
                ? taskType.GetProperty("Result")!.GetValue(raw)
                : null;
        }

        // Step 8: F# option<T> coercion.
        // FSharpOption<T> is a class where null == None and non-null == Some(value).
        // We detect it by checking whether the runtime type is a closed generic
        // whose open form is Microsoft.FSharp.Core.FSharpOption`1.
        if (raw != null)
        {
            var rawType = raw.GetType();

            // ── option<T> ─────────────────────────────────────────────────
            if (rawType.IsGenericType &&
                rawType.GetGenericTypeDefinition().FullName == "Microsoft.FSharp.Core.FSharpOption`1")
            {
                // Some: extract the .Value property
                var valueProperty = rawType.GetProperty("Value");
                raw = valueProperty?.GetValue(raw);
                // If somehow Value is null treat as None → fail
                if (raw == null) { NonExceptionFailure(); return; }
            }
            // ── discriminated union (any F# DU) ──────────────────────────
            // F# DUs compile to abstract classes; the Tag property (int) identifies
            // the case and there is one static readonly field per case label.
            // We render as "<Tag> [field1 [field2 ...]]" (space-separated).
            else if (IsFSharpUnion(rawType))
            {
                raw = FSharpUnionToString(raw, rawType);
            }
        }
        else
        {
            // raw == null  →  F# None (option returned null object ref)
            NonExceptionFailure();
            return;
        }

        // Map return value to Var
        // Step 7: Var subclasses (ArrayVar, TableVar, PatternVar, ProgramDefinedDataVar)
        // pass through directly — zero-copy, no coercion overhead.
        Var result = raw switch
        {
            Var    v => v,
            long   l => new IntegerVar(l),
            int    n => new IntegerVar(n),
            double d => new RealVar(d),
            float  f => new RealVar(f),
            string s => new StringVar(s),
            null     => StringVar.Null(),
            _        => new StringVar(raw.ToString() ?? ""),
        };

        SystemStack.Push(result);
        Failure = false;
    }
}
