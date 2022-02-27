namespace CollectionComparison
{
    public sealed class ObjectComparer<T> : IObjectComparer
    {
        public ObjectComparisonStrategy Strategy { get; set; }

        public double Compare(object left, object right)
        {
            return Strategy.Compare(left, right);
        }
    }
}
