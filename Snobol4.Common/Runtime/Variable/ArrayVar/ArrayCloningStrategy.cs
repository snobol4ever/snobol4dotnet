namespace Snobol4.Common;

public class ArrayCloningStrategy : ICloningStrategy
{
    public Var Clone(Var self)
    {
        var arraySelf = (ArrayVar)self;
        var dimensions = (int)arraySelf.Dimensions;
        var totalSize = (int)arraySelf.TotalSize;

        var clonedArray = new ArrayVar
        {
            Symbol = arraySelf.Symbol,
            Prototype = arraySelf.Prototype,
            Dimensions = arraySelf.Dimensions,
            TotalSize = arraySelf.TotalSize,
            InputChannel = arraySelf.InputChannel,
            OutputChannel = arraySelf.OutputChannel,
            Succeeded = arraySelf.Succeeded
        };

        CloneDimensionMetadata(arraySelf, clonedArray, dimensions);
        CloneDataElements(arraySelf, clonedArray, totalSize);
        clonedArray.Fill = arraySelf.Fill;
        return clonedArray;
    }

                private static void CloneDimensionMetadata(ArrayVar source, ArrayVar target, int dimensions)
    {
        // Pre-allocate with exact capacity to avoid resizing
        target.Sizes.Capacity = dimensions;
        target.LowerBounds.Capacity = dimensions;
        target.UpperBounds.Capacity = dimensions;
        target.Multipliers.Capacity = dimensions;
        
        target.Sizes.AddRange(source.Sizes);
        target.LowerBounds.AddRange(source.LowerBounds);
        target.UpperBounds.AddRange(source.UpperBounds);
        target.Multipliers.AddRange(source.Multipliers);
    }

                private static void CloneDataElements(ArrayVar source, ArrayVar target, int totalSize)
    {
        target.Data.Capacity = totalSize;
        
        // Use for loop instead of foreach for better performance
        var sourceData = source.Data;
        for (var i = 0; i < totalSize; i++)
        {
            target.Data.Add(sourceData[i].Clone());
        }
    }
}