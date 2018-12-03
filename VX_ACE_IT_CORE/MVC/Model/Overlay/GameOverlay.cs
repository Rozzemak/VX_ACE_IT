using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Overlay.NET.Common;
using Overlay.NET.Wpf;
using Process.NET.Windows;
using SharpDX.Direct2D1;
using OverlayWindow = Overlay.NET.Wpf.OverlayWindow;

namespace VX_ACE_IT_CORE.MVC.Model.Overlay
{
    public class OverlaySettings
    {
        // 60 frames/sec roughly
        public int UpdateRate { get; set; }

        public string Author { get; set; }
        public string Description { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
    }

    public class GameOverlay : WpfOverlayPlugin
    {
        public ISettings<OverlaySettings> Settings { get; } = new SerializableSettings<OverlaySettings>();
        // Used to limit update rates via timestamps 
        // This way we can avoid thread issues with wanting to delay updates
        private readonly TickEngine _tickEngine = new TickEngine();
        private System.Windows.Shapes.Ellipse _ellipse;

        private bool _isDisposed;

        private bool _isSetup;

        // Shapes used in the demo
        private Line _line;
        private Polygon _polygon;
        private Rectangle _rectangle;

        public override void Enable()
        {
            _tickEngine.IsTicking = true;
            base.Enable();
        }

        public override void Disable()
        {
            _tickEngine.IsTicking = false;
            base.Disable();
        }

        public override void Initialize(IWindow targetWindow)
        {
            // Set target window by calling the base method
            base.Initialize(targetWindow);

            OverlayWindow = new OverlayWindow(targetWindow);

            // For demo, show how to use settings
            var current = Settings.Current;
            var type = GetType();

            current.UpdateRate = 1000 / 60;
            current.Author = GetAuthor(type);
            current.Description = GetDescription(type);
            current.Identifier = GetIdentifier(type);
            current.Name = GetName(type);
            current.Version = GetVersion(type);

            // File is made from above info
            Settings.Save();
            Settings.Load();

            // Set up update interval and register events for the tick engine.
            _tickEngine.Interval = Settings.Current.UpdateRate.Milliseconds();
            _tickEngine.PreTick += OnPreTick;
            _tickEngine.Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs eventArgs)
        {
            // This will only be true if the target window is active
            // (or very recently has been, depends on your update rate)
            if (OverlayWindow.IsVisible)
            {
                OverlayWindow.Update();
            }
        }

        private void OnPreTick(object sender, EventArgs eventArgs)
        {
            // Only want to set them up once.
            if (!_isSetup)
            {
                SetUp();
                _isSetup = true;
            }

            var activated = TargetWindow.IsActivated;
            var visible = OverlayWindow.IsVisible;

            // Ensure window is shown or hidden correctly prior to updating
            if (!activated && visible)
            {
                OverlayWindow.Hide();
            }

            else if (activated && !visible)
            {
                OverlayWindow.Show();
            }
        }

        public override void Update() => _tickEngine.Pulse();

        // Clear objects
        public override void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (IsEnabled)
            {
                Disable();
            }

            OverlayWindow?.Hide();
            OverlayWindow?.Close();
            OverlayWindow = null;
            _tickEngine.Stop();
            Settings.Save();

            base.Dispose();
            _isDisposed = true;
        }

        ~GameOverlay()
        {
            Dispose();
        }

        // Random shapes.. thanks Julian ^_^
        private void SetUp()
        {
            _polygon = new Polygon
            {
                Points = new PointCollection(5) {
                    new Point(100, 150),
                    new Point(120, 130),
                    new Point(140, 150),
                    new Point(140, 200),
                    new Point(100, 200)
                },
                Stroke = new System.Windows.Media.SolidColorBrush(Color.FromRgb(0, 0, 255)),
                Fill =
                    new System.Windows.Media.RadialGradientBrush(
                        Color.FromRgb(255, 255, 0),
                        Color.FromRgb(255, 0, 255))
            };

            OverlayWindow.Add(_polygon);

            // Create a line
            _line = new Line
            {
                X1 = 100,
                X2 = 300,
                Y1 = 200,
                Y2 = 200,
                Stroke = new System.Windows.Media.SolidColorBrush(Color.FromRgb(0, 255, 0)),
                StrokeThickness = 2
            };

            OverlayWindow.Add(_line);

            // Create an ellipse (circle)
            _ellipse = new System.Windows.Shapes.Ellipse
            {
                Width = 15,
                Height = 15,
                Margin = new Thickness(300, 300, 0, 0),
                Stroke =
                    new System.Windows.Media.SolidColorBrush(Color.FromRgb(0, 255, 255))
            };

            OverlayWindow.Add(_ellipse);

            // Create a rectangle
            _rectangle = new Rectangle
            {
                RadiusX = 2,
                RadiusY = 2,
                Width = 50,
                Height = 100,
                Margin = new Thickness(400, 400, 0, 0),
                Stroke = new System.Windows.Media.SolidColorBrush(Color.FromRgb(255, 0, 0)),
                Fill =
                    new System.Windows.Media.SolidColorBrush(Color.FromArgb(100, 255, 255,
                        255))
            };

            OverlayWindow.Add(_rectangle);
        }

        public void AddShape(Shape shape) => OverlayWindow.Add(shape);

        public void AddEvent(Shape shape, Action action, Dispatcher dispatcher) => shape.MouseDown += (sender, args) =>
        {
            dispatcher.Invoke(() =>
            {
                action?.Invoke();
            });
        };
    }
}
