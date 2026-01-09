namespace Snobol4.Common;

public partial class Executive
{
    // Lock object for thread synchronization
    private readonly Lock _sortLock = new();

    #region BaseSort

    private void BaseSort(List<Var> arguments, bool ascending)
    {
        lock (_sortLock)
        {
            // Note: Do not convert arguments other than to convert from table to array.
            // Otherwise, bad side effects will occur

            //"erroneous 2nd arg in sort/rsort of vector" /* 257 */
            //"sort/rsort 1st arg not suitable array or table" /* 256 */
            //"sort/rsort 2nd arg out of range or non-integer" /* 258 */

            // First argument must be an array or table
            if (!arguments[0].Convert(VarType.ARRAY, out var a, out _, this))
            {
                LogRuntimeException(256);
                return;
            }

            var array = (ArrayVar)a;

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

            // Transfer array to an m x n matrix where n may be a single column
            var columns = array.Dimensions > 1 ? array.Sizes[1] : 1;
            for (var colNum = 0; colNum < columns; colNum++)
            {
                List<Var> row = [];
                matrix.Add(row);
                var start = colNum * (int)array.Sizes[0];
                var finish = colNum * (int)array.Sizes[0] + (int)array.Sizes[0];
                matrix[^1].AddRange(array.Data[start..finish]);
            }

            // Sort matrix
            matrix.Sort((row1, row2) =>
            {
                var v1 = row1[(int)sortColumn];
                var v2 = row2[(int)sortColumn];
                return ascending ? v1.Compare(v2) : -v1.Compare(v2);
            });

            //Transfer sorted matrix back to new array
            var index = 0;
            var arraySort = new ArrayVar();
            arraySort.ConfigurePrototype(array.Prototype, StringVar.Null());

            foreach (var cell in matrix.SelectMany(row => row))
                arraySort.Data[index++] = cell;

            SystemStack.Push(arraySort);
        }
    }

    #endregion
}