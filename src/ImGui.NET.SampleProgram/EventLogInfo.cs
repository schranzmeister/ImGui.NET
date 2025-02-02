using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using Microsoft.Win32;

namespace ImGuiNET;

public class EventLogInfo
{
    public static string ipAddress = "";

    public static List<EventLogData> eventLogData        = new();
    public static bool               eventLogDataFetched = false;


    public static void ShowEventLog()
    {
        ListAvailableLogs();

        if (ImGui.BeginTabItem("EventLog"))
        {
            if (eventLogDataFetched)
            {
                if (eventLogData.Count > 0)
                {
                    ImGui.BeginTable("EventLogTable", 4,
                                     ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.BordersOuterV |
                                     ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.RowBg |
                                     ImGuiTableFlags.ScrollY | ImGuiTableFlags.ScrollX | ImGuiTableFlags.SizingFixedFit);

                    ImGui.TableSetupColumn("TimeGenerated",     ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("ID",                ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("ProviderName",      ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("FormatDescription", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);

                    ImGui.TableHeadersRow();


                    foreach (var logData in eventLogData)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text(logData.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(logData.Id.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(logData.ProviderName);
                        ImGui.TableNextColumn();
                        ImGui.Text(logData.FormatDescription);
                    }


                ImGui.EndTable();
                }
            }
            else
            {
                ImGui.Text("No EventLog data fetched yet.");
            }

            ImGui.EndTabItem();
        }
    }

    static void ListAvailableLogs(string filter = "Logitech-Lamparray-Driver/Errors")
    {
        if(!Program.IsAdministrator()) return;
        if (eventLogDataFetched) return;

        try
        {
            string[] windowsLogs = ["Install"];

            foreach (string logName in windowsLogs)
            {
                var eLogData = new EventLogData
                               {
                                   EventLogName = logName
                               };
                eventLogData.Add(eLogData);

                ReadEventLog(logName);
            }

            eventLogDataFetched = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Abrufen der Protokollnamen: {ex.Message}");
            Console.ReadKey( );
        }
    }

    static void ReadEventLog(string logName)
    {
        try
        {
            EventLogQuery  eventLogQuery  = new EventLogQuery(logName, PathType.LogName);
            EventLogReader eventLogReader = new EventLogReader(eventLogQuery);


            for (EventRecord eventRecord = eventLogReader.ReadEvent(); eventRecord != null; eventRecord = eventLogReader.ReadEvent())
            {
                var elog = eventLogData.Find(e => e.EventLogName == logName);
                elog.TimeCreated       = eventRecord.TimeCreated ?? DateTime.MinValue;
                elog.Id                = eventRecord.Id;
                elog.ProviderName      = eventRecord.ProviderName;
                elog.FormatDescription = eventRecord.FormatDescription();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Zugriff auf {logName}: {ex.Message}");
            Console.ReadKey( );
        }
    }


    public class EventLogData
    {
        public EventLog eventLog          { get; set; }
        public string   EventLogName      { get; set; }
        public DateTime TimeCreated       { get; set; }
        public int      Id                { get; set; }
        public string   ProviderName      { get; set; }
        public string   FormatDescription { get; set; }
    }
}
