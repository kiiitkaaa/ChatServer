namespace KSIS_server.UserData
{
    /// <summary>
    /// Класс представляющий сообщение
    /// </summary>
    class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string? KeyClient1 { get; set; }
        public string? KeyClient2 { get; set; }

        public Chat Chat { get; set; }
        public User Sender { get; set; }
    }
}
