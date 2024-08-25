using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace testQuiz.Models
{
	public class OptionChosen
	{
        [BsonElement("question_id")]
        public ObjectId QuestionId { get; set; }

        [BsonElement("option_selected")]
        public string? OptionSelected { get; set; }

        [BsonElement("is_correct")]
        public bool IsCorrect { get; set; }

    }
}

