using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using SnmpSharpNet;

namespace ImGuiNET;

public static class SnmpInfo
{
    public static string ipAddress = "";

    public static SnmpData snmpData        = new SnmpData();
   public static        bool     snmpDataFetched = false;

    public class SnmpData
    {
        public string PrinterName  { get; set; }
        public string SerialNumber { get; set; }

        public string CyanTonerCapacity { get; set; }
        public string CyanTonerLevel    { get; set; }

        private float CyanToner = -1;

        public float GetCyantoner()
        {
            if (CyanToner < 0)
            {
                CyanToner = int.TryParse(CyanTonerCapacity, out var capacity) && int.TryParse(CyanTonerLevel, out var level)
                                ? (level / (float)capacity) * 100
                                : 0;
                return CyanToner;
            }
            else
            {
                return CyanToner;
            }
        }

        public string MagentaTonerCapacity { get; set; }
        public string MagentaTonerLevel    { get; set; }

        private float MagentaToner  = -1;

        public float GetMagentaToner()
        {
            if (MagentaToner < 0)
            {
                MagentaToner = int.TryParse(MagentaTonerCapacity, out var capacity) && int.TryParse(MagentaTonerLevel, out var level)
                                   ? (level / (float)capacity) * 100
                                   : 0;
                return MagentaToner;
            }
            else
            {
                return MagentaToner;
            }
        }

        public string YellowTonerCapacity { get; set; }
        public string YellowTonerLevel    { get; set; }

        private float YellowToner  = -1;

        public float GetYellowToner()
        {
            if (YellowToner < 0)
            {
                YellowToner = int.TryParse(YellowTonerCapacity, out var capacity) && int.TryParse(YellowTonerLevel, out var level)
                                  ? (level / (float)capacity) * 100
                                  : 0;
                return YellowToner;
            }
            else
            {
                return YellowToner;
            }
        }

        public  string BlackTonerCapacity { get; set; }
        public  string BlackTonerLevel    { get; set; }
        private float  BlackToner = -1;

        public float GetBlackToner()
        {
            if (BlackToner < 0)
            {
                BlackToner = int.TryParse(BlackTonerCapacity, out var capacity) && int.TryParse(BlackTonerLevel, out var level)
                                 ? (level / (float)capacity) * 100
                                 : 0;
                return BlackToner;
            }
            else
            {
                return BlackToner;
            }
        }

        public void Reset()
        {
            CyanToner = -1;
            MagentaToner = -1;
            YellowToner = -1;
            BlackToner = -1;
        }
    }

    static void GetSnmpData()
    {
        if (snmpDataFetched) return;
        snmpDataFetched = true;

        snmpData.Reset();

        snmpData.PrinterName  = GetSnmpData("1.3.6.1.2.1.43.5.1.1.16.1").Result;
        snmpData.SerialNumber = GetSnmpData("1.3.6.1.2.1.43.5.1.1.17.1").Result;

        snmpData.CyanTonerCapacity = GetSnmpData("1.3.6.1.2.1.43.11.1.1.8.1.1").Result;
        snmpData.CyanTonerLevel    = GetSnmpData("1.3.6.1.2.1.43.11.1.1.9.1.1").Result;

        snmpData.MagentaTonerCapacity = GetSnmpData("1.3.6.1.2.1.43.11.1.1.8.1.2").Result;
        snmpData.MagentaTonerLevel    = GetSnmpData("1.3.6.1.2.1.43.11.1.1.9.1.2").Result;

        snmpData.YellowTonerCapacity = GetSnmpData("1.3.6.1.2.1.43.11.1.1.8.1.3").Result;
        snmpData.YellowTonerLevel    = GetSnmpData("1.3.6.1.2.1.43.11.1.1.9.1.3").Result;

        snmpData.BlackTonerCapacity = GetSnmpData("1.3.6.1.2.1.43.11.1.1.8.1.4").Result;
        snmpData.BlackTonerLevel    = GetSnmpData("1.3.6.1.2.1.43.11.1.1.9.1.4").Result;
    }

    public static void ShowSnmpInfo()
    {
        GetSnmpData();

        ImGui.SetNextWindowSize( new Vector2(220, 170));

        if (ImGui.Begin("SNMP Info",ref Program._showSnmpWindow,  ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoDocking))
        {
            SnmpRow("Printer Name",  snmpData.PrinterName);
            SnmpRow("Serial Number", snmpData.SerialNumber);

            if (ImGui.BeginTable("TonerLevelTable", 2))
            {
                SnmpTonerLevelRow("Cyan Toner",    snmpData.GetCyantoner());
                SnmpTonerLevelRow("Magenta Toner", snmpData.GetMagentaToner());
                SnmpTonerLevelRow("Yellow Toner",  snmpData.GetYellowToner());
                SnmpTonerLevelRow("Black Toner",   snmpData.GetBlackToner());

                ImGui.EndTable();
            }

            ImGui.End();
        }
    }

    static void SnmpRow(string name, string value)
    {
        if (ImGui.BeginTable("oidTable", 2))
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(name);
            ImGui.TableNextColumn();
            ImGui.Text(value);

            ImGui.EndTable();
        }
    }

    static void SnmpTonerLevelRow(string name, float value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(name);
        ImGui.TableNextColumn();
        ImGui.ProgressBar(value / 100, new Vector2(0.0f, 0.0f));
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
