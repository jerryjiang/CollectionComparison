namespace CollectionComparison
{
    public interface IObjectComparer
    {
        ObjectComparisonStrategy Strategy { get; set; }

        double Compare(object left, object right);
    }
}
