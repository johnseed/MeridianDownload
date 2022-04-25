// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Text.Json;
using HtmlAgilityPack;

const string baseUrl = "https://data.meridianproject.ac.cn";

// const string cookie = "session=xxx";

Console.WriteLine("Enter cookie value");
string? cookie = "session=" + Console.ReadLine();
Console.WriteLine("Enter file id");
string fileId = Console.ReadLine();
FileInfo fileDictPath = new($"{fileId}-files.json");
var fileDict = fileDictPath.Exists ? JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(fileDictPath.FullName)) ?? new() : await FetchFileList(fileId);
await DownloadFiles(fileDict);

async Task<Dictionary<string, string>> FetchFileList(string fileId)
{
    string html = await GetPageHTML(fileId, 1);
    int pos = html.IndexOf("总页数");
    string totalPages = string.Empty;
    if (pos != -1) 
        totalPages = html.Substring(pos + 4, 2);
    else
    {
        pos = html.IndexOf("Total Pages");
        totalPages = html.Substring(pos + 12, 2);
    }
    int pages = Convert.ToInt32(totalPages);
    Dictionary<string, string> fileDict = new();
    for (int i = 0; i < pages; i++)
    {
        html = await GetPageHTML(fileId, i + 1);
        ExtractFiles(html, fileId, fileDict);
    }
    string json = JsonSerializer.Serialize(fileDict);
    File.WriteAllText(fileDictPath.FullName, json);
    return fileDict;
}

async Task<string> GetPageHTML(string fileId, int page = 1)
{
    using HttpClient client = new();
    string endTime = DateTime.Now.ToString("yyyyMMddHHmmss");

    // string url = $"https://data.meridianproject.ac.cn/science-data/download-list/?file_type=file&ift_id=130&datetime1=20120101000000&datetime2=20211222235959&page_num={page}";
    // string url = $"https://data.meridianproject.ac.cn/science-data/download-list/?file_type=file&ift_id=205&datetime1=20100101000000&datetime2=20211223235959&page_num={page}";
    string url = $"https://data.meridianproject.ac.cn/science-data/download-list/?file_type=file&ift_id={fileId}&datetime1=20100101000000&datetime2={endTime}&page_num={page}";
    var message = new HttpRequestMessage(HttpMethod.Get, url);
    message.Headers.Add("Cookie", cookie);
    var response = client.Send(message);
    var result = await response.Content.ReadAsStringAsync();
    return result;
}

void ExtractFiles(string html, string fileId, Dictionary<string, string> fileDict)
{
    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
    doc.LoadHtml(html);
    var trs = doc.DocumentNode.SelectNodes($"//table[@id='mt{fileId}']/tbody/tr");
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
    Queue<KeyValuePair<string,string>> fileQueue = new(fileDict);
    
    if (!datDir.Exists) datDir.Create();
    // foreach (var kv in fileDict)
    while(fileQueue.Count > 0)
    {
        var kv = fileQueue.Dequeue();
        try
        {
            // string fileName = line.Replace("https://data.meridianproject.ac.cn/science-data/download/?file_type=file&sf_id=", string.Empty);
            FileInfo file = new(Path.Combine(datDir.FullName, kv.Key));
            if (file.Exists)
            {
                string content = File.ReadAllText(file.FullName);
                if (string.IsNullOrWhiteSpace(content) || content.Contains("nginx") || content.Contains("html"))
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{kv.Key} is broken");
                    Console.ResetColor();
                }
                else
                {
                    // Console.WriteLine($"{kv.Key} already exists");
                    continue;
                }
            }
            var message = new HttpRequestMessage(HttpMethod.Get, kv.Value);
            message.Headers.Add("Cookie", cookie);
            var result = client.Send(message);
            byte[] bytes = await result.Content.ReadAsByteArrayAsync();
            File.WriteAllBytes(Path.Combine(datDir.FullName, Path.GetFileName(kv.Key)), bytes);
            Console.WriteLine($"{kv.Key} downloaded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{kv.Key} download failed, add to retry queue, exception : {ex}");
            fileQueue.Enqueue(kv);
        }
    }
}

Console.WriteLine("Hello, World!");
