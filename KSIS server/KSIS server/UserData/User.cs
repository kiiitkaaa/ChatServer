namespace KSIS_server.UserData
{
    /// <summary>
    /// Класс представляющий пользователя
    /// </summary>
    class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public string? PersonalName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? RSAPublicKey { get; set; }

        public ICollection<Chat> ChatsAsUser1 { get; set; }
        public ICollection<Chat> ChatsAsUser2 { get; set; }
        public ICollection<Message> SentMessages { get; set; }
    }
}
