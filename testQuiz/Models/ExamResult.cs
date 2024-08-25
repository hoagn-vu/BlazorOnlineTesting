using System;
namespace testQuiz.Models
{
	public class ExamResult
	{
        public string ExamName { get; set; }
        public string SubjectName { get; set; }
        public string CandidateName { get; set; }
        public DateTime? FinishDatetime { get; set; }
        public double? TotalScore { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
    }
}

