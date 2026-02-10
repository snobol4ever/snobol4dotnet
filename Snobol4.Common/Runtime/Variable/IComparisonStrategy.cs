namespace Snobol4.Common;

public interface IComparisonStrategy
{
                int CompareTo(Var self, Var other);

                bool Equals(Var self, Var other);

                bool IsIdentical(Var self, Var other);
}