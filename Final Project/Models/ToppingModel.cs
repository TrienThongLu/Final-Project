using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Final_Project.Models
{
    public class ToppingModel
    {
        public ToppingModel()
        {
            this.Id = ObjectId.GenerateNewId().ToString();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public long Price { get; set; }
    }
}
