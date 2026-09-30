using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Ac8MouseYaw
{
    internal sealed class Config
    {
        public bool EnabledOnStart = true;
        public int PollHz = 1000;
        public int SourceXInputSlot = -1;
        public string ToggleKey = "F10";
        public double XDpi = 800.0;
        public double YDpi = 800.0;
        public double Sensitivity = 1.0;
        public double XSensitivity = 1.0;
        public double YSensitivity = 1.0;
        public double FullSpeedInchesPerSecond = 2.0;
        public double Deadzone = 0.0;
        public double DeadzoneX = 0.0;
        public double DeadzoneY = 0.0;
        public double BoostX = 0.0;
        public double BoostY = 0.0;
        public double HoldMs = 16.0;
        public bool InvertY = false;
        public bool ShowNotifications = true;

        public static Config Load(string path)
        {
            Config c = new Config();
            double defaultDpi = 800.0;
            bool sawXDpi = false;
            bool sawYDpi = false;
            double defaultDeadzone = 0.0;
            bool sawDeadzoneX = false;
            bool sawDeadzoneY = false;
            double defaultBoost = 0.0;
            bool sawBoostX = false;
            bool sawBoostY = false;
            if (!File.Exists(path))
            {
                c.Save(path);
                return c;
            }

            string[] lines;
            try { lines = File.ReadAllLines(path); }
            catch { return c; }

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#"))
                    continue;
                int eq = line.IndexOf('=');
                if (eq <= 0)
                    continue;
                string key = line.Substring(0, eq).Trim().ToLowerInvariant();
                string value = line.Substring(eq + 1).Trim();
                try
                {
                    switch (key)
                    {
                        case "enabled_on_start": c.EnabledOnStart = ParseBool(value, c.EnabledOnStart); break;
                        case "poll_hz": c.PollHz = (int)Clamp(ParseDouble(value, c.PollHz), 60, 1000); break;
                        case "source_xinput_slot": c.SourceXInputSlot = (int)Clamp(ParseDouble(value, c.SourceXInputSlot), -1, 3); break;
                        case "toggle_key": c.ToggleKey = value.Length == 0 ? "F10" : value; break;
                        case "dpi": defaultDpi = Clamp(ParseDouble(value, defaultDpi), 50, 32000); break;
                        case "x_dpi": c.XDpi = Clamp(ParseDouble(value, c.XDpi), 50, 32000); sawXDpi = true; break;
                        case "y_dpi": c.YDpi = Clamp(ParseDouble(value, c.YDpi), 50, 32000); sawYDpi = true; break;
                        case "sensitivity": c.Sensitivity = Clamp(ParseDouble(value, c.Sensitivity), 0.05, 20); break;
                        case "x_sensitivity": c.XSensitivity = Clamp(ParseDouble(value, c.XSensitivity), 0.05, 20); break;
                        case "y_sensitivity": c.YSensitivity = Clamp(ParseDouble(value, c.YSensitivity), 0.05, 20); break;
                        case "full_speed_inches_per_second": c.FullSpeedInchesPerSecond = Clamp(ParseDouble(value, c.FullSpeedInchesPerSecond), 0.1, 100); break;
                        case "deadzone": defaultDeadzone = Clamp(ParseDouble(value, defaultDeadzone), 0, 0.95); break;
                        case "deadzone_x": c.DeadzoneX = Clamp(ParseDouble(value, c.DeadzoneX), 0, 0.95); sawDeadzoneX = true; break;
                        case "deadzone_y": c.DeadzoneY = Clamp(ParseDouble(value, c.DeadzoneY), 0, 0.95); sawDeadzoneY = true; break;
                        case "boost": defaultBoost = Clamp(ParseDouble(value, defaultBoost), -1, 1); break;
                        case "boost_x": c.BoostX = Clamp(ParseDouble(value, c.BoostX), -1, 1); sawBoostX = true; break;
                        case "boost_y": c.BoostY = Clamp(ParseDouble(value, c.BoostY), -1, 1); sawBoostY = true; break;
                        case "hold_ms": c.HoldMs = Clamp(ParseDouble(value, c.HoldMs), 1, 100); break;
                        case "invert_y": c.InvertY = ParseBool(value, c.InvertY); break;
                        case "show_notifications": c.ShowNotifications = ParseBool(value, c.ShowNotifications); break;
                    }
                }
                catch { }
            }
            if (!sawXDpi) c.XDpi = defaultDpi;
            if (!sawYDpi) c.YDpi = defaultDpi;
            if (!sawDeadzoneX) c.DeadzoneX = defaultDeadzone;
            if (!sawDeadzoneY) c.DeadzoneY = defaultDeadzone;
            if (!sawBoostX) c.BoostX = defaultBoost;
            if (!sawBoostY) c.BoostY = defaultBoost;
            return c;
        }

        public void Save(string path)
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine("; AC8 MouseYaw configuration");
            b.AppendLine("; Raw mouse counts are never blocked or consumed by this tool.");
            b.AppendLine("; dpi is the legacy default; x_dpi/y_dpi are the actual per-axis hardware DPI values.");
            b.AppendLine("; full_speed_inches_per_second: physical mouse speed that equals full stick/trigger deflection.");
            b.AppendLine("; sensitivity is global; x_sensitivity/y_sensitivity are extra per-axis multipliers.");
            b.AppendLine();
            b.AppendLine("enabled_on_start=" + (EnabledOnStart ? "true" : "false"));
            b.AppendLine("toggle_key=" + ToggleKey);
            b.AppendLine("poll_hz=" + PollHz);
            b.AppendLine("source_xinput_slot=" + SourceXInputSlot);
            b.AppendLine();
            b.AppendLine("dpi=" + XDpi.ToString(System.Globalization.CultureInfo.InvariantCulture));
            b.AppendLine("x_dpi=" + XDpi.ToString(System.Globalization.CultureInfo.InvariantCulture));
            b.AppendLine("y_dpi=" + YDpi.ToString(System.Globalization.CultureInfo.InvariantCulture));
            b.AppendLine("sensitivity=" + Sensitivity.ToString(System.Globalization.CultureInfo.InvariantCulture));
            b.AppendLine("x_sensitivity=" + XSensitivity.ToString(System.Globalization.CultureInfo.InvariantCulture));
            b.AppendLine("y_sensitivity=" + YSensitivity.ToString(System.Globalization.CultureInfo.InvariantCulture));
            b.AppendLine("full_speed_inches_per_second=" + FullSpeedInchesPerSecond.ToString(System.Globalization.CultureInfo.InvariantCulture));
            b.AppendLine("deadzone_x=" + DeadzoneX.ToString(System.Globalization.CultureInfo.InvariantCulture));
            b.AppendLine("deadzone_y=" + DeadzoneY.ToString(System.Globalization.CultureInfo.InvariantCulture));
            b.AppendLine("boost_x=" + BoostX.ToString(System.Globalization.CultureInfo.InvariantCulture));
            b.AppendLine("boost_y=" + BoostY.ToString(System.Globalization.CultureInfo.InvariantCulture));
            b.AppendLine("hold_ms=" + HoldMs.ToString(System.Globalization.CultureInfo.InvariantCulture));
            b.AppendLine("invert_y=" + (InvertY ? "true" : "false"));
            b.AppendLine("show_notifications=" + (ShowNotifications ? "true" : "false"));
            File.WriteAllText(path, b.ToString(), new UTF8Encoding(false));
        }
        private static bool ParseBool(string s, bool fallback)
        {
            if (s.Equals("1") || s.Equals("true", StringComparison.OrdinalIgnoreCase) || s.Equals("yes", StringComparison.OrdinalIgnoreCase) || s.Equals("on", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.Equals("0") || s.Equals("false", StringComparison.OrdinalIgnoreCase) || s.Equals("no", StringComparison.OrdinalIgnoreCase) || s.Equals("off", StringComparison.OrdinalIgnoreCase)) return false;
            return fallback;
        }
        private static double ParseDouble(string s, double fallback)
        {
            double v;
            return double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v) ? v : fallback;
        }
        private static double Clamp(double v, double lo, double hi)
        {
            if (v < lo) return lo;
            if (v > hi) return hi;
            return v;
        }
    }

    internal sealed class RawInputWindow : NativeWindow
    {
        private const int WmInput = 0x00FF;
        private const int WmToggleRequest = 0x8001;
        private const int WmQuitRequest = 0x8002;
        private const int WmSetEnabledRequest = 0x8003;
        public event Action ToggleRequested;
        public event Action<bool> EnabledRequested;

        public RawInputWindow()
        {
            CreateParams cp = new CreateParams();
            cp.ClassName = "STATIC";
            cp.Caption = "AC8 MouseYaw raw input sink";
            cp.Style = 0;
            cp.ExStyle = 0x00000080;
            cp.Parent = new IntPtr(-3);
            CreateHandle(cp);
        }

        public bool RegisterMouse()
        {
            RawInputDevice rid = new RawInputDevice();
            rid.UsagePage = 0x01;
            rid.Usage = 0x02;
            rid.Flags = 0x00000100; // RIDEV_INPUTSINK: observe background input, never consume it.
            rid.Target = Handle;
            RawInputDevice[] a = new RawInputDevice[] { rid };
            return RegisterRawInputDevices(a, 1, (uint)Marshal.SizeOf(typeof(RawInputDevice)));
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmInput)
            {
                Program.ProcessRawInput(m.LParam);
            }
            else if (m.Msg == WmToggleRequest)
            {
                Action h = ToggleRequested;
                if (h != null) h();
            }
            else if (m.Msg == WmSetEnabledRequest)
            {
                Action<bool> h = EnabledRequested;
                if (h != null) h(m.WParam.ToInt32() != 0);
            }
            else if (m.Msg == WmQuitRequest)
            {
                Application.ExitThread();
                return;
            }
            base.WndProc(ref m);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RawInputDevice
        {
            public ushort UsagePage;
            public ushort Usage;
            public uint Flags;
            public IntPtr Target;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterRawInputDevices(RawInputDevice[] devices, uint count, uint size);
    }

    internal static class Program
    {
        private const int VkF10 = 0x79;
        private const uint RidInput = 0x10000003;
        private const uint RimTypeMouse = 0;
        private const uint MouseMoveAbsolute = 0x0001;

        private static string _baseDir;
        private static string _configPath;
        private static string _logPath;
        private static Config _config;
        private static RawInputWindow _raw;
        private static ViGEmClient _client;
        private static IXbox360Controller _pad;
        private static Thread _worker;
        private static NotifyIcon _tray;
        private static Mutex _mutex;
        private static volatile bool _running;
        private static volatile bool _enabled;
        private static int _pendingDx;
        private static int _pendingDy;
        private static double _currentY;
        private static double _currentLt;
        private static double _currentRt;
        private static int _exitAfterSeconds;
        private static int _padUserIndex = -1;
        private static int _lastSourceSlot = -2;
        private static bool _togglePrevious;
        private static readonly object LogLock = new object();

        [STAThread]
        private static void Main(string[] args)
        {
            _baseDir = Path.GetDirectoryName(Application.ExecutablePath);
            _configPath = Path.Combine(_baseDir, "config.ini");
            _logPath = Path.Combine(_baseDir, "MouseYaw.log");
            ParseArgs(args);
            RotateLog();
            _config = Config.Load(_configPath);

            bool created;
            _mutex = new Mutex(true, "AC8MouseYaw_Singleton", out created);
            if (!created)
            {
                MessageBox.Show("AC8 MouseYaw is already running.", "AC8 MouseYaw", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                try
                {
                    _client = new ViGEmClient();
                    _pad = _client.CreateXbox360Controller();
                    _pad.AutoSubmitReport = false;
                    _pad.Connect();
                    Thread.Sleep(250);
                    int idx = -1;
                    try { idx = _pad.UserIndex; } catch { }
                    _padUserIndex = idx;
                    Log("Virtual Xbox controller connected. XInput slot=" + idx + ".");
                    if (idx != 0)
                    {
                        Log("WARNING: the virtual controller is not XInput slot 0. Exit MouseYaw, unplug or switch off the keyboard, start MouseYaw again, then reconnect the keyboard.");
                    }
                }
                catch (Exception ex)
                {
                    Log("ViGEm init failed: " + ex.ToString());
                    MessageBox.Show("ViGEmBus could not create a virtual Xbox controller.\r\nSee MouseYaw.log.", "AC8 MouseYaw", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Cleanup();
                    return;
                }

                _raw = new RawInputWindow();
                if (!_raw.RegisterMouse())
                {
                    int err = Marshal.GetLastWin32Error();
                    Log("RegisterRawInputDevices failed. Win32 error=" + err);
                    MessageBox.Show("Raw mouse input registration failed. See MouseYaw.log.", "AC8 MouseYaw", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Cleanup();
                    return;
                }
                _raw.ToggleRequested += ToggleEnabled;
                _raw.EnabledRequested += SetEnabled;

                Log("Toggle key is polled globally; the key is not consumed and remains available to other programs. Key=" + _config.ToggleKey);

                CreateTray();
                _enabled = _config.EnabledOnStart;
                UpdateTray();
                if (_padUserIndex != 0 && _config.ShowNotifications)
                {
                    _tray.BalloonTipTitle = "Controller slot warning";
                    _tray.BalloonTipText = "Virtual pad is XInput slot " + _padUserIndex + ". Exit MouseYaw, unplug/switch off the keyboard, start MouseYaw, then reconnect the keyboard.";
                    _tray.ShowBalloonTip(3000);
                }
                _running = true;

                _worker = new Thread(WorkerLoop);
                _worker.IsBackground = true;
                _worker.Name = "AC8MouseYaw.Poll";
                _worker.Start();
                Thread consoleThread = new Thread(ConsoleLoop);
                consoleThread.IsBackground = true;
                consoleThread.Name = "AC8MouseYaw.Console";
                consoleThread.Start();

                Log("Started. enabled=" + _enabled + ", xDpi=" + _config.XDpi + ", yDpi=" + _config.YDpi + ", sensitivity=" + _config.Sensitivity + ", fullSpeed=" + _config.FullSpeedInchesPerSecond + " in/s.");
                if (_exitAfterSeconds > 0)
                {
                    System.Windows.Forms.Timer exitTimer = new System.Windows.Forms.Timer();
                    exitTimer.Interval = _exitAfterSeconds * 1000;
                    exitTimer.Tick += delegate { exitTimer.Stop(); Application.ExitThread(); };
                    exitTimer.Start();
                }

                Application.Run();
            }
            catch (Exception ex)
            {
                Log("Fatal: " + ex.ToString());
                MessageBox.Show(ex.Message, "AC8 MouseYaw", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cleanup();
            }
        }

        private static void ParseArgs(string[] args)
        {
            if (args == null) return;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--exit-after=", StringComparison.OrdinalIgnoreCase))
                {
                    int v;
                    if (int.TryParse(args[i].Substring(13), out v) && v > 0) _exitAfterSeconds = v;
                }
            }
        }

        private static void CreateTray()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem toggle = new ToolStripMenuItem("Enable / disable (F10)");
            toggle.Click += delegate { ToggleEnabled(); };
            ToolStripMenuItem openConfig = new ToolStripMenuItem("Open config");
            openConfig.Click += delegate { OpenPath(_configPath); };
            ToolStripMenuItem openFolder = new ToolStripMenuItem("Open folder");
            openFolder.Click += delegate { OpenPath(_baseDir); };
            ToolStripMenuItem exit = new ToolStripMenuItem("Exit");
            exit.Click += delegate { _running = false; Application.ExitThread(); };
            menu.Items.Add(toggle);
            menu.Items.Add(openConfig);
            menu.Items.Add(openFolder);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exit);

            _tray = new NotifyIcon();
            _tray.Icon = System.Drawing.SystemIcons.Application;
            _tray.ContextMenuStrip = menu;
            _tray.Visible = true;
            _tray.DoubleClick += delegate { ToggleEnabled(); };
        }

        private static void OpenPath(string path)
        {
            try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
            catch (Exception ex) { Log("Open path failed: " + ex.Message); }
        }

        private static void ToggleEnabled()
        {
            SetEnabled(!_enabled);
        }

        private static void SetEnabled(bool value)
        {
            if (_enabled == value) return;
            _enabled = value;
            if (!_enabled)
            {
                Interlocked.Exchange(ref _pendingDx, 0);
                Interlocked.Exchange(ref _pendingDy, 0);
            }
            Log("Set enabled=" + _enabled);
            if (_tray != null) _tray.Text = _enabled ? "AC8 MouseYaw - enabled" : "AC8 MouseYaw - disabled";
            if (_config.ShowNotifications && _tray != null)
            {
                _tray.BalloonTipTitle = "AC8 MouseYaw";
                _tray.BalloonTipText = _enabled ? "Mouse increment enabled." : "Mouse increment disabled.";
                _tray.ShowBalloonTip(1000);
            }
        }

        private static void ConsoleLoop()
        {
            try
            {
                Console.Title = "AC8 MouseYaw Bridge";
                Console.WriteLine("AC8 MouseYaw Bridge console");
                Console.WriteLine("Type `help` for commands, `list` for current settings, `exit` to quit.");
                Console.WriteLine();
                PrintSettings();
                while (_running)
                {
                    string line = Console.ReadLine();
                    if (line == null) break;
                    if (!HandleConsoleCommand(line)) break;
                }
            }
            catch (Exception ex)
            {
                Log("Console loop failed: " + ex.Message);
            }
        }

        private static bool HandleConsoleCommand(string line)
        {
            string text = line == null ? string.Empty : line.Trim();
            if (text.Length == 0) return true;
            string[] parts = text.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            string command = parts[0].ToLowerInvariant();

            if (command == "help" || command == "?" || command == "\u5e2e\u52a9")
            {
                PrintHelp();
                return true;
            }
            if (command == "list" || command == "ls" || command == "\u5217\u51fa" || command == "\u5217\u8868")
            {
                PrintSettings();
                return true;
            }
            if (command == "save" || command == "\u4fdd\u5b58")
            {
                SaveConsoleConfig();
                return true;
            }
            if (command == "exit" || command == "quit" || command == "\u9000\u51fa")
            {
                if (_raw != null) PostMessage(_raw.Handle, 0x8002, IntPtr.Zero, IntPtr.Zero);
                else _running = false;
                return false;
            }
            if (command != "set" && command != "\u8bbe\u7f6e")
            {
                Console.WriteLine("ERR unknown command: " + text);
                Console.WriteLine("Hint: set sensitivity 1.2 | set x 1.0 | set y 1.0 | list");
                return true;
            }
            if (parts.Length < 3)
            {
                Console.WriteLine("ERR usage: set <key> <value>");
                return true;
            }

            string key = parts[1].ToLowerInvariant();
            string value = parts[2];
            double number;
            int integer;

            if (key == "sensitivity" || key == "sens" || key == "\u7075\u654f\u5ea6")
            {
                if (TryNumber(value, 0.05, 20, out number)) { _config.Sensitivity = number; SaveAndReport("global sensitivity", number); }
                else Console.WriteLine("ERR sensitivity must be 0.05..20");
                return true;
            }
            if (key == "x" || key == "x-sensitivity" || key == "xsens" || key == "x\u8f74\u7075\u654f\u5ea6")
            {
                if (TryNumber(value, 0.05, 20, out number)) { _config.XSensitivity = number; SaveAndReport("X sensitivity (LT/RT)", number); }
                else Console.WriteLine("ERR X sensitivity must be 0.05..20");
                return true;
            }
            if (key == "y" || key == "y-sensitivity" || key == "ysens" || key == "y\u8f74\u7075\u654f\u5ea6")
            {
                if (TryNumber(value, 0.05, 20, out number)) { _config.YSensitivity = number; SaveAndReport("Y sensitivity (left stick Y)", number); }
                else Console.WriteLine("ERR Y sensitivity must be 0.05..20");
                return true;
            }
            if (key == "dpi")
            {
                if (TryNumber(value, 50, 32000, out number)) { _config.XDpi = number; _config.YDpi = number; SaveAndReport("dpi X/Y", number); }
                else Console.WriteLine("ERR dpi must be 50..32000");
                return true;
            }
            if (key == "x_dpi" || key == "xdpi" || key == "x\u8f74dpi")
            {
                if (TryNumber(value, 50, 32000, out number)) { _config.XDpi = number; SaveAndReport("X axis dpi", number); }
                else Console.WriteLine("ERR X axis dpi must be 50..32000");
                return true;
            }
            if (key == "y_dpi" || key == "ydpi" || key == "y\u8f74dpi")
            {
                if (TryNumber(value, 50, 32000, out number)) { _config.YDpi = number; SaveAndReport("Y axis dpi", number); }
                else Console.WriteLine("ERR Y axis dpi must be 50..32000");
                return true;
            }
            if (key == "speed" || key == "full_speed_inches_per_second")
            {
                if (TryNumber(value, 0.1, 100, out number)) { _config.FullSpeedInchesPerSecond = number; SaveAndReport("full speed (in/s)", number); }
                else Console.WriteLine("ERR speed must be 0.1..100");
                return true;
            }
            if (key == "deadzone" || key == "dz")
            {
                if (TryNumber(value, 0, 0.95, out number)) { _config.DeadzoneX = number; _config.DeadzoneY = number; SaveAndReport("deadzone X/Y", number); }
                else Console.WriteLine("ERR deadzone must be 0..0.95");
                return true;
            }
            if (key == "x_deadzone" || key == "x_dz" || key == "x\u8f74\u6b7b\u533a")
            {
                if (TryNumber(value, 0, 0.95, out number)) { _config.DeadzoneX = number; SaveAndReport("X deadzone", number); }
                else Console.WriteLine("ERR X deadzone must be 0..0.95");
                return true;
            }
            if (key == "y_deadzone" || key == "y_dz" || key == "y\u8f74\u6b7b\u533a")
            {
                if (TryNumber(value, 0, 0.95, out number)) { _config.DeadzoneY = number; SaveAndReport("Y deadzone", number); }
                else Console.WriteLine("ERR Y deadzone must be 0..0.95");
                return true;
            }
            if (key == "boost" || key == "boost_xy")
            {
                if (TryNumber(value, -1, 1, out number)) { _config.BoostX = number; _config.BoostY = number; SaveAndReport("boost X/Y", number); }
                else Console.WriteLine("ERR boost must be -1..1");
                return true;
            }
            if (key == "x_boost" || key == "xboost" || key == "x\u8f74boost")
            {
                if (TryNumber(value, -1, 1, out number)) { _config.BoostX = number; SaveAndReport("X boost", number); }
                else Console.WriteLine("ERR X boost must be -1..1");
                return true;
            }
            if (key == "y_boost" || key == "yboost" || key == "y\u8f74boost")
            {
                if (TryNumber(value, -1, 1, out number)) { _config.BoostY = number; SaveAndReport("Y boost", number); }
                else Console.WriteLine("ERR Y boost must be -1..1");
                return true;
            }            if (key == "hold" || key == "hold_ms")
            {
                if (TryNumber(value, 1, 100, out number)) { _config.HoldMs = number; SaveAndReport("hold (ms)", number); }
                else Console.WriteLine("ERR hold must be 1..100");
                return true;
            }            if (key == "poll_hz" || key == "poll_rate" || key == "hz")
            {
                if (int.TryParse(value, out integer) && integer >= 60 && integer <= 1000) { _config.PollHz = integer; SaveAndReport("poll_hz", integer); }
                else Console.WriteLine("ERR poll_hz must be 60..1000");
                return true;
            }
            if (key == "source_slot" || key == "source_xinput_slot")
            {
                if (int.TryParse(value, out integer) && integer >= -1 && integer <= 3) { _config.SourceXInputSlot = integer; _lastSourceSlot = -2; SaveAndReport("source_xinput_slot", integer); }
                else Console.WriteLine("ERR source_slot must be -1..3");
                return true;
            }
            if (key == "toggle_key")
            {
                string v = value.ToUpperInvariant();
                if (v == "F10" || v == "F11" || v == "F12")
                {
                    _config.ToggleKey = v;
                    SaveAndReport("toggle_key", v);
                }
                else Console.WriteLine("ERR toggle_key must be F10, F11 or F12");
                return true;
            }
            if (key == "invert_y")
            {
                bool b;
                if (TryBool(value, out b)) { _config.InvertY = b; SaveAndReport("invert_y", b); }
                else Console.WriteLine("ERR invert_y must be true/false");
                return true;
            }
            if (key == "enabled")
            {
                bool b;
                if (TryBool(value, out b))
                {
                    if (_raw != null) PostMessage(_raw.Handle, 0x8003, b ? new IntPtr(1) : IntPtr.Zero, IntPtr.Zero);
                    else _enabled = b;
                    Console.WriteLine("OK enabled = " + b);
                    SaveConsoleConfig();
                }
                else Console.WriteLine("ERR enabled must be true/false");
                return true;
            }

            Console.WriteLine("ERR unknown setting: " + parts[1]);
            return true;
        }

        private static bool TryNumber(string text, double min, double max, out double value)
        {
            if (!double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value)) return false;
            return value >= min && value <= max;
        }

        private static bool TryBool(string text, out bool value)
        {
            string s = (text ?? string.Empty).ToLowerInvariant();
            if (s == "1" || s == "true" || s == "on" || s == "yes")
            {
                value = true;
                return true;
            }
            if (s == "0" || s == "false" || s == "off" || s == "no")
            {
                value = false;
                return true;
            }
            value = false;
            return false;
        }

        private static void SaveAndReport(string name, double value)
        {
            SaveConsoleConfig();
            Console.WriteLine("OK " + name + " = " + value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        private static void SaveAndReport(string name, int value)
        {
            SaveConsoleConfig();
            Console.WriteLine("OK " + name + " = " + value);
        }

        private static void SaveAndReport(string name, bool value)
        {
            SaveConsoleConfig();
            Console.WriteLine("OK " + name + " = " + value);
        }

        private static void SaveAndReport(string name, string value)
        {
            SaveConsoleConfig();
            Console.WriteLine("OK " + name + " = " + value);
        }

        private static void SaveConsoleConfig()
        {
            try
            {
                _config.Save(_configPath);
                Console.WriteLine("Saved: " + _configPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERR save failed: " + ex.Message);
            }
        }

        private static void PrintSettings()
        {
            Console.WriteLine("--- current settings ---");
            Console.WriteLine("enabled                 : " + _enabled);
            Console.WriteLine("virtual pad slot        : " + (_padUserIndex >= 0 ? _padUserIndex.ToString() : "pending"));
            Console.WriteLine("source XInput slot      : " + (_lastSourceSlot >= 0 ? _lastSourceSlot.ToString() : (_config.SourceXInputSlot >= 0 ? _config.SourceXInputSlot.ToString() : "auto")));
            Console.WriteLine("toggle key              : " + _config.ToggleKey);
            Console.WriteLine("X axis dpi              : " + _config.XDpi.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("Y axis dpi              : " + _config.YDpi.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("poll rate (Hz)          : " + _config.PollHz);
            Console.WriteLine("global sensitivity      : " + _config.Sensitivity.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("X sensitivity (LT/RT)   : " + _config.XSensitivity.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("Y sensitivity (stick Y) : " + _config.YSensitivity.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("effective X sensitivity : " + (_config.Sensitivity * _config.XSensitivity).ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("effective Y sensitivity : " + (_config.Sensitivity * _config.YSensitivity).ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("full speed (in/s)       : " + _config.FullSpeedInchesPerSecond.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("X deadzone              : " + _config.DeadzoneX.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("Y deadzone              : " + _config.DeadzoneY.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("X boost                 : " + _config.BoostX.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("Y boost                 : " + _config.BoostY.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("hold (ms)               : " + _config.HoldMs.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("invert Y                : " + _config.InvertY);
            Console.WriteLine("source_xinput_slot      : " + _config.SourceXInputSlot);
            Console.WriteLine("config file             : " + _configPath);
            Console.WriteLine();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("  list                              show current settings");
            Console.WriteLine("  set sensitivity <value>           global mouse sensitivity");
            Console.WriteLine("  set x <value>                     X / LT-RT sensitivity");
            Console.WriteLine("  set y <value>                     Y / left-stick sensitivity");
            Console.WriteLine("  set dpi <value>                   set both X/Y hardware DPI");
            Console.WriteLine("  set x_dpi <value>                 X axis hardware DPI");
            Console.WriteLine("  set y_dpi <value>                 Y axis hardware DPI");
            Console.WriteLine("  set speed <value>                 full deflection speed, inches/sec");
            Console.WriteLine("  set deadzone <value>              set both X/Y deadzone, 0..0.95");
            Console.WriteLine("  set x_deadzone <value>            X / LT-RT deadzone");
            Console.WriteLine("  set y_deadzone <value>            Y / left-stick deadzone");
            Console.WriteLine("  set boost <value>                 set both X/Y boost, -1..1");
            Console.WriteLine("  set x_boost <value>               X / LT-RT boost");
            Console.WriteLine("  set y_boost <value>               Y / left-stick boost");
            Console.WriteLine("  set hold <ms>                     latest-speed hold, 1..100");
            Console.WriteLine("  set invert_y true|false");
            Console.WriteLine("  set enabled true|false");
            Console.WriteLine("  set toggle_key F10|F11|F12");
            Console.WriteLine("  set source_slot -1|0|1|2|3        -1 = auto");
            Console.WriteLine("  save                              write config.ini");
            Console.WriteLine("  exit                              quit program");
            Console.WriteLine("Chinese aliases: set \u7075\u654f\u5ea6 / set X\u8f74\u7075\u654f\u5ea6 / set Y\u8f74\u7075\u654f\u5ea6 / list / help / \u9000\u51fa");
            Console.WriteLine();
        }
        private static void UpdateTray()
        {
            if (_tray != null)
                _tray.Text = _enabled ? "AC8 MouseYaw - enabled" : "AC8 MouseYaw - disabled";
        }

        internal static void ProcessRawInput(IntPtr rawInputHandle)
        {
            try
            {
                uint size = 0;
                uint headerSize = (uint)Marshal.SizeOf(typeof(RawInputHeader));
                GetRawInputData(rawInputHandle, RidInput, IntPtr.Zero, ref size, headerSize);
                if (size == 0 || size > 1024 * 1024) return;

                IntPtr buffer = Marshal.AllocHGlobal((int)size);
                try
                {
                    uint written = GetRawInputData(rawInputHandle, RidInput, buffer, ref size, headerSize);
                    if (written == 0) return;
                    RawInputHeader header = (RawInputHeader)Marshal.PtrToStructure(buffer, typeof(RawInputHeader));
                    if (header.Type != RimTypeMouse) return;
                    RawMouse mouse = (RawMouse)Marshal.PtrToStructure(new IntPtr(buffer.ToInt64() + headerSize), typeof(RawMouse));
                    if ((mouse.Flags & MouseMoveAbsolute) != 0) return;
                    if (mouse.LastX != 0) Interlocked.Add(ref _pendingDx, mouse.LastX);
                    if (mouse.LastY != 0) Interlocked.Add(ref _pendingDy, mouse.LastY);
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            catch (Exception ex)
            {
                Log("Raw input parse failed: " + ex.Message);
            }
        }

        private static void WorkerLoop()
        {
            Stopwatch sw = Stopwatch.StartNew();
            _togglePrevious = IsKeyDown(GetToggleVirtualKey());
            double last = sw.Elapsed.TotalSeconds;
            double next = last;
            double lastVx = 0.0;
            double lastVy = 0.0;
            double lastMoveX = double.NegativeInfinity;
            double lastMoveY = double.NegativeInfinity;

            while (_running)
            {
                double now = sw.Elapsed.TotalSeconds;
                double dt = now - last;
                last = now;
                if (dt <= 0) dt = 0.001;
                if (dt > 0.05) dt = 0.05;

                bool toggleDown = IsKeyDown(GetToggleVirtualKey());
                if (toggleDown && !_togglePrevious && _raw != null)
                {
                    PostMessage(_raw.Handle, 0x8001, IntPtr.Zero, IntPtr.Zero);
                }
                _togglePrevious = toggleDown;

                int dx = Interlocked.Exchange(ref _pendingDx, 0);
                int dy = Interlocked.Exchange(ref _pendingDy, 0);

                double holdSeconds = _config.HoldMs / 1000.0;
                if (dx != 0)
                {
                    lastVx = dx / _config.XDpi / dt; // inches/second
                    lastMoveX = now;
                }
                else if (now - lastMoveX > holdSeconds)
                {
                    lastVx = 0;
                }
                if (dy != 0)
                {
                    lastVy = dy / _config.YDpi / dt;
                    lastMoveY = now;
                }
                else if (now - lastMoveY > holdSeconds)
                {
                    lastVy = 0;
                }

                double vx = lastVx;
                double vy = lastVy;
                if (_config.InvertY) vy = -vy;

                double rawX = vx / _config.FullSpeedInchesPerSecond * _config.Sensitivity * _config.XSensitivity;
                double rawY = -vy / _config.FullSpeedInchesPerSecond * _config.Sensitivity * _config.YSensitivity;
                double targetY = 0.0;
                double targetLt = 0.0;
                double targetRt = 0.0;
                double absX = Math.Abs(rawX);
                double absY = Math.Abs(rawY);

                if (absX > _config.DeadzoneX)
                {
                    double activeX = ApplyDeadzone(absX, _config.DeadzoneX);
                    activeX = Clamp(activeX + _config.BoostX, 0, 1);
                    if (rawX < 0) targetLt = activeX;
                    else targetRt = activeX;
                }

                if (absY > _config.DeadzoneY)
                {
                    double mappedY = ApplyDeadzone(rawY, _config.DeadzoneY);
                    double direction = mappedY >= 0 ? 1.0 : -1.0;
                    targetY = Clamp(mappedY + direction * _config.BoostY, -1, 1);
                }

                if (!_enabled)
                {
                    targetY = 0;
                    targetLt = 0;
                    targetRt = 0;
                }

                _currentY = targetY;
                _currentLt = targetLt;
                _currentRt = targetRt;

                if (_pad != null)
                {
                    try
                    {
                        XInputGamepad original;
                        ReadOriginalState(out original);
                        SubmitMergedPad(original);
                    }
                    catch (Exception ex)
                    {
                        Log("Pad submit failed: " + ex.Message);
                    }
                }

                double period = 1.0 / Math.Max(60, _config.PollHz);
                next += period;
                double remaining = next - sw.Elapsed.TotalSeconds;
                if (remaining > 0.002) Thread.Sleep((int)(remaining * 1000.0));
                else if (remaining > 0) Thread.SpinWait(50);
                else next = sw.Elapsed.TotalSeconds;
            }

            if (_pad != null)
            {
                try
                {
                    _pad.SetAxisValue(Xbox360Axis.LeftThumbY, 0);
                    _pad.SetSliderValue(Xbox360Slider.LeftTrigger, 0);
                    _pad.SetSliderValue(Xbox360Slider.RightTrigger, 0);
                    _pad.SubmitReport();
                }
                catch { }
            }
        }
        private static double ApplyDeadzone(double value, double deadzone)
        {
            double a = Math.Abs(value);
            if (a <= deadzone) return 0;
            double v = (a - deadzone) / Math.Max(0.0001, 1.0 - deadzone);
            return value < 0 ? -Math.Min(1, v) : Math.Min(1, v);
        }

        private static double Clamp(double v, double lo, double hi)
        {
            if (v < lo) return lo;
            if (v > hi) return hi;
            return v;
        }

        private static void RotateLog()
        {
            try
            {
                if (File.Exists(_logPath) && new FileInfo(_logPath).Length > 2 * 1024 * 1024)
                    File.Copy(_logPath, _logPath + ".old", true);
                File.WriteAllText(_logPath, "", new UTF8Encoding(false));
            }
            catch { }
        }

        private static void Log(string message)
        {
            lock (LogLock)
            {
                try { File.AppendAllText(_logPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + message + Environment.NewLine, new UTF8Encoding(false)); }
                catch { }
            }
        }

        private static void Cleanup()
        {
            _running = false;
            // The toggle key is polled; no hotkey needs to be unregistered.
            if (_tray != null)
            {
                try { _tray.Visible = false; _tray.Dispose(); } catch { }
                _tray = null;
            }
            if (_worker != null && _worker.IsAlive)
            {
                try { _worker.Join(500); } catch { }
            }
            if (_pad != null)
            {
                try { _pad.Disconnect(); } catch { }
                _pad = null;
            }
            if (_client != null)
            {
                try { _client.Dispose(); } catch { }
                _client = null;
            }
            if (_raw != null)
            {
                try { _raw.DestroyHandle(); } catch { }
                _raw = null;
            }
            if (_mutex != null)
            {
                try { _mutex.ReleaseMutex(); } catch { }
                try { _mutex.Dispose(); } catch { }
                _mutex = null;
            }
        }

        private static int GetToggleVirtualKey()
        {
            if (!string.IsNullOrEmpty(_config.ToggleKey) && _config.ToggleKey.Equals("F12", StringComparison.OrdinalIgnoreCase)) return 0x7B;
            if (!string.IsNullOrEmpty(_config.ToggleKey) && _config.ToggleKey.Equals("F11", StringComparison.OrdinalIgnoreCase)) return 0x7A;
            return VkF10;
        }

        private static void ReadOriginalState(out XInputGamepad gamepad)
        {
            gamepad = new XInputGamepad();
            if (_config.SourceXInputSlot >= 0)
            {
                XInputGamepad explicitState;
                if (TryReadXInput((uint)_config.SourceXInputSlot, out explicitState))
                {
                    gamepad = explicitState;
                    LogSourceSlot(_config.SourceXInputSlot);
                }
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                if (i == _padUserIndex) continue;
                XInputGamepad candidate;
                if (TryReadXInput((uint)i, out candidate))
                {
                    gamepad = candidate;
                    LogSourceSlot(i);
                    return;
                }
            }
        }

        private static void LogSourceSlot(int slot)
        {
            if (slot == _lastSourceSlot) return;
            _lastSourceSlot = slot;
            Log("Source XInput slot=" + slot);
        }
        private static bool TryReadXInput(uint index, out XInputGamepad gamepad)
        {
            XInputState state = new XInputState();
            int result;
            try
            {
                result = XInputGetState14(index, ref state);
            }
            catch
            {
                try { result = XInputGetState910(index, ref state); }
                catch { result = -1; }
            }
            if (result == 0)
            {
                gamepad = state.Gamepad;
                return true;
            }
            gamepad = new XInputGamepad();
            return false;
        }

        private static void SubmitMergedPad(XInputGamepad original)
        {
            ushort buttons = original.Buttons;
            _pad.SetButtonState(Xbox360Button.Up, (buttons & 0x0001) != 0);
            _pad.SetButtonState(Xbox360Button.Down, (buttons & 0x0002) != 0);
            _pad.SetButtonState(Xbox360Button.Left, (buttons & 0x0004) != 0);
            _pad.SetButtonState(Xbox360Button.Right, (buttons & 0x0008) != 0);
            _pad.SetButtonState(Xbox360Button.Start, (buttons & 0x0010) != 0);
            _pad.SetButtonState(Xbox360Button.Back, (buttons & 0x0020) != 0);
            _pad.SetButtonState(Xbox360Button.LeftThumb, (buttons & 0x0040) != 0);
            _pad.SetButtonState(Xbox360Button.RightThumb, (buttons & 0x0080) != 0);
            _pad.SetButtonState(Xbox360Button.LeftShoulder, (buttons & 0x0100) != 0);
            _pad.SetButtonState(Xbox360Button.RightShoulder, (buttons & 0x0200) != 0);
            _pad.SetButtonState(Xbox360Button.A, (buttons & 0x1000) != 0);
            _pad.SetButtonState(Xbox360Button.B, (buttons & 0x2000) != 0);
            _pad.SetButtonState(Xbox360Button.X, (buttons & 0x4000) != 0);
            _pad.SetButtonState(Xbox360Button.Y, (buttons & 0x8000) != 0);

            int mouseY = (int)Math.Round(_currentY * 32767.0);
            int mouseLt = (int)Math.Round(_currentLt * 255.0);
            int mouseRt = (int)Math.Round(_currentRt * 255.0);

            _pad.SetAxisValue(Xbox360Axis.LeftThumbX, original.LeftThumbX);
            _pad.SetAxisValue(Xbox360Axis.LeftThumbY, ClampToShort(original.LeftThumbY + mouseY));
            _pad.SetAxisValue(Xbox360Axis.RightThumbX, original.RightThumbX);
            _pad.SetAxisValue(Xbox360Axis.RightThumbY, original.RightThumbY);
            _pad.SetSliderValue(Xbox360Slider.LeftTrigger, ClampToByte(original.LeftTrigger + mouseLt));
            _pad.SetSliderValue(Xbox360Slider.RightTrigger, ClampToByte(original.RightTrigger + mouseRt));
            _pad.SubmitReport();
        }

        private static short ClampToShort(int value)
        {
            if (value > 32767) return 32767;
            if (value < -32768) return -32768;
            return (short)value;
        }

        private static byte ClampToByte(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)value;
        }
        private static bool IsKeyDown(int virtualKey)
        {
            return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
        }
        private struct XInputGamepad
        {
            public ushort Buttons;
            public byte LeftTrigger;
            public byte RightTrigger;
            public short LeftThumbX;
            public short LeftThumbY;
            public short RightThumbX;
            public short RightThumbY;
        }

        private struct XInputState
        {
            public uint PacketNumber;
            public XInputGamepad Gamepad;
        }
        private struct RawInputHeader
        {
            public uint Type;
            public uint Size;
            public IntPtr Device;
            public IntPtr WParam;
        }

        private struct RawMouse
        {
            public ushort Flags;
            public ushort Padding;
            public ushort ButtonFlags;
            public ushort ButtonData;
            public uint RawButtons;
            public int LastX;
            public int LastY;
            public uint ExtraInformation;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputData(IntPtr rawInput, uint command, IntPtr data, ref uint size, uint headerSize);

        [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
        private static extern int XInputGetState14(uint index, ref XInputState state);

        [DllImport("xinput9_1_0.dll", EntryPoint = "XInputGetState")]
        private static extern int XInputGetState910(uint index, ref XInputState state);
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int virtualKey);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
    }
}

































