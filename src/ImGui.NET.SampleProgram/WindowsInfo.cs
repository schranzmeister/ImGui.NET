using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ImGuiNET;

public class WindowsInfo
{
    private static string comboValue      = "Ethernet";
    private static int    comboValueIndex = 0;

    static bool showPingInLanTab = false;

    static NetworkInterface   networkInterface;
    static IPGlobalProperties ipProperties;

    static List<string> ips = new();
    
    public static void ShowWindowsInfo()
    {
        if (ImGui.BeginTabItem("WindowsInfo"))
        {
            networkInterface = NetworkInterface.GetAllNetworkInterfaces()[comboValueIndex];
            ipProperties     = IPGlobalProperties.GetIPGlobalProperties();

            if (ImGui.BeginTabBar("WindowsInfoTabBar", ImGuiTabBarFlags.None))
            {
                if (ImGui.BeginTabItem("Info"))
                {
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


                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("NetzwerkInfo"))
                {
                    if (ImGui.BeginCombo("Netzwerkadapter", comboValue))
                    {
                        var index = 0;
                        foreach (var allNetworkInterface in System.Net.NetworkInformation.NetworkInterface
                                                                  .GetAllNetworkInterfaces())
                        {
                            if (allNetworkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                            if (ImGui.Selectable(allNetworkInterface.Name))
                            {
                                comboValueIndex = index;
                            }

                            index++;
                        }

                        ImGui.EndCombo();
                    }


                    if (ImGui.BeginTable("NetworkInfoTable", 3, ImGuiTableFlags.RowBg))
                    {
                        ImGui.TableSetupColumn("Type",
                                               ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                               250);
                        ImGui.TableSetupColumn("...",
                                               ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                               250);
                        ImGui.TableSetupColumn("....",
                                               ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);

                        ImGui.TableNextRow();

                        Row("Interface Type", networkInterface.NetworkInterfaceType.ToString(), false);

                        Row("Speed",    $"{networkInterface.Speed / 1000000} Mbit", false);
                        Row("Hostname", System.Net.Dns.GetHostName());

                        Row("IPv4-Adresse", networkInterface.GetIPProperties().UnicastAddresses[0].Address.ToString());
                        Row("Subnetzmaske", networkInterface.GetIPProperties().UnicastAddresses[0].IPv4Mask.ToString());


                        if (networkInterface.GetIPProperties().GatewayAddresses.Count > 0)
                        {
                            Row("Gateway",    networkInterface.GetIPProperties().GatewayAddresses[0].Address.ToString());
                            Row("DHCP",       networkInterface.GetIPProperties().DhcpServerAddresses[0].ToString());
                            Row("DNS-Server", networkInterface.GetIPProperties().DnsAddresses[0].ToString());
                        }

                        Row("Primary DNS",          ipProperties.DomainName);
                        Row("Knotentyp",            ipProperties.NodeType.ToString(),    false);
                        Row("WINS-Proxy aktiviert", ipProperties.IsWinsProxy.ToString(), false);
                        Row("DHCP aktiviert",
                            networkInterface.GetIPProperties().GetIPv4Properties().IsDhcpEnabled.ToString(), false);
                        Row("Autokonfiguration aktiviert",
                            networkInterface.GetIPProperties().GetIPv4Properties().IsAutomaticPrivateAddressingActive
                                            .ToString(), false);


                        ImGui.EndTable();
                    }

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("PerformanceInfo"))
                {
                    if (ImGui.BeginTable("PerformanceInfoTable", 3, ImGuiTableFlags.RowBg))
                    {
                        ImGui.TableSetupColumn("Type",
                                               ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                               250);
                        ImGui.TableSetupColumn("...",
                                               ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                               250);
                        ImGui.TableSetupColumn("....",
                                               ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);

                        ImGui.TableNextRow();

                        Row("Bytes Received", (networkInterface.GetIPStatistics().BytesReceived).ToString("N0"), false);
                        Row("Bytes Sent",     (networkInterface.GetIPStatistics().BytesSent).ToString("N0"),     false);


                        var pReceived = (networkInterface.GetIPStatistics().IncomingPacketsDiscarded).ToString("N0");
                        var pReceivedErrors =
                            (networkInterface.GetIPStatistics().IncomingPacketsWithErrors).ToString("N0");
                        var pSent       = (networkInterface.GetIPStatistics().OutgoingPacketsDiscarded).ToString("N0");
                        var pSentErrors = (networkInterface.GetIPStatistics().OutgoingPacketsWithErrors).ToString("N0");

                        Row("Packets Received/ Errors", $"{pReceived} / {pReceivedErrors}", false);
                        Row("Packets Sent/ Errors",     $"{pSent} / {pSentErrors}",         false);


                        Row("Non Unicast Packets Received",
                            (networkInterface.GetIPStatistics().NonUnicastPacketsReceived).ToString("N0"), false);
                        Row("Non Unicast Packets Sent",
                            (networkInterface.GetIPStatistics().NonUnicastPacketsSent).ToString("N0"), false);

                        Row("Output Queue Length",
                            (networkInterface.GetIPStatistics().OutputQueueLength).ToString("N0"),
                            false);
                        Row("Incoming Unknown Protocol Packets",
                            (networkInterface.GetIPStatistics().IncomingUnknownProtocolPackets).ToString("N0"), false);

                        Row("Latenz Cloudflare",    GetPing("1.1.1.1"), false);
                        Row("Latenz Google",        GetPing("8.8.8.8"), false);
                        Row("Latenz Google Gaming", GetPing("8.8.4.4"), false);

                        ImGui.EndTable();
                    }

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("TCPInfo"))
                {
                  
                    DrawConnectionsTable();
                    
                    DrawConnectionsTabBar();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("LANInfo"))
                {
                    Task.Run(async () => await LanInfoFetcher.Scan());

                    ImGui.Text(LanInfoFetcher.estimatedIPRange);
                    ImGui.Text(LanInfoFetcher.statusText);

                    if (ImGui.Button("Show Ping"))
                    {
                        showPingInLanTab = !showPingInLanTab;
                    }

                    if (ImGui.BeginTable("LANTable", 6, ImGuiTableFlags.RowBg))
                    {
                        ImGui.TableSetupColumn("IP Adresse",
                                               ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                               250);
                        ImGui.TableSetupColumn("Hostname",
                                               ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                               250);
                        ImGui.TableSetupColumn("IsPrinter",
                                               ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableSetupColumn("MAC Adresse",
                                               ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableSetupColumn("SMPT Settings",
                                               ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableSetupColumn("Open Ports",
                                               ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);

                        ImGui.TableHeadersRow();
                        ImGui.TableNextRow();


                        if (LanInfoFetcher.Devices != null)
                        {
                            foreach (var device in LanInfoFetcher.Devices)
                            {
                                LanIpInfoRow(device);
                            }
                        }


                        ImGui.EndTable();
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.EndTabItem();
        }

        static void DrawConnectionsTable()
        {
              if (ImGui.BeginTable("ConnectionsOverview", 3, ImGuiTableFlags.RowBg))
                    {
                        ImGui.TableSetupColumn("Type",
                                               ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                               250);
                        ImGui.TableSetupColumn("...",
                                               ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                               250);
                        ImGui.TableSetupColumn("....",
                                               ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);

                        ImGui.TableNextRow();

                        Row("Active TCP Connections", (ipProperties.GetActiveTcpConnections().Length).ToString(),
                            false);
                        Row("Connections Established",
                            (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.Established)
                                         .Count())
                            .ToString(), false);
                        Row("Connections Listen",
                            (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.Listen).Count())
                            .ToString(),
                            false);
                        Row("Connections TimeWait",
                            (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.TimeWait).Count())
                            .ToString(), false);
                        Row("Connections CloseWait",
                            (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.CloseWait).Count())
                            .ToString(), false);
                        Row("Connections LastAck",
                            (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.LastAck).Count())
                            .ToString(), false);
                        Row("Connections FinWait1",
                            (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.FinWait1).Count())
                            .ToString(), false);
                        Row("Connections FinWait2",
                            (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.FinWait2).Count())
                            .ToString(), false);
                        Row("Connections Closing",
                            (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.Closing).Count())
                            .ToString(), false);
                        Row("Connections DeleteTcb",
                            (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.DeleteTcb).Count())
                            .ToString(), false);
                        Row("Connections Unknown",
                            (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.Unknown).Count())
                            .ToString(), false);
                        Row("Connections Total",
                            (ipProperties.GetActiveTcpConnections()?.Where(x => x.State == TcpState.Unknown).Count())
                            .ToString(), false);


                        ImGui.EndTable();
                    }
        }

        static void DrawConnectionsTabBar()
        {
              if (ImGui.BeginTabBar("ConnectionsTabBar"))
                                {
                                    
                                    if(ImGui.Button("Refresh"))
                                    {
                                        Task.Run(() => IPInfoFetcher.FetchIPInfo(ips));
                                    }
                                    
                                    if (ImGui.BeginTabItem("Established"))
                                    {
                                        ImGui.Text("The TCP handshake is complete. The connection has been established and data can be sent.");
            
                                        ImGui.Spacing();
                                        ImGui.Separator();
            
                                        if (ImGui.BeginTable("EstablishedTable", 4, ImGuiTableFlags.RowBg))
                                        {
                                            ImGui.TableSetupColumn("Type",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("...",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                                            ImGui.TableSetupColumn(".....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableNextRow();
            
                                            ips.Clear();
            
                                            foreach (var connection in ipProperties.GetActiveTcpConnections())
                                            {
                                                if (connection.State != TcpState.Established) continue;
                                                if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                                ips.Add(connection.RemoteEndPoint.Address.ToString());
                                            }
            
            
                                            //Task.Run(() => IPInfoFetcher.FetchIPInfo(ips));
            
                                            foreach (var lastResult in IPInfoFetcher.GetLastResults())
                                            {
                                                IpInfoRow(lastResult);
                                            }
            
            
                                            ImGui.EndTable();
                                        }
            
            
                                        ImGui.EndTabItem();
                                    }
            
                                    if (ImGui.BeginTabItem("Listen"))
                                    {
                                        ImGui.Text("The local endpoint of the TCP connection is listening for a connection request from any remote endpoint.");
            
                                        ImGui.Spacing();
                                        ImGui.Separator();
            
                                        if (ImGui.BeginTable("ListenTable", 5, ImGuiTableFlags.RowBg))
                                        {
                                            ImGui.TableSetupColumn("Type",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("...",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableSetupColumn(".....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                                            ImGui.TableSetupColumn("......",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableNextRow();
                                           
                                            ips.Clear();
            
                                            foreach (var connection in ipProperties.GetActiveTcpConnections())
                                            {
                                                if (connection.State != TcpState.Listen) continue;
                                                if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                                ips.Add(connection.RemoteEndPoint.Address.ToString());
                                            }
                                            
                                            //Task.Run(() => IPInfoFetcher.FetchIPInfo(ips));
                                            
                                            foreach (var lastResult in IPInfoFetcher.GetLastResults())
                                            {
                                                IpInfoRow(lastResult);
                                            }
                                            
                                            ImGui.EndTable();
                                            
                                             
            
                                        }
                                    
                                        ImGui.EndTabItem();
                                        
                                    }
                                    
                                    if (ImGui.BeginTabItem("CloseWait"))
                                    {
                                        ImGui.Text("The local endpoint of the TCP connection is waiting for a connection termination request from the local user.");
            
                                        ImGui.Spacing();
                                        ImGui.Separator();
            
                                        if (ImGui.BeginTable("CloseWaitTable", 5, ImGuiTableFlags.RowBg))
                                        {
                                            ImGui.TableSetupColumn("Type",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("...",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableSetupColumn(".....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                                            ImGui.TableSetupColumn("......",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableNextRow();
                                            ips.Clear();
            
                                            foreach (var connection in ipProperties.GetActiveTcpConnections())
                                            {
                                                if (connection.State != TcpState.CloseWait) continue;
                                                if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                                ips.Add(connection.RemoteEndPoint.Address.ToString());
                                            }
                                            
                                           // Task.Run(() => IPInfoFetcher.FetchIPInfo(ips));
                                            
                                            foreach (var lastResult in IPInfoFetcher.GetLastResults())
                                            {
                                                IpInfoRow(lastResult);
                                            }
                                            
                                            ImGui.EndTable();
                                            
                                             
            
                                        }
                                    
                                        ImGui.EndTabItem();
                                        
                                    }
                                    
                                    if (ImGui.BeginTabItem("DeleteTcb"))
                                    {
                                        ImGui.Text("The transmission control buffer (TCB) for the TCP connection is being deleted.");
            
                                        ImGui.Spacing();
                                        ImGui.Separator();
            
                                        if (ImGui.BeginTable("DeleteTcbTable", 5, ImGuiTableFlags.RowBg))
                                        {
                                            ImGui.TableSetupColumn("Type",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("...",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableSetupColumn(".....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                                            ImGui.TableSetupColumn("......",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableNextRow();
                                            ips.Clear();
            
                                            foreach (var connection in ipProperties.GetActiveTcpConnections())
                                            {
                                                if (connection.State != TcpState.DeleteTcb) continue;
                                                if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                                ips.Add(connection.RemoteEndPoint.Address.ToString());
                                            }
                                            
                                            //Task.Run(() => IPInfoFetcher.FetchIPInfo(ips));
                                            
                                            foreach (var lastResult in IPInfoFetcher.GetLastResults())
                                            {
                                                IpInfoRow(lastResult);
                                            }
                                            
                                            ImGui.EndTable();
                                            
                                             
            
                                        }
                                    
                                        ImGui.EndTabItem();
                                        
                                    }
                                   
                                    if (ImGui.BeginTabItem("FinWait1"))
                                    {
                                        ImGui.Text("The local endpoint of the TCP connection is waiting for a connection termination request from\nthe remote endpoint or for an acknowledgement of the connection termination request sent previously.");
            
                                        ImGui.Spacing();
                                        ImGui.Separator();
            
                                        if (ImGui.BeginTable("FinWait1Table", 5, ImGuiTableFlags.RowBg))
                                        {
                                            ImGui.TableSetupColumn("Type",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("...",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableSetupColumn(".....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                                            ImGui.TableSetupColumn("......",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableNextRow();
                                            ips.Clear();
            
                                            foreach (var connection in ipProperties.GetActiveTcpConnections())
                                            {
                                                if (connection.State != TcpState.FinWait1) continue;
                                                if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                                ips.Add(connection.RemoteEndPoint.Address.ToString());
                                            }
                                            
                                           // Task.Run(() => IPInfoFetcher.FetchIPInfo(ips));
                                            
                                            foreach (var lastResult in IPInfoFetcher.GetLastResults())
                                            {
                                                IpInfoRow(lastResult);
                                            }
                                            
                                            ImGui.EndTable();
                                            
                                             
            
                                        }
                                    
                                        ImGui.EndTabItem();
                                        
                                    }  
                                 
                                    if (ImGui.BeginTabItem("FinWait2"))
                                    {
                                        ImGui.Text("The local endpoint of the TCP connection is waiting for a connection termination request from the remote endpoint.");
            
                                        ImGui.Spacing();
                                        ImGui.Separator();
            
                                        if (ImGui.BeginTable("FinWait2Table", 5, ImGuiTableFlags.RowBg))
                                        {
                                            ImGui.TableSetupColumn("Type",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("...",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableSetupColumn(".....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                                            ImGui.TableSetupColumn("......",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableNextRow();
                                            ips.Clear();
            
                                            foreach (var connection in ipProperties.GetActiveTcpConnections())
                                            {
                                                if (connection.State != TcpState.FinWait2) continue;
                                                if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                                ips.Add(connection.RemoteEndPoint.Address.ToString());
                                            }
                                            
                                           // Task.Run(() => IPInfoFetcher.FetchIPInfo(ips));
                                            
                                            foreach (var lastResult in IPInfoFetcher.GetLastResults())
                                            {
                                                IpInfoRow(lastResult);
                                            }
                                            
                                            ImGui.EndTable();
                                            
                                             
            
                                        }
                                    
                                        ImGui.EndTabItem();
                                        
                                    }
                                    
                                    if (ImGui.BeginTabItem("CloseWait"))
                                    {
                                        ImGui.Text("The local endpoint of the TCP connection is waiting for a connection termination request from the local user.");
            
                                        ImGui.Spacing();
                                        ImGui.Separator();
            
                                        if (ImGui.BeginTable("CloseWaitTable", 5, ImGuiTableFlags.RowBg))
                                        {
                                            ImGui.TableSetupColumn("Type",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("...",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableSetupColumn(".....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                                            ImGui.TableSetupColumn("......",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableNextRow();
                                            ips.Clear();
            
                                            foreach (var connection in ipProperties.GetActiveTcpConnections())
                                            {
                                                if (connection.State != TcpState.CloseWait) continue;
                                                if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                                ips.Add(connection.RemoteEndPoint.Address.ToString());
                                            }
                                            
                                           // Task.Run(() => IPInfoFetcher.FetchIPInfo(ips));
                                            
                                            foreach (var lastResult in IPInfoFetcher.GetLastResults())
                                            {
                                                IpInfoRow(lastResult);
                                            }
                                            
                                            ImGui.EndTable();
                                            
                                             
            
                                        }
                                    
                                        ImGui.EndTabItem();
                                        
                                    }
                                   
                                    if (ImGui.BeginTabItem("Closing"))
                                    {
                                        ImGui.Text("The local endpoint of the TCP connection is waiting for an acknowledgement of the connection termination request sent previously.");
            
                                        ImGui.Spacing();
                                        ImGui.Separator();
            
                                        if (ImGui.BeginTable("ClosingTable", 5, ImGuiTableFlags.RowBg))
                                        {
                                            ImGui.TableSetupColumn("Type",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("...",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableSetupColumn(".....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                                            ImGui.TableSetupColumn("......",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableNextRow();
                                            ips.Clear();
            
                                            foreach (var connection in ipProperties.GetActiveTcpConnections())
                                            {
                                                if (connection.State != TcpState.Closing) continue;
                                                if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                                ips.Add(connection.RemoteEndPoint.Address.ToString());
                                            }
                                            
                                           // Task.Run(() => IPInfoFetcher.FetchIPInfo(ips));
                                            
                                            foreach (var lastResult in IPInfoFetcher.GetLastResults())
                                            {
                                                IpInfoRow(lastResult);
                                            }
                                            
                                            ImGui.EndTable();
                                            
                                             
            
                                        }
                                    
                                        ImGui.EndTabItem();
                                        
                                    }
                                   
                                    if (ImGui.BeginTabItem("Unknown"))
                                    {
                                        ImGui.Text("The TCP connection state is unknown.");
            
                                        ImGui.Spacing();
                                        ImGui.Separator();
            
                                        if (ImGui.BeginTable("UnknownTable", 5, ImGuiTableFlags.RowBg))
                                        {
                                            ImGui.TableSetupColumn("Type",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("...",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableSetupColumn(".....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                                            ImGui.TableSetupColumn("......",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableNextRow();
                                            ips.Clear();
            
                                            foreach (var connection in ipProperties.GetActiveTcpConnections())
                                            {
                                                if (connection.State != TcpState.Unknown) continue;
                                                if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                                ips.Add(connection.RemoteEndPoint.Address.ToString());
                                            }
                                            
                                           // Task.Run(() => IPInfoFetcher.FetchIPInfo(ips));
                                            
                                            foreach (var lastResult in IPInfoFetcher.GetLastResults())
                                            {
                                                IpInfoRow(lastResult);
                                            }
                                            
                                            ImGui.EndTable();
                                            
                                             
            
                                        }
                                    
                                        ImGui.EndTabItem();
                                        
                                    }
                                   
                                    if (ImGui.BeginTabItem("LastAck"))
                                    {
                                        ImGui.Text("The local endpoint of the TCP connection is waiting for the final acknowledgement of the connection termination request sent previously.");
            
                                        ImGui.Spacing();
                                        ImGui.Separator();
            
                                        if (ImGui.BeginTable("LastAckTable", 5, ImGuiTableFlags.RowBg))
                                        {
                                            ImGui.TableSetupColumn("Type",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("...",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableSetupColumn(".....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                                            ImGui.TableSetupColumn("......",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableNextRow();
                                            ips.Clear();
            
                                            foreach (var connection in ipProperties.GetActiveTcpConnections())
                                            {
                                                if (connection.State != TcpState.LastAck) continue;
                                                if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                                ips.Add(connection.RemoteEndPoint.Address.ToString());
                                            }
                                            
                                           // Task.Run(() => IPInfoFetcher.FetchIPInfo(ips));
                                            
                                            foreach (var lastResult in IPInfoFetcher.GetLastResults())
                                            {
                                                IpInfoRow(lastResult);
                                            }
                                            
                                            ImGui.EndTable();
                                            
                                             
            
                                        }
                                    
                                        ImGui.EndTabItem();
                                        
                                    }
                                   
                                    if (ImGui.BeginTabItem("SynSent"))
                                    {
                                        ImGui.Text("The local endpoint of the TCP connection has sent the remote endpoint a segment header with\nthe synchronize (SYN) control bit set and is waiting for a matching connection request.");
            
                                        ImGui.Spacing();
                                        ImGui.Separator();
            
                                        if (ImGui.BeginTable("SynSentTable", 5, ImGuiTableFlags.RowBg))
                                        {
                                            ImGui.TableSetupColumn("Type",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("...",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableSetupColumn(".....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                                            ImGui.TableSetupColumn("......",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableNextRow();
                                            ips.Clear();
            
                                            foreach (var connection in ipProperties.GetActiveTcpConnections())
                                            {
                                                if (connection.State != TcpState.SynSent) continue;
                                                if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                                ips.Add(connection.RemoteEndPoint.Address.ToString());
                                            }
                                            
                                            //Task.Run(() => IPInfoFetcher.FetchIPInfo(ips));
                                            
                                            foreach (var lastResult in IPInfoFetcher.GetLastResults())
                                            {
                                                IpInfoRow(lastResult);
                                            }
                                            
                                            ImGui.EndTable();
                                            
                                             
            
                                        }
                                    
                                        ImGui.EndTabItem();
                                        
                                    }
                                   
                                    if (ImGui.BeginTabItem("SynReceived"))
                                    {
                                        ImGui.Text("The local endpoint of the TCP connection has sent and received a connection request and is waiting for an acknowledgment.");
            
                                        ImGui.Spacing();
                                        ImGui.Separator();
            
                                        if (ImGui.BeginTable("SynReceivedTable", 5, ImGuiTableFlags.RowBg))
                                        {
                                            ImGui.TableSetupColumn("Type",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("...",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableSetupColumn(".....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                                            ImGui.TableSetupColumn("......",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableNextRow();
                                            ips.Clear();
            
                                            foreach (var connection in ipProperties.GetActiveTcpConnections())
                                            {
                                                if (connection.State != TcpState.SynReceived) continue;
                                                if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                                ips.Add(connection.RemoteEndPoint.Address.ToString());
                                            }
                                            
                                            //Task.Run(() => IPInfoFetcher.FetchIPInfo(ips));
                                            
                                            foreach (var lastResult in IPInfoFetcher.GetLastResults())
                                            {
                                                IpInfoRow(lastResult);
                                            }
                                            
                                            ImGui.EndTable();
                                            
                                             
            
                                        }
                                    
                                        ImGui.EndTabItem();
                                        
                                    }
                                   
                                    if (ImGui.BeginTabItem("TimeWait"))
                                    {
                                        ImGui.Text("The local endpoint of the TCP connection is waiting for enough time to pass to ensure\nthat the remote endpoint received the acknowledgement of its connection termination request.");
            
                                        ImGui.Spacing();
                                        ImGui.Separator();
            
                                        if (ImGui.BeginTable("TimeWaitTable", 5, ImGuiTableFlags.RowBg))
                                        {
                                            ImGui.TableSetupColumn("Type",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("...",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed,
                                                                   250);
                                            ImGui.TableSetupColumn("....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableSetupColumn(".....",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                                            ImGui.TableSetupColumn("......",
                                                                   ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            
                                            ImGui.TableNextRow();
                                             ips.Clear();
            
                                            foreach (var connection in ipProperties.GetActiveTcpConnections())
                                            {
                                                if (connection.State != TcpState.TimeWait) continue;
                                                if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                                ips.Add(connection.RemoteEndPoint.Address.ToString());
                                            }
                                            
                                            //Task.Run(() => IPInfoFetcher.FetchIPInfo(ips));
                                            
                                            foreach (var lastResult in IPInfoFetcher.GetLastResults())
                                            {
                                                IpInfoRow(lastResult);
                                            }
                                            
                                            ImGui.EndTable();
                                            
                                             
            
                                        }
                                    
                                        ImGui.EndTabItem();
                                        
                                    }
            
                                    ImGui.EndTabBar();
                                }
        }
        
        static void Row(string type, string value, bool withButton = true)
        {
            ImGui.TableNextColumn();
            ImGui.Text(type);
            ImGui.TableNextColumn();
            ImGui.Text(value);
            ImGui.TableNextColumn();

            if (!withButton) return;
            if (ImGui.Button("Copy##" + type))
            {
                ImGui.SetClipboardText(value);
            }
        }

        static void IpRow(string localIp, string remoteIp)
        {
            ImGui.TableNextColumn();
            ImGui.Text(localIp);
            ImGui.TableNextColumn();
            ImGui.Text(remoteIp);
            ImGui.TableNextColumn();
            IPHostEntry entry = null;

            try
            {
                entry = Dns.GetHostEntry(remoteIp.Split(":")[0]);
                ImGui.Text(entry.HostName);
            }
            catch (Exception e)
            {
                ImGui.Text("Host unbekannt...");
            }

            ImGui.TableNextColumn();

            // ImGui.Text(IPInfoFetcher.GetLastResult());
            ImGui.TableNextColumn();

            if (ImGui.Button("Copy Remote IP##" + remoteIp))
            {
                ImGui.SetClipboardText(remoteIp);
            }

            ImGui.SameLine();


            if (entry != null)
            {
                if (ImGui.Button("Copy Host IP##" + remoteIp))
                {
                    ImGui.SetClipboardText(entry.HostName);
                }
            }
        }

        static void IpInfoRow(string lastResult)
        {
            foreach (var s in lastResult.Split(";"))
            {
                ImGui.TableNextColumn();
                ImGui.Text(s);
            }
        }

        static void LanIpInfoRow(Device device)
        {
            ImGui.TableNextColumn();
            ImGui.Text(device.IpAddress);
            ImGui.TableNextColumn();
            ImGui.Text(device.HostName);
            ImGui.TableNextColumn();
            ImGui.Text($"{device.IsPrinter}");
            ImGui.TableNextColumn();
            ImGui.Text(device.MacAddress);
            ImGui.TableNextColumn();
            if (ImGui.Button("Show SNMP##" + device.MacAddress))
            {
                SnmpInfo.ipAddress      = device.IpAddress;
                Program._showSnmpWindow = true;
            }

            ImGui.TableNextColumn();

            foreach (var port in device.OpenPorts)
            {
                ImGui.Text($"{port}");
                ImGui.SameLine();
                if (ImGui.Button("Open##" + device.IpAddress + port))
                {
                    var path = GetDefaultBrowserPath();
                    System.Diagnostics.Process.Start(path, $"http://{device.IpAddress}:{port}");
                }
            }

            if (showPingInLanTab)
            {
                ImGui.TableNextColumn();
                ImGui.Text(GetPing(device.IpAddress));
            }
        }

        static string GetDefaultBrowserPath()
        {
            string  registryKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
            string? progId      = Registry.GetValue(registryKey, "ProgId", null) as string;

            if (string.IsNullOrEmpty(progId)) return null;

            registryKey = $"HKEY_CLASSES_ROOT\\{progId}\\shell\\open\\command";
            string? browserPath = Registry.GetValue(registryKey, null, null) as string;

            return browserPath?.Trim('"').Split('"')[0];
        }
    }

    static string GetCountryInfo(string ip)
    {
        //return "Not implemented yet";
        string url = $"http://ip-api.com/json/{ip}";

        using (HttpClient client = new HttpClient())
        {
            Task<HttpResponseMessage> response = client.GetAsync(url);
            if (response.Result.IsSuccessStatusCode)
            {
                string json = response.Result.Content.ReadAsStream().ToString();
                return json;
            }
        }

        return "";
    }

    static string GetPing(string host)
    {
        using Ping ping = new();

        try
        {
            var reply = ping.Send(host);
            if (reply?.Status == IPStatus.Success)
            {
                return $"{reply.RoundtripTime} ms";
            }
            else
            {
                return $"Ping fehlgeschlagen: {reply?.Status}";
            }
        }
        catch (Exception ex)
        {
            return $"Fehler: {ex.Message}";
        }
    }

    static Task<string> GetPingAsync(string host)
    {
        return GetPingAsyncA(host);
    }

    static async Task<string> GetPingAsyncA(string host)
    {
        using Ping ping = new();

        try
        {
            var reply = await ping.SendPingAsync(host);
            if (reply?.Status == IPStatus.Success)
            {
                return $"{reply.RoundtripTime} ms";
            }
            else
            {
                return $"Ping fehlgeschlagen: {reply?.Status}";
            }
        }
        catch (Exception ex)
        {
            return $"Fehler: {ex.Message}";
        }
    }
}
