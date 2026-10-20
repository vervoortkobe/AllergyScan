using AllergyScan.DAL.DAL;
using AllergyScan.DAL.Repositories;

namespace AllergyScan.DAL.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private GameContext _gameContext;
        private IPersonRepository _personRepository;
        private IStoreRepository _storeRepository;

        public UnitOfWork(GameContext gameContext, IPersonRepository personRepository, IStoreRepository storeRepository)
        {
            _gameContext = gameContext;
            _personRepository = personRepository;
            _storeRepository = storeRepository;
        }

        public IPersonRepository PersonRepository { get { return _personRepository; } }

        public IStoreRepository StoreRepository { get { return _storeRepository; } }

        public int Commit()
        {
            return _gameContext.SaveChanges();
        }
    }
}
