using System.Net;
using System.Net.Sockets;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

UdpClient udpServer = new UdpClient(2004);
Console.WriteLine("Сервер запущено на порту 2004...");

Dictionary<string, string> validUsers = new()
{
    { "admin", "admin123" },
    { "user1", "12345" },
    { "user2", "2233" },
    { "client1", "client123" },
    { "client2", "pass22" }
};

HashSet<IPEndPoint> clients = new();
Dictionary<IPEndPoint, int> clientRequests = new();
const int maxRequests = 3;
const int maxClients = 4;
TimeSpan resetTime = TimeSpan.FromMinutes(1);

Dictionary<string, decimal> exchangeRates = new()
{
    { "USD EURO", 0.88m }, { "UAH BRL", 0.14m },
    { "USD UAH", 41.57m }, { "UAH USD", 0.024m },
    { "USD BRL", 5.68m }, { "UAH EURO", 0.021m },
    { "EURO UAH", 47.07m }, { "BRL USD", 0.18m },
    { "EURO BRL", 6.41m }, { "BRL EURO", 0.16m },
    { "EURO USD", 1.13m }, { "BRL UAH", 7.33m }
};

IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
Dictionary<IPEndPoint, DateTime> blockedClients = new();

while (true) {
    byte[] receivedBytes = udpServer.Receive(ref remoteEndPoint);
    string request = Encoding.UTF8.GetString(receivedBytes).Trim();
    if (!clients.Contains(remoteEndPoint)){
        string[] parts = request.Split(' ');
        if (parts.Length < 2){
            udpServer.Send(Encoding.UTF8.GetBytes("Введіть: [ім'я] [пароль]"), remoteEndPoint);
            continue;
        }
        string username = parts[0];
        string password = parts[1];
        if (!validUsers.ContainsKey(username) || validUsers[username] != password) {
            udpServer.Send(Encoding.UTF8.GetBytes("Невірне ім'я користувача або пароль"), remoteEndPoint);
            Console.WriteLine($"Невірна авторизація {remoteEndPoint}: {request}");
            continue;
        }
        if (clients.Count >= maxClients){
            string overloadMessage = $"Сервер перевантажений. Спробуйте пізніше.";
            udpServer.Send(Encoding.UTF8.GetBytes(overloadMessage), remoteEndPoint);
            Console.WriteLine($"Сервер перевантажений. Клієнт {remoteEndPoint} не може підключитися");
            continue;
        }
        clients.Add(remoteEndPoint);
        Console.WriteLine($"Клієнт підключився: {remoteEndPoint} | Час: {DateTime.UtcNow}");
        udpServer.Send(Encoding.UTF8.GetBytes("Вітаємо! Ви успішно авторизовані"), remoteEndPoint);
        continue;
    }
    
    
    if (blockedClients.ContainsKey(remoteEndPoint) && DateTime.UtcNow < blockedClients[remoteEndPoint] + resetTime){
        string blockedMessage = $"Ви перевищили ліміт запитів. Ви заблоковані до {blockedClients[remoteEndPoint] + resetTime} | Час: {DateTime.Now}";
        udpServer.Send(Encoding.UTF8.GetBytes(blockedMessage), remoteEndPoint);
        continue;
    }
    else if (blockedClients.ContainsKey(remoteEndPoint)&& DateTime.UtcNow >= blockedClients[remoteEndPoint]+resetTime){
        blockedClients.Remove(remoteEndPoint);
        clientRequests[remoteEndPoint] = 0;
        Console.WriteLine($"Клієнт {remoteEndPoint} розблокований | Час: {DateTime.UtcNow}");
    }

    Console.WriteLine($"Запит від {remoteEndPoint}: {request}");

    string response;
    if (!clientRequests.ContainsKey(remoteEndPoint)){
        clientRequests[remoteEndPoint] = 1;
    }
    else{
        clientRequests[remoteEndPoint]++;
    }
    if (clientRequests[remoteEndPoint] > maxRequests){
        response = $"Ви перевищили ліміт запитів ({maxRequests}). Ви заблоковані на {resetTime.TotalMinutes} хвилину";
        blockedClients[remoteEndPoint] = DateTime.UtcNow;
        Console.WriteLine($"Клієнт {remoteEndPoint} перевищив ліміт запитів | Час блокування {DateTime.UtcNow}");
        udpServer.Send(Encoding.UTF8.GetBytes(response), remoteEndPoint);
        continue;
    }

    if (exchangeRates.ContainsKey(request)){
        response = $"Курс {request}: {exchangeRates[request]}";
    }
    else if (request.ToLower() == "exit"){
        response = "Сервер завершує роботу";
        Console.WriteLine($"{remoteEndPoint} відключився | Час: {DateTime.Now}");
        clients.Remove(remoteEndPoint);
        clientRequests.Remove(remoteEndPoint);
        blockedClients.Remove(remoteEndPoint);
        continue;
    }
    else{
        response = "Некоректний запит";
    }
    Console.WriteLine($"Відправлено {remoteEndPoint}: {response}");
    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
    udpServer.Send(responseBytes, remoteEndPoint);
}
udpServer.Close();