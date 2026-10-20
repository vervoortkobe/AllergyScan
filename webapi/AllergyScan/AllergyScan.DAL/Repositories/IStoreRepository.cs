namespace AllergyScan.DAL.Repositories
{
    public interface IStoreRepository
    {
        public IEnumerable<Store> GetAll(Sort sort, SortBy sortBy, string city, string zipCode, int page, int pageLength);

        public Store GetById(int id);

        public void Create(Store person);
        public void Update(Store modifiedPerson);

        public void Delete(int id);
    }
}
