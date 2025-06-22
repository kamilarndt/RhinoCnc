using Rhino.UI;
using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace RhinoCncSuite.ui
{
    /// <summary>
    /// Rhino panel that hosts the Element Outliner WPF control
    /// </summary>
    [System.Runtime.InteropServices.Guid("C3D4E5F6-A7B8-4901-C234-567890123456")]
    public class ElementOutlinerPanel : Panel
    {
        private ElementOutlinerControl _wpfControl;
        private ElementHost _elementHost;

        /// <summary>
        /// Panel constructor
        /// </summary>
        public ElementOutlinerPanel()
        {
            InitializePanel();
        }

        /// <summary>
        /// Initializes the panel with the WPF control
        /// </summary>
        private void InitializePanel()
        {
            try
            {
                // Create the WPF control
                _wpfControl = new ElementOutlinerControl();

                // Create the ElementHost to host the WPF control
                _elementHost = new ElementHost
                {
                    Dock = System.Windows.Forms.DockStyle.Fill,
                    Child = _wpfControl
                };

                // Add the ElementHost to the panel
                Controls.Add(_elementHost);
            }
            catch (Exception ex)
            {
                Rhino.RhinoApp.WriteLine($"RhinoCNC Error initializing Element Outliner panel: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when the panel is closed
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _elementHost?.Dispose();
                _wpfControl = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Panel ID for registration
        /// </summary>
        public static System.Guid PanelId => new System.Guid("C3D4E5F6-A7B8-4901-C234-567890123456");
    }
} 