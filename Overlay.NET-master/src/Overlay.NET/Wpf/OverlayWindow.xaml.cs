using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Overlay.NET.Common;
using Process.NET.Windows;

namespace Overlay.NET.Wpf {
    /// <summary>
    ///     Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window {
        private readonly IWindow _targetWindow;
        private readonly double _dpiX = (int)typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null, null);
        private readonly double _dpiY = (int)typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null, null);



        /// <summary>
        ///     Initializes a new instance of the <see cref="OverlayWindow" /> class.
        /// </summary>
        /// <param name="targetWindow">The window.</param>
        public OverlayWindow(IWindow targetWindow) {
            _targetWindow = targetWindow;
            InitializeComponent();
        }

        public event EventHandler<DrawingContext> Draw;

        /// <summary>
        ///     Updates this instance.
        /// </summary>
        public void Update() {
            this.Width = (double)this._targetWindow.Width * 96.0 / _dpiX;
            this.Height = (double)this._targetWindow.Height * 96.0 / _dpiY;
            this.Left = (double)this._targetWindow.X * 96.0 / _dpiX;
            this.Top = (double)this._targetWindow.Y * 96.0 / _dpiY;
        }

        /// <summary>
        ///     Raises the <see cref="E:System.Windows.Window.SourceInitialized" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
            // We need to do this in order to allow shapes
            // drawn on the canvas to be click-through. 
            // There is no other way to do this.
            // Source: https://social.msdn.microsoft.com/Forums/en-US/c32889d3-effa-4b82-b179-76489ffa9f7d/how-to-clicking-throughpassing-shapesellipserectangle?forum=wpf
            this.MakeWindowTransparent();
        }

        /// <summary>
        ///     When overridden in a derived class, participates in rendering operations that are directed by the layout system.
        ///     The rendering instructions for this element are not used directly when this method is invoked, and are instead
        ///     preserved for later asynchronous use by layout and drawing.
        /// </summary>
        /// <param name="drawingContext">
        ///     The drawing instructions for a specific element. This context is provided to the layout
        ///     system.
        /// </param>
        protected override void OnRender(DrawingContext drawingContext) {
            OnDraw(drawingContext);
            base.OnRender(drawingContext);
        }

        /// <summary>
        ///     Adds the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        public void Add(UIElement element) => OverlayGrid.Children.Add(element);

        /// <summary>
        ///     Removes the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        public void Remove(UIElement element) => OverlayGrid.Children.Remove(element);

        /// <summary>
        ///     Adds the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="index">The index.</param>
        public void Add(UIElement element, int index) => OverlayGrid.Children[index] = element;

        /// <summary>
        ///     Removes the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void Remove(int index) => OverlayGrid.Children.RemoveAt(index);

        /// <summary>
        ///     Called when [draw].
        /// </summary>
        /// <param name="e">The e.</param>
        protected virtual void OnDraw(DrawingContext e) => Draw?.Invoke(this, e);
    }
}