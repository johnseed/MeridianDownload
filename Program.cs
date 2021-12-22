// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Text.Json;
using HtmlAgilityPack;

// const string cookie = "session=xxx";
const string baseUrl = "https://data.meridianproject.ac.cn";

string cookie = Console.ReadLine();
Dictionary<string, string> fileDict = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("files.json"));
DownloadFiles(fileDict);

async Task FetchFiles()
{
    Dictionary<string, string> fileDict = new();
    for (int i = 0; i < 26; i++)
    {
        string html = await GetPageHTML(i + 1);
        ExtractFiles(html, fileDict);
    }
    string json = JsonSerializer.Serialize(fileDict);
    File.WriteAllText("files.json", json);
}

async Task<string> GetPageHTML(int page = 1)
{
    using HttpClient client = new();
    string url = $"https://data.meridianproject.ac.cn/science-data/download-list/?file_type=file&ift_id=130&datetime1=20120101000000&datetime2=20211222235959&page_num={page}";
    var message = new HttpRequestMessage(HttpMethod.Get, url);
    message.Headers.Add("Cookie", cookie);
    var response = client.Send(message);
    var result = await response.Content.ReadAsStringAsync();
    return result;
}

void ExtractFiles(string html, Dictionary<string, string> fileDict)
{
    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
    doc.LoadHtml(html);
    var trs = doc.DocumentNode.SelectNodes("//table[@id='mt130']/tbody/tr");
    int pos = 0;
    foreach (var tr in trs)
    {
        if (pos++ == 0) continue;
        string name = tr.ChildNodes[5].InnerText.Trim();
        string href = tr.ChildNodes[9].ChildNodes[1].GetAttributeValue("href", "");
        Debug.Assert(!string.IsNullOrWhiteSpace(href));
        href = baseUrl + href;
        fileDict[name] = href;
    }
    Console.WriteLine($"{fileDict.Count} files found");
}

async Task DownloadFiles(Dictionary<string, string> fileDict)
{
    using HttpClient client = new();

    // string json = File.ReadAllText("MST-low-2010-2011.json");
    // var fileDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    DirectoryInfo datDir = new("dat");
    if (!datDir.Exists) datDir.Create();
    foreach (var kv in fileDict)
    {
        // string fileName = line.Replace("https://data.meridianproject.ac.cn/science-data/download/?file_type=file&sf_id=", string.Empty);
        var message = new HttpRequestMessage(HttpMethod.Get, kv.Value);
        message.Headers.Add("Cookie", cookie);
        var result = client.Send(message);
        byte[] bytes = await result.Content.ReadAsByteArrayAsync();
        File.WriteAllBytes(Path.Combine(datDir.FullName, Path.GetFileName(kv.Key)), bytes);
        Console.WriteLine($"{kv.Key} downloaded");
    }
}

Console.WriteLine("Hello, World!");
