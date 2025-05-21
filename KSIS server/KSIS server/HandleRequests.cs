using KSIS_server.UserData;

namespace KSIS_server
{
    static class HandleRequests
    {
        /// <summary>
        /// Обработка запроса на вход
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string HandleLoginRequest(string message, ConnectedClient client)
        {
            var (Success, UserId, Username, Name, Email, Number, RSAPublicKey) = DatabaseHelper.LoginUser(message);

            if (Success && UserId.HasValue)
            {
                client.UserId = UserId.Value;
                return $"{UserId},{Username},{Name},{Email},{Number},{RSAPublicKey}";
            }
            return "ERROR";
        }

        /// <summary>
        /// Обработка запроса на регистрацию
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string HandleRegistrationRequest(string message, ConnectedClient client)
        {
            var result = DatabaseHelper.RegisterUser(message);

            if (result.Success && result.UserId.HasValue)
            {
                client.UserId = result.UserId.Value;

                return $"{result.UserId},{result.Username},{result.Name},{result.Email},{result.Number},{result.RSAPublicKey}";
            }
            return result.Message;
        }

        /// <summary>
        /// Обработка запроса на поиск по имени пользователя
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string HandleFindUserRequest(string message)
        {
            string username = message;

            var (Success, UserId, Username, PersonalName, PhoneNumber) = DatabaseHelper.FindUserByUsername(username);

            if (Success && UserId.HasValue)
            {
                return $"{UserId},{Username},{PersonalName},{PhoneNumber}";
            }
            else
            {
                return "NO";
            }
        }

        /// <summary>
        /// Обработка на создание чата
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string HandleCreateChatRequest(string message, ConnectedClient client)
        {
            if (client.UserId == null)
            {
                return "ERROR";
            }

            if (int.TryParse(message, out int user2Id))
            {
                return DatabaseHelper.CreateChat(client.UserId.Value, user2Id);
            }
            return "ERROR";
        }

        /// <summary>
        /// Обработка запроса на получени всех чатов
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string HandleGetChatsRequest(string message)
        {
            string username = message;

            List<string> chats = DatabaseHelper.GetUserChats(username);

            if (chats.Count == 0)
            {
                return "NO";
            }

            return string.Join("|", chats);
        }

        /// <summary>
        /// Обработка запроса на получение всех сообщений конкретного чата
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string HandleGetMessagesForChat(string message)
        {
            if (int.TryParse(message, out int chatID))
            {
                var messages = DatabaseHelper.GetMessagesForChat(chatID);
                if (messages.Count == 0)
                    return "NO";

                var formattedMessages = messages.Select(m => $"{m.SenderUsername},{m.Content},{m.Timestamp:O},{m.key1},{m.key2}");
                return string.Join("|", formattedMessages);
            }
            return "ERROR";
        }

        /// <summary>
        /// Обработка запроса на отправку сообщения
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string HandleSendMessage(string message)
        {
            bool success = DatabaseHelper.AddMessageToChat(message);
            return success ? "SUCCESS" : "ERROR";
        }
    }
}
