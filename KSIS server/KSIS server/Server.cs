using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KSIS_server
{
    /// <summary>
    /// Сервер
    /// </summary>
    class Server
    {
        private const int PORT = 11000;
        private IPAddress? IPAddress;
        private TcpListener? server;
        private bool isRunning;
        private int clientCount = 1;
        private readonly Dictionary<int, ConnectedClient> connectedClients = [];

        /// <summary>
        /// Получение текушего IP для запуска
        /// </summary>
        /// <returns></returns>
        private static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "IP не найден";
        }

        /// <summary>
        /// Запуск сервера
        /// </summary>
        public void StartServer()
        {
            // Определяем локальный IP-адрес
            var localIp = GetLocalIpAddress();
            if (localIp == "IP не найден")
            {
                Console.WriteLine("Не удалось определить локальный IP-адрес.");
                return;
            }

            server = new TcpListener(IPAddress.Parse(localIp), PORT);
            server.Start();
            isRunning = true;
            Console.WriteLine($"Сервер запущен на IP {localIp} и порту {PORT}");
            IPAddress = IPAddress.Parse(localIp);

            while (isRunning)
            {
                int clientID = clientCount;
                var clientSocket = server.AcceptSocket();
                Console.WriteLine("Клиент {0} подключен: {1}", clientID, clientSocket.RemoteEndPoint);

                // Создание нового потока для каждого клиента
                Thread clientThread = new(() => HandleClient(clientSocket, clientID));
                clientThread.Start();
                clientCount++;
            }
        }

        /// <summary>
        /// Обработка клиента в отдельном потоке
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="clientId"></param>
        private void HandleClient(Socket clientSocket, int clientId)
        {
            NetworkStream stream = new(clientSocket);

            var client = new ConnectedClient
            {
                ClientId = clientId,
                Socket = clientSocket,
                Stream = stream,
                UserId = null
            };

            connectedClients[clientCount] = client;

            byte[] buffer = new byte[65534];
            int bytesRead;

            try
            {
                while (true)
                {
                    // Чтение данных от клиента
                    bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Клиент {0} закрыл соединение", client.ClientId);
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Клиент {0}: Получено сообщение: {1}", client.ClientId, message);

                    // Разбор сообщения
                    string[] parts = message.Split(' ', 2);
                    if (parts.Length < 2)
                    {
                        Console.WriteLine("Клиент {0}: Неверный формат сообщения.", client.ClientId);
                        continue;
                    }

                    // Парсим код
                    if (int.TryParse(parts[0], out int code))
                    {
                        if (code == 0)
                        {
                            Console.WriteLine("Отключение");
                        }
                        else
                        {
                            string response = code switch
                            {
                                // Вход
                                1 => HandleRequests.HandleLoginRequest(parts[1], client),
                                // Регистрация
                                2 => HandleRequests.HandleRegistrationRequest(parts[1], client),
                                // Поск пользователей
                                3 => HandleRequests.HandleFindUserRequest(parts[1]),
                                // Создание чата
                                4 => HandleRequests.HandleCreateChatRequest(parts[1], client),
                                // загрузка всех чатов
                                5 => HandleRequests.HandleGetChatsRequest(parts[1]),
                                // получение сообщений
                                6 => HandleRequests.HandleGetMessagesForChat(parts[1]),
                                // отправка сообщения
                                7 => HandleRequests.HandleSendMessage(parts[1]),
                                _ => "ERROR",
                            };
                            Console.WriteLine("Отправлено сообщение: " + response);
                            // Отправка ответа клиенту
                            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                            stream.Write(responseBytes, 0, responseBytes.Length);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Клиент {0}: Неверный код, игнорируем сообщение.", client.ClientId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Клиент {0}: Ошибка при обработке сообщения: {1}", client.ClientId, ex.Message);
            }
            finally
            {
                // Закрытие соединения с клиентом
                clientSocket.Close();
                Console.WriteLine("Клиент {0} отключен", client.ClientId);
                connectedClients.Remove(client.ClientId);
            }
        }

        /// <summary>
        /// Отключение сервера
        /// </summary>
        public void StopServer()
        {
            isRunning = false;
            server?.Server.Close();
        }
    }
}
