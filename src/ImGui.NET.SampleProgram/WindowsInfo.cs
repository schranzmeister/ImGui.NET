using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace ImGuiNET;

public class WindowsInfo
{
    private static string comboValue = "Ethernet";
    private static int comboValueIndex = 0;

    public static void ShowWindowsInfo()
    {
        if (ImGui.BeginTabItem("WindowsInfo"))
        {
            var networkInterface =
                System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[comboValueIndex];
            var ipProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();

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


                    if (ImGui.BeginTable("NetworkTable", 3, ImGuiTableFlags.RowBg))
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

                        Row("Speed", $"{networkInterface.Speed / 1000000} Mbit", false);
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
                    if (ImGui.BeginTable("NetworkTable2", 3, ImGuiTableFlags.RowBg))
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
                        Row("Bytes Sent", (networkInterface.GetIPStatistics().BytesSent).ToString("N0"), false);


                        var pReceived = (networkInterface.GetIPStatistics().IncomingPacketsDiscarded).ToString("N0");
                        var pReceivedErrors =
                            (networkInterface.GetIPStatistics().IncomingPacketsWithErrors).ToString("N0");
                        var pSent = (networkInterface.GetIPStatistics().OutgoingPacketsDiscarded).ToString("N0");
                        var pSentErrors = (networkInterface.GetIPStatistics().OutgoingPacketsWithErrors).ToString("N0");

                        Row("Packets Received/ Errors", $"{pReceived} / {pReceivedErrors}", false);
                        Row("Packets Sent/ Errors", $"{pSent} / {pSentErrors}", false);


                        Row("Non Unicast Packets Received",
                            (networkInterface.GetIPStatistics().NonUnicastPacketsReceived).ToString("N0"), false);
                        Row("Non Unicast Packets Sent",
                            (networkInterface.GetIPStatistics().NonUnicastPacketsSent).ToString("N0"), false);

                        Row("Output Queue Length",
                            (networkInterface.GetIPStatistics().OutputQueueLength).ToString("N0"),
                            false);
                        Row("Incoming Unknown Protocol Packets",
                            (networkInterface.GetIPStatistics().IncomingUnknownProtocolPackets).ToString("N0"), false);

                        Row("Latenz Cloudflare", GetPing("1.1.1.1"), false);
                        Row("Latenz Google", GetPing("8.8.8.8"), false);
                        Row("Latenz Google Gaming", GetPing("8.8.4.4"), false);

                        ImGui.EndTable();
                    }

                    ImGui.EndTabItem();
                }


                if (ImGui.BeginTabItem("TCPInfo"))
                {
                    if (ImGui.BeginTable("NetworkTable3", 3, ImGuiTableFlags.RowBg))
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

                    if (ImGui.BeginTabBar("ConnectionsTabBar"))
                    {
                        if (ImGui.BeginTabItem("Established"))
                        {
                                ImGui.Text("The TCP handshake is complete. The connection has been established and data can be sent.");
                            
                                ImGui.Spacing();
                                ImGui.Separator();
                                
                            if (ImGui.BeginTable("EstablishedTable", 5, ImGuiTableFlags.RowBg))
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
                                foreach (var connection in ipProperties.GetActiveTcpConnections())
                                {
                                    if (connection.State != TcpState.Established) continue;
                                    if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                    IpRow(connection.LocalEndPoint.ToString(), connection.RemoteEndPoint.ToString());
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
                                foreach (var connection in ipProperties.GetActiveTcpConnections())
                                {
                                    if (connection.State != TcpState.Listen) continue;
                                    if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                    IpRow(connection.LocalEndPoint.ToString(), connection.RemoteEndPoint.ToString());
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
                                foreach (var connection in ipProperties.GetActiveTcpConnections())
                                {
                                    if (connection.State != TcpState.CloseWait) continue;
                                    if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                    IpRow(connection.LocalEndPoint.ToString(), connection.RemoteEndPoint.ToString());
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
                                foreach (var connection in ipProperties.GetActiveTcpConnections())
                                {
                                    if (connection.State != TcpState.DeleteTcb) continue;
                                    if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                    IpRow(connection.LocalEndPoint.ToString(), connection.RemoteEndPoint.ToString());
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
                                foreach (var connection in ipProperties.GetActiveTcpConnections())
                                {
                                    if (connection.State != TcpState.FinWait1) continue;
                                    if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                    IpRow(connection.LocalEndPoint.ToString(), connection.RemoteEndPoint.ToString());
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
                                foreach (var connection in ipProperties.GetActiveTcpConnections())
                                {
                                    if (connection.State != TcpState.FinWait2) continue;
                                    if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                    IpRow(connection.LocalEndPoint.ToString(), connection.RemoteEndPoint.ToString());
                                }

                                ImGui.EndTable();
                            }

                            ImGui.EndTabItem();
                        }

                        if (ImGui.BeginTabItem("Closed"))
                        {
                            ImGui.Text("The TCP connection is closed.");
                            
                            ImGui.Spacing();
                            ImGui.Separator();

                            if (ImGui.BeginTable("ClosedTable", 5, ImGuiTableFlags.RowBg))
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
                                foreach (var connection in ipProperties.GetActiveTcpConnections())
                                {
                                    if (connection.State != TcpState.Closed) continue;
                                    if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                    IpRow(connection.LocalEndPoint.ToString(), connection.RemoteEndPoint.ToString());
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
                                foreach (var connection in ipProperties.GetActiveTcpConnections())
                                {
                                    if (connection.State != TcpState.Closing) continue;
                                    if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                    IpRow(connection.LocalEndPoint.ToString(), connection.RemoteEndPoint.ToString());
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
                                foreach (var connection in ipProperties.GetActiveTcpConnections())
                                {
                                    if (connection.State != TcpState.Unknown) continue;
                                    if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                    IpRow(connection.LocalEndPoint.ToString(), connection.RemoteEndPoint.ToString());
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
                                foreach (var connection in ipProperties.GetActiveTcpConnections())
                                {
                                    if (connection.State != TcpState.LastAck) continue;
                                    if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                    IpRow(connection.LocalEndPoint.ToString(), connection.RemoteEndPoint.ToString());
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
                                foreach (var connection in ipProperties.GetActiveTcpConnections())
                                {
                                    if (connection.State != TcpState.SynSent) continue;
                                    if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                    IpRow(connection.LocalEndPoint.ToString(), connection.RemoteEndPoint.ToString());
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
                                foreach (var connection in ipProperties.GetActiveTcpConnections())
                                {
                                    if (connection.State != TcpState.SynReceived) continue;
                                    if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                    IpRow(connection.LocalEndPoint.ToString(), connection.RemoteEndPoint.ToString());
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
                                foreach (var connection in ipProperties.GetActiveTcpConnections())
                                {
                                    if (connection.State != TcpState.TimeWait) continue;
                                    if (connection.RemoteEndPoint.Address.ToString() == "127.0.0.1") continue;
                                    IpRow(connection.LocalEndPoint.ToString(), connection.RemoteEndPoint.ToString());
                                }

                                ImGui.EndTable();
                            }

                            ImGui.EndTabItem();
                        }

                        ImGui.EndTabBar();
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.EndTabItem();
        }

        static void Row(string type, string value, bool withButton = true)
        {
           Task.Run(() => IPInfoFetcher.FetchIPInfo("8.8.8.8"));
            
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
            Task.Run(() => IPInfoFetcher.FetchIPInfo(remoteIp.Split(":")[0]));
            
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
            
            ImGui.Text(IPInfoFetcher.GetLastResult());
            ImGui.TableNextColumn();

            if (ImGui.Button("Copy Remote IP##" + remoteIp))
            {
                ImGui.SetClipboardText(remoteIp);
            }
            ImGui.SameLine();


            if (entry != null)
            {
                if (ImGui.Button("Copy Host##" + remoteIp))
                {
                    ImGui.SetClipboardText(entry.HostName);
                }
            }
        }

        static string GetCountryInfo(string ip)
        {
            //return "Not implemented yet";
            string url = $"http://ip-api.com/json/{ip}";

            using (HttpClient client = new HttpClient())
            {
                Task<HttpResponseMessage> response =  client.GetAsync(url);
                if (response.Result.IsSuccessStatusCode)
                {
                    string json =  response.Result.Content.ReadAsStream().ToString();
                    return json;
                }
               
            }
            
            return "";
        }
        
        static string GetPing(string host)
        {
            using Ping ping = new();
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