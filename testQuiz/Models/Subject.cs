using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace testQuiz.Models
{
	public class Subject
	{
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        public string SubjectName { get; set; }
    }
}

