namespace Snobol4.Common;

public partial class Executive
{
    internal void Ident(List<Var> arguments)
    {
        if (arguments[0].IsIdentical(arguments[1]))
        {
            PredicateSuccess();
            return;
        }

        NonExceptionFailure();
    }
}