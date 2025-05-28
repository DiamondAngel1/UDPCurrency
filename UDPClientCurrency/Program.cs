using System.Net;
using System.Net.Sockets;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

UdpClient udpClient = new UdpClient();
IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2004);

Console.WriteLine("Введіть ім'я користувача:");
string username = Console.ReadLine();
Console.WriteLine("Введіть пароль:");
string password = Console.ReadLine();

string authRequest = $"{username} {password}";
await udpClient.SendAsync(Encoding.UTF8.GetBytes(authRequest), serverEndPoint);
UdpReceiveResult authResult = await udpClient.ReceiveAsync();
string authResponse = Encoding.UTF8.GetString(authResult.Buffer);
if(authResponse.Contains("Невірне ім'я користувача або пароль")){
    Console.WriteLine(authResponse);
    udpClient.Close();
    return;
}
else if (authResponse.Contains("Сервер перевантажений. Спробуйте пізніше.")){
    Console.WriteLine(authResponse);
    udpClient.Close();
    return;
}
else if (authResponse.Contains("Ви перевищили ліміт запитів")){
    Console.WriteLine(authResponse);
    udpClient.Close();
    return;
}
Console.WriteLine(authResponse);

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