using System;
using MongoDB.Bson;
using MongoDB.Driver;
using testQuiz.Models;

namespace testQuiz.Services
{
    public class ExamService
    {
        private readonly IMongoCollection<Exam> _examsCollection;
        private readonly IMongoCollection<Subject> _subjectCollection;
        private readonly IMongoCollection<User> _userCollection;

        public ExamService(IMongoDatabase database)
        {
            _examsCollection = database.GetCollection<Exam>("OrganizeExams");
            _subjectCollection = database.GetCollection<Subject>("Subjects");
            _userCollection = database.GetCollection<User>("Users");
        }

        public List<Exam> GetAllExams()
        {
            return _examsCollection.Find(FilterDefinition<Exam>.Empty).ToList();
        }

        public Exam GetExamById(ObjectId examId)
        {
            return _examsCollection.Find(x => x.Id == examId).FirstOrDefault();
        }

        public string GetExamName(ObjectId examId)
        {
            Exam examObj = _examsCollection.Find(x => x.Id == examId).FirstOrDefault();
            return examObj.ExamName;
        }

        public void SaveOrUpdate(Exam exam)
        {
            var examObj = _examsCollection.Find(x => x.Id == exam.Id).FirstOrDefault();
            if (examObj == null)
            {
                _examsCollection.InsertOne(exam);
            }
            else
            {
                _examsCollection.ReplaceOne(x => x.Id == exam.Id, exam);
            }
        }

        public string Delete(ObjectId examId)
        {
            var result = _examsCollection.DeleteOne(x => x.Id == examId);
            if (result.DeletedCount > 0)
            {
                return "Deleted";
            }
            return "Subject not found";
        }

        public void AddSessionToExam(ObjectId examId, Session newSession)
        {
            var exam = _examsCollection.Find(e => e.Id == examId).FirstOrDefault();
            if (exam != null)
            {
                if (exam.Sessions == null)
                {
                    exam.Sessions = new List<Session>();
                }
                exam.Sessions.Add(newSession);
                _examsCollection.ReplaceOne(e => e.Id == examId, exam); // Lưu lại sự thay đổi
            }
        }

        public async Task DeleteSession(ObjectId examId, ObjectId roomId, DateTime actDatetime)
        {
            // Tạo filter để tìm tài liệu kỳ thi theo examId
            var filter = Builders<Exam>.Filter.Eq("_id", examId);

            // Tạo filter để xóa session dựa trên RoomId và ActivationDatetime
            var update = Builders<Exam>.Update.PullFilter(e => e.Sessions,
                s => s.RoomId == roomId && s.ActivationDatetime == actDatetime);

            // Thực hiện cập nhật và xóa session
            var result = await _examsCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                Console.WriteLine("Session đã được xóa thành công.");
            }
            else
            {
                Console.WriteLine("Không tìm thấy session cần xóa.");
            }
        }

        // moi them 19/8
        public async Task<bool> CandidateCanTakeExam(ObjectId candId, ObjectId examId)
        {
            // Tìm document Exam có Id tương ứng
            var exam = await _examsCollection.Find(x => x.Id == examId).FirstOrDefaultAsync();
            if (exam == null) return false;

            // Lấy tất cả candidate_id từ các session
            foreach (var session in exam.Sessions)
            {
                foreach (var candidate in session.Candidates)
                {
                    if (candidate.CandidateId == candId)
                    {
                        return true; // Nếu tìm thấy candidateId thì trả về true ngay lập tức
                    }
                }
            }

            return false; // Nếu không tìm thấy candidateId trong bất kỳ session nào thì trả về false
        }

        public async Task<bool> CanAddCandidateToSession(ObjectId candId, ObjectId examId)
        {
            // Tìm document Exam có Id tương ứng
            var exam = await _examsCollection.Find(x => x.Id == examId).FirstOrDefaultAsync();
            if (exam == null) return false;

            // Lấy tất cả candidate_id từ các session
            foreach (var session in exam.Sessions)
            {
                foreach (var candidate in session.Candidates)
                {
                    if (candidate.CandidateId == candId)
                    {
                        return false; // Nếu tìm thấy candidateId thì trả về true ngay lập tức
                    }
                }
            }

            return true; // Nếu không tìm thấy candidateId trong bất kỳ session nào thì trả về false
        }

        //public async Task<bool> CanAddSupervisorToSession(ObjectId supvId, ObjectId examId)
        //{
        //    // Tìm document Exam có Id tương ứng
        //    var exam = await _examsCollection.Find(x => x.Id == examId).FirstOrDefaultAsync();
        //    if (exam == null) return false;

        //    // Lấy tất cả candidate_id từ các session
        //    foreach (var session in exam.Sessions)
        //    {
        //        foreach (var supervisor in session.SupervisorId)
        //        {
        //            if (supervisor.CandidateId == supvId)
        //            {
        //                return false; // Nếu tìm thấy candidateId thì trả về true ngay lập tức
        //            }
        //        }
        //    }

        //    return true; // Nếu không tìm thấy candidateId trong bất kỳ session nào thì trả về false
        //}



        // moi them 19/8

        public async Task StartExamAsync(ObjectId examId, ObjectId candidateId)
        {
            // Tìm document kỳ thi
            var exam = await _examsCollection.Find(x => x.Id == examId).FirstOrDefaultAsync();
            if (exam == null)
                throw new Exception("Exam not found!");

            // Tìm session chứa candidate cần cập nhật
            var sessionIndex = exam.Sessions.FindIndex(s => s.Candidates.Any(c => c.CandidateId == candidateId));
            if (sessionIndex == -1)
                throw new Exception("Session not found!");

            var candidateIndex = exam.Sessions[sessionIndex].Candidates.FindIndex(c => c.CandidateId == candidateId);
            if (candidateIndex == -1)
                throw new Exception("Candidate not found!");

            // Sử dụng filter và update với toán tử $ cho session và chỉ số cho candidate
            var filter = Builders<Exam>.Filter.Eq(e => e.Id, examId);
            var update = Builders<Exam>.Update
                .Set($"session.{sessionIndex}.candidates.{candidateIndex}.start_datetime", DateTime.Now)
                .Set($"session.{sessionIndex}.candidates.{candidateIndex}.current_status", "InExam");

            // Cập nhật câu trả lời và tổng điểm
            await _examsCollection.UpdateOneAsync(filter, update);
        }

        public async Task SaveAnswersAsync(ObjectId examId, ObjectId candidateId, List<Question> questions, Dictionary<ObjectId, string> selectedAnswers)
        {
            // Tìm document kỳ thi
            var exam = await _examsCollection.Find(x => x.Id == examId).FirstOrDefaultAsync();
            if (exam == null)
                throw new Exception("Exam not found!");

            // Tìm session chứa candidate cần cập nhật
            var sessionIndex = exam.Sessions.FindIndex(s => s.Candidates.Any(c => c.CandidateId == candidateId));
            if (sessionIndex == -1)
                throw new Exception("Session not found!");

            var candidateIndex = exam.Sessions[sessionIndex].Candidates.FindIndex(c => c.CandidateId == candidateId);
            if (candidateIndex == -1)
                throw new Exception("Candidate not found!");

            // Khởi tạo danh sách câu trả lời
            List<OptionChosen> answers = new List<OptionChosen>();
            int correctAnswersCount = 0;

            // Lặp qua từng câu hỏi để lưu kết quả
            foreach (var question in questions)
            {
                //string selectedOption = selectedAnswers[question.Id];
                //bool isCorrect = question.Options.Any(opt => opt.OptionText == selectedOption && opt.IsCorrectOption);
                string selectedOption = selectedAnswers[question.Id];
                bool isCorrect;

                // Nếu selectedOption là null, thì ta sẽ lưu giá trị là null và isCorrect là false
                if (selectedOption == null)
                {
                    isCorrect = false;
                }
                else
                {
                    isCorrect = question.Options.Any(opt => opt.OptionText == selectedOption && opt.IsCorrectOption);
                }

                // Nếu không có option nào khớp với selectedOption, ta sẽ lưu isCorrect là false
                if (!isCorrect)
                {
                    selectedOption = null;
                    isCorrect = false;
                }

                answers.Add(new OptionChosen
                {
                    QuestionId = question.Id,
                    OptionSelected = selectedOption,
                    IsCorrect = isCorrect
                });

                if (isCorrect)
                    correctAnswersCount++;
            }

            // Tính tổng điểm
            double Score = (exam.MaxScore / exam.NumberOfQuestions) * correctAnswersCount;
            double totalScore = Math.Round(Score, 2);

            // Lấy múi giờ Việt Nam (GMT+7)
            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            // Sử dụng filter và update với toán tử $ cho session và chỉ số cho candidate
            var filter = Builders<Exam>.Filter.Eq(e => e.Id, examId);
            var update = Builders<Exam>.Update
                .Set($"session.{sessionIndex}.candidates.{candidateIndex}.answers", answers)
                .Set($"session.{sessionIndex}.candidates.{candidateIndex}.total_score", totalScore)
                .Set($"session.{sessionIndex}.candidates.{candidateIndex}.finish_datetime", TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone))
                .Set($"session.{sessionIndex}.candidates.{candidateIndex}.current_status", "Done");
                //.Set($"session.{sessionIndex}.candidates.{candidateIndex}.finish_datetime", DateTime.Now)



            // Cập nhật câu trả lời và tổng điểm
            await _examsCollection.UpdateOneAsync(filter, update);
        }


        public List<Session> GetExamSessions(ObjectId examId)
        {
            var exam = _examsCollection.Find(e => e.Id == examId).FirstOrDefault();

            if (exam != null && exam.Sessions != null)
            {
                Console.WriteLine($"Number of sessions: {exam.Sessions.Count}");
                return exam.Sessions;
            }

            return new List<Session>();
        }

        public Session GetExamSessionByRoomAndTime(ObjectId examId, ObjectId roomId, DateTime actDateTime)
        {
            var exam = _examsCollection.Find(e => e.Id == examId).FirstOrDefault();

            return exam.Sessions.Where(s => s.RoomId == roomId && s.ActivationDatetime == actDateTime).FirstOrDefault();
        }

        public List<ObjectId> GetSupervisorIdPerSessionRoomTime(ObjectId examId, ObjectId roomId, DateTime actDateTime)
        {
            var exam = _examsCollection.Find(e => e.Id == examId).FirstOrDefault();

            return exam.Sessions
                    .Where(s => s.RoomId == roomId && s.ActivationDatetime == actDateTime)
                    .SelectMany(s => s.SupervisorId)
                    .ToList();
        }

        public async Task<List<ObjectId>> GetAllExamIdsFromCandidateId(ObjectId candidateId)
        {
            // Tạo filter để tìm các kỳ thi có session chứa candidateId và activationDatetime
            var filter = Builders<Exam>.Filter.ElemMatch(
                e => e.Sessions,
                s => s.Candidates.Any(c => c.CandidateId == candidateId)
            );

            // Tìm tất cả các kỳ thi với filter đã tạo
            var exams = await _examsCollection.Find(filter).ToListAsync();

            // Trả về danh sách examId nếu có kỳ thi phù hợp, nếu không trả về danh sách rỗng
            return exams.Select(e => e.Id).ToList();
        }

        public async Task<List<ObjectId>> GetExamNotDoneFromCandidateId(ObjectId candidateId)
        {
            // Tạo filter để tìm các kỳ thi có session chứa candidateId và activationDatetime
            var filter = Builders<Exam>.Filter.ElemMatch(
                e => e.Sessions,
                s => s.Candidates.Any(c => c.CandidateId == candidateId && c.CurrentStatus != "Done")
            );

            // Tìm tất cả các kỳ thi với filter đã tạo
            var exams = await _examsCollection.Find(filter).ToListAsync();

            // Trả về danh sách examId nếu có kỳ thi phù hợp, nếu không trả về danh sách rỗng
            return exams.Select(e => e.Id).ToList();
        }

        public async Task<List<ObjectId>> GetExamDoneFromCandidateId(ObjectId candidateId)
        {
            // Tạo filter để tìm các kỳ thi có session chứa candidateId và activationDatetime
            var filter = Builders<Exam>.Filter.ElemMatch(
                e => e.Sessions,
                s => s.Candidates.Any(c => c.CandidateId == candidateId && c.CurrentStatus == "Done")
            );

            // Tìm tất cả các kỳ thi với filter đã tạo
            var exams = await _examsCollection.Find(filter).ToListAsync();

            // Trả về danh sách examId nếu có kỳ thi phù hợp, nếu không trả về danh sách rỗng
            return exams.Select(e => e.Id).ToList();
        }

        public async Task<List<ObjectId>> GetExamIdsFromCandidateIdAndActivationDateTime(ObjectId candidateId, DateTime activationDatetime)
        {
            // Tạo filter để tìm các kỳ thi có session chứa candidateId và activationDatetime
            var filter = Builders<Exam>.Filter.ElemMatch(
                e => e.Sessions,
                s => s.Candidates.Any(c => c.CandidateId == candidateId) && s.ActivationDatetime == activationDatetime
            );

            // Tìm tất cả các kỳ thi với filter đã tạo
            var exams = await _examsCollection.Find(filter).ToListAsync();

            // Trả về danh sách examId nếu có kỳ thi phù hợp, nếu không trả về danh sách rỗng
            return exams.Select(e => e.Id).ToList();
        }

        //public async Task SaveAnswersAsync(ObjectId examId, ObjectId candidateId, List<Question> questions, Dictionary<ObjectId, string> selectedAnswers)
        //{
        //    // Tìm document kỳ thi
        //    var exam = await _examsCollection.Find(x => x.Id == examId).FirstOrDefaultAsync();
        //    if (exam == null)
        //        throw new Exception("Exam not found!");

        //    // Tìm session chứa candidate cần cập nhật
        //    var sessionIndex = exam.Sessions.FindIndex(s => s.Candidates.Any(c => c.CandidateId == candidateId));
        //    if (sessionIndex == -1)
        //        throw new Exception("Session not found!");

        //    var candidateIndex = exam.Sessions[sessionIndex].Candidates.FindIndex(c => c.CandidateId == candidateId);
        //    if (candidateIndex == -1)
        //        throw new Exception("Candidate not found!");

        //    // Khởi tạo danh sách câu trả lời
        //    List<OptionChosen> answers = new List<OptionChosen>();
        //    int correctAnswersCount = 0;

        //    // Lặp qua từng câu hỏi để lưu kết quả
        //    foreach (var question in questions)
        //    {
        //        string selectedOption = selectedAnswers[question.Id];
        //        bool isCorrect = question.Options.Any(opt => opt.OptionText == selectedOption && opt.IsCorrectOption);

        //        answers.Add(new OptionChosen
        //        {
        //            QuestionId = question.Id,
        //            OptionSelected = selectedOption,
        //            IsCorrect = isCorrect
        //        });

        //        if (isCorrect)
        //            correctAnswersCount++;
        //    }

        //    // Tính tổng điểm
        //    double Score = (exam.MaxScore / exam.NumberOfQuestions) * correctAnswersCount;
        //    double totalScore = Math.Round(Score, 2);

        //    // Sử dụng filter và update với toán tử $ cho session và chỉ số cho candidate
        //    var filter = Builders<Exam>.Filter.Eq(e => e.Id, examId);
        //    var update = Builders<Exam>.Update
        //        .Set($"session.{sessionIndex}.candidates.{candidateIndex}.answers", answers)
        //        .Set($"session.{sessionIndex}.candidates.{candidateIndex}.total_score", totalScore)
        //        .Set($"session.{sessionIndex}.candidates.{candidateIndex}.finish_datetime", DateTime.Now);

        //    // Cập nhật câu trả lời và tổng điểm
        //    await _examsCollection.UpdateOneAsync(filter, update);
        //}

        //Thừa 
        //public async Task<ExamResult> GetExamResultAsync(ObjectId examId, ObjectId candidateId)
        //{
        //    var exam = await _examsCollection.Find(e => e.Id == examId).FirstOrDefaultAsync();
        //    if (exam == null) return null;

        //    var session = exam.Sessions.FirstOrDefault(s => s.Candidates.Any(c => c.CandidateId == candidateId));
        //    if (session == null) return null;

        //    var candidate = session.Candidates.FirstOrDefault(c => c.CandidateId == candidateId);
        //    if (candidate == null) return null;

        //    var subject = await _subjectCollection.Find(s => s.Id == exam.SubjectId).FirstOrDefaultAsync();
        //    var user = await _userCollection.Find(u => u.Id == candidateId).FirstOrDefaultAsync();

        //    //var correctAnswers = candidate.Answers.Count(a => a.IsCorrect);
        //    int correctAnswers = 0;
        //    foreach (var answer in candidate.Answers)
        //    {
        //        if (answer != null && answer.IsCorrect)
        //        {
        //            correctAnswers++;
        //        }
        //    }

        //    var totalQuestions = exam.NumberOfQuestions;

        //    return new ExamResult
        //    {
        //        ExamName = exam.ExamName,
        //        SubjectName = subject.SubjectName,
        //        CandidateName = user.FullName,
        //        FinishDatetime = candidate.FinishDatetime,
        //        TotalScore = candidate.TotalScore,
        //        CorrectAnswers = correctAnswers,
        //        TotalQuestions = totalQuestions
        //    };
        //}

        public async Task UpdateSessionAsync(ObjectId examId, ObjectId roomId, DateTime actDateTime, Session sessionUpdate)
        {
            // Tìm và cập nhật Session trong kỳ thi dựa trên examId, roomId và actDateTime
            var updateResult = await _examsCollection.UpdateOneAsync(
                exam => exam.Id == examId && exam.Sessions.Any(s => s.RoomId == roomId && s.ActivationDatetime == actDateTime),
                Builders<Exam>.Update.Set(exam => exam.Sessions[-1], sessionUpdate)
            );

            if (updateResult.MatchedCount == 0)
            {
                throw new Exception("Exam or Session not found!");
            }
        }

        public async Task<Exam> GetResultByCandidateAsync(ObjectId examId, ObjectId candidateId)
        {
            // Tìm kỳ thi dựa trên examId
            var exam = await _examsCollection.Find(x => x.Id == examId).FirstOrDefaultAsync();
            if (exam == null)
                throw new Exception("Exam not found!");

            // Tìm session chứa thí sinh có candidateId
            var session = exam.Sessions.FirstOrDefault(s => s.Candidates.Any(c => c.CandidateId == candidateId));
            if (session == null)
                throw new Exception("Session with candidate not found!");

            // Tìm thí sinh có candidateId trong session
            var candidate = session.Candidates.FirstOrDefault(c => c.CandidateId == candidateId);
            if (candidate == null)
                throw new Exception("Candidate not found!");

            var answers = candidate.Answers.ToList();

            // Tạo danh sách OptionChosen từ answers
            var optionChosenList = answers.Select(ans => new OptionChosen
            {
                QuestionId = ans.QuestionId,
                OptionSelected = ans.OptionSelected,
                IsCorrect = ans.IsCorrect
            }).ToList();

            // Chỉ trả về thông tin kỳ thi và session với thí sinh tương ứng
            return new Exam
            {
                Id = exam.Id,
                ExamName = exam.ExamName,
                SubjectId = exam.SubjectId,
                NumberOfQuestions = exam.NumberOfQuestions,
                MaxScore = exam.MaxScore,
                Duration = exam.Duration,
                Sessions = new List<Session>
                {
                    new Session
                    {
                        RoomId = session.RoomId,
                        ActivationDatetime = session.ActivationDatetime,
                        Candidates = new List<CandidateTakeExam>
                        {
                            new CandidateTakeExam
                            {
                                CandidateId = candidate.CandidateId,
                                Answers = optionChosenList,
                                TotalScore = candidate.TotalScore,
                                StartDatetime = candidate.StartDatetime,
                                FinishDatetime = candidate.FinishDatetime,
                                OutExamCount = candidate.OutExamCount,
                                RecognizedResult = candidate.RecognizedResult,
                                ReasonUnrecognized = candidate.ReasonUnrecognized
                            }
                        }
                    }
                }
            };
        }

        public Exam GetResultByCandidate(ObjectId examId, ObjectId candidateId)
        {
            // Tìm kỳ thi dựa trên examId
            var exam = _examsCollection.Find(x => x.Id == examId).FirstOrDefault();
            if (exam == null)
                throw new Exception("Exam not found!");

            // Tìm session chứa thí sinh có candidateId
            var session = exam.Sessions.FirstOrDefault(s => s.Candidates.Any(c => c.CandidateId == candidateId));
            if (session == null)
                throw new Exception("Session with candidate not found!");

            // Tìm thí sinh có candidateId trong session
            var candidate = session.Candidates.FirstOrDefault(c => c.CandidateId == candidateId);
            if (candidate == null)
                throw new Exception("Candidate not found!");

            var answers = candidate.Answers.ToList();
            if (answers.Count ==0) { throw new Exception("Answers not found!"); }
            // Tạo danh sách OptionChosen từ answers
            var optionChosenList = answers.Select(ans => new OptionChosen
            {
                QuestionId = ans.QuestionId,
                OptionSelected = ans.OptionSelected,
                IsCorrect = ans.IsCorrect
            }).ToList();

            // Chỉ trả về thông tin kỳ thi và session với thí sinh tương ứng
            return new Exam
            {
                Id = exam.Id,
                ExamName = exam.ExamName,
                SubjectId = exam.SubjectId,
                NumberOfQuestions = exam.NumberOfQuestions,
                MaxScore = exam.MaxScore,
                Duration = exam.Duration,
                Sessions = new List<Session>
            {
            new Session
            {
                RoomId = session.RoomId,
                ActivationDatetime = session.ActivationDatetime,
                Candidates = new List<CandidateTakeExam>
                {
                    new CandidateTakeExam
                    {
                        CandidateId = candidate.CandidateId,
                        Answers = optionChosenList,
                        TotalScore = candidate.TotalScore,
                        StartDatetime = candidate.StartDatetime,
                        FinishDatetime = candidate.FinishDatetime,
                        OutExamCount = candidate.OutExamCount,
                        RecognizedResult = candidate.RecognizedResult,
                        ReasonUnrecognized = candidate.ReasonUnrecognized
                    }
                }
            }
        }
            };
        }

        // GetExamDoneFromCandidateId(candId return listExamId) + GetResultByCandidateAsync (examId+candId return ObjectExam) = GetExamHistory
        public async Task<List<Exam>> GetExamHistory(ObjectId candidateId)
        {
            List<Exam> examHistory = new List<Exam>();
            List<ObjectId> listExamId = await GetExamDoneFromCandidateId(candidateId);

            foreach (var eId in listExamId)
            {
                examHistory.Add(await GetResultByCandidateAsync(eId, candidateId));
            }

            return examHistory;
        }



    }
}
