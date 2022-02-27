namespace CollectionComparison
{
    public sealed class CollectionComparer
    {
        private bool _visiting = false;
        private IObjectComparer _objectComparer;

        public ObjectComparisonStrategy Strategy { get; set; } = new SimpleObjectComparisonStrategy();

        public IEnumerable<CollectionComparisonResult> Compare<T>(IEnumerable<T> left, IEnumerable<T> right, double minimumSimilarity = 1.0d, bool expand = false)
        {
            if (_visiting)
            {
                throw new InvalidOperationException("There is a comparision in progress, the comparer only support one job at a single time.");
            }

            _visiting = true;
            _objectComparer = new ObjectComparer<T>
            {
                Strategy = Strategy
            };

            try
            {
                var leftItems = GetOrderedItems(left);
                var rightItems = GetOrderedItems(right);

                var solution = CompareItems(leftItems, rightItems, minimumSimilarity);

                if (expand)
                {
                    ExpandSolution(solution, leftItems, rightItems);
                }

                return solution;
            }
            finally
            {
                _visiting = false;
            }
        }

        private List<CollectionComparisonResult> CompareItems(List<OrderedItem> leftItems, List<OrderedItem> rightItems, double minimumSimilarity)
        {
            var candidates = new Dictionary<int, CollectionComparisonResult>
            {
                {
                    0,
                    new CollectionComparisonResult
                    {
                        Left = new OrderedItem { Index = -1 },
                        Right = new OrderedItem { Index = -1 },
                        Chain = null
                    }
                }
            };

            foreach (var leftItem in leftItems)
            {
                var matchedIndices = new List<(OrderedItem Item, double Similarity)>();

                foreach (var rightItem in rightItems)
                {
                    var similarity = _objectComparer.Compare(leftItem.Value, rightItem.Value);

                    if (similarity < minimumSimilarity)
                    {
                        // Similarity too low, drop the result.
                        continue;
                    }

                    matchedIndices.Add((rightItem, similarity));
                }

                var targetPos = 0;
                var parentPos = 0;

                var candidate = candidates[0];

                for (var jX = 0; jX < matchedIndices.Count; jX++)
                {
                    var right = matchedIndices[jX].Item;
                    var similarity = matchedIndices[jX].Similarity;

                    for (parentPos = targetPos; parentPos < candidates.Count; parentPos++)
                    {
                        if ((candidates[parentPos].Right.Index < right.Index) &&
                            ((parentPos == candidates.Count - 1) || (candidates[parentPos + 1].Right.Index > right.Index)))
                        {
                            break;
                        }
                    }

                    if (parentPos < candidates.Count)
                    {
                        var newCandidate = new CollectionComparisonResult
                        {
                            Left = leftItem,
                            Right = right,
                            Similarity = similarity,
                            ResultType = similarity == 1 ? ResultType.Identical : ResultType.Different,
                            Chain = candidates[parentPos]
                        };

                        candidates[targetPos] = candidate;
                        targetPos = parentPos + 1;
                        candidate = newCandidate;

                        if (targetPos == candidates.Count)
                        {
                            break; // no point in examining further (j)s
                        }
                    }
                }

                candidates[targetPos] = candidate;
            }

            var child = candidates[candidates.Count - 1];
            var solution = new List<CollectionComparisonResult>();

            while (child != null && child.Left.Index >= 0)
            {
                solution.Add(child);
                child = child.Chain;
            }

            solution.Reverse();

            return solution;
        }

        private static void ExpandSolution(List<CollectionComparisonResult> solution, List<OrderedItem> leftItems, List<OrderedItem> rightItems)
        {
            var leftEndIndex = leftItems.Count - 1;
            var rightEndIndex = rightItems.Count - 1;
            var expandedIndex = leftItems.Count + rightItems.Count - solution.Count - 1;

            for (int index = solution.Count - 1; index >= 0; index--)
            {
                var current = solution[index];

                for (var i = rightEndIndex; i > current.Right.Index; i--)
                {
                    solution.Insert(index + 1, new CollectionComparisonResult
                    {
                        ExpandedIndex = expandedIndex--,
                        Similarity = 0,
                        Left = null,
                        Right = rightItems[i],
                        ResultType = ResultType.RightOnly
                    });
                }

                for (var i = leftEndIndex; i > current.Left.Index; i--)
                {
                    solution.Insert(index + 1, new CollectionComparisonResult
                    {
                        ExpandedIndex = expandedIndex--,
                        Similarity = 0,
                        Left = leftItems[i],
                        Right = null,
                        ResultType = ResultType.LeftOnly
                    });
                }

                current.ExpandedIndex = expandedIndex--;
                leftEndIndex = current.Left.Index - 1;
                rightEndIndex = current.Right.Index - 1;
            }

            for (int i = rightEndIndex; i >= 0; i--)
            {
                solution.Insert(0, new CollectionComparisonResult
                {
                    ExpandedIndex = expandedIndex--,
                    Similarity = 0,
                    Left = null,
                    Right = rightItems[i],
                    ResultType = ResultType.RightOnly
                });
            }

            for (var i = leftEndIndex; i >= 0; i--)
            {
                solution.Insert(0, new CollectionComparisonResult
                {
                    ExpandedIndex = expandedIndex--,
                    Similarity = 0,
                    Left = leftItems[i],
                    Right = null,
                    ResultType = ResultType.LeftOnly
                });
            }
        }

        private static List<OrderedItem> GetOrderedItems<T>(IEnumerable<T> values)
        {
            var index = 0;
            var items = new List<OrderedItem>();

            foreach (var value in values)
            {
                items.Add(new OrderedItem { Index = index++, Value = value });
            }

            return items;
        }
    }
}
