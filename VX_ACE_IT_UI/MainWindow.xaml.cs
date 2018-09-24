using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VX_ACE_IT_CORE;
using VX_ACE_IT_CORE.MVC.Model.GameWindow;
using VX_ACE_IT_CORE.MVC._Common;
using VX_ACE_IT_CORE.Debug;

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

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            config = new Config(debug);

            App.Current.Dispatcher.Invoke(() =>
            {
                if (!config.ConfigVariables.IsInitial)
                {
                    CloseAllDialogs();
                    RootLogicStackPanel.Visibility = Visibility.Visible;
                }
                ForceExitButtonsColors();
            });
            _core = new Core(debug, config);
            SubscribeEvents(_core);            

            _core._controller.GameProcess.FetchProcess(config.ConfigVariables.ProcessName);
        }

        private void SubscribeEvents(Core core)
        {
            core._controller.GameProcess.OnProcessFound += OnProcessFound;
            core._controller.GameProcess.OnNoProcessFound += OnNoProcessFound;
            core._controller.GameProcess.OnKill += GameProcess_OnKill;
        }

        private void GameProcess_OnKill(object sender, EventArgs e)
        {
            ShowWelcomeScreen();
            MainWindow_Loaded(this, null);
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
        }

        private void CloseAllDialogs()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                WelcomeStackPanel.Visibility = Visibility.Collapsed;
                DimmGrid.Visibility = Visibility.Collapsed;
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
                    height  = Math.Abs(int.Parse(m.Groups[3].Value));
                }
            }
            if (width != 0 && height != 0)
            {
                config.ConfigVariables = new ConfigVariables(){Width = width, Height = height,
                    IsInitial = false, IsWindowBorder = WelcomeBorder.IsChecked.Value, ProcessName = WelcomeProcessNameTextBox.Text};
                config.ReplaceXmlConfig();
                CloseAllDialogs();
                _core._controller.GameProcess.FetchProcess(config.ConfigVariables.ProcessName);
                //core._controller.SetWindowStyle(config.ConfigVariables.IsWindowBorder ? WindowStyles.NoBorder : WindowStyles.Border);
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
                ExitCloseButton.Foreground = Brushes.White;
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
            debug.AddMessage<object>(new Message<object>(
                //"AdressValue: " + _core._controller.ProcessMethods.RPM<int>(new IntPtr(0x0F6532D0)) +""
                "AdressValue: " + _core._controller.ProcessMethods.RPM(new IntPtr(0x0F6532D0)) + ""
                )); 
        }
    }
}
