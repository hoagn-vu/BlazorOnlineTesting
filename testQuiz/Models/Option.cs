using System;
using MongoDB.Bson.Serialization.Attributes;

namespace testQuiz.Models
{
	public class Option
	{
        [BsonElement("option_text")]
        public string OptionText { get; set; } = string.Empty;

        [BsonElement("is_correct_option")]
        public bool IsCorrectOption { get; set; }
    }
}

