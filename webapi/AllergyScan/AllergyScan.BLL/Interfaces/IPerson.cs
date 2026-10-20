using AllergyScan.DAL;

namespace AllergyScan.BLL.Interfaces
{
    public interface IPerson
    {
        public List<Person> GetAll();

        public Person GetbyId(int id);

        public void Add(Person person);

        public void Update(Person person);

        public void Delete(int id);
    }
}
