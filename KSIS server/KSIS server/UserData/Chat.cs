namespace KSIS_server.UserData
{
    /// <summary>
    /// Класс представляющий чат
    /// </summary>
    class Chat
    {
        public int Id { get; set; }
        public int User1Id { get; set; }
        public int User2Id { get; set; }

        public User User1 { get; set; }
        public User User2 { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
