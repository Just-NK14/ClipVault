using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using NHotkey;
using NHotkey.Wpf;

using TextBox = System.Windows.Controls.TextBox;
using Forms = System.Windows.Forms;

namespace ClipVault
{
    public partial class MainWindow : Window
    {
        // OS level overrides for instant modifier release
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        const byte VK_MENU = 0x12;
        const byte VK_CONTROL = 0x11;
        const uint KEYEVENTF_KEYUP = 0x0002;

        private Forms.NotifyIcon _notifyIcon = null!;

        private readonly string _saveFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NoeticKit", "ClipVault", "vault_data.json");

        public MainWindow()
        {
            InitializeComponent();
            SetupHotkeys();
            SetupTrayIcon();
            LoadFromDisk();
        }

        private void SetupTrayIcon()
        {
            _notifyIcon = new Forms.NotifyIcon();
            string iconPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "NoeticKit.ico"
            );
            _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "ClipVault";
            _notifyIcon.DoubleClick += (s, e) => ShowApp();

            _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Open ClipVault", null, (s, e) => ShowApp());
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => {
                SaveToDisk();
                _notifyIcon.Dispose();
                System.Windows.Application.Current.Shutdown();
            });
        }

        // --- PERSISTENCE LOGIC ---
        private void SaveToDisk()
        {
            try
            {
                string? directory = Path.GetDirectoryName(_saveFilePath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string[] dataToSave = new string[10];
                for (int i = 0; i <= 9; i++)
                {
                    dataToSave[i] = GetTextBoxForSlot(i).Text;
                }

                string jsonString = JsonSerializer.Serialize(dataToSave);
                File.WriteAllText(_saveFilePath, jsonString);
            }
            catch (Exception) { /* Fail silently */ }
        }

        private void LoadFromDisk()
        {
            try
            {
                if (File.Exists(_saveFilePath))
                {
                    string jsonString = File.ReadAllText(_saveFilePath);
                    string[]? loadedData = JsonSerializer.Deserialize<string[]>(jsonString);

                    if (loadedData != null && loadedData.Length == 10)
                    {
                        for (int i = 0; i <= 9; i++)
                        {
                            GetTextBoxForSlot(i).Text = loadedData[i];
                        }
                    }
                }
            }
            catch (Exception) { /* Fail silently */ }
        }

        private void ShowApp()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            SaveToDisk();
            this.Hide();
        }

        // --- HOTKEY & CORE ENGINE ---
        private void SetupHotkeys()
        {
            for (int i = 1; i <= 9; i++)
            {
                int slot = i;
                Key key = (Key)Enum.Parse(typeof(Key), $"D{slot}");
                HotkeyManager.Current.AddOrReplace($"Copy{slot}", key, ModifierKeys.Alt, (s, e) => HandleCopy(slot, e));
                HotkeyManager.Current.AddOrReplace($"Paste{slot}", key, ModifierKeys.Alt | ModifierKeys.Control, (s, e) => HandlePaste(slot, e));
            }
            HotkeyManager.Current.AddOrReplace("Copy0", Key.D0, ModifierKeys.Alt, (s, e) => HandleCopy(0, e));
            HotkeyManager.Current.AddOrReplace("Paste0", Key.D0, ModifierKeys.Alt | ModifierKeys.Control, (s, e) => HandlePaste(0, e));
        }

        private void ReleaseModifiers()
        {
            keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
        }

        private async void HandleCopy(int slot, HotkeyEventArgs e)
        {
            e.Handled = true;
            ReleaseModifiers();
            Forms.SendKeys.SendWait("^c");
            await Task.Delay(100);

            try
            {
                if (System.Windows.Clipboard.ContainsText())
                {
                    TextBox targetBox = GetTextBoxForSlot(slot);
                    if (targetBox != null)
                    {
                        targetBox.Text = System.Windows.Clipboard.GetText();
                        SaveToDisk();
                    }
                }
            }
            catch (Exception) { }
        }

        private async void HandlePaste(int slot, HotkeyEventArgs e)
        {
            e.Handled = true;
            TextBox targetBox = GetTextBoxForSlot(slot);

            if (targetBox != null && !string.IsNullOrEmpty(targetBox.Text))
            {
                try
                {
                    System.Windows.Clipboard.SetText(targetBox.Text);
                    ReleaseModifiers();
                    await Task.Delay(50);
                    Forms.SendKeys.SendWait("^v");
                }
                catch (Exception) { }
            }
        }

        private TextBox GetTextBoxForSlot(int slot)
        {
            return slot switch
            {
                1 => Slot1TextBox,
                2 => Slot2TextBox,
                3 => Slot3TextBox,
                4 => Slot4TextBox,
                5 => Slot5TextBox,
                6 => Slot6TextBox,
                7 => Slot7TextBox,
                8 => Slot8TextBox,
                9 => Slot9TextBox,
                0 => Slot0TextBox,
                _ => null!
            };
        }
    }
}