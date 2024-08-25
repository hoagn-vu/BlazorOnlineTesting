using System;
using MongoDB.Bson;
using MongoDB.Driver;
using testQuiz.Models;

namespace testQuiz.Services
{
    public class SubjectService
    {
        private readonly IMongoCollection<Subject> _subjectCollection;

        public SubjectService(IMongoDatabase database)
        {
            _subjectCollection = database.GetCollection<Subject>("Subjects");
        }

        // Lấy Subject theo ID
        public Subject GetSubject(ObjectId subjectId)
        {
            var subject = _subjectCollection.Find(s => s.Id == subjectId).FirstOrDefault();
            if (subject == null)
            {
                return new Subject { SubjectName = "Không xác định" }; // Trả về giá trị mặc định nếu không tìm thấy
            }
            return subject;
        }
        public async Task<Subject> GetSubjectAsync(ObjectId subjectId)
        {
            return await _subjectCollection.Find(s => s.Id == subjectId).FirstOrDefaultAsync();
        }

        // Lấy Subject name
        public string GetNameSubject(ObjectId subjectId)
        {
            Subject subj = _subjectCollection.Find(x => x.Id == subjectId).FirstOrDefault();
            return subj.SubjectName;
        }

        // Lấy tất cả Subjects
        public List<Subject> GetSubjects()
        {
            return _subjectCollection.Find(FilterDefinition<Subject>.Empty).ToList();
        }

        // Lưu hoặc cập nhật Subject
        public void SaveOrUpdate(Subject subject)
        {
            var subjectObj = _subjectCollection.Find(x => x.Id == subject.Id).FirstOrDefault();
            if (subjectObj == null)
            {
                _subjectCollection.InsertOne(subject);
            }
            else
            {
                _subjectCollection.ReplaceOne(x => x.Id == subject.Id, subject);
            }
        }

        // Xóa Subject theo ID
        public string Delete(ObjectId subjectId)
        {
            var result = _subjectCollection.DeleteOne(x => x.Id == subjectId);
            if (result.DeletedCount > 0)
            {
                return "Deleted";
            }
            return "Subject not found";
        }
    }

}

