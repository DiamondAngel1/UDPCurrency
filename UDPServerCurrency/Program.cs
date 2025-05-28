using System.Net;
using System.Net.Sockets;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

UdpClient udpServer = new UdpClient(2004);
Console.WriteLine("Сервер запущено на порту 2004...");

HashSet<IPEndPoint> clients = new();
Dictionary<IPEndPoint, int> clientRequests = new();
const int maxRequests = 3;
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

while (true){
    byte[] receivedBytes = udpServer.Receive(ref remoteEndPoint);
    string request = Encoding.UTF8.GetString(receivedBytes).Trim();

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
    if (!clients.Contains(remoteEndPoint))
    {
        clients.Add(remoteEndPoint);
        Console.WriteLine($"Клієнт підключився: {remoteEndPoint} | Час: {DateTime.UtcNow}");
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
        break;
    }
    else{
        response = "Некоректний запит";
    }
    Console.WriteLine($"Відправлено {remoteEndPoint}: {response}");
    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
    udpServer.Send(responseBytes, remoteEndPoint);
}
udpServer.Close();