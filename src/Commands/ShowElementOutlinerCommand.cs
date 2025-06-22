using Rhino;
using Rhino.Commands;
using Rhino.UI;
using RhinoCncSuite.ui;

namespace RhinoCncSuite.Commands
{
    /// <summary>
    /// Command to show the Element Outliner panel
    /// </summary>
    public class ShowElementOutlinerCommand : Command
    {
        private static ShowElementOutlinerCommand _instance;

        /// <summary>
        /// Public constructor
        /// </summary>
        public ShowElementOutlinerCommand()
        {
            _instance = this;
        }

        /// <summary>
        /// Command name (this is what users will type to run the command)
        /// </summary>
        public override string EnglishName => "RhinoCncElementOutliner";

        /// <summary>
        /// Executes the command
        /// </summary>
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                // Show the Element Outliner panel
                Panels.OpenPanel(ElementOutlinerPanel.PanelId);
                RhinoApp.WriteLine("RhinoCNC: Element Outliner panel opened.");
                return Result.Success;
            }
            catch (System.Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error opening Element Outliner panel: {ex.Message}");
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