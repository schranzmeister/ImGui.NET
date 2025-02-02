using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.ServiceProcess;
using Microsoft.Win32;

#pragma warning disable CA1416
namespace ImGuiNET;

public class ServicesInfo
{
    private static ServiceController[]               services;
    static         Dictionary<string, List<Service>> serviceGroups;
    private static int                               selectedServiceGroup = -1;
    private static int                               selectedService      = -1;
    static         string                            errorText            = "";

    public static void ShowServicesInfo()
    {
        if (services == null)
        {
            GetServices();
        }

        if (ImGui.BeginTabItem("ServicesInfo"))
        {
            if (ImGui.BeginChild("ServicesListChild", new Vector2(350, -1), ImGuiChildFlags.Borders))
            {
                foreach (var group in serviceGroups)
                {
                    if (ImGui.TreeNodeEx($"{group.Key}##{group.GetHashCode()}"))
                    {
                        foreach (var service in group.Value)
                        {
                            if (ImGui.Selectable($"{service.DisplayName}##{service.GetHashCode()}", selectedServiceGroup == service.GetHashCode()))
                            {
                                selectedServiceGroup = service.GetHashCode();
                            }
                        }

                        ImGui.TreePop();
                    }
                }

                ImGui.EndChild();
            }

            ImGui.SameLine();

            if (ImGui.BeginChild("ServiceInfoChild", new Vector2(-1, -1), ImGuiChildFlags.Borders))
            {
                if (selectedServiceGroup != -1)
                {
                    foreach (var group in serviceGroups)
                    {
                        foreach (var service in group.Value)
                        {
                            if (selectedServiceGroup == service.GetHashCode())
                            {
                                if (ImGui.BeginTable("ServiceInfoTable", 2))
                                {
                                    ServiceInfoRow("Name",         service.Name);
                                    ServiceInfoRow("Display Name", service.DisplayName);
                                    ServiceInfoRow("Status",       service.Status.ToString());

                                    ServiceInfoRow("Start Type",             service.ServiceController.StartType.ToString());
                                    ServiceInfoRow("Service Type",           service.ServiceController.ServiceType.ToString());
                                    ServiceInfoRow("Can Pause and Continue", service.ServiceController.CanPauseAndContinue.ToString());
                                    ServiceInfoRow("Can Shutdown",           service.ServiceController.CanShutdown.ToString());
                                    ServiceInfoRow("Can Stop",               service.ServiceController.CanStop.ToString());
                                    ServiceInfoRow("Machine Name",           service.ServiceController.MachineName);
                                    ServiceInfoRow("Service Name",           service.ServiceController.ServiceName);

                                    if (Program.IsAdministrator())
                                    {
                                        ServiceInfoRow("Service Handle", service.ServiceController.ServiceHandle.ToString());
                                    }


                                    ImGui.EndTable();
                                }

                                if (ImGui.Button("Starten"))
                                {
                                    ServiceController sc = new ServiceController(service.Name);
                                    try
                                    {
                                        sc.Start();
                                    }
                                    catch (Exception e)
                                    {
                                        errorText = e.Message;
                                    }
                                }

                                if (service.ServiceController.CanStop)
                                {
                                    ImGui.SameLine();
                                    if (ImGui.Button("Beenden"))
                                    {
                                        ServiceController sc = new ServiceController(service.Name);
                                        try
                                        {
                                            sc.Stop();
                                            sc.WaitForStatus(ServiceControllerStatus.Stopped);
                                        }
                                        catch (Exception e)
                                        {
                                            errorText = e.Message;
                                        }
                                    }

                                    ImGui.SameLine();
                                    if (ImGui.Button("Neu starten"))
                                    {
                                        ServiceController sc = new ServiceController(service.Name);
                                        try
                                        {
                                            sc.Stop();
                                            sc.WaitForStatus(ServiceControllerStatus.Stopped);
                                            sc.Start();
                                        }
                                        catch (Exception e)
                                        {
                                            errorText = e.Message;
                                        }
                                    }
                                }


                                if (service.ServiceController.CanPauseAndContinue)
                                {
                                    ImGui.SameLine();
                                    if (ImGui.Button("Anhalten"))
                                    {
                                        ServiceController sc = new ServiceController(service.Name);
                                        try
                                        {
                                            sc.Pause();
                                        }
                                        catch (Exception e)
                                        {
                                            errorText = e.Message;
                                        }
                                    }

                                    ImGui.SameLine();
                                    if (ImGui.Button("Fortsetzen"))
                                    {
                                        ServiceController sc = new ServiceController(service.Name);
                                        try
                                        {
                                            sc.Continue();
                                        }
                                        catch (Exception e)
                                        {
                                            errorText = e.Message;
                                        }
                                    }
                                }

                                ImGui.Spacing();

                                if (Program.IsAdministrator())
                                {
                                    ImGui.Text("Dienste, die von diesem Dienst abhängen");
                                    ImGui.Indent(10);
                                    foreach (var dependentService in service.DependentServices)
                                    {
                                        if (ImGui.BeginTable("DependentServicesTable", 4))
                                        {
                                            ImGui.TableNextRow();
                                            ImGui.TableNextColumn();
                                            ImGui.Text(dependentService.ServiceName);
                                            ImGui.TableNextColumn();
                                            ImGui.Text(dependentService.DisplayName);
                                            ImGui.TableNextColumn();
                                            ImGui.Text(dependentService.Status.ToString());
                                            ImGui.TableNextColumn();

                                            if (ImGui.Button("Starten##" + dependentService.ServiceName))
                                            {
                                                ServiceController sc = new ServiceController(dependentService.ServiceName);
                                                try
                                                {
                                                    sc.Start();
                                                }
                                                catch (Exception e)
                                                {
                                                    errorText = e.Message;
                                                }
                                            }

                                            if (dependentService.CanStop)
                                            {
                                                ImGui.SameLine();
                                                if (ImGui.Button("Beenden##" + dependentService.ServiceName))
                                                {
                                                    try
                                                    {
                                                        dependentService.Stop();
                                                        dependentService.WaitForStatus(ServiceControllerStatus.Stopped);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        errorText = e.Message;
                                                    }
                                                }

                                                ImGui.SameLine();

                                                if (ImGui.Button("Neu starten##" + dependentService.ServiceName))
                                                {
                                                    try
                                                    {
                                                        dependentService.Stop();
                                                        dependentService.WaitForStatus(ServiceControllerStatus.Stopped);
                                                        dependentService.Start();
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        errorText = e.Message;
                                                    }
                                                }
                                            }

                                            if (dependentService.CanPauseAndContinue)
                                            {
                                                ImGui.SameLine();
                                                if (ImGui.Button("Anhalten##" + dependentService.ServiceName))
                                                {
                                                    try
                                                    {
                                                        dependentService.Pause();
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        errorText = e.Message;
                                                    }
                                                }

                                                ImGui.SameLine();

                                                if (ImGui.Button("Fortsetzen##" + dependentService.ServiceName))
                                                {
                                                    try
                                                    {
                                                        dependentService.Continue();
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        errorText = e.Message;
                                                    }
                                                }
                                            }

                                            ImGui.EndTable();
                                        }
                                    }

                                    ImGui.Spacing();
                                    ImGui.Unindent(10);

                                    ImGui.Text("Dienste, von denen dieser Dienst abhängt");
                                    ImGui.Indent(10);
                                    foreach (var dependentOnService in service.DependentOnServices)
                                    {
                                        if (ImGui.BeginTable("DependentOnServicesTable", 4))
                                        {
                                            ImGui.TableNextRow();
                                            ImGui.TableNextColumn();
                                            ImGui.Text(dependentOnService.ServiceName);
                                            ImGui.TableNextColumn();
                                            ImGui.Text(dependentOnService.DisplayName);
                                            ImGui.TableNextColumn();
                                            ImGui.Text(dependentOnService.Status.ToString());
                                            ImGui.TableNextColumn();

                                            if (ImGui.Button("Starten##" + dependentOnService.ServiceName))
                                            {
                                                ServiceController sc = new ServiceController(dependentOnService.ServiceName);
                                                try
                                                {
                                                    sc.Start();
                                                }
                                                catch (Exception e)
                                                {
                                                    errorText = e.Message;
                                                }
                                            }

                                            if (dependentOnService.CanStop)
                                            {
                                                ImGui.SameLine();
                                                if (ImGui.Button("Beenden##" + dependentOnService.ServiceName))
                                                {
                                                    try
                                                    {
                                                        dependentOnService.Stop();
                                                        dependentOnService.WaitForStatus(ServiceControllerStatus.Stopped);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        errorText = e.Message;
                                                    }
                                                }

                                                ImGui.SameLine();

                                                if (ImGui.Button("Neu starten##" + dependentOnService.ServiceName))
                                                {
                                                    try
                                                    {
                                                        dependentOnService.Stop();
                                                        dependentOnService.WaitForStatus(ServiceControllerStatus.Stopped);
                                                        dependentOnService.Start();
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        errorText = e.Message;
                                                    }
                                                }
                                            }

                                            if (dependentOnService.CanPauseAndContinue)
                                            {
                                                ImGui.SameLine();
                                                if (ImGui.Button("Anhalten##" + dependentOnService.ServiceName))
                                                {
                                                    try
                                                    {
                                                        dependentOnService.Pause();
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        errorText = e.Message;
                                                    }
                                                }

                                                ImGui.SameLine();

                                                if (ImGui.Button("Fortsetzen##" + dependentOnService.ServiceName))
                                                {
                                                    try
                                                    {
                                                        dependentOnService.Continue();
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        errorText = e.Message;
                                                    }
                                                }
                                            }

                                            ImGui.EndTable();
                                        }
                                    }
                                }
                                else
                                {
                                    ImGui.TextColored(new Vector4(1, 0, 0, 1),
                                                      "Administratorrechte erforderlich um Dienste anzuzeigen, die von diesem Dienst abhängen.");
                                    ImGui.TextColored(new Vector4(1, 0, 0, 1),
                                                      "Administratorrechte erforderlich um Dienste anzuzeigen, von denen dieser Dienst abhängt.");
                                }

                                ImGui.Unindent(10);
                            }
                        }
                    }
                }

                ImGui.EndChild();
            }


            ImGui.SetCursorScreenPos(new Vector2(366, Program._window.Height - 40));
            if (ImGui.BeginChild("ServiceConsoleChild", new Vector2(-1, 30)))
            {
                ImGui.InputTextMultiline("text", ref errorText, 1000, new Vector2(-1, -1), ImGuiInputTextFlags.ReadOnly);
                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }

        void ServiceInfoRow(string name, string value)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(name);
            ImGui.TableNextColumn();
            ImGui.Text(value);

            ImGui.TableNextRow();
        }
    }

    static void GetServices()
    {
        services ??= ServiceController.GetServices();

        serviceGroups = new Dictionary<string, List<Service>>();

        foreach (ServiceController service in ServiceController.GetServices())
        {
            string group = GetServiceGroup(service.ServiceName);

            var serviceInfo = new Service
                              {
                                  Name              = service.ServiceName,
                                  DisplayName       = service.DisplayName,
                                  Group             = group,
                                  Status            = service.Status,
                                  ServiceController = service
                              };

            if (Program.IsAdministrator())
            {
                serviceInfo.DependentServices   = service.DependentServices;
                serviceInfo.DependentOnServices = service.ServicesDependedOn;
            }

            if (!serviceGroups.ContainsKey(group))
                serviceGroups[group] = new List<Service>();

            serviceGroups[group].Add(serviceInfo);
        }
    }

    static string GetServiceGroup(string serviceName)
    {
        try
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey($"SYSTEM\\CurrentControlSet\\Services\\{serviceName}"))
            {
                return key?.GetValue("Group") as string ?? "Unbekannt";
            }
        }
        catch
        {
            return "Fehler";
        }
    }


    public class Service
    {
        public string                  Name                { get; set; }
        public string                  DisplayName         { get; set; }
        public string                  Description         { get; set; }
        public string                  Group               { get; set; }
        public ServiceController       ServiceController   { get; set; }
        public ServiceControllerStatus Status              { get; set; }
        public ServiceController[]     DependentServices   { get; set; }
        public ServiceController[]     DependentOnServices { get; set; }
    }
}
