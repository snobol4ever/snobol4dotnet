namespace Snobol4.Common;

//"erroneous 2nd arg in sort/rsort of vector" /* 257 */
//"sort/rsort 1st arg not suitable array or table" /* 256 */
//"sort/rsort 2nd arg out of range or non-integer" /* 258 */

public partial class Executive
{

    private void BaseSort(List<Var> arguments, bool ascending)
    {
        // Note: Do not convert arguments other than to convert from table to array.
        // Otherwise, bad side effects will occur

        // First argument must be an array or table
        if (!arguments[0].Convert(VarType.ARRAY, out var a, out _, this))
        {
            // a table with no non-null entries has no array to sort, and SORT fails, as in SPITBOL
            if (arguments[0] is TableVar)
            {
                NonExceptionFailure();
                return;
            }

            LogRuntimeException(256);
            return;
        }

        var array = (ArrayVar)a;

        // the array made from a table to be sorted is no object the dump numbers: only the array SORT returns is one
        if (arguments[0] is TableVar && array.BlockId == BlockCounterValue)
        {
            array.BlockId = 0;
            BlockCounterValue--;
        }

        // Array must be one or two-dimensional array
        if (array is { Dimensions: > 2 })
        {
            LogRuntimeException(256);
            return;
        }

        long column = 0;

        switch (arguments[1])
        {
            case StringVar { Data: "" }:
                column = array.LowerBounds[0];
                break;

            case IntegerVar iv when iv.Data >= array.LowerBounds[0] && iv.Data <= array.UpperBounds[0]:
                column = iv.Data;
                break;

            case NameVar when array.Dimensions == 1:
                break;

            default:
                LogRuntimeException(258);
                break;
        }

        var sortColumn = column - array.LowerBounds[0];
        List<List<Var>> matrix = [];

        // Transfer array to an m x n matrix.
        // For 1D arrays: each element is a row of 1 cell.
        // For 2D arrays: each column-slice is a row (rows = Sizes[1], cols = Sizes[0]).
        if (array.Dimensions == 1)
        {
            for (var rowNum = 0; rowNum < (int)array.Sizes[0]; rowNum++)
                matrix.Add([array.Data[rowNum]]);
            sortColumn = 0; // 1D: always sort by the single element
        }
        else
        {
            var columns = array.Sizes[1];
            for (var colNum = 0; colNum < columns; colNum++)
            {
                List<Var> row = [];
                matrix.Add(row);
                var start = colNum * (int)array.Sizes[0];
                var finish = colNum * (int)array.Sizes[0] + (int)array.Sizes[0];
                matrix[^1].AddRange(array.Data[start..finish]);
            }
        }

        // The array SPITBOL sorts for a table lists the buckets last first and each bucket's entries in the order they were added
        // (CONVERT lists them in the order added); rows are the table's entries that are not null, as ConvertToArray took them
        if (arguments[0] is TableVar table)
        {
            var buckets = table.Data.Where(entry => entry.Value is not StringVar { Data: "" })
                                    .Select(entry => KeyBucket(entry.Key, table.Buckets)).ToList();
            matrix = matrix.Select((row, position) => (row, position))
                           .OrderByDescending(entry => buckets[entry.position])
                           .Select(entry => entry.row).ToList();
        }

        // Sort matrix. Rows with equal keys keep the order they had, in SORT and RSORT alike: SPITBOL compares the offsets of equal
        // keys (sortc), and List.Sort is not stable
        var ranked = matrix.Select((row, position) => (row, position)).ToList();
        ranked.Sort((first, second) =>
        {
            var v1 = first.row[(int)sortColumn];
            var v2 = second.row[(int)sortColumn];
            var order = ascending ? v1.Compare(v2) : -v1.Compare(v2);
            return order != 0 ? order : first.position.CompareTo(second.position);
        });
        matrix = ranked.Select(entry => entry.row).ToList();

        //Transfer sorted matrix back to new array
        var index = 0;
        var arraySort = new ArrayVar { BlockId = NewBlockId() };
        arraySort.ConfigurePrototype(array.Prototype, StringVar.Null());

        foreach (var cell in matrix.SelectMany(row => row))
            arraySort.Data[index++] = cell;

        SystemStack.Push(arraySort);
    }
}