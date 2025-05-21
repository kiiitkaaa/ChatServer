using Microsoft.EntityFrameworkCore;

namespace KSIS_server.UserData
{
    /// <summary>
    /// Класс для работы с объектами базы данных
    /// </summary>
    class DatabaseHelper
    {
        private static DbContextOptions<MessengerContext>? options;

        /// <summary>
        /// Подключение базы данных
        /// </summary>
        public static void InitializeDatabase()
        {
            options = new DbContextOptionsBuilder<MessengerContext>()
                .UseSqlite("Data Source=messenger.db")
                .Options;

            using var context = new MessengerContext(options);
            bool flag = context.Database.EnsureCreated();
            if (flag)
            {
                Console.WriteLine("База данных создана.");
            }
            else
            {
                Console.WriteLine("База данных подключена.");
            }
        }

        /// <summary>
        /// Регистрация пользователя
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static (bool Success, string Message, int? UserId, string? Username, string? Name, string? Email, string? Number, string? RSAPublicKey) RegisterUser(string message)
        {
            var data = Requests.ParseRegistrationMessage(message);

            using var db = new MessengerContext(options);
            string username = data["username"];
            string email = data["email"];
            string number = data["phoneNumber"];

            if (db.Users.Any(u => u.Username == username))
                return (false, "Username", null, null, null, null, null, null);

            if (db.Users.Any(u => u.Email == email))
                return (false, "Email", null, null, null, null, null, null);
            if (db.Users.Any(u => u.PhoneNumber == number))
                return (false, "Number", null, null, null, null, null, null);

            var newUser = new User
            {
                Username = username,
                PasswordHash = data["hashPassword"],
                PersonalName = data["personalName"],
                Email = email,
                PhoneNumber = number,
                RSAPublicKey = data["RSAPublicKey"]
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            return (true, "SUCCESS", newUser.Id, newUser.Username, newUser.PersonalName, newUser.Email, newUser.PhoneNumber, newUser.RSAPublicKey);
        }

        /// <summary>
        /// Проверка входа пользователя
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static (bool Success, int? UserId, string? Username, string? Name, string? Email, string? Number, string? RSAPublicKey) LoginUser(string message)
        {
            var data = Requests.ParseConnectMessage(message);

            using var db = new MessengerContext(options);
            string email = data["email"];
            string hashPassword = data["hashPassword"];

            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return (false, null, null, null, null, null, null);

            if (user.PasswordHash != hashPassword)
                return (false, null, null, null, null, null, null);

            db.SaveChanges();
            return (true, user.Id, user.Username, user.PersonalName, user.Email, user.PhoneNumber, user.RSAPublicKey);
        }

        /// <summary>
        /// Поиск пользователя по username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static (bool Success, int? UserId, string? Username, string? PersonalName, string? PhoneNumber) FindUserByUsername(string username)
        {
            using var db = new MessengerContext(options);
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return (false, null, null, null, null);

            return (true, user.Id, user.Username, user.PersonalName, user.PhoneNumber);
        }

        /// <summary>
        /// Создание чата
        /// </summary>
        /// <param name="user1Id"></param>
        /// <param name="user2Id"></param>
        /// <returns></returns>
        public static string CreateChat(int user1Id, int user2Id)
        {
            using var db = new MessengerContext(options);
            // Проверка: не существует ли уже такой чат
            var existingChat = db.Chats.FirstOrDefault(c =>
                (c.User1Id == user1Id && c.User2Id == user2Id) ||
                (c.User1Id == user2Id && c.User2Id == user1Id));

            if (existingChat != null)
                return $"NO,{existingChat.Id}";

            var newChat = new Chat
            {
                User1Id = user1Id,
                User2Id = user2Id
            };

            db.Chats.Add(newChat);
            db.SaveChanges();

            return $"SUCCESS,{newChat.Id}";
        }

        /// <summary>
        /// Получение списка чатов пользователя
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static List<string> GetUserChats(string username)
        {
            using var db = new MessengerContext(options);
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return [];

            var chats = db.Chats
                .Where(c => c.User1Id == user.Id || c.User2Id == user.Id)
                .Include(c => c.User1)
                .Include(c => c.User2)
                .Include(c => c.Messages)
                .ToList();

            var chatSummaries = new List<string>();

            foreach (var chat in chats)
            {
                var otherUser = chat.User1Id == user.Id ? chat.User2 : chat.User1;
                var lastMessage = chat.Messages
                    .OrderByDescending(m => m.Timestamp)
                    .FirstOrDefault();

                if (lastMessage != null)
                {
                    var sender = db.Users.FirstOrDefault(u => u.Id == lastMessage.SenderId);
                    string senderUsername = sender != null ? sender.Username : "Неизвестно";
                    string lastMsgText = lastMessage.Content;
                    string key1 = lastMessage.KeyClient1 ?? "";
                    string key2 = lastMessage.KeyClient2 ?? "";

                    chatSummaries.Add($"{otherUser.PersonalName},{otherUser.Username},{otherUser.RSAPublicKey},{chat.Id},{lastMsgText},{senderUsername},{key1},{key2}");
                }
                else
                {
                    chatSummaries.Add($"{otherUser.PersonalName},{otherUser.Username},{otherUser.RSAPublicKey},{chat.Id},Нет сообщений");
                }
            }

            return chatSummaries;
        }

        /// <summary>
        /// Получение списка сообщений определённого чата
        /// </summary>
        /// <param name="chatID"></param>
        /// <returns></returns>
        public static List<(string SenderUsername, string Content, DateTime Timestamp, string key1, string key2)> GetMessagesForChat(int chatId)
        {
            using var db = new MessengerContext(options);
            var messages = db.Messages
                .Where(m => m.ChatId == chatId)
                .Include(m => m.Sender)
                .OrderBy(m => m.Timestamp)
                .Select(m => new
                {
                    m.Sender.Username,
                    m.Content,
                    m.Timestamp,
                    m.KeyClient1,
                    m.KeyClient2
                })
                .ToList();

            return messages.Select(m => (m.Username ?? "Unknown", m.Content ?? "", m.Timestamp, m.KeyClient1 ?? "", m.KeyClient2 ?? "")).ToList();
        }

        /// <summary>
        /// Отправка собщения
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="senderId"></param>
        /// <param name="content"></param>
        /// <param name="isEncrypted"></param>
        /// <returns></returns>
        public static bool AddMessageToChat(string message)
        {
            try
            {
                var data = Requests.ParseSentMessage(message);

                string senderUsername = data["senderUsername"];
                int chatId = int.Parse(data["chatID"]);
                string content = data["messageText"];
                string key1 = data["key1"];
                string key2 = data["key2"];

                using var db = new MessengerContext(options);
                var user = db.Users.FirstOrDefault(u => u.Username == senderUsername);
                var chat = db.Chats.FirstOrDefault(c => c.Id == chatId);

                if (user == null || chat == null)
                {
                    Console.WriteLine("error user or chat");
                    return false;
                }

                var newMessage = new Message
                {
                    ChatId = chatId,
                    SenderId = user.Id,
                    Content = content,
                    Timestamp = DateTime.UtcNow,
                    KeyClient1 = key1,
                    KeyClient2 = key2
                };

                db.Messages.Add(newMessage);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}