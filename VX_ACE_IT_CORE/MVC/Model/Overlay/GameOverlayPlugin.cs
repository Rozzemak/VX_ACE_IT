/*using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Overlay.NET;
using Overlay.NET.Common;
using Overlay.NET.Wpf;
using Process.NET;
using Process.NET.Memory;

namespace VX_ACE_IT_CORE.MVC.Model.Overlay
{
    public class GameOverlayPlugin
    {
        /// <summary>
        ///     The overlay
        /// </summary>
        public GameOverlay Overlay;

        /// <summary>
        ///     The process sharp
        /// </summary>
        private ProcessSharp _processSharp;

        /// <summary>
        ///     The work
        /// </summary>
        private bool _work;

        /// <summary>
        ///     Starts the demo.
        /// </summary>
        public void StartDemo(System.Diagnostics.Process gameProcess, Dispatcher dispatcher)
        {
            dispatcher.Invoke(() =>
            {
                // Set up objects/overlay
                var process = gameProcess;

                _processSharp = new ProcessSharp(process, MemoryType.Remote);
                Overlay = new GameOverlay(dispatcher);

                var wpfOverlay = Overlay;

                // This is done to focus on the fact the Init method
                // is overriden in the wpf overlay demo in order to set the
                // wpf overlay window instance
                wpfOverlay.Initialize(_processSharp.WindowFactory.MainWindow);
                wpfOverlay.Enable();

                _work = true;

                // Log some info about the overlay.
                Log.Debug("Starting update loop (open the process you specified and drag around)");
                Log.Debug("Update rate: " + wpfOverlay.Settings.Current.UpdateRate.Milliseconds());

                var info = wpfOverlay.Settings.Current;

                Log.Debug($"Author: {info.Author}");
                Log.Debug($"Description: {info.Description}");
                Log.Debug($"Name: {info.Name}");
                Log.Debug($"Identifier: {info.Identifier}");
                Log.Debug($"Version: {info.Version}");

                Log.Info("Note: Settings are saved to a settings folder in your main app folder.");

                Log.Info("Give your window focus to enable the overlay (and unfocus to disable..)");

                Log.Info("Close the console to end the demo.");

                wpfOverlay.OverlayWindow.Draw += OnDraw;

                // Do work
            });

            new Task(() =>
            {
                while (_work)
                {
                    Thread.Sleep(16);
                    dispatcher.Invoke(() =>
                    {
                        Overlay.Update();
                    });
                }

            }).Start();

            Log.Debug("Demo complete.");
        }

        private void OnDraw(object sender, DrawingContext context)
        {
            // Draw a formatted text string into the DrawingContext.
            context.DrawText(
                new FormattedText("Click Me!", CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight,
                    new Typeface("Verdana"), 36, Brushes.BlueViolet), new Point(200, 116));

            context.DrawLine(new Pen(Brushes.Blue, 10), new Point(100, 100), new Point(10, 10));
        }


    }
}
*/