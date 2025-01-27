using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Vulkan.Xlib;

namespace ImGuiNET;

public class TaskManager
{
    static int columnCount = 3;
    static bool showMemory = false;


    public static void ShowTaskManager()
    {
        if (ImGui.BeginTabItem("TaskManager"))
        {
            ShowMemory();

            List<Process> processes = new List<Process>(Process.GetProcesses());

            ImGui.BeginTable("TaskManagerTable", columnCount,
                ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.BordersOuterV |
                ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.RowBg |
                ImGuiTableFlags.ScrollY | ImGuiTableFlags.ScrollX | ImGuiTableFlags.SizingFixedFit);

            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("PID", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);

            if (showMemory)
            {
                ImGui.TableSetupColumn("Memory (b)",
                    ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Memory (kb)",
                    ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Memory (MB)",
                    ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            }

            ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableHeadersRow();

            foreach (var process in processes)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(process.ProcessName);
                ImGui.TableNextColumn();
                ImGui.Text(process.Id.ToString());
                ImGui.TableNextColumn();


                if (showMemory)
                {
                    string memoryBytes = process.WorkingSet64.ToString("N0");
                    string memoryKb = (process.WorkingSet64 / 1024).ToString("N0");
                    string memoryMb = (process.WorkingSet64 / 1024 / 1024).ToString("N0");

                    ImGui.Text(memoryBytes);
                    ImGui.TableNextColumn();
                    ImGui.Text(memoryKb);
                    ImGui.TableNextColumn();
                    ImGui.Text(memoryMb);
                    ImGui.TableNextColumn();
                }


                if (ImGui.Button("Kill##" + process.Id))
                {
                    process.Kill();
                }
            }

            ImGui.EndTable();


            ImGui.EndTabItem();
        }
    }


    static void ShowMemory()
    {
        if (ImGui.Button("Show Memory"))
        {
            showMemory = !showMemory;

            if (showMemory)
            {
                columnCount += 3;
            }
            else
            {
                columnCount -= 3;
            }
        }
    }
}