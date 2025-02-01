using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using SnmpSharpNet;

namespace ImGuiNET;

public static class SnmpInfo
{
    public static string ipAddress = "";

    public static void ShowSnmpInfo()
    {
        if (ImGui.Begin("SNMP Info", ref Program._showSnmpWindow))
        {
            SnmpRow("Printer Name",  "1.3.6.1.2.1.43.5.1.1.16.1");
            SnmpRow("Serial Number", "1.3.6.1.2.1.43.5.1.1.17.1");

            var capacity = 0;
            var level    = 0;
            if (ImGui.BeginTable("TonerLevelTable", 2))
            {
                var cyanTonerCapacity = GetSnmpData("1.3.6.1.2.1.43.11.1.1.8.1.1").Result;
                var cyanTonerLevel    = GetSnmpData("1.3.6.1.2.1.43.11.1.1.9.1.1").Result;

                var cyanToner = int.TryParse(cyanTonerCapacity, out capacity) && int.TryParse(cyanTonerLevel, out level)
                                    ? (level / (float)capacity) * 100
                                    : 0;

                SnmpTonerLevelRow("Cyan Toner", cyanToner);

                var magentaTonerCapacity = GetSnmpData("1.3.6.1.2.1.43.11.1.1.8.1.2").Result;
                var magentaTonerLevel    = GetSnmpData("1.3.6.1.2.1.43.11.1.1.9.1.2").Result;

                var magentaToner = int.TryParse(magentaTonerCapacity, out capacity) && int.TryParse(magentaTonerLevel, out level)
                                       ? (level / (float)capacity) * 100
                                       : 0;

                SnmpTonerLevelRow("Magenta Toner", magentaToner);

                var yellowTonerCapacity = GetSnmpData("1.3.6.1.2.1.43.11.1.1.8.1.3").Result;
                var yellowTonerLevel    = GetSnmpData("1.3.6.1.2.1.43.11.1.1.9.1.3").Result;

                var yellowToner = int.TryParse(yellowTonerCapacity, out capacity) && int.TryParse(yellowTonerLevel, out level)
                                      ? (level / (float)capacity) * 100
                                      : 0;

                 SnmpTonerLevelRow("Yellow Toner", yellowToner);

                var blackTonerCapacity = GetSnmpData("1.3.6.1.2.1.43.11.1.1.8.1.4").Result;
                var blackTonerLevel    = GetSnmpData("1.3.6.1.2.1.43.11.1.1.9.1.4").Result;

                var blackToner = int.TryParse(blackTonerCapacity, out capacity) && int.TryParse(blackTonerLevel, out level)
                                     ? (level / (float)capacity) * 100
                                     : 0;

                SnmpTonerLevelRow("Black Toner", blackToner);


                ImGui.EndTable();
            }

            ImGui.End();
        }
    }

    static int ToInt(this string value)
    {
        return int.TryParse(value, out var result) ? result : 0;
    }

    static int ToInt(this float value)
    {
        return (int)value;
    }

    static float ToFloat(this string value)
    {
        return float.TryParse(value, out var result) ? result : 0;
    }

    static void SnmpRow(string name, string oid)
    {
        var snmpData = GetSnmpData(oid).Result;

        if (ImGui.BeginTable("oidTable", 2))
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(name);
            ImGui.TableNextColumn();
            ImGui.Text(snmpData);

            ImGui.EndTable();
        }
    }

    static void SnmpTonerLevelRow(string name, float value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(name);
        ImGui.TableNextColumn();
        ImGui.ProgressBar(value  / 100, new Vector2(0.0f, 0.0f));



    }

    public static async Task<string> GetSnmpData(string oid)
    {
        try
        {
            var target = new IpAddress(ipAddress);
            var param  = new AgentParameters(SnmpVersion.Ver1);
            // param.Community = new OctetString("public");
            var pdu = new Pdu(PduType.Get);
            pdu.VbList.Add(oid);

            using var agent  = new UdpTarget((IPAddress)target, 161, 2000, 1);
            var       result = (SnmpV1Packet)agent.Request(pdu, param);
            return result?.Pdu?.VbList[0]?.Value?.ToString() ?? "Keine SNMP-Daten gefunden";
        }
        catch
        {
            return "Fehler beim Abrufen der SNMP-Daten";
        }
    }
}
