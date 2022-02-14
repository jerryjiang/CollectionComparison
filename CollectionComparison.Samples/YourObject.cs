namespace CollectionComparison.Samples
{
    public class YourObject
    {
        public string Name { get; set; }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is YourObject aobj)
            {
                return Name.Equals(aobj.Name);
            }

            return false;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
