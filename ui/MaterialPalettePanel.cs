using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace RhinoCncSuite.ui
{
    /// <summary>
    /// Rhino panel that hosts the Material Palette WPF control
    /// </summary>
    [Guid("A4F5E6D7-C8B9-4A12-B345-678901234567")]
    public class MaterialPalettePanelHost : UserControl
    {
        private readonly MaterialPaletteControl _wpfControl;

        /// <summary>
        /// Panel constructor
        /// </summary>
        public MaterialPalettePanelHost()
        {
            _wpfControl = new MaterialPaletteControl();
            var elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = _wpfControl
            };
            Controls.Add(elementHost);
        }

        /// <summary>
        /// Panel ID for registration
        /// </summary>
        public static Guid PanelId => typeof(MaterialPalettePanelHost).GUID;
    }
} 