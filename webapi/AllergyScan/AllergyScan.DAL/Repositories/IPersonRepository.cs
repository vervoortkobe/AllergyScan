namespace AllergyScan.DAL.Repositories
{
    public interface IPersonRepository
    {
        public IEnumerable<Person> GetAll();

        public Person GetById(int id);

        public void Create(Person person);
        public void Update(Person modifiedPerson);

        public void Delete(int id);
    }
}
