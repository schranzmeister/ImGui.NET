using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace ImGuiNET;

public class WindowsInfo
{
    private static string comboValue = "Ethernet";
    private static int comboValueIndex = 0;

    public static void ShowWindowsInfo()
    {
        if (ImGui.BeginTabItem("WindowsInfo"))
        {
            ImGui.TextColored(new System.Numerics.Vector4(0.25f, 1, 0.25f, 1), "Windows Info");


            ImGui.Text("Gerätename: " + System.Environment.MachineName);
            ImGui.Text("Betriebssystem: " + System.Environment.OSVersion);
            ImGui.Text("Benutzername: " + System.Environment.UserName);
            ImGui.Text("Benutzerdomäne: " + System.Environment.UserDomainName);
            ImGui.Text("Benutzerzeitzone: " + System.TimeZoneInfo.Local);

            ImGui.Text("Prozessoranzahl: " + System.Environment.ProcessorCount);
            ImGui.Text("Arbeitsspeicher: " + System.Environment.WorkingSet / 1024 / 1024);
            ImGui.Text("Systemverzeichnis: " + System.Environment.SystemDirectory);
            ImGui.Text("Systemlaufzeit: " + System.Environment.TickCount / 1000 / 60);
            ImGui.Text("Systemversion: " + System.Environment.Version);


            ImGui.TextColored(new System.Numerics.Vector4(0.25f, 1, 0.25f, 1), "Netzwerk Info");

            
            if (ImGui.BeginCombo("Netzwerkadapter", comboValue))
            {
                var index = 0;
                foreach (var allNetworkInterface in System.Net.NetworkInformation.NetworkInterface
                             .GetAllNetworkInterfaces())
                {
                    if(allNetworkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                    if (ImGui.Selectable(allNetworkInterface.Name))
                    {
                        comboValueIndex = index;
                    }

                    index++;
                }

                ImGui.EndCombo();
            }


            var networkInterface =
                System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[comboValueIndex];
            var ipProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();

            if (ImGui.BeginTable("NetworkTable", 3, ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                    250);
                ImGui.TableSetupColumn("...", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                    250);
                ImGui.TableSetupColumn("....", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);

                ImGui.TableNextRow();

                Row("Interface Type", networkInterface.NetworkInterfaceType.ToString(), false);
                
                Row("Speed", $"{ networkInterface.Speed / 1000000} Mbit", false);
                Row("Hostname", System.Net.Dns.GetHostName());

                Row("IPv4-Adresse", networkInterface.GetIPProperties().UnicastAddresses[0].Address.ToString());
                Row("Subnetzmaske", networkInterface.GetIPProperties().UnicastAddresses[0].IPv4Mask.ToString());


                if (networkInterface.GetIPProperties().GatewayAddresses.Count > 0)
                {
                    Row("Gateway", networkInterface.GetIPProperties().GatewayAddresses[0].Address.ToString());
                    Row("DHCP", networkInterface.GetIPProperties().DhcpServerAddresses[0].ToString());
                    Row("DNS-Server", networkInterface.GetIPProperties().DnsAddresses[0].ToString());
                }

                Row("Primary DNS", ipProperties.DomainName);
                Row("Knotentyp", ipProperties.NodeType.ToString(), false);
                Row("WINS-Proxy aktiviert", ipProperties.IsWinsProxy.ToString(), false);
                Row("DHCP aktiviert", networkInterface.GetIPProperties().GetIPv4Properties().IsDhcpEnabled.ToString(), false);
                Row("Autokonfiguration aktiviert", networkInterface.GetIPProperties().GetIPv4Properties().IsAutomaticPrivateAddressingActive.ToString(), false);


                ImGui.EndTable();
            }

            ImGui.TextColored(new System.Numerics.Vector4(0.25f, 1, 0.25f, 1), "Performance Info");

            if (ImGui.BeginTable("NetworkTable2", 3, ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                    250);
                ImGui.TableSetupColumn("...", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                    250);
                ImGui.TableSetupColumn("....", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);

                ImGui.TableNextRow();

                Row("Bytes Received", (networkInterface.GetIPStatistics().BytesReceived).ToString("N0"), false);
                Row("Bytes Sent", (networkInterface.GetIPStatistics().BytesSent).ToString("N0"), false);

                
                var pReceived =  (networkInterface.GetIPStatistics().IncomingPacketsDiscarded).ToString("N0");
                var pReceivedErrors = (networkInterface.GetIPStatistics().IncomingPacketsWithErrors).ToString("N0");
                var pSent = (networkInterface.GetIPStatistics().OutgoingPacketsDiscarded).ToString("N0"); 
                var pSentErrors = (networkInterface.GetIPStatistics().OutgoingPacketsWithErrors).ToString("N0");
                
                Row("Packets Received/ Errors",$"{pReceived} / {pReceivedErrors}", false);
                Row("Packets Sent/ Errors",  $"{pSent} / {pSentErrors}", false);

               

                Row("Non Unicast Packets Received",
                    (networkInterface.GetIPStatistics().NonUnicastPacketsReceived).ToString("N0"), false);
                Row("Non Unicast Packets Sent",
                    (networkInterface.GetIPStatistics().NonUnicastPacketsSent).ToString("N0"), false);

                Row("Output Queue Length", (networkInterface.GetIPStatistics().OutputQueueLength).ToString("N0"), false);
                Row("Incoming Unknown Protocol Packets",
                    (networkInterface.GetIPStatistics().IncomingUnknownProtocolPackets).ToString("N0"), false);

                Row("Latenz Cloudflare", GetPing("1.1.1.1"), false);
                Row("Latenz Google", GetPing("8.8.8.8"), false);
                Row("Latenz Google Gaming", GetPing("8.8.4.4"), false);

                ImGui.EndTable();
            }

            ImGui.TextColored(new System.Numerics.Vector4(0.25f, 1, 0.25f, 1), "TCP Info");
            
             if (ImGui.BeginTable("NetworkTable3", 3, ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                    250);
                ImGui.TableSetupColumn("...", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                    250);
                ImGui.TableSetupColumn("....", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);

                ImGui.TableNextRow();

                Row("Active TCP Connections", (ipProperties.GetActiveTcpConnections().Length).ToString(), false);
                Row("Connections Established", (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.Established).Count()).ToString(), false); 
                Row("Connections Listen", (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.Listen).Count()).ToString(), false);
                Row("Connections TimeWait", (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.TimeWait).Count()).ToString(), false);
                Row("Connections CloseWait", (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.CloseWait).Count()).ToString(), false);
                Row("Connections LastAck", (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.LastAck).Count()).ToString(), false);
                Row("Connections FinWait1", (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.FinWait1).Count()).ToString(), false);
                Row("Connections FinWait2", (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.FinWait2).Count()).ToString(), false);
                Row("Connections Closing", (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.Closing).Count()).ToString(), false);
                Row("Connections DeleteTcb", (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.DeleteTcb).Count()).ToString(), false);
                Row("Connections Unknown", (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.Unknown).Count()).ToString(), false);
                Row("Connections Total", (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.Unknown).Count()).ToString(), false);

                foreach (var connection  in ipProperties.GetActiveTcpConnections())
                {
                    if(connection.State != TcpState.Established) continue;
                    if(connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                    Row("Local Endpoint", connection.LocalEndPoint.ToString());
                    Row("Remote Endpoint", connection.RemoteEndPoint.ToString());
                    Row( "State", connection.State.ToString());
                }
                
                
                ImGui.EndTable();
            }
           
            // Row("Active TCP Listeners",ipProperties.GetActiveTcpListeners().Length.ToString(), false);
             
            ImGui.EndTabItem();
        }

        static void Row(string type, string value, bool withButton = true)
        {
            ImGui.TableNextColumn();
            ImGui.Text(type);
            ImGui.TableNextColumn();
            ImGui.Text(value);
            ImGui.TableNextColumn();

            if (!withButton) return;
            if ( ImGui.Button("Copy##" + type))
            {
                ImGui.SetClipboardText(value);
            }
        }

        static string GetPing(string host)
        {
            using Ping ping = new ();
            var time = 0;
            try
            {
                var reply = ping.Send(host);
                for (var i = 0; i < 1; i++)
                {
                    if (reply?.Status == IPStatus.Success)
                    {
                        time += (int)reply.RoundtripTime;
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Fehler: {ex.Message}";
            }

            return $"{time / 1} ms";
        }
    }
}