using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace testQuiz.Models
{
	public class Room
	{
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        public String RoomName { get; set; }

        [BsonElement("capacity")]
        public int Capacity { get; set; }

        [BsonElement("location")]
        public string Location { get; set; }

        //[BsonElement("status")]
        //public string rStatus { get; set; }
    }
}

