﻿using Final_Project.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using static Final_Project.Requests.Itemrequests.AddItemRequest;

namespace Final_Project.Models
{
    public class ItemModel
    {
        public ItemModel()
        {
            this.Id = ObjectId.GenerateNewId().ToString();
        }
        [BsonElement("id")]
        public string Id { get; set; }
        [BsonElement("Name")]
        public string Name { get; set; }
        public string Image { get; set; }
        public int Price { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string TypeId { get; set; }
        public List<Sizes> GroupSizes { get; set; }
        public List<string> ToppingIds { get; set; }
        public bool IsStock { get; set; }
        public static Task UniqueItemIndex(ItemService ItemService, ILogger logger)
        {
            logger.LogInformation("Creating index 'Name' as Unique on ItemModel");
            var IndexName = Builders<ItemModel>.IndexKeys.Ascending("Name");
            var IndexOptions = new CreateIndexOptions() { Unique = true };
            return ItemService.itemCollection.Indexes.CreateOneAsync(new CreateIndexModel<ItemModel>(IndexName, IndexOptions));
        }           
    }
}
