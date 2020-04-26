using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using Microsoft.Extensions.Configuration;
using VX_ACE_IT_CORE;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC._Common;
using VX_ACE_IT_CORE.MVC.Model.Configuration;
using VX_ACE_IT_CORE.MVC.Model.GameWindow;
using VX_ACE_IT_CORE.MVC.Model.Plugins;
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_TYPES;
using VX_ACE_IT_UI_CORE;

namespace VX_ACE_IT_UI_CORE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Config _config;
        private Core _core;
        private readonly BaseDebug _debug = new BaseDebug();
        private UIElement _processListDefaultItem = new UIElement();



        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _config = new Config(_debug);
            new Task(() =>
            {
               
                if (!_config.Configuration.GetSection(nameof(AppCfg)).Get<AppCfg>().IsInitial)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CloseAllDialogs();
                        RootLogicStackPanel.Visibility = Visibility.Visible;
                        ForceExitButtonsColors();
                    });
                }
            }); // .Start() if no config && is initial is chosen, but it´s annoying for debug, so implement later

            _core = new Core(_debug, _config);
            SubscribeEvents(_core);

            //AutoFetch is so annoying...
            // _core._controller.GameProcess.FetchProcess(config.ConfigVariables.ProcessName);}}

            _processListDefaultItem = ProcessListItemDefault;

            // Just call the method to init found proceses.
            WelcomeProcessNameTextBox_OnTextChanged(WelcomeProcessNameTextBox, null);

            InitUiFromConfig();
        }

        private void InitUiFromConfig()
        {
            //_config.LoadXmlConfig();

            var pluginsCfg =  _config.Configuration.GetSection(nameof(PluginsCfg)).Get<PluginsCfg>();
            var window =  _config.Configuration.GetSection(nameof(GameWindowCfg)).Get<GameWindowCfg>();
            WelcomeProcessNameTextBox.Text = pluginsCfg.DefaultProcessName;
            WelcomeResolution.Text = window.Width + "x" + window.Height;
            foreach (var windowDefaultResolution in window.DefaultResolutions)
            {
                WelcomeResolution.Items.Add(new ComboBoxItem(){ Content = windowDefaultResolution });
            }
            WelcomePlugin.Text = pluginsCfg.DefaultPluginName;
            foreach (var plugin in pluginsCfg.Plugins)
            {
                WelcomePlugin.Items.Add(new ComboBoxItem(){ Content = plugin.Name });
            }
            WelcomeBorder.IsChecked = window.IsWindowBorderVisible;
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
                var processCfg = _config.Configuration.GetSection(nameof(GameProcessCfg)).Get<GameProcessCfg>();
                WelcomeProcessNameTextBox.Text = processCfg.ProcessName.Split('_').Length > 1
                    ? processCfg.ProcessName.Split('_').First() : processCfg.ProcessName;
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                RootLogicStackPanel.Visibility = Visibility.Collapsed;
                DimmGrid.Visibility = Visibility.Visible;
                WelcomeStackPanel.Visibility = Visibility.Visible;
            });
        }

        private void InjectButton_Click(object sender, RoutedEventArgs e)
        {
            _core._controller.SetWindowPosFromConfig();
            var window = _config.Configuration.GetSection(nameof(GameWindowCfg)).Get<GameWindowCfg>();
            _core._controller.SetWindowStyle(window.IsWindowBorderVisible ? WindowStyles.NoBorder : WindowStyles.Border);
        }

        private void CloseAllDialogs()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                WelcomeStackPanel.Visibility = Visibility.Collapsed;
                DimmGrid.Visibility = Visibility.Collapsed;
                WelcomeSettingsCardErrorCard.Visibility = Visibility.Collapsed;
            });
        }

        private void ShowDefaultRootLogicStackPanel()
        {
            Application.Current.Dispatcher.Invoke(() =>
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
                const string re1 = "(\\d+)"; // Integer Number 1
                const string re2 = "(.)"; // Any Single Character 1
                const string re3 = "(\\d+)"; // Integer Number 2

                var r = new Regex(re1 + re2 + re3, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var m = r.Match(text);
                if (m.Success)
                {
                    width = Math.Abs(int.Parse(m.Groups[1].Value));
                    height = Math.Abs(int.Parse(m.Groups[3].Value));
                }
            }
            if (width != 0 && height != 0)
            {
                var processCfg = _config.Configuration.GetSection(nameof(GameProcessCfg)).Get<GameProcessCfg>();
                _core._controller.GameProcess.FetchProcess(processCfg.ProcessName);
            }
            else
            {
                WelcomeSettingsCardErrorCard.Visibility = Visibility.Visible;
            }
        }

        private void ExitMinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
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
            Application.Current.MainWindow.Visibility = Visibility.Collapsed;

            Dispatcher.Invoke(() =>
            {
                //_core._controller?.GameOverlayPlugin?.Overlay?.BeforeDispose();
            });

            new Task(() =>
            {
                Thread.Sleep(10);
                Environment.Exit(0);
            }).Start();

        }

        private void RPMButton_OnClick(object sender, RoutedEventArgs e)
        {
            //_core._controller.GameOverlayPlugin.StartDemo(_core._controller.GameProcess.Process, Application.Current.Dispatcher);

            //debug.AddMessage<object>(new Message<object>(
            //    //"AdressValue: " + _core._controller.ProcessMethods.RPM<int>(new IntPtr(0x0F6532D0)) +""
            //    "AdressValue: " + _core._controller.ProcessMethods.Rpm<int>(new IntPtr(Convert.ToUInt32(AdressTextBox.Text, 16))) + ""
            //    ));
            //int address = Convert.ToInt32(AdressTextBox.Text,16);
            _debug.AddMessage<object>(new Message<object>((_core._controller.PluginService.Plugins.FirstOrDefault()?.UpdatableTypes.FirstOrDefault() as UpdatableType<Player>)?.Type.ToString()));
            (_core._controller.PluginService.Plugins.First().UpdatableTypes.First() as UpdatableType<Player>)?.SetValue("Hp", new Numeric<int>(460, true).EngineValue);
            new Task(() =>
            {
                // Terraria cheatsheetTest: base: "THREADSTACK0"-00000FB8 + 0x54 + 0x24 + 0xEC + F0 + 388
                while (false)
                {
                    Thread.Sleep(22);
                    var i = _core._controller.ProcessMethods.Rpm<int>(
                        _core._controller.PluginService.Plugins.First().ModuleBaseAddr,
                        new List<IntPtr>() { new IntPtr(0x25A8B0), new IntPtr(0x30), new IntPtr(0x18), new IntPtr(0x20), new IntPtr(0x38) }, out var intPtr);
                    // <- rpgmaker_vx_ace 4:1.                                                                                                                                     
                    //  debug.AddMessage<object>(new Message<object>(                                                                               
                    //      "AdressValue: engine[" + new Numeric<int>(i).EngineValue + "] actual[" + new Numeric<int>(i).ActualValue + "]"
                    _debug.AddMessage<object>(new Message<object>(i + "bs" + _core._controller.PluginService.Plugins.First().ModuleBaseAddr));
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
                    var processName = (sender as TextBox)?.Text ?? "";

                    foreach (var process in System.Diagnostics.Process.GetProcessesByName(processName))
                    {
                        if (!Is64BitProcess(process))
                        {
                            var element = CloneUIElement(_processListDefaultItem);
                            ((ListBoxItem)element).Content = process.ProcessName + "_" + process.Id;
                            ((ListBoxItem)element).Visibility = Visibility.Visible;
                            ((ListBoxItem)element).Selected += ListBoxItem_OnSelected;
                            ProcessListListBox.Items.Add(element);
                        }
                        else
                        {
                            var element = CloneUIElement(_processListDefaultItem);
                            ((ListBoxItem)element).Content = process.ProcessName + "-x64-" + process.Id;
                            ((ListBoxItem)element).Visibility = Visibility.Visible;
                            ((ListBoxItem)element).Selected += ListBoxItem_OnSelected;
                            ((ListBoxItem)element).Background = Brushes.LightCoral;
                            ((ListBoxItem)element).ToolTip = "Not selectable, \nProcess is x64 => not compatible";
                            ((ListBoxItem)element).Focusable = false;
                            ProcessListListBox.Items.Add(element);
                        }
                    }

                    ProcessListExpander.IsExpanded = System.Diagnostics.Process.GetProcessesByName(processName).Length > 0;
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

        private bool Is64BitProcess(System.Diagnostics.Process process)
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

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            /*var shape = new Rectangle()
            {
                Visibility = Visibility.Visible,
                Name = "rectiongle",
                Height = 50,
                Width = 50,
                Fill = Brushes.BurlyWood,
                Focusable = true,
                ToolTip = "hey, im over",
            };
            App.Current.Dispatcher.Invoke(() =>
            {
                TranslateTransform transform = new TranslateTransform(shape.RenderTransform.Value.M21, shape.RenderTransform.Value.M22);
                _core._controller.Keyboard.Interceptor.KeyDown += (o, args) =>
                {
                    //var n = shape.RenderTransform.Value.M12;
                    //UIElement uiElement = _core._controller.GameOverlayPlugin.Overlay.GetUiElement("rectiongle");
                    transform.X += 5;
                    uiElement.RenderTransform = transform;
                };
                //Mouse events will not work in current impl.
                shape.MouseEnter += (o, args) =>
                    _debug.AddMessage<object>(new Message<object>(shape.GetType() + " was clicked."));
                //_core._controller.GameOverlayPlugin.Overlay.AddShape(shape);
                //_core._controller.GameOverlayPlugin.Overlay.AddEvent(shape, () => _debug.AddMessage<object>(new Message<object>(shape.GetType() + " was clicked.")), Dispatcher.CurrentDispatcher);
            });
            */
        }
    }
}