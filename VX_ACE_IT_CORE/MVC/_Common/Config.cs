using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.Async;


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

    public class Config : BaseAsync<object>
    {

        public ConfigVariables ConfigVariables = new ConfigVariables();

        public const string ConfigFileName = "Config.xml";

        private XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConfigVariables));
        private TextWriter _writer;
        private XmlReader _xmlReader;

        public Config(BaseDebug debug, int width = 1280, int height = 720, string processName = "game", bool windowBorder = true, bool forceRes = true)
            : base(debug, null)
        {
            AddWork(new Task<List<object>>(() =>
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
                return null;
            }));
        }

        public Config(BaseDebug debug, bool diff)
        : base(debug, null)
        {
            AddWork(new Task<List<object>>(() =>
            {
                lock (xmlSerializer)
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
                return null;
            }));
        }

        private void ConfigVariables_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ReplaceXmlConfig();
        }

        public void CreateAndSaveDefaultXmlConfig()
        {
            AddWork(new Task<List<object>>(() =>
            {
                lock (xmlSerializer)
                {
                    ConfigVariables = new ConfigVariables();
                    _writer = new StreamWriter(ConfigFileName, false);
                    xmlSerializer.Serialize(_writer, ConfigVariables);
                    _writer.Dispose();
                }
                return null;
            }));
        }

        public void ReplaceXmlConfig()
        {
            AddWork(new Task<List<object>>(() =>
            {
                lock (xmlSerializer)
                {
                    _writer = new StreamWriter(ConfigFileName, false);
                    xmlSerializer.Serialize(_writer, this.ConfigVariables);
                    _writer.Dispose();
                }
                return null;
            }));
        }

        public void LoadXmlConfig()
        {
            var t = new Task<List<object>>(() =>
            {

                if (CheckConfigIntegrity())
                {
                    _xmlReader = XmlReader.Create(ConfigFileName);
                    // Just whatever the exception is, the xml file is damaged, user will be asked to remove it.
                    try
                    {
                        ConfigVariables = (ConfigVariables) xmlSerializer.Deserialize(_xmlReader);
                        _xmlReader.Dispose();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Your Config.xml is damaged.\nRemove it / Save new one.");
                        _xmlReader.Dispose();
                        throw;
                    }
                    finally
                    {

                    }
                }
                else
                {
                    MessageBox.Show("Your Config.xml is damaged.\n App is goona be raplace it after save.");

                }
                return null;
            });
            AddWork(t);
            t.Wait(-1);
        }

        public bool CheckConfigIntegrity()
        {
            Task<List<object>> tsk = new Task<List<object>>(() =>
            {

                lock (xmlSerializer)
                {
                    _xmlReader = XmlReader.Create(ConfigFileName);
                    try
                    {
                        var b = xmlSerializer.CanDeserialize(_xmlReader);
                        _xmlReader.Dispose();
                        return new List<object>() { b };
                    }
                    catch (XmlException e)
                    {
                        MessageBox.Show("Config.xml file is damaged. Remove it / Save new one");
                        _xmlReader.Dispose();
                        Debug.AddMessage<object>(new Message<object>("[" + typeof(XmlReader).Name + "]" + " Remove your config.xml file! => " + e.Message,MessageTypeEnum.Exception));
                        throw;
                    }
                }
            });
            AddWork(tsk);
            tsk.Wait(-1);
            return ((bool)ResultHandler(tsk).First());
        }

    }
}