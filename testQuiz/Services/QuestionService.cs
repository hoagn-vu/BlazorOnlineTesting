using System;
using System.Security.Cryptography;
using MongoDB.Bson;
using MongoDB.Driver;
using testQuiz.Models;

namespace testQuiz.Services
{
    public class QuestionService
    {
        private readonly IMongoCollection<Question> _questionsCollection;

        public QuestionService(IMongoDatabase database)
        {
            _questionsCollection = database.GetCollection<Question>("Questions");
        }

        public List<Question> GetQuestionsBySubjectId(ObjectId subjId)
        {
            return _questionsCollection.Find(q => q.SubjectId == subjId).SortByDescending(doc => doc.Id).ToList();
        }

        public Question GetQuestionsById(ObjectId qId)
        {
            return _questionsCollection.Find(q => q.Id == qId).FirstOrDefault();
        }

        public async Task<List<Question>> GetRandomQuestions(ObjectId subjId, int numQuest)
        {
            try
            {
                var allQuestions = await _questionsCollection.Find(q => q.SubjectId == subjId).ToListAsync();

                // Xáo trộn câu hỏi ngẫu nhiên
                var randomQuestions = allQuestions.OrderBy(q => Guid.NewGuid()).Take(numQuest).ToList();

                // Xáo trộn tùy chọn cho mỗi câu hỏi nếu cần
                foreach (var question in randomQuestions)
                {
                    if (question.RandomizeOptionsOrder)
                    {
                        question.ShuffledOptions = question.Options.OrderBy(o => Guid.NewGuid()).ToList();
                    }
                    else
                    {
                        question.ShuffledOptions = question.Options;
                    }
                }

                return randomQuestions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Question>> GetRandomQuestionsAsync(int count)
        {
            try
            {
                var allQuestions = await _questionsCollection.AsQueryable().ToListAsync();

                // Xáo trộn câu hỏi ngẫu nhiên
                var randomQuestions = allQuestions.OrderBy(q => Guid.NewGuid()).Take(count).ToList();

                // Xáo trộn tùy chọn cho mỗi câu hỏi nếu cần
                foreach (var question in randomQuestions)
                {
                    if (question.RandomizeOptionsOrder)
                    {
                        question.ShuffledOptions = question.Options.OrderBy(o => Guid.NewGuid()).ToList();
                    }
                    else
                    {
                        question.ShuffledOptions = question.Options;
                    }
                }

                return randomQuestions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        // Lưu hoặc cập nhật Question
        public void SaveOrUpdate(Question question)
        {
            var questionObj = _questionsCollection.Find(x => x.Id == question.Id).FirstOrDefault();
            if (questionObj == null)
            {
                _questionsCollection.InsertOne(question);
            }
            else
            {
                _questionsCollection.ReplaceOne(x => x.Id == question.Id, question);
            }
        }

        public void Delete(ObjectId subjId)
        {
            _questionsCollection.DeleteOne(q => q.Id == subjId);
        }
    }
}


