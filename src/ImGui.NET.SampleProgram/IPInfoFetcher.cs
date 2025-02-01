using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public static class IPInfoFetcher
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly HashSet<string> lastResults = new HashSet<string>();
    private static readonly TimeSpan cacheDuration = TimeSpan.FromSeconds(60);
    private static DateTime lastFetchTime = DateTime.MinValue;
    private static readonly SemaphoreSlim rateLimiter = new SemaphoreSlim(1, 1);
    private static readonly ConcurrentQueue<string> ipQueue = new ConcurrentQueue<string>();

    public static async Task FetchIPInfo(List<string> ips)
    {
        if ((DateTime.Now - lastFetchTime) < cacheDuration)
            return;

        lastFetchTime = DateTime.Now;
        lock (lastResults)
        {
            lastResults.Clear();
        }

        foreach (string ip in ips)
        {
            if (!IsPrivateIP(ip))
            {
                ipQueue.Enqueue(ip);
            }
        }

        List<Task> workers = new List<Task>();
        for (int i = 0; i < 45; i++)
        {
            workers.Add(Task.Run(ProcessQueue));
        }

        await Task.WhenAll(workers);
    }

    private static async Task ProcessQueue()
    {
        while (ipQueue.TryDequeue(out string ip))
        {
            await rateLimiter.WaitAsync();
            try
            {
                await FetchSingleIP(ip);
            }
            finally
            {
                rateLimiter.Release();
            }
        }
    }

    private static async Task FetchSingleIP(string ip)
    {
        string url = $"http://ip-api.com/json/{ip}?fields=country,regionName,city,isp,org,query,as,asname,mobile,proxy,hosting";

        try
        {
            string response = await client.GetStringAsync(url);
            var ipInfo = JsonSerializer.Deserialize<IPInfo>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (ipInfo != null)
            {
                lock (lastResults)
                {
                    string result = $"{ipInfo.Query};" +
                                    $"Country: {ipInfo.Country}\nRegion: {ipInfo.RegionName}\nCity: {ipInfo.City};" +
                                    $"ISP: {ipInfo.Isp}\nOrganisation: {ipInfo.Org}\nAS: {ipInfo.As}\nAS Name:{ipInfo.AsName};" +
                                    $"Mobile: {ipInfo.Mobile}\nProxy: {ipInfo.Proxy}\nHosting: {ipInfo.Hosting}";
                    lastResults.Add(result);
                }
            }
        }
        catch (Exception ex)
        {
            lock (lastResults)
            {
                lastResults.Add($"Fehler bei {ip}: {ex.Message}");
            }
        }
    }

    public static List<string> GetLastResults()
    {
        lock (lastResults)
        {
            return new List<string>(lastResults);
        }
    }

    private static bool IsPrivateIP(string ipAddress)
    {
        if (IPAddress.TryParse(ipAddress, out IPAddress ip))
        {
            byte[] bytes = ip.GetAddressBytes();
            return (bytes[0] == 10) ||
                   (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                   (bytes[0] == 192 && bytes[1] == 168);
        }

        return false;
    }
}

public class IPInfo
{
    public string Query   { get; set; }
    public string Country { get; set; }
    public string RegionName  { get; set; }
    public string City    { get; set; }
    public string Isp     { get; set; }
    public string Org     { get; set; }
    public string As      { get; set; }
    public string AsName  { get; set; }
    public bool   Mobile  { get; set; }
    public bool   Proxy   { get; set; }
    public bool   Hosting { get; set; }
}
