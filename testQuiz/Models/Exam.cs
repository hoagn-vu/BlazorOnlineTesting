using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace testQuiz.Models
{
	public class Exam
	{
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("exam_name")]
        public string ExamName { get; set; }

        [BsonElement("subject_id")]
        public ObjectId SubjectId { get; set; }

        // Thuộc tính chuỗi để bind trong giao diện
        [BsonIgnore]
        public string SubjectIdString
        {
            get => SubjectId.ToString();
            set => SubjectId = ObjectId.Parse(value);
        }

        [BsonElement("number_question")]
        public int NumberOfQuestions { get; set; }

        [BsonElement("max_score")]
        public int MaxScore { get; set; }

        [BsonElement("duration")]
        public int Duration { get; set; }

        [BsonElement("session")]
        public List<Session> Sessions { get; set; }
    }
}

