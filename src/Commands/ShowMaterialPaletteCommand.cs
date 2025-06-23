using Rhino;
using Rhino.Commands;
using Rhino.UI;
using RhinoCncSuite.ui;

namespace RhinoCncSuite.Commands
{
    /// <summary>
    /// Command to show/toggle the Material Palette panel
    /// </summary>
    public class ShowMaterialPaletteCommand : Command
    {
        private static ShowMaterialPaletteCommand _instance;

        /// <summary>
        /// Public constructor
        /// </summary>
        public ShowMaterialPaletteCommand()
        {
            _instance = this;
        }

        /// <summary>
        /// Gets the command name
        /// </summary>
        public override string EnglishName => "RhinoCncMaterialPalette";

        /// <summary>
        /// Executes the command
        /// </summary>
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                // Check if the panel is already visible
                var panelId = typeof(MaterialPalettePanel).GUID;
                var isVisible = Panels.IsPanelVisible(panelId);

                if (isVisible)
                {
                    // Hide the panel
                    Panels.ClosePanel(panelId);
                    RhinoApp.WriteLine("RhinoCNC: Material Palette panel closed.");
                }
                else
                {
                    // Show the panel
                    Panels.OpenPanel(panelId);
                    RhinoApp.WriteLine("RhinoCNC: Material Palette panel opened.");
                }

                return Result.Success;
            }
            catch (System.Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC Error: Failed to toggle Material Palette panel: {ex.Message}");
                return Result.Failure;
            }
        }

        /// <summary>
        /// Registers the command with Rhino
        /// </summary>
        public static void Register(RhinoCncPlugin plugin)
        {
            // The command will be created by Rhino when the assembly is loaded
        }
    }
} 