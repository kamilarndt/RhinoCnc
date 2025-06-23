using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Drawing;
using Rhino;

namespace RhinoCncSuite.ui
{
    /// <summary>
    /// Rhino panel that hosts the Element Outliner WPF control
    /// </summary>
    [Guid("8A435887-2342-4F53-A338-92715A0E880C")]
    public class ElementOutlinerPanel : UserControl
    {
        private ElementOutlinerControl _wpfControl;
        private ElementHost _elementHost;

        public ElementOutlinerPanel()
        {
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                Controls.Clear();

                _wpfControl = new ElementOutlinerControl();

                _elementHost = new ElementHost
                {
                    Dock = DockStyle.Fill,
                    Child = _wpfControl
                };

                Controls.Add(_elementHost);
                BackColor = Color.FromArgb(64, 64, 64);

                RhinoApp.WriteLine("RhinoCNC: Element Outliner panel initialized successfully.");
            }
            catch (Exception exception)
            {
                CreateFallbackErrorDisplay(exception);
            }
        }

        private void RetryInitialization()
        {
            RhinoApp.WriteLine("RhinoCNC: Retrying Element Outliner panel initialization...");
            Initialize();
        }

        private void CreateFallbackErrorDisplay(Exception exception)
        {
            Controls.Clear();

            var errorPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(64, 64, 64),
                Padding = new Padding(10)
            };

            var errorLabel = new Label
            {
                Text = $"Element Outliner Error:\n{exception.Message}\n\nPlease check the Rhino command line for details.",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font(new FontFamily("Segoe UI"), 9, System.Drawing.FontStyle.Regular),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var retryButton = new Button
            {
                Text = "Retry",
                Size = new System.Drawing.Size(100, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            retryButton.Click += (s, e) => RetryInitialization();

            errorPanel.Controls.Add(errorLabel);
            errorPanel.Controls.Add(retryButton);
            Controls.Add(errorPanel);

            RhinoApp.WriteLine($"RhinoCNC: Element Outliner panel error: {exception.Message}");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _elementHost?.Dispose();
                _wpfControl = null;
            }
            base.Dispose(disposing);
        }
    }
} 