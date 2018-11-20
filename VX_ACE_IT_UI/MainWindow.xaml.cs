using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using VX_ACE_IT_CORE;
using VX_ACE_IT_CORE.MVC.Model.GameWindow;
using VX_ACE_IT_CORE.MVC._Common;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.Offsets;
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_TYPES;

namespace VX_ACE_IT_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Config config;
        private Core _core;
        private BaseDebug debug = new BaseDebug();
        private UIElement _processListDefaultItem = new UIElement();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            config = new Config(debug);

            new Task(() =>
            {
                if (!config.ConfigVariables.IsInitial)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        CloseAllDialogs();
                        RootLogicStackPanel.Visibility = Visibility.Visible;
                        ForceExitButtonsColors();
                    });
                }
            }); // .Start() if no config && is initial is chosen, but it´s annoying for debug, so implement later

            _core = new Core(debug, config);
            SubscribeEvents(_core);

            //AutoFetch is so annoying...
            // _core._controller.GameProcess.FetchProcess(config.ConfigVariables.ProcessName);}}

            _processListDefaultItem = ProcessListItemDefault;

            // Just call the method to init found proceses.
            WelcomeProcessNameTextBox_OnTextChanged(this.WelcomeProcessNameTextBox, null);

            InitUiFromConfig();
        }

        private void InitUiFromConfig()
        {
            config.LoadXmlConfig();

            WelcomeProcessNameTextBox.Text = config.ConfigVariables.ProcessName;
            WelcomeResolution.Text = config.ConfigVariables.Width + "x" + config.ConfigVariables.Height;
            WelcomeBorder.IsChecked = config.ConfigVariables.IsWindowBorder;
        }

        private void SubscribeEvents(Core core)
        {
            core._controller.GameProcess.OnProcessFound += OnProcessFound;
            core._controller.GameProcess.OnNoProcessFound += OnNoProcessFound;
            core._controller.GameProcess.OnKill += GameProcess_OnKill;
        }

        private void GameProcess_OnKill(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ShowWelcomeScreen();
                MainWindow_Loaded(this, null);
                this.WelcomeProcessNameTextBox.Text = config.ConfigVariables.ProcessName.Split('_').Length > 1
                    ? config.ConfigVariables.ProcessName.Split('_').First() : config.ConfigVariables.ProcessName;
            });
        }

        private void OnNoProcessFound(object sender, EventArgs eventArgs)
        {
            ShowWelcomeScreen();
        }

        private void OnProcessFound(object sender, EventArgs eventArgs)
        {
            CloseAllDialogs();
            ShowDefaultRootLogicStackPanel();
        }

        private void ShowWelcomeScreen()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                RootLogicStackPanel.Visibility = Visibility.Collapsed;
                DimmGrid.Visibility = Visibility.Visible;
                WelcomeStackPanel.Visibility = Visibility.Visible;
            });
        }

        private void InjectButton_Click(object sender, RoutedEventArgs e)
        {
            _core._controller.SetWindowPosFromConfig();
            _core._controller.SetWindowStyle(config.ConfigVariables.IsWindowBorder ? WindowStyles.NoBorder : WindowStyles.Border);
        }

        private void CloseAllDialogs()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                WelcomeStackPanel.Visibility = Visibility.Collapsed;
                DimmGrid.Visibility = Visibility.Collapsed;
                WelcomeSettingsCardErrorCard.Visibility = Visibility.Collapsed;
            });
        }

        private void ShowDefaultRootLogicStackPanel()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                RootLogicStackPanel.Visibility = Visibility.Visible;
                ForceExitButtonsColors();
            });
        }

        private void WelcomeConfigButton_Click(object sender, RoutedEventArgs e)
        {
            int width = 0, height = 0;
            if (WelcomeResolution.Text != null)
            {
                var text = (WelcomeResolution.SelectedItem as ComboBoxItem)?.Content.ToString() ??
                      WelcomeResolution.Text;
                string re1 = "(\\d+)";  // Integer Number 1
                string re2 = "(.)"; // Any Single Character 1
                string re3 = "(\\d+)";  // Integer Number 2

                Regex r = new Regex(re1 + re2 + re3, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                Match m = r.Match(text);
                if (m.Success)
                {
                    width = Math.Abs(int.Parse(m.Groups[1].Value));
                    height = Math.Abs(int.Parse(m.Groups[3].Value));
                }
            }
            if (width != 0 && height != 0)
            {
                config.ConfigVariables = new ConfigVariables()
                {
                    Width = width,
                    Height = height,
                    IsInitial = false,
                    IsWindowBorder = WelcomeBorder.IsChecked.Value,
                    ProcessName = WelcomeProcessNameTextBox.Text
                };
                config.ReplaceXmlConfig();
                _core._controller.GameProcess.FetchProcess(config.ConfigVariables.ProcessName);
            }
            else
            {
                WelcomeSettingsCardErrorCard.Visibility = Visibility.Visible;
            }
        }

        private void ExitMinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void ForceExitButtonsColors()
        {
            if (DimmGrid.Visibility != Visibility.Visible)
            {
                ExitCloseButton.Foreground = Brushes.Black;
                ExitMinimizeButton.Foreground = Brushes.Black;
            }
            else
            {
                ExitCloseButton.Foreground = Brushes.White;
                ExitMinimizeButton.Foreground = Brushes.White;
            }
        }

        private void ExitCloseButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.Visibility = Visibility.Collapsed;

            new Task(() =>
            {
                Thread.Sleep(10);
                Environment.Exit(0);
            }).Start();

        }

        private void RPMButton_OnClick(object sender, RoutedEventArgs e)
        {

            //debug.AddMessage<object>(new Message<object>(
            //    //"AdressValue: " + _core._controller.ProcessMethods.RPM<int>(new IntPtr(0x0F6532D0)) +""
            //    "AdressValue: " + _core._controller.ProcessMethods.Rpm<int>(new IntPtr(Convert.ToUInt32(AdressTextBox.Text, 16))) + ""
            //    ));
            //int address = Convert.ToInt32(AdressTextBox.Text,16);
            new Task(() =>
            {
                // Terraria cheatsheetTest: base: "THREADSTACK0"-00000FB8 + 0x54 + 0x24 + 0xEC + F0 + 388
                while (false)
                {
                    Thread.Sleep(22);
                    int i = _core._controller.ProcessMethods.Rpm<int>(
                        _core._controller.PluginService.Plugins.First().ModuleBaseAddr,
                        new List<IntPtr>(){new IntPtr(0x25A8B0), new IntPtr(0x30), new IntPtr(0x18), new IntPtr(0x20), new IntPtr(0x38)}, out var intPtr);
                    // <- rpgmaker_vx_ace 4:1.                                                                                                                                     
                    //  debug.AddMessage<object>(new Message<object>(                                                                               
                    //      "AdressValue: engine[" + new Numeric<int>(i).EngineValue + "] actual[" + new Numeric<int>(i).ActualValue + "]"
                    debug.AddMessage<object>(new Message<object>(i+"bs" + _core._controller.PluginService.Plugins.First().ModuleBaseAddr));                                                                                 
                    //  ));
                    // if (i != 0) _core._controller.ProcessMethods.Wpm<int>(
                    //      _core._controller.VxAceModule.RgssBase, new Numeric<int>(250, true).EngineValue
                    //      , new List<IntPtr>() { new IntPtr(0x25A8B0), new IntPtr(0x30), new IntPtr(0x18), new IntPtr(0x20), new IntPtr(0x38) });}}
                }
            }).Start();

        }

        private void ListBoxItem_OnSelected(object sender, RoutedEventArgs e)
        {
            WelcomeProcessNameTextBox.Text = (sender as ListBoxItem)?.Content.ToString()
                                             ?? throw new InvalidOperationException("ListBox is null (UI-Welcome)");
        }



        private void WelcomeProcessNameTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (!(ProcessListListBox is null))
                {
                    ProcessListListBox.Items.Clear();
                    string processName = (sender as TextBox)?.Text ?? "";

                    foreach (var process in Process.GetProcessesByName(processName))
                    {
                        if (!Is64BitProcess(process))
                        {
                            UIElement element = CloneUIElement(_processListDefaultItem);
                            ((ListBoxItem)element).Content = process.ProcessName + "_" + process.Id;
                            ((ListBoxItem)element).Visibility = Visibility.Visible;
                            ((ListBoxItem)element).Selected += ListBoxItem_OnSelected;
                            ProcessListListBox.Items.Add(element);
                        }
                        else
                        {
                            UIElement element = CloneUIElement(_processListDefaultItem);
                            ((ListBoxItem)element).Content = process.ProcessName + "-x64-" + process.Id;
                            ((ListBoxItem)element).Visibility = Visibility.Visible;
                            ((ListBoxItem)element).Selected += ListBoxItem_OnSelected;
                            ((ListBoxItem)element).Background = Brushes.LightCoral;
                            ((ListBoxItem)element).ToolTip = "Not selectable, \nProcess is x64 => not compatible";
                            ((ListBoxItem)element).Focusable = false;
                            ProcessListListBox.Items.Add(element);
                        }
                    }

                    ProcessListExpander.IsExpanded = Process.GetProcessesByName(processName).Length > 0;
                }
            });
        }

        #region SharedCommonLogic
        /// <summary>
        /// Clones UIElement via xaml reader/writer.
        /// </summary>
        /// <param name="uiElement"></param>
        /// <returns></returns>
        private UIElement CloneUIElement(UIElement uiElement)
        {
            var xamlElement = XamlWriter.Save(uiElement);
            var xamlString = new StringReader(xamlElement);
            var xmlTextReader = new XmlTextReader(xamlString);
            return (UIElement)XamlReader.Load(xmlTextReader);
        }

        private bool Is64BitProcess(Process process)
        {
            if (!Environment.Is64BitOperatingSystem)
                return false;

            if (!IsWow64Process(process.Handle, out var isWow64Process))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return !isWow64Process;
        }

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr processHandle, [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process);
        #endregion



    }
}
