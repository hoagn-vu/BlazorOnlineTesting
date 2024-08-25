using System;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using testQuiz.Models;
using testQuiz.Pages;
//using BCrypt.Net;

namespace testQuiz.Services
{
    public class UserService
	{
        private readonly IMongoCollection<User> _userCollection;

        public UserService(IMongoDatabase database)
        {
            _userCollection = database.GetCollection<User>("Users");
        }

        // Tìm kiếm user theo Id
        public User GetUserById(ObjectId id)
        {
            return _userCollection.Find(user => user.Id == id).FirstOrDefault();
        }
        public async Task<User> GetUserByIdAsync(ObjectId userId)
        {
            return await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        }

        // Tìm kiếm user theo username
        public User GetUserByUsername(string username)
        {
            return _userCollection.Find(user => user.Username == username).FirstOrDefault();
        }

        // Tìm kiếm user theo name
        public List<User> GetUserByName(string searchTerm)
        {
            return _userCollection.Find(u => Regex.IsMatch(u.FullName, $".*{searchTerm}.*", RegexOptions.IgnoreCase)).ToList();
        }

        // Lấy danh sách tất cả users
        public List<User> GetAllUsers()
        {
            return _userCollection.Find(FilterDefinition<User>.Empty).ToList();
        }

        // Lấy danh sách tất cả users theo role
        public List<User> GetUsersByRole(string role)
        {
            return _userCollection.Find(x => x.Role == role).ToList();
        }

        public string GetNameById(ObjectId id)
        {
            if (id == null)
            {
                return "Not Found User";
            }
            var user = _userCollection.Find(u => u.Id == id).FirstOrDefault();
            return user != null ? user.FullName : "Unknown User"; // Nếu không tìm thấy user, trả về "Unknown User"
        }

        // Thêm/sửa user 
        //public void SaveOrUpdate(User user)
        //{
        //    var userObj = _userCollection.Find(x => x.Id == user.Id).FirstOrDefault();

        //    user.PasswordHash = HashPw(user.PasswordText);

        //    if (userObj == null)
        //    {
        //        user.Status = "active";
        //        _userCollection.InsertOne(user);
        //    }
        //    else
        //    {
        //        user.Status = "active";
        //        _userCollection.ReplaceOne(x => x.Id == user.Id, user);
        //    }
        //}
        public void SaveOrUpdate(User user)
        {
            if (user.Id == ObjectId.Empty)
            {
                // Thêm người dùng mới
                user.Status = "active";
                _userCollection.InsertOne(user);
            }
            else
            {
                // Cập nhật người dùng
                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
                var update = Builders<User>.Update
                    .Set(u => u.FullName, user.FullName)
                    .Set(u => u.Username, user.Username)
                    .Set(u => u.Role, user.Role);
                    //.Set(u => u.Status, "active");

                if (user.PasswordHash.Any())
                {
                    update = update.Set(u => u.PasswordHash, user.PasswordHash); // Cập nhật mật khẩu nếu có
                }

                _userCollection.UpdateOne(filter, update);
            }
        }

        public byte[] HashPw(string TextPw)
        {
            // Mã hóa mật khẩu bằng BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(TextPw);
            // Chuyển chuỗi đã mã hóa thành mảng byte
            return System.Text.Encoding.UTF8.GetBytes(hashedPassword);
        }

        //// Ví dụ dùng Verify
        //public bool VerifyPassword(User user, string passwordInput)
        //{
        //    // Chuyển đổi byte[] thành chuỗi gốc đã mã hóa
        //    string storedHashedPassword = System.Text.Encoding.UTF8.GetString(user.PasswordHash);

        //    // Dùng BCrypt để kiểm tra mật khẩu đã nhập
        //    return BCrypt.Net.BCrypt.Verify(passwordInput, storedHashedPassword);
        //}

        public bool VerifyPassword(User user, string passwordInput)
        {
            // Chuyển đổi byte[] thành chuỗi gốc đã mã hóa
            string storedHashedPassword = System.Text.Encoding.UTF8.GetString(user.PasswordHash);

            // Dùng BCrypt để kiểm tra mật khẩu đã nhập
            return BCrypt.Net.BCrypt.Verify(passwordInput, storedHashedPassword);
        }

        // Xóa user
        public string Delete(ObjectId userId)
        {
            var result = _userCollection.DeleteOne(x => x.Id == userId);
            if (result.DeletedCount > 0)
            {
                return "Deleted";
            }
            return "Subject not found";
        }

        public async Task Deactive(ObjectId userId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Set(u => u.Status, "deactive");
            await _userCollection.UpdateOneAsync(filter, update);  // Sử dụng phương thức async
        }

        public async Task Active(ObjectId userId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Set(u => u.Status, "active");
            await _userCollection.UpdateOneAsync(filter, update);  // Sử dụng phương thức async
        }

        public string GetStatus(ObjectId userId)
        {
            var userObj = _userCollection.Find(user => user.Id == userId).FirstOrDefault();
            return userObj.Status;
        }

        public async Task<bool> IsIdentityUsername(string username)
        {
            // Tìm tất cả các user có username tương ứng
            var usn = await _userCollection.Find(x => x.Username == username).ToListAsync();

            // Nếu danh sách trả về không có bản ghi nào, nghĩa là username không tồn tại
            if (usn.Count == 0)
            {
                return true; // Username không tồn tại
            }

            return false; // Username tồn tại
        }




    }
}


