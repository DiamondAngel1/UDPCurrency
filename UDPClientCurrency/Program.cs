using System.Net;
using System.Net.Sockets;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

UdpClient udpClient = new UdpClient();
IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2004);

while (true){
    Console.WriteLine("Валюти (USD, EURO, UAH, BRL)");
    Console.WriteLine("Введіть запит (наприклад, 'USD EURO'), або 'exit' для виходу:");
    string request = Console.ReadLine();
    if (string.IsNullOrEmpty(request) || request.ToLower() == "exit"){
        await udpClient.SendAsync(Encoding.UTF8.GetBytes("exit"), serverEndPoint);
        Console.WriteLine("Ви вийшли з чату");
        break;
    }
    byte[] requestBytes = Encoding.UTF8.GetBytes(request);
    await udpClient.SendAsync(requestBytes, requestBytes.Length, serverEndPoint);
    UdpReceiveResult result = await udpClient.ReceiveAsync();
    string response = Encoding.UTF8.GetString(result.Buffer);
    Console.WriteLine(response);
}
udpClient.Close();