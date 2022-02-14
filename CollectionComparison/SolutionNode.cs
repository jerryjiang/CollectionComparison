using System.Collections.ObjectModel;

namespace CollectionComparison
{
    public class SolutionNode
    {
        public SolutionNode()
        {
            Children = new Collection<SolutionNode>();
        }

        public double Weight { get; set; }

        public SolutionNode Parent { get; set; }

        public ICollection<SolutionNode> Children { get; set; }

        public CollectionComparisonResult Result { get; set; }

        public override string ToString()
        {
            return $"{Result.ResultType}: {Result.Left.Index} ({Result.Left.Value}) vs {Result.Right.Index} ({Result.Right.Value})";
        }
    }
}
