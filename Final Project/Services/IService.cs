namespace Final_Project.Services
{
    public interface IService<T>
    {
        public Task<List<T>> GetAsync();
        public Task<T> GetAsync(string id);
        public Task CreateAsync(T objectData);
        public Task DeleteAsync(string id);
        public Task UpdateAsync(string id, T objectData);

    }
}
