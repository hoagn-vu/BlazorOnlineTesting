using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace testQuiz.Models
{
	public class Session
	{
        [BsonElement("room_id")]
        public ObjectId RoomId { get; set; }

        // Thuộc tính chuỗi để bind trong giao diện
        [BsonIgnore]
        public string RoomIdString
        {
            get => RoomId.ToString();
            set => RoomId = ObjectId.Parse(value);
        }

        [BsonElement("supervisor_id")]
        public List<ObjectId>? SupervisorId { get; set; }

        [BsonElement("activation_datetime")]
        public DateTime ActivationDatetime { get; set; }

        [BsonElement("candidates")]
        public List<CandidateTakeExam> Candidates { get; set; }

        [BsonElement("exam_status")]
        public string? ExamStatus { get; set; }
    }
}

