namespace KSIS_server
{
    /// <summary>
    /// Обработка строк запроса от клиента
    /// </summary>
    static class Requests
    {
        /// <summary>
        /// Строка на вход
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns>Словарь с почтой и хэшем пароля</returns>
        public static Dictionary<string, string> ParseConnectMessage(string message)
        {
            string[] parts = message.Split(',');

            var data = new Dictionary<string, string>
            {
                { "email", parts[0] },
                { "hashPassword", parts[1] }
            };

            return data;
        }

        /// <summary>
        /// Строка на регистрацию
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns>Словарь с почтой, номером телефона, собственным именем, именем пользователя, хэшем пароля и публичным RSA ключом</returns>
        public static Dictionary<string, string> ParseRegistrationMessage(string message)
        {
            string[] parts = message.Split(',');

            var data = new Dictionary<string, string>
            {
                { "email", parts[0] },
                { "phoneNumber", parts[1] },
                { "personalName", parts[2] },
                { "username", parts[3] },
                { "hashPassword", parts[4] },
                { "RSAPublicKey", parts[5] }
            };

            return data;
        }

        /// <summary>
        /// Строка отправки сообщения
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns>Словарь с именем отправителя, ID чата, сообшением, ключом AES зашифрованным 
        /// ключом RSA отправителя, ключом AES зашифрованным ключом RSA получателя</returns>
        public static Dictionary<string, string> ParseSentMessage(string message)
        {
            string[] parts = message.Split(',');

            var data = new Dictionary<string, string>
            {
                { "senderUsername", parts[0] },
                { "chatID", parts[1] },
                { "messageText", parts[2] },
                { "key1", parts[3] },
                { "key2", parts[4] }
            };

            return data;
        }
    }
}
