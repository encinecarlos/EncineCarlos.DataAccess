namespace EncineCarlos.DataAccess.MongoDb
{
    public class BaseEntity<TId>
    {
        public TId Id { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is BaseEntity<TId> entity &&
                   EqualityComparer<TId>.Default.Equals(Id, entity.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
}
