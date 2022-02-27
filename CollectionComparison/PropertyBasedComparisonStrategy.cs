using System.Reflection;

namespace CollectionComparison
{
    public sealed class PropertyBasedComparisonStrategy<T> : ObjectComparisonStrategy
    {
        private readonly double _maxWeight = 0;
        private readonly PropertyInfo[] _properties;
        private Dictionary<object, Dictionary<string, object>> _valueCache;
        private IList<string> _valueKeys;

        public PropertyBasedComparisonStrategy()
        {
            _properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            _maxWeight = _properties.Length;

            _valueCache = new Dictionary<object, Dictionary<string, object>>();
            _valueKeys = _properties.Select(x => x.Name).ToList();
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

            if (!_valueCache.ContainsKey(left))
            {
                _valueCache.Add(left, GetValues(left));
            }

            if (!_valueCache.ContainsKey(right))
            {
                _valueCache.Add(right, GetValues(right));
            }

            var identicalCount = 0;

            foreach (var key in _valueKeys)
            {
                var lv = _valueCache[left][key];
                var rv = _valueCache[right][key];

                if (lv == null || rv == null)
                {
                    identicalCount += lv == rv ? 1 : 0;
                }
                else
                {
                    identicalCount += object.Equals(lv, rv) ? 1 : 0;
                }

                // identicalCount += _valueCache[left][key] == _valueCache[right][key] ? 1 : 0;
            }

            // foreach (var property in _properties)
            // {
            //     var lv = property.GetValue(left);
            //     var rv = property.GetValue(right);

            //     identicalCount += lv == rv ? 1 : 0;
            // }

            return identicalCount / _maxWeight;
        }

        private Dictionary<string, object> GetValues(object value)
        {
            var cache = new Dictionary<string, object>();

            foreach (var property in _properties)
            {
                var data = property.GetValue(value);
                cache.Add(property.Name, data);
            }

            return cache;
        }
    }
}
