using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace RhinoCncSuite.ui
{
    /// <summary>
    /// Rhino panel that hosts the Element Outliner WPF control
    /// </summary>
    [Guid("0D1EAD9E-96A1-4959-B245-A3231BEF8B43")]
    public class ElementOutlinerPanelHost : UserControl
    {
        private readonly ElementOutlinerControl _wpfControl;

        public ElementOutlinerPanelHost(uint documentSerialNumber)
        {
            _wpfControl = new ElementOutlinerControl { DataContext = new ElementOutlinerViewModel(documentSerialNumber) };
            var elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = _wpfControl
            };
            Controls.Add(elementHost);
        }
    }

    public class ElementOutlinerViewModel
    {
        public ElementOutlinerViewModel(uint documentSerialNumber)
        {
        }
    }
} 