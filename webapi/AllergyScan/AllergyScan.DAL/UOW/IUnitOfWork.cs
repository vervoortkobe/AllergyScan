using AllergyScan.DAL.Repositories;

namespace AllergyScan.DAL.UOW
{
    public interface IUnitOfWork
    {
        public int Commit();

        public IPersonRepository PersonRepository { get; }
        public IStoreRepository StoreRepository { get; }
    }
}
