using System;
using MongoDB.Bson;
using MongoDB.Driver;
using testQuiz.Models;

namespace testQuiz.Services
{
	public class RoomService
	{
        private readonly IMongoCollection<Room> _roomCollection;

        public RoomService(IMongoDatabase database)
        {
            _roomCollection = database.GetCollection<Room>("Rooms");
        }

        public Room GetRoom(ObjectId roomId)
        {
            return _roomCollection.Find(x => x.Id == roomId).FirstOrDefault();
        }

        public String GetNameRoom(ObjectId roomId)
        {
            Room room = _roomCollection.Find(x => x.Id == roomId).FirstOrDefault();
            return room.RoomName;
        }

        // Lấy tất cả Subjects
        public List<Room> GetAllRooms()
        {
            return _roomCollection.Find(FilterDefinition<Room>.Empty).ToList();
        }

        // Lưu hoặc cập nhật Subject
        public void SaveOrUpdate(Room room)
        {
            var roomObj = _roomCollection.Find(x => x.Id == room.Id).FirstOrDefault();
            if (roomObj == null)
            {
                _roomCollection.InsertOne(room);
            }
            else
            {
                _roomCollection.ReplaceOne(x => x.Id == room.Id, room);
            }
        }

        // Xóa Subject theo ID
        public string Delete(ObjectId roomId)
        {
            var result = _roomCollection.DeleteOne(x => x.Id == roomId);
            if (result.DeletedCount > 0)
            {
                return "Deleted";
            }
            return "Subject not found";
        }
    }
}

