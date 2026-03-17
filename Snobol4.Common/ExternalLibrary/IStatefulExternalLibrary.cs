namespace Snobol4.Common;

/// <summary>
/// Optional interface for IExternalLibrary implementations that need
/// per-entry initialisation, equivalent to SPITBOL's xn1st / first_call
/// mechanism for C external functions.
///
/// If a library class also implements IStatefulExternalLibrary, the runtime
/// calls OnFirstCall() exactly once — the first time any SNOBOL4 statement
/// calls the function — before the function body executes.
///
/// Usage:
///   public class MyLib : IExternalLibrary, IStatefulExternalLibrary
///   {
///       private int _callCount;
///       public void Init(Executive e) { e.FunctionTable["MYFN"] = ...; }
///       public void OnFirstCall(Executive e) { _callCount = 0; }
///       public Var Execute(Executive e, List&lt;Var&gt; args) { return ...; }
///   }
///
/// The IExternalLibrary instance is retained across calls (Init is called once
/// at LOAD time), so instance fields serve as persistent state — no separate
/// dictionary is required unless you register multiple functions per class.
/// </summary>
public interface IStatefulExternalLibrary
{
    /// <summary>
    /// Called exactly once, on the first execution of any function registered
    /// by this library instance's Init() call.  Equivalent to testing
    /// snobol4_first_call() == 1 in a C external function.
    /// </summary>
    void OnFirstCall(Executive executive);
}
