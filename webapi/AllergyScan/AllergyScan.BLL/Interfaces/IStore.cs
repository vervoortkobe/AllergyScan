namespace AllergyScan.BLL.Interfaces
{
    public interface IStore
    {
        public List<Store> GetAll(Sort sort, SortBy sortBy, string city, string zipCode, int page, int pageLength);

        public Store GetbyId(int id);

        public void Add(Store store);

        public void Update(Store store);

        public void Delete(int id);
    }
}
