using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace testQuiz.Models
{
	public class User
	{
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("password")]
        public byte[] PasswordHash { get; set; }

        [BsonElement("role")]
        public string Role { get; set; }

        [BsonElement("full_name")]
        public string FullName { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }

        [BsonIgnore]
        public string PasswordText { get; set; }

        //public override bool Equals(object obj)
        //{
        //    if (obj is User otherUser)
        //    {
        //        return this.Id == otherUser.Id;
        //    }
        //    return false;
        //}

        //public override int GetHashCode()
        //{
        //    return Id.GetHashCode();
        //}
    }
}

