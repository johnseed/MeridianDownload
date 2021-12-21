// See https://aka.ms/new-console-template for more information
using System.Text.Json;
string json = File.ReadAllText("MST-low-2010-2011.json");
var fileDict = JsonSerializer.Deserialize<Dictionary<string,string>>(json);
using HttpClient client = new();
DirectoryInfo datDir = new("dat");
if(!datDir.Exists) datDir.Create();
foreach (var kv in fileDict)
{
    // string fileName = line.Replace("https://data.meridianproject.ac.cn/science-data/download/?file_type=file&sf_id=", string.Empty);
    var message = new HttpRequestMessage(HttpMethod.Get, kv.Value);
    message.Headers.Add("Cookie", "session=xxx");
    var result = client.Send(message);
    byte[] bytes = await result.Content.ReadAsByteArrayAsync();
    File.WriteAllBytes(Path.Combine(datDir.FullName, Path.GetFileName(kv.Key)), bytes);
    Console.WriteLine($"{kv.Key} downloaded");
}

Console.WriteLine("Hello, World!");
