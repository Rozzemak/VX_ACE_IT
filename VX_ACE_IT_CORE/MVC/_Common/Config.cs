using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace VX_ACE_IT_CORE.MVC._Common
{
    [XmlRoot("ConfigVariables")]
    public class ConfigVariables : INotifyPropertyChanged
    {
        private int _width = 1280;
        private int _height = 720;

        private bool _isWindowBorder = true;
        private bool _isCheckForUpdates = false;
        private bool _forceRes = true;

        private bool _isInitial = true;

        private string _processName = "game";


        #region GetSetProps

        public bool IsWindowBorder
        {
            get { return _isWindowBorder; }
            set
            {
                if (_isWindowBorder != value)
                {
                    _isWindowBorder = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsForceRes
        {
            get { return _forceRes; }
            set
            {
                if (_forceRes != value)
                {
                    _forceRes = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsCheckForUpdates
        {
            get { return _isCheckForUpdates; }
            set
            {
                if (_isCheckForUpdates != value)
                {
                    _isCheckForUpdates = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ProcessName
        {
            get { return _processName; }
            set
            {
                if (_processName != value)
                {
                    _processName = value;
                    OnPropertyChanged();
                }
            }
        }
        public int Width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }
        public int Height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsInitial
        {
            get { return _isInitial; }
            set
            {
                if (_isInitial != value)
                {
                    _isInitial = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public ConfigVariables()
        {

        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Config
    {

        public ConfigVariables ConfigVariables = new ConfigVariables();

        public const string ConfigFileName = "Config.xml";

        public Config(int width = 1280, int height = 720, string processName = "game", bool windowBorder = true, bool forceRes = true)
        {
            if (File.Exists(ConfigFileName) && CheckConfigIntegrity())
            {
                LoadXmlConfig();
                ConfigVariables.IsInitial = false;
            }
            else
            {
                ConfigVariables.Width = width;
                ConfigVariables.Height = height;
                ConfigVariables.ProcessName = processName;
                ConfigVariables.IsWindowBorder = windowBorder;
                ConfigVariables.IsForceRes = forceRes;
                ReplaceXmlConfig();
            }
            ConfigVariables.PropertyChanged += ConfigVariables_PropertyChanged;
        }

        public Config(bool diff)
        {
            if (File.Exists(ConfigFileName) && CheckConfigIntegrity())
            {
                LoadXmlConfig();
            }
            else
            {
                CreateAndSaveDefaultXmlConfig();
            }
            ConfigVariables.PropertyChanged += ConfigVariables_PropertyChanged;
        }

        private void ConfigVariables_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ReplaceXmlConfig();
        }

        public void CreateAndSaveDefaultXmlConfig()
        {
            XmlSerializer x = new XmlSerializer(typeof(ConfigVariables));
            TextWriter writer = new StreamWriter(ConfigFileName, false);
            ConfigVariables = new ConfigVariables();
            x.Serialize(writer, ConfigVariables);
            writer.Close();
        }

        public void ReplaceXmlConfig()
        {
            XmlSerializer x = new XmlSerializer(typeof(ConfigVariables));
            TextWriter writer = new StreamWriter(ConfigFileName, false);
            x.Serialize(writer, this.ConfigVariables);
            writer.Close();
        }

        public void LoadXmlConfig()
        {
            if (CheckConfigIntegrity())
            {
                XmlSerializer cvars = new XmlSerializer(typeof(ConfigVariables));
                XmlReader reader = XmlReader.Create(ConfigFileName);
                // Just whatever the exception is, the xml file is damaged, user will be asked to remove it.
                try
                {
                    ConfigVariables = (ConfigVariables)cvars.Deserialize(reader);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Your Config.xml is damaged.\nRemove it.");
                    throw;
                }
            }
            else
            {
                MessageBox.Show("Your Config.xml is damaged.\n It is goona be rapla it.");

            }
        }

        public bool CheckConfigIntegrity()
        {
            XmlSerializer x = new XmlSerializer(typeof(ConfigVariables));
            XmlReader reader = XmlReader.Create(ConfigFileName);
            return x.CanDeserialize(reader);
        }

    }
}