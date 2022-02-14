using CollectionComparison;
using CollectionComparison.Samples;

var left = new[] { "7", "0", "1", "5", "2", "3", "4" };
var right = new[] { "8", "0", "2", "3", "1", "4", "5" };

var leftObjects = left.Select(x => new YourObject { Name = x });
var rightObjects = right.Select(x => new YourObject { Name = x });

var comparer = new CollectionComparer
{
    Strategy = new PropertyBasedComparisonStrategy<YourObject>()
};
var result = comparer.Compare(leftObjects, rightObjects, expand: true);

foreach (var item in result)
{
    Console.WriteLine(item);
}