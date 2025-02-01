using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using SnmpSharpNet;

namespace ImGuiNET
{
    public static class LanInfoFetcher
    {
        public static List<Device> Devices { get; set; }

        private static readonly TimeSpan cacheDuration = TimeSpan.FromSeconds(30);
        public static           DateTime lastFetchTime = DateTime.MinValue;

        public static string estimatedIPRange = "";
        public static string statusText       = "Noch keine Geräte gefunden";

        private static bool isLoading;

        public static async Task Scan()
        {
            // if (!isLoading)
            // {
            //     statusText = $"{DateTime.Now.Second - lastFetchTime.Second}";
            // }


            if ((DateTime.Now - lastFetchTime) < cacheDuration)
                return;
            isLoading = true;

            lastFetchTime = DateTime.Now;
            string localIP    = GetLocalIP();
            string subnetMask = GetSubnetMask(localIP);
            var    ipRange    = GetIpRange(localIP, subnetMask);

            estimatedIPRange = $"Ermittelter IP-Bereich: {ipRange.network}.{ipRange.start}-{ipRange.end}";

            List<Device>                                     reachableDevices = new List<Device>();
            List<Task<(string ipAddress, bool isReachable)>> pingTasks        = new List<Task<(string, bool)>>();

            statusText = "Loading";
            for (int i = ipRange.start + 1; i < ipRange.end; i++) // Ping von 1 bis zum Ende der Range
            {
                string ipAddress = $"{ipRange.network}.{i}";
                // Asynchronen Task für jedes Ping
                pingTasks.Add(PingIp(ipAddress).ContinueWith(task => (ipAddress, task.Result)));
            }

            // Alle Pings parallel ausführen und warten, bis sie abgeschlossen sind

            var results = await Task.WhenAll(pingTasks);

            foreach (var result in results)
            {
                if (result.isReachable)
                {
                    Device device = await GetDeviceInfo(result.ipAddress);
                    statusText += ".";
                    reachableDevices.Add(device);
                }
            }

            Devices    = reachableDevices;
            statusText = "";
            isLoading  = false;
        }

        // Ermittelt die lokale IP-Adresse des Computers (NICHT 127.0.0.1)
        public static string GetLocalIP()
        {
            string localIP = string.Empty;
            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var address in netInterface.GetIPProperties().UnicastAddresses)
                    {
                        // Vermeide Loopback-Adresse 127.0.0.1 und wähle die erste nicht-Loopback IPv4-Adresse
                        if (address.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address.Address))
                        {
                            localIP = address.Address.ToString();
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(localIP)) break;
            }

            return localIP;
        }

        // Ermittelt die Subnetzmaske (angenommen, dass das erste Netzwerkinterface verwendet wird)
        public static string GetSubnetMask(string localIP)
        {
            var ip     = IPAddress.Parse(localIP);
            var ipBits = ip.GetAddressBytes();
            return "255.255.255.0"; // Standardmäßig als Beispiel
        }

        // Berechnet den IP-Bereich basierend auf IP und Subnetzmaske
        public static (string network, int start, int end) GetIpRange(string ip, string subnetMask)
        {
            var ipBytes      = IPAddress.Parse(ip).GetAddressBytes();
            var subnetBytes  = IPAddress.Parse(subnetMask).GetAddressBytes();
            var networkBytes = new byte[ipBytes.Length];

            for (int i = 0; i < ipBytes.Length; i++)
            {
                networkBytes[i] = (byte)(ipBytes[i] & subnetBytes[i]);
            }

            string network = string.Join(".", networkBytes);
            network = network.Substring(0, network.LastIndexOf('.')); // Entferne letzte Stelle
            int start = 1;
            int end   = 254; // Standardmäßig 192.168.x.x Bereich

            return (network, start, end);
        }

        // Prüft, ob die IP-Adresse über Ping erreichbar ist
        public static async Task<bool> PingIp(string ipAddress)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(ipAddress, 1000); // Timeout von 1000ms
                    return reply.Status == IPStatus.Success;
                }
            }
            catch (PingException)
            {
                // Fehler beim Senden des Pings, z. B. Netzwerkausfall
                return false;
            }
            catch (Exception)
            {
                // Allgemeiner Fehler, z. B. Netzwerkfehler
                return false;
            }
        }

        // Holt Geräteinformationen für die gegebene IP (z.B. Hostname oder IP-Adresse)
        public static async Task<Device> GetDeviceInfo(string ipAddress)
        {
            try
            {
                // Versuchen, den Hostnamen zu ermitteln
                string hostName   = await GetHostName(ipAddress);
                string macAddress = GetMacAddress(ipAddress);
                // string    manufacturer = await GetManufacturer(macAddress);
                List<int> openPorts = await ScanOpenPorts(ipAddress);
                bool      isPrinter = openPorts.Contains(9100) || openPorts.Contains(515) || openPorts.Contains(631);
                // string    snmpData  = isPrinter ? await GetSnmpData(ipAddress) : "N/A";

                return new Device
                       {
                           IpAddress    = ipAddress,
                           HostName     = !string.IsNullOrEmpty(hostName) ? hostName : "Kein Hostname gefunden",
                           MacAddress   = macAddress,
                           Manufacturer = "",
                           OpenPorts    = openPorts,
                           IsPrinter    = isPrinter
                       };
            }
            catch
            {
                return new Device
                       {
                           IpAddress = ipAddress,
                           HostName  = "Fehler beim Abrufen der Geräteinformationen"
                       };
            }
        }

        // Versucht, den Hostnamen einer IP zu ermitteln
        public static async Task<string> GetHostName(string ipAddress)
        {
            try
            {
                IPHostEntry hostEntry = await Dns.GetHostEntryAsync(ipAddress);
                return hostEntry.HostName;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetMacAddress(string ipAddress)
        {
            string output = ExecuteCommand("arp -a");
            Match  match  = Regex.Match(output, $"{ipAddress}\\s+([a-fA-F0-9:-]+)");
            return match.Success ? match.Groups[1].Value : "Unbekannt";
        }

        public static async Task<string> GetManufacturer(string macAddress)
        {
            return "Not implemented Yet";

            if (string.IsNullOrEmpty(macAddress) || macAddress == "Unbekannt")
                return "Unbekannt";

            string oui    = macAddress.Substring(0, 8).Replace("-", ":");
            string apiUrl = $"https://macvendors.co/api/{oui}/json";

            try
            {
                using HttpClient client   = new HttpClient();
                string           response = await client.GetStringAsync(apiUrl);

                using JsonDocument doc = JsonDocument.Parse(response);
                if (doc.RootElement.TryGetProperty("result", out JsonElement result) &&
                    result.TryGetProperty("company", out JsonElement company))
                {
                    return company.GetString() ?? "Nicht gefunden";
                }
            }
            catch
            {
                return "Nicht gefunden";
            }

            return "Nicht gefunden";
        }

        public static async Task<List<int>> ScanOpenPorts(string ipAddress)
        {
            List<int> openPorts    = new List<int>();
            int[]     portsToCheck = { 80, 443, 22, 21, 25, 3389, 9100, 515, 631 };

            var tasks = portsToCheck.Select(async port =>
                                            {
                                                using var tcpClient = new TcpClient();
                                                try
                                                {
                                                    var connectTask = tcpClient.ConnectAsync(ipAddress, port);
                                                    if (await Task.WhenAny(connectTask, Task.Delay(200)) == connectTask && tcpClient.Connected)
                                                    {
                                                        lock (openPorts)
                                                        {
                                                            openPorts.Add(port);
                                                        }
                                                    }
                                                }
                                                catch
                                                {
                                                }
                                            }).ToArray();

            await Task.WhenAll(tasks);
            return openPorts;
        }



        private static string ExecuteCommand(string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + command)
                                   { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
            Process process = new Process { StartInfo = psi };
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }
    }

    // Device-Klasse für die Speicherung von IP-Adresse und Hostname
    public class Device
    {
        public string    IpAddress    { get; set; }
        public string    HostName     { get; set; }
        public string    MacAddress   { get; set; }
        public string    Manufacturer { get; set; }
        public List<int> OpenPorts    { get; set; }
        public bool      IsPrinter    { get; set; }

        public string    SnmpData     { get; set; }

        public override string ToString()
        {
            return $"{IpAddress} ({HostName})";
        }
    }
}
