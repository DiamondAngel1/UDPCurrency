using System.Net;
using System.Net.Sockets;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

UdpClient udpServer = new UdpClient(2004);
Console.WriteLine("Сервер запущено на порту 2004...");

HashSet<IPEndPoint> clients = new();

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

while (true){
    byte[] receivedBytes = udpServer.Receive(ref remoteEndPoint);
    string request = Encoding.UTF8.GetString(receivedBytes).Trim();

    if (!clients.Contains(remoteEndPoint)){
        clients.Add(remoteEndPoint);
        Console.WriteLine($"Клієнт підключився: {remoteEndPoint} | Час: {DateTime.Now}");
    }

    Console.WriteLine($"Запит від {remoteEndPoint}: {request}");

    string response;
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
    udpServer.Send(responseBytes, responseBytes.Length, remoteEndPoint);
}
udpServer.Close();