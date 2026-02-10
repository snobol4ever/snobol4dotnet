namespace Snobol4.Common;

public class SubjectConversionStrategy : IConversionStrategy
{
    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        var subjectSelf = (SubjectVar)self;
        varOut = StringVar.Null();
        valueOut = "";

        switch (targetType)
        {
            case Executive.VarType.STRING:
                varOut = new StringVar(subjectSelf.Subject);
                valueOut = subjectSelf.Subject;
                return true;

            case Executive.VarType.INTEGER:
                if (!Var.ToInteger(subjectSelf.Subject, out var intValue))
                    return false;
                varOut = new IntegerVar(intValue);
                valueOut = intValue;
                return true;

            case Executive.VarType.REAL:
                if (!Var.ToReal(subjectSelf.Subject, out var realValue))
                    return false;
                varOut = new RealVar(realValue);
                valueOut = realValue;
                return true;

            case Executive.VarType.PATTERN:
                varOut = new PatternVar(new LiteralPattern(subjectSelf.Subject));
                valueOut = ((PatternVar)varOut).Data;
                return true;

            case Executive.VarType.ARRAY:
            case Executive.VarType.TABLE:
            case Executive.VarType.NAME:
            case Executive.VarType.EXPRESSION:
            case Executive.VarType.CODE:
            default:
                return false;
        }
    }

    public string GetDataType(Var self)
    {
        return "subject";
    }

    public object GetTableKey(Var self)
    {
        var subjectSelf = (SubjectVar)self;
        // Use the subject string as the key
        return subjectSelf.Subject;
    }
}