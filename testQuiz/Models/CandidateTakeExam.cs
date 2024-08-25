using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace testQuiz.Models
{
	public class CandidateTakeExam
	{
        [BsonElement("candidate_id")]
        public ObjectId CandidateId { get; set; }

        [BsonElement("start_datetime")]
        public DateTime? StartDatetime { get; set; }

        [BsonElement("answers")]
        public List<OptionChosen>? Answers { get; set; }

        [BsonElement("progress")]
        public int? Progress { get; set; }

        [BsonElement("current_status")]
        public string? CurrentStatus { get; set; }

        [BsonElement("finish_datetime")]
        public DateTime? FinishDatetime { get; set; }

        [BsonElement("total_score")]
        public double? TotalScore { get; set; }

        [BsonElement("out-exam_count")]
        public int? OutExamCount { get; set; }

        [BsonElement("recognized_result")]
        public bool? RecognizedResult { get; set; }

        [BsonElement("reason_unrecognized")]
        public string? ReasonUnrecognized { get; set; }
    }
}

