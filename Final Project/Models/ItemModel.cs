﻿using Final_Project.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models
{
    public class ItemModel
    {
        public ItemModel()
        {
            this.Id = ObjectId.GenerateNewId().ToString();
        }
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public long Price { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string TypeId { get; set; }
        public string Status { get; set; }
        public static Task UniqueRoleIndex(ItemService ItemService, ILogger logger)
        {
            logger.LogInformation("Creating index 'Name' as Unique on ItemModel");
            var IndexName = Builders<ItemModel>.IndexKeys.Ascending("name");
            var IndexOptions = new CreateIndexOptions() { Unique = true };
            return ItemService.itemCollection.Indexes.CreateOneAsync(new CreateIndexModel<ItemModel>(IndexName, IndexOptions));
        }
    }
}
