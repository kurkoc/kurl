using Cocona;
using Kurl;
using System.Text.Json;
using System.Text;

var builder = CoconaApp.CreateBuilder();

var app = builder.Build();

app.AddCommand(async
                    (
                        CommonParameters commonParameters,
                        [Argument] string url,
                        [Option('d', Description = "data")] string? data,
                        [Option('o', Description = "Get a webpage and store in a local file with a specific name")] string? fileName,
                        [Option('m', Description = "http verb")] string method = "GET"
                    ) =>
{
    HttpMethod httpMethod = ParseHttpMethod(method);

    if (commonParameters.Verbose) Console.WriteLine("Http metodu belirlendi...");


    if (httpMethod == HttpMethod.Post && string.IsNullOrEmpty(data)) throw new CommandExitedException("Post requestlerinde data alanı zorunludur", 128);

    using var httpClient = new HttpClient();
    var requestMessage = CreateHttpRequestMessage(httpMethod, url, data);

    if (commonParameters.Verbose) Console.WriteLine("istek gönderiliyor...");


    var response = await httpClient.SendAsync(requestMessage);

    if (commonParameters.Verbose) Console.WriteLine("istek gönderildi cevap bekleniyor...");

    if (!response.IsSuccessStatusCode) throw new CommandExitedException($"HTTP requesti sırasında hata oluştu", 128);


    string responseAsStr = await response.Content.ReadAsStringAsync();
    if (string.IsNullOrEmpty(fileName))
    {
        Console.WriteLine(responseAsStr);
    }
    else
    {
        await File.WriteAllTextAsync(fileName, responseAsStr);
        Console.WriteLine($"Response, {fileName} adındaki dosyaya çıkartıldı");
    }
})
    .WithDescription("basic httpclient tool");



app.Run();

HttpMethod ParseHttpMethod(string method)
{
    return method.ToUpper() switch
    {
        "GET" => HttpMethod.Get,
        "POST" => HttpMethod.Post,
        "PUT" => HttpMethod.Put,
        "DELETE" => HttpMethod.Delete,
        "HEAD" => HttpMethod.Head,
        "OPTIONS" => HttpMethod.Options,
        "TRACE" => HttpMethod.Trace,
        "PATCH" => HttpMethod.Patch,
        _ => throw new CommandExitedException($"Hatalı HTTP method: {method}", 128)
    };
}

HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string url, string? data)
{
    var request = new HttpRequestMessage
    {
        Method = method,
        RequestUri = new Uri(url)
    };

    request.Headers.Add("User-Agent", "kurl");

    if (method == HttpMethod.Post && data != null)
    {
        request.Content = IsValidJson(data)
            ? new StringContent(data, Encoding.UTF8, "application/json")
            : new StringContent(data);
    }

    return request;
}

bool IsValidJson(string jsonString)
{
    try
    {
        JsonDocument.Parse(jsonString);
        return true;
    }
    catch
    {
        return false;
    }
}
