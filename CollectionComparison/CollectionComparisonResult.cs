namespace CollectionComparison
{
    public class CollectionComparisonResult
    {
        public int ExpandedIndex { get; set; }

        public double Similarity { get; set; }

        public ResultType ResultType { get; set; }

        public OrderedItem Left { get; set; }

        public OrderedItem Right { get; set; }

        public CollectionComparisonResult Chain { get; set; }

        public override string ToString()
        {
            return $"{ResultType,-12} {Similarity,-5:N2} {ExpandedIndex,4} {Left?.Value,-20} {Right?.Value,-20}";
        }
    }
}
