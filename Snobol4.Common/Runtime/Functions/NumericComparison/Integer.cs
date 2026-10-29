namespace Snobol4.Common;

public partial class Executive
{
    public void Integer(List<Var> arguments)
    {
        var arg = arguments[0];
        // IntegerVar: always succeeds
        if (arg is IntegerVar) { PredicateSuccess(); return; }
        // RealVar: always fails (even 3.0 is not an integer)
        if (arg is RealVar)    { NonExceptionFailure(); return; }
        // StringVar: succeed only if the string parses as a plain integer (not real)
        if (arg is StringVar sv && Var.ToInteger(sv.Data, out _)) { PredicateSuccess(); return; }
        // a name made of a variable's text is that text, as wherever a number is wanted (INTEGER(.$'3') succeeds), as in SPITBOL; the
        // name of an element or a keyword has no text
        if (arg is NameVar { HasText: true, Collection: null } name && Var.ToInteger(name.Pointer, out _)) { PredicateSuccess(); return; }
        NonExceptionFailure();
    }
}