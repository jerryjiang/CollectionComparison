using System.Reflection;

namespace CollectionComparison
{
    public abstract class ObjectComparisonStrategy
    {
        public abstract double Compare(object left, object right);
    }

    public sealed class SimpleObjectComparisonStrategy : ObjectComparisonStrategy
    {
        public override double Compare(object left, object right)
        {
            return left.Equals(right) ? 1 : 0;
        }
    }

    public sealed class PropertyBasedComparisonStrategy<T> : ObjectComparisonStrategy
    {
        private readonly double _maxWeight = 0;
        private readonly PropertyInfo[] _properties;

        public PropertyBasedComparisonStrategy()
        {
            _properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            _maxWeight = _properties.Length;
        }

        public override double Compare(object left, object right)
        {
            if (left == null || right == null)
            {
                return left == right ? 1 : 0;
            }

            if (left is not T || right is not T)
            {
                return 0;
            }

            var identicalCount = 0;

            foreach (var property in _properties)
            {
                var lv = property.GetValue(left);
                var rv = property.GetValue(right);

                identicalCount += lv == rv ? 1 : 0;
            }

            return identicalCount / _maxWeight;
        }
    }
}
