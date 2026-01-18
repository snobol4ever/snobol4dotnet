namespace Snobol4.Common;

public partial class Executive
{
    /// <summary>
    /// Executes the interrogation operation (unary question mark) or "value
    /// annihilation." If X is an expression which fails, ?X also fails. However,
    /// if X succeeds, ?X also succeeds, returning the null string.
    /// </summary>
    internal void Interrogation(List<Var> arguments)
    {
        var v = SystemStack.Pop();
        var stringVar = StringVar.Null();
        stringVar.Succeeded = v.Succeeded;
        SystemStack.Push(stringVar);
    }
}