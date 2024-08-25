using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace testQuiz.Models
{
	public class Question
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("subject_id")]
        public ObjectId SubjectId { get; set; }

        [BsonElement("chapter")]
        public int Chapter { get; set; }

        [BsonElement("difficulty")]
        public int Difficulty { get; set; }

        [BsonElement("question_text")]
        public string QuestionText { get; set; }

        [BsonElement("options")]
        public List<Option> Options { get; set; }

        [BsonElement("randomize_options_order")]
        public bool RandomizeOptionsOrder { get; set; }

        [BsonIgnore]
        public List<Option> ShuffledOptions { get; set; } = new List<Option>();

        public Option CorrectOption => Options?.FirstOrDefault(o => o.IsCorrectOption);
    }
}

