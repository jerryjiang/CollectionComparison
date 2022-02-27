namespace CollectionComparison
{
    public sealed class SimpleObjectComparisonStrategy : ObjectComparisonStrategy
    {
        public override double Compare(object left, object right)
        {
            return left.Equals(right) ? 1 : 0;
        }
    }
}
