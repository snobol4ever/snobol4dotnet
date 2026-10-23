namespace Snobol4.Common;

public partial class Executive
{
    // COPY(x): a new array, table or DATA object holding the same values one level deep (nested arrays,
    // tables and DATA objects are shared), as SPITBOL's COPY makes it, and a pattern, an expression or a code that is not IDENT to
    // its argument (a Clone keeps their identity, as an assignment does); any other value as it is
    internal void Copy(List<Var> arguments)
    {
        if (arguments[0] is not ProgramDefinedDataVar data)
        {
            SystemStack.Push(arguments[0] switch
            {
                PatternVar pattern => new PatternVar(pattern.Data.Clone()),
                ExpressionVar expression => new ExpressionVar(expression),
                CodeVar code => new CodeVar(code),
                var other => other.Clone()
            });
            return;
        }

        // a DATA object's Clone shares its fields (its identity); a copy has fields of its own
        var copy = new ProgramDefinedDataVar(data.DataName, data.Definition.Prototype, data.Definition.FieldNames);
        for (var i = 0; i < data.FieldValues.Data.Count; i++)
        {
            var field = data.FieldValues.Data[i];
            copy.FieldValues.Data[i] = field is ArrayVar or TableVar or ProgramDefinedDataVar ? field : field.Clone();
        }

        SystemStack.Push(copy);
    }
}