namespace Snobol4.Common;

//"dump argument is not integer" /* 88 */,
//"dump argument is negative or too large" /* 89 */,

public partial class Executive
{

    internal void DisplayVariableValues(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.INTEGER, out _, out var value, this))
        {
            LogRuntimeException(88);
            return;
        }

        DisplayVariableValues((long)value);
    }

    internal void DisplayVariableValues()
    {
        var dumpVar = IdentifierTable["&dump"];

        if (!dumpVar.Convert(VarType.INTEGER, out _, out var value, this))
        {
            LogRuntimeException(88);
            return;
        }

        DisplayVariableValues((long)value);
    }

    private void DisplayVariableValues(long value)
    {
        switch (value)
        {
            case 0:
                break;

            case 1:
                DumpNaturalVariables(false);
                DumpKeywords(false);
                break;

            case 2:
                DumpNaturalVariables(false);
                DumpKeywords(false);
                DumpArrayAndTableElements(false);
                break;

            case 3:
                DumpNaturalVariables(true);
                DumpKeywords(true);
                DumpArrayAndTableElements(true);
                break;

            default:
                LogRuntimeException(89);
                break;
        }
    }

    private void DumpNaturalVariables(bool includeNull)
    {
        var i = 1;
        Console.Error.WriteLine("""

                          dump of natural variables

                          """);

        foreach (var kvp in IdentifierTable.Where(kvp => kvp.Key[0] != '&' && !kvp.Value.IsReadOnly))
        {
            if (includeNull || kvp.Value is not StringVar || (kvp.Value is StringVar stringVar && stringVar.Data != ""))
            {
                switch (kvp.Value)
                {
                    case ArrayVar arrayVar:
                        Console.Error.WriteLine($@"{kvp.Key} = array({arrayVar.Prototype}) #{i++}");
                        break;

                    case TableVar tableVar:
                        Console.Error.WriteLine($@"{kvp.Key} = table({tableVar.Data.Count}) #{i++}");
                        break;

                    default:
                        Console.Error.WriteLine($@"{kvp.Key} = {kvp.Value.DumpString()}");
                        break;
                }
            }
        }
    }

    private void DumpKeywords(bool includeNull)
    {
        Console.Error.WriteLine("""

                                dump of keyword values

                                """);

        foreach (var kvp in IdentifierTable.Where(kvp => kvp.Key[0] == '&' && !kvp.Value.IsReadOnly))
        {
            if (includeNull || kvp.Value.ToString() != "")
                Console.Error.WriteLine($@"{kvp.Key} = {kvp.Value.DumpString()}");
        }
        Console.Error.WriteLine("""



                                """);
    }

    private void DumpArrayAndTableElements(bool includeNull)
    {
        var i = 1;

        foreach (var kvp in IdentifierTable.Where(kvp => kvp.Value is TableVar or ArrayVar))
        {
            switch (kvp.Value)
            {
                case ArrayVar arrayVar:
                    Console.Error.WriteLine($"""

                                       array({arrayVar.Prototype}) #{i++}
                                       """);
                    List<List<long>> indices = [];
                    var incrementEvery = 1;

                    for (var j = (int)arrayVar.Dimensions - 1; j >= 0; --j)
                    {
                        indices.Add([]);
                        var index = indices[^1];
                        var increment = 0;
                        var fill = arrayVar.LowerBounds[j];

                        for (var k = 0; k < arrayVar.TotalSize; ++k)
                        {
                            index.Add(fill);
                            if (++increment == incrementEvery)
                            {
                                increment = 0;
                                fill++;
                            }

                            if (fill > arrayVar.UpperBounds[j])
                                fill = arrayVar.LowerBounds[j];
                        }

                        incrementEvery *= (int)arrayVar.Sizes[j];
                    }

                    for (var key = 0; key < arrayVar.TotalSize; ++key)
                    {
                        List<long> subscripts = [];

                        for (var m = 0; m < arrayVar.Dimensions; ++m)
                        {
                            subscripts.Add(indices[m][key]);
                        }

                        if (includeNull || arrayVar.Data[key] is not StringVar { Data: "" })
                            Console.Error.WriteLine($@"{kvp.Key}<{string.Join(",", subscripts)}> = {arrayVar.Data[key].DumpString()}");
                    }

                    break;

                case TableVar tableVar:
                    Console.Error.WriteLine($"""

                                       table({tableVar.Data.Count}) #{i++}
                                       """);

                    if (includeNull)
                    {
                        foreach (var kvp2 in tableVar.Data)
                            Console.Error.WriteLine($@"{kvp.Key}<{kvp2.Key}> = {kvp2.Value.DumpString()}");
                        break;
                    }

                    foreach (var kvp2 in tableVar.Data.Where(kvp2 => kvp2.Value is not StringVar { Data: "" }))
                        Console.Error.WriteLine($@"{kvp.Key}<{kvp2.Key}> = {kvp2.Value.DumpString()}");
                    break;
            }
        }
    }

}