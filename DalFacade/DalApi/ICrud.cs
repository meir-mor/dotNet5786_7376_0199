using DO;

namespace DalApi;
public interface ICrud <T> where T : class
{
    void Create(T item); //Creates new entity object in DAL
    IEnumerable<T> ReadAll(Func<T, bool>? filter = null); //stage 1 only, Reads all entity objects
    T? Read(Func<T, bool> filter); //Reads entity object by where condition
    T? Read(int id); //Reads entity object by where condition
    void Update(T item); //Updates entity object
    void Delete(int id); //Deletes an object by its Id
    void DeleteAll(); //Delete all entity objects
}
