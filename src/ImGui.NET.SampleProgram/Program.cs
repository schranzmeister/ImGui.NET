using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Principal;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace ImGuiNET
{
    class Program
    {
        public static Sdl2Window _window;
        private static GraphicsDevice _gd;
        private static CommandList _cl;
        private static ImGuiController _controller;
        // private static MemoryEditor _memoryEditor;

        // UI state
        private static float _f = 0.0f;
        private static int _counter = 0;
        private static int _dragInt = 0;
        private static Vector3 _clearColor = new Vector3(0.45f, 0.55f, 0.6f);
        private static bool _showImGuiDemoWindow = true;
        private static bool _showAnotherWindow = false;
        private static bool _showMemoryEditor = false;
        private static byte[] _memoryEditorData;
        private static uint s_tab_bar_flags = (uint)ImGuiTabBarFlags.Reorderable;
        static bool[] s_opened = { true, true, true, true }; // Persistent user state

        public static bool _showSnmpWindow = false;

        static void SetThing(out float i, float val)
        {
            i = val;
        }

        static void Main(string[] args)
        {
            // Create window, GraphicsDevice, and all resources necessary for the demo.
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(1280, 384, 1280, 720, WindowState.Normal, "ImGui.NET Sample Program"),
                new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
                out _window,
                out _gd);
            _window.Resized += () =>
            {
                _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
                _controller.WindowResized(_window.Width, _window.Height);
            };
            _cl = _gd.ResourceFactory.CreateCommandList();
            _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width,
                _window.Height);
            // _memoryEditor = new MemoryEditor();
            Random random = new Random();
            _memoryEditorData = Enumerable.Range(0, 1024).Select(i => (byte)random.Next(255)).ToArray();

            var stopwatch = Stopwatch.StartNew();
            float deltaTime = 0f;
            // Main application loop
            while (_window.Exists)
            {
                deltaTime = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
                stopwatch.Restart();
                InputSnapshot snapshot = _window.PumpEvents();
                if (!_window.Exists)
                {
                    break;
                }

                _controller.Update(deltaTime,
                    snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.

                SubmitUI();

                _cl.Begin();
                _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
                _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
                _controller.Render(_gd, _cl);
                _cl.End();
                _gd.SubmitCommands(_cl);
                _gd.SwapBuffers(_gd.MainSwapchain);
            }

            // Clean up Veldrid resources
            _gd.WaitForIdle();
            _controller.Dispose();
            _cl.Dispose();
            _gd.Dispose();
        }
      public  static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        private static unsafe void SubmitUI()
        {
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(_window.Width, _window.Height));

            if (ImGui.Begin("MainWindow",
                    ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBringToFrontOnFocus ))
            {
                if (!IsAdministrator())
                {
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), "You need to run this program as administrator to use all features.");
                    if (ImGui.Button("Restart as Administrator"))
                    {
                        RestartAsAdministrator();
                        _window.Close();
                    }
                }
                if (ImGui.BeginTabBar("MainTabBar"))
                {

                    WindowsInfo.ShowWindowsInfo();
                    TaskManager.ShowTaskManager();
                    ServicesInfo.ShowServicesInfo();
                    EventLogInfo.ShowEventLog();

                    ImGui.EndTabBar();
                }

                ImGui.End();
            }

            if(_showSnmpWindow)
                SnmpInfo.ShowSnmpInfo();
        }

        static void RestartAsAdministrator()
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            if (processModule == null) return;

            var exeName = processModule.FileName;
            var startInfo = new ProcessStartInfo(exeName)
                            {
                                UseShellExecute = true,
                                Verb            = "runas" // Erzwingt Admin-Rechte
                            };

            try
            {
                Process.Start(startInfo);
            }
            catch
            {
                Console.WriteLine("Fehler: Das Programm konnte nicht mit Administratorrechten neu gestartet werden.");
            }
        }
    }
}
