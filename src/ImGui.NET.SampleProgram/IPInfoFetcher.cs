using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public static class IPInfoFetcher
{
    private static readonly HttpClient client = new HttpClient();
    private static string lastResult = "Lade IP-Info...";
    private static DateTime lastFetchTime = DateTime.MinValue;
    private static readonly TimeSpan cacheDuration = TimeSpan.FromSeconds(30);
    
    public static async Task FetchIPInfo(string ip)
    {
        if ((DateTime.Now - lastFetchTime) < cacheDuration)
            return; // Verhindert zu viele API-Anfragen

        lastFetchTime = DateTime.Now;
        
        string url = $"http://ip-api.com/json/{ip}?fields=country,city,isp,query";
        
        try
        {
            string response = await client.GetStringAsync(url);
            var ipInfo = JsonSerializer.Deserialize<IPInfo>(response);

            lastResult = $"IP: {ipInfo.Query}\nLand: {ipInfo.Country}\nStadt: {ipInfo.City}\nProvider: {ipInfo.Isp}";
        }
        catch (Exception ex)
        {
            lastResult = "Fehler: " + ex.Message;
        }
    }

    public static string GetLastResult()
    {
        return lastResult;
    }
}

// Hilfsklasse für JSON-Deserialisierung
public  class IPInfo
{
    public  string Query { get; set; }
    public  string Country { get; set; }
    public  string City { get; set; }
    public  string Isp { get; set; }
}