
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;

namespace GlobalHotKeysWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;

        private uint VK;



        public MainWindow()
        {
            InitializeComponent();
        }

        private IntPtr _windowHandle;
        private HwndSource _source;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            //bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_ALT | MOD_CONTROL | MOD_SHIFT , VK_CAPITAL);

            //Console.WriteLine(success);


        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == VK)
                            {
                                StatusTextBox.Text = "检测到热键";
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }


        //功能键 [1,3] 个
        //基础键 1 个
        List<Key> funcKeys = new List<Key>();
        Key key = Key.None;


        List<Key> _funcKeys = new List<Key>();
        Key _key = Key.None;



        private void hotkeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            Key currentKey= (e.Key == Key.System ? e.SystemKey : e.Key);

            if (currentKey == Key.LeftCtrl | currentKey == Key.LeftAlt | currentKey == Key.LeftShift )
            {
                if (!funcKeys.Contains(currentKey)) funcKeys.Add(currentKey);
            }
            else if ((currentKey >= Key.A && currentKey <= Key.Z) || (currentKey >= Key.D0 && currentKey <= Key.D9) || (currentKey >= Key.NumPad0 && currentKey <= Key.NumPad9))
            {
                key = currentKey;
            }
            else
            {
                //Console.WriteLine("不支持");
            }

            string singleKey = key.ToString();
            if (key.ToString().Length > 1)
            {
                singleKey = singleKey.ToString().Replace("D", "");
            }

            if (funcKeys.Count > 0)
            {
                if (key == Key.None)
                {
                    hotkeyTextBox.Text = string.Join("+", funcKeys);
                    _funcKeys = new List<Key>();
                    _funcKeys.AddRange(funcKeys);
                    _key = Key.None;
                }
                else
                {
                    hotkeyTextBox.Text = string.Join("+", funcKeys) + "+" + singleKey;
                    _funcKeys = new List<Key>();
                    _funcKeys.AddRange(funcKeys);
                    _key = key;
                }

            }
            else
            {
                if (key != Key.None)
                {
                    hotkeyTextBox.Text = singleKey;
                    _funcKeys = new List<Key>();
                    _key = key;
                }
            }




        }

        private void hotkeyTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {

            Key currentKey = (e.Key == Key.System ? e.SystemKey : e.Key);

            if (currentKey == Key.LeftCtrl | currentKey == Key.LeftAlt | currentKey == Key.LeftShift  )
            {
                if (funcKeys.Contains(currentKey)) funcKeys.Remove(currentKey);
            }
            else if ((currentKey >= Key.A && currentKey <= Key.Z) || (currentKey >= Key.D0 && currentKey <= Key.D9) || (currentKey >= Key.F1 && currentKey <= Key.F12))
            {
                if (currentKey == key)
                {
                    key = Key.None;
                }
                
            }


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool containsFunKey = _funcKeys.Contains(Key.LeftAlt) | _funcKeys.Contains(Key.LeftCtrl) | _funcKeys.Contains(Key.LeftShift) | _funcKeys.Contains(Key.CapsLock);


            if (!containsFunKey | _key==Key.None)
            {
                Console.WriteLine("必须为 功能键 + 数字/字母");
            }
            else
            {
                Console.WriteLine(string.Join("+", _funcKeys) + "+" + _key);

                //注册热键
                uint fsModifiers = (uint)Modifiers.None;
                foreach (Key key in _funcKeys)
                {
                    if (key == Key.LeftCtrl) fsModifiers = fsModifiers | (uint)Modifiers.Control;
                    if (key == Key.LeftAlt) fsModifiers = fsModifiers | (uint)Modifiers.Alt;
                    if (key == Key.LeftShift) fsModifiers = fsModifiers | (uint)Modifiers.Shift;
                }
                 VK = (uint)KeyInterop.VirtualKeyFromKey(_key);

                bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, fsModifiers, VK);
                Console.WriteLine(success);



            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            StatusTextBox.Text = "";
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
        }
    }
}