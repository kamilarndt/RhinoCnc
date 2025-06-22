using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace RhinoCncSuite.Models
{
    /// <summary>
    /// Represents a material used in CNC manufacturing.
    /// This class defines the structure for materials in both the Global Catalog and Project Palette.
    /// </summary>
    public class Material : INotifyPropertyChanged
    {
        private static readonly Regex UnitRegex = new Regex(@"(\d+(\.\d+)?)\s*(mm|cm|m)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private const double InchToMmFactor = 25.4;
        private static readonly System.Text.RegularExpressions.Regex _nameRegex = new System.Text.RegularExpressions.Regex(@"\s*\d+(\.\d+)?\s*mm", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);
        private const double SquareMillimetersInSquareMeter = 1_000_000.0;

        /// <summary>
        /// Unique identifier for the material (UUID)
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Display name of the material (e.g., "MDF 18mm", "Plywood Birch 12mm")
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Type of material - either "Sheet" or "Length"
        /// </summary>
        [JsonProperty("type")]
        public MaterialType Type { get; set; }

        /// <summary>
        /// Thickness of the material in millimeters
        /// </summary>
        [JsonProperty("thickness")]
        public double Thickness { get; set; }

        /// <summary>
        /// Length of the material in millimeters
        /// </summary>
        [JsonProperty("length")]
        public double Length { get; set; }

        /// <summary>
        /// Width of the material in millimeters
        /// </summary>
        [JsonProperty("width")]
        public double Width { get; set; }

        /// <summary>
        /// Weight per square meter (for sheets) or per meter (for lengths) in kg
        /// </summary>
        [JsonProperty("weight")]
        public double? Weight { get; set; }

        /// <summary>
        /// Density of the material in kg/m³
        /// </summary>
        [JsonProperty("density")]
        public double Density { get; set; }

        /// <summary>
        /// Color of the material (optional)
        /// </summary>
        [JsonProperty("color")]
        public string Color { get; set; }

        /// <summary>
        /// Additional notes about the material
        /// </summary>
        [JsonProperty("notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Price per square meter of the material
        /// </summary>
        public double PricePerSquareMeter { get; set; }

        /// <summary>
        /// Gets a formatted string for the material's thickness for display purposes.
        /// </summary>
        [JsonIgnore] // This property should not be part of the JSON serialization
        public string FormattedThickness => $"{Thickness}mm";

        /// <summary>
        /// Gets the material name without thickness information for display purposes.
        /// </summary>
        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                // Remove thickness pattern like "18mm", "6mm", etc. from the name
                return _nameRegex.Replace(Name, "").Trim();
            }
            set
            {
                // This setter is provided to resolve the "property is read-only" compilation error
            }
        }

        /// <summary>
        /// Gets the material thickness as a string without unit, formatted to 1 decimal place if needed.
        /// </summary>
        [JsonIgnore]
        public string ThicknessDisplay => $"{Thickness:G29}";

        /// <summary>
        /// Gets the material dimensions as a string (e.g., "2800x2070"). Not serialized.
        /// </summary>
        [JsonIgnore]
        public string Dimensions => $"{Width}x{Length}";

        /// <summary>
        /// Default constructor
        /// </summary>
        [JsonConstructor]
        public Material()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Constructor for sheet materials
        /// </summary>
        public Material(string name, MaterialType type, double thickness, double width, double length) : this()
        {
            Name = name;
            Type = type;
            Thickness = thickness;
            Width = width;
            Length = length;
        }

        /// <summary>
        /// Constructor for length-based materials (profiles, lumber, etc.)
        /// </summary>
        public Material(string name, MaterialType type, double thickness) : this()
        {
            Name = name;
            Type = type;
            Thickness = thickness;
        }

        /// <summary>
        /// Returns a user-friendly string representation of the material
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({Thickness}mm)";
        }

        /// <summary>
        /// Checks if this material has valid sheet dimensions
        /// </summary>
        public bool HasValidSheetDimensions()
        {
            return Type == MaterialType.Sheet && Width > 0 && Length > 0;
        }

        /// <summary>
        /// Gets the area of the sheet in square meters (for Sheet type materials)
        /// </summary>
        public double? GetSheetAreaSquareMeters()
        {
            if (!HasValidSheetDimensions())
                return null;
            return (Width * Length) / 1_000_000.0; // Convert mm² to m²
        }

        /// <summary>
        /// Gets the total weight of a full sheet (for Sheet type materials)
        /// </summary>
        public double? GetSheetWeight()
        {
            var area = GetSheetAreaSquareMeters();
            if (!area.HasValue || !Weight.HasValue)
                return null;

            return area.Value * Weight.Value;
        }

        /// <summary>
        /// Creates a shallow copy of the material.
        /// </summary>
        public Material Clone()
        {
            return (Material)this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Enumeration of supported material types
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MaterialType
    {
        /// <summary>
        /// Sheet material (has width and height dimensions)
        /// </summary>
        Sheet,

        /// <summary>
        /// Linear material sold by length (profiles, trim, etc.)
        /// </summary>
        Length, // For materials sold by length, like profiles or lumber
        SolidWood,
        Other
    }
} 