using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace RhinoCncSuite.Models
{
    /// <summary>
    /// Represents an element (block/component) in the Element Outliner
    /// </summary>
    public class ElementInfo
    {
        /// <summary>
        /// Unique identifier for the element
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the element/block
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Rhino object/block ID
        /// </summary>
        public string RhinoId { get; set; }

        /// <summary>
        /// Type of element (Block, Component, Assembly, etc.)
        /// </summary>
        public ElementType Type { get; set; }

        /// <summary>
        /// Description or notes about the element
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Material assigned to this element
        /// </summary>
        public string MaterialId { get; set; }

        /// <summary>
        /// Quantity of this element in the project
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// List of attached files (PDFs, DXFs, etc.)
        /// </summary>
        public List<AttachedFile> AttachedFiles { get; set; }

        /// <summary>
        /// Manufacturing instructions or notes
        /// </summary>
        public string ManufacturingNotes { get; set; }

        /// <summary>
        /// Priority level for manufacturing
        /// </summary>
        public ElementPriority Priority { get; set; }

        /// <summary>
        /// Status of the element in the manufacturing process
        /// </summary>
        public ElementStatus Status { get; set; }

        /// <summary>
        /// Date when the element was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date when the element was last modified
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Tags for categorization
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// Name of the assigned material (for display purposes)
        /// </summary>
        [JsonIgnore]
        public string MaterialName { get; set; }

        /// <summary>
        /// Color of the assigned material (for display purposes)
        /// </summary>
        [JsonIgnore]
        public string MaterialColor { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ElementInfo()
        {
            Id = Guid.NewGuid().ToString();
            AttachedFiles = new List<AttachedFile>();
            Tags = new List<string>();
            Quantity = 1;
            Priority = ElementPriority.Normal;
            Status = ElementStatus.Design;
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

        /// <summary>
        /// Constructor with name
        /// </summary>
        public ElementInfo(string name, string rhinoId, ElementType type) : this()
        {
            Name = name;
            RhinoId = rhinoId;
            Type = type;
        }

        public ElementInfo Clone()
        {
            var clone = (ElementInfo)this.MemberwiseClone();
            clone.AttachedFiles = this.AttachedFiles.Select(f => f.Clone()).ToList();
            clone.Tags = new List<string>(this.Tags);
            return clone;
        }
    }

    /// <summary>
    /// Represents a file attached to an element
    /// </summary>
    public class AttachedFile
    {
        private const int BytesInKilobyte = 1024;
        /// <summary>
        /// Unique identifier for the attached file
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Original filename
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// File path (relative to project or absolute)
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// File type/extension
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Description of the file content
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Category of the file (Drawing, Manual, Specification, etc.)
        /// </summary>
        public FileCategory Category { get; set; }

        /// <summary>
        /// Date when the file was attached
        /// </summary>
        public DateTime AttachedDate { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public AttachedFile()
        {
            Id = Guid.NewGuid().ToString();
            AttachedDate = DateTime.Now;
            Category = FileCategory.Other;
        }

        /// <summary>
        /// Constructor with file path
        /// </summary>
        public AttachedFile(string filePath) : this()
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            FileType = Path.GetExtension(filePath);
            
            try
            {
            if (File.Exists(filePath))
                {
                    FileSize = new FileInfo(filePath).Length;
                }
            }
            catch (Exception)
            {
                // Log exception here if a logging mechanism is available
                // For now, we fail silently and FileSize remains 0
                FileSize = 0;
            }
        }

        /// <summary>
        /// Gets a user-friendly file size string
        /// </summary>
        [JsonIgnore]
        public string FileSizeString
        {
            get
            {
                if (FileSize < BytesInKilobyte)
                    return $"{FileSize} B";
                
                double kilobytes = FileSize / (double)BytesInKilobyte;
                if (kilobytes < BytesInKilobyte)
                    return $"{kilobytes:F1} KB";

                double megabytes = kilobytes / (double)BytesInKilobyte;
                return $"{megabytes:F1} MB";
            }
        }

        /// <summary>
        /// Checks if the file exists on disk
        /// </summary>
        [JsonIgnore]
        public bool FileExists => !string.IsNullOrEmpty(FilePath) && File.Exists(FilePath);

        public AttachedFile Clone()
        {
            return (AttachedFile)this.MemberwiseClone();
        }
    }

    /// <summary>
    /// Types of elements in the outliner
    /// </summary>
    public enum ElementType
    {
        Block,
        Component,
        Assembly,
        Part,
        Hardware,
        Other
    }

    /// <summary>
    /// Priority levels for elements
    /// </summary>
    public enum ElementPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    /// <summary>
    /// Status of elements in the manufacturing process
    /// </summary>
    public enum ElementStatus
    {
        Design,
        ReadyForManufacturing,
        InProgress,
        Completed,
        OnHold,
        Cancelled
    }

    /// <summary>
    /// Categories for attached files
    /// </summary>
    public enum FileCategory
    {
        Drawing,
        Manual,
        Specification,
        CNCProgram,
        Assembly,
        Reference,
        Other
    }
} 