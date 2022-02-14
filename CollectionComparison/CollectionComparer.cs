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

        private List<CollectionComparisonResult> CompareItems(List<OrderedItem> leftItems, List<OrderedItem> rightItems, double minimumSimilarity)
        {
            var leafNodes = new List<SolutionNode>();
            var rootNode = CreateRootNode();
            var optimalNode = rootNode;

            leafNodes.Add(rootNode);

            foreach (var leftItem in leftItems)
            {
                foreach (var rightItem in rightItems)
                {
                    var similarity = _objectComparer.Compare(leftItem.Value, rightItem.Value);

                    if (similarity < minimumSimilarity)
                    {
                        // Similarity too low, drop the result.
                        continue;
                    }

                    var result = new CollectionComparisonResult
                    {
                        Similarity = similarity,
                        ResultType = similarity == 1 ? ResultType.Identical : ResultType.Different,
                        Left = leftItem,
                        Right = rightItem,
                    };

                    AttachToTree(leafNodes, result, ref optimalNode);
                }
            }

            var child = optimalNode;
            var solution = new List<CollectionComparisonResult>();

            while (child != null && child != rootNode)
            {
                solution.Add(child.Result);
                child = child.Parent;
            }

            solution.Reverse();

            return solution;
        }

        private static void AttachToTree(List<SolutionNode> leafNodes, CollectionComparisonResult result, ref SolutionNode optimalNode)
        {
            var processedParents = new List<SolutionNode>();

            for (int i = leafNodes.Count - 1; i >= 0; i--)
            {
                var currentLeaf = leafNodes[i];
                var potentialParent = currentLeaf;

                do
                {
                    if (processedParents.Contains(potentialParent))
                    {
                        break;
                    }

                    if (potentialParent.Result.Left.Index < result.Left.Index &&
                        potentialParent.Result.Right.Index < result.Right.Index)
                    {
                        var newLeaf = new SolutionNode
                        {
                            Parent = potentialParent,
                            Weight = potentialParent.Weight + result.Similarity,
                            Result = result
                        };

                        if (optimalNode == null ||
                            optimalNode.Weight < newLeaf.Weight)
                        {
                            optimalNode = newLeaf;
                        }

                        leafNodes.Add(newLeaf);
                        potentialParent.Children.Add(newLeaf);
                        processedParents.Add(potentialParent);

                        break;
                    }

                    potentialParent = potentialParent.Parent;
                } while (null != potentialParent);

                // When the new node append to the current leaf,
                // current leaf will no longer a leaf.
                if (currentLeaf.Children.Count > 0)
                {
                    leafNodes.Remove(currentLeaf);
                }
            }
        }

        private static SolutionNode CreateRootNode()
        {
            return new SolutionNode
            {
                Weight = 0,
                Result = new CollectionComparisonResult
                {
                    Similarity = 0,
                    Left = new OrderedItem
                    {
                        Index = -1
                    },
                    Right = new OrderedItem
                    {
                        Index = -1
                    }
                }
            };
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
