using System.Net.Sockets;

namespace KSIS_server
{
    /// <summary>
    /// Подключённый клиент
    /// </summary>
    class ConnectedClient
    {
        /// <summary>
        /// ID, назначаемый сервером
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Сокет клиента
        /// </summary>
        public Socket? Socket { get; set; }

        /// <summary>
        /// Поток для общения
        /// </summary>
        public NetworkStream? Stream { get; set; }

        /// <summary>
        /// ID из базы после входа
        /// </summary>
        public int? UserId { get; set; }
    }
}
