# Epic 6: Import/Export

**Priorytet**: Medium  
**Szacowany czas**: 3-4 dni  
**Status**: Nie rozpoczęty

## Cel
Implementacja systemu import/export dla bazy materiałów z support dla różnych formatów danych, backup/restore funkcjonalności oraz synchronizacji z zewnętrznymi źródłami materiałów.

## Zadania

### Task 6.1: JSON Import/Export
**Priorytet**: Medium  
**Szacowany czas**: 1 dzień  
**Plik**: `task_6_1_json_import_export.md`

Export całej bazy materiałów do JSON i import z validation i merge handling.

### Task 6.2: CSV Import/Export  
**Priorytet**: Medium  
**Szacowany czas**: 1 dzień  
**Plik**: `task_6_2_csv_import_export.md`

Import/export materiałów w formacie CSV dla spreadsheet compatibility.

### Task 6.3: Backup/Restore System
**Priorytet**: Medium  
**Szacowany czas**: 1 dzień  
**Plik**: `task_6_3_backup_restore.md`

Automatyczne backup bazy materiałów z timestamped versions i restore functionality.

### Task 6.4: External Sources Integration
**Priorytet**: Low  
**Szacowany czas**: 1 dzień  
**Plik**: `task_6_4_external_sources.md`

Integration z external material databases i suppliers' catalogs.

## Zależności
- **Epic 2**: Data Models (Material, MaterialCollection)
- **Epic 5**: Material Catalog (dla merge operations)

## Kluczowe Features

### Supported Formats
- **JSON**: Native format z full fidelity
- **CSV**: Simplified format dla Excel compatibility
- **XML**: Future compatibility z other systems
- **Custom**: Extensible format support

### Import Features
- **Validation**: Data integrity checks przed import
- **Merge Options**: Add new, update existing, replace all
- **Conflict Resolution**: Handling duplicate materials
- **Preview**: Show changes przed actual import
- **Rollback**: Undo import operations

### Export Features
- **Selective Export**: Export filtered materials
- **Template Export**: Export structure for bulk entry
- **Full Catalog**: Complete database export
- **Incremental**: Export tylko changes since date

### Backup System
- **Automatic**: Daily/weekly scheduled backups
- **Manual**: On-demand backup creation
- **Versioned**: Multiple backup versions
- **Compression**: Space-efficient storage
- **Validation**: Backup integrity checks

## Data Flow Architecture

### Import Process
```
External File → Validation → Preview → User Confirmation → Merge → Update Database
```

### Export Process  
```
Database Query → Filter/Transform → Format Conversion → File Generation → Save
```

### Backup Process
```
Database State → Compression → Timestamped File → Cleanup Old Versions
```

## File Format Specifications

### JSON Format
```json
{
  "version": "1.0",
  "exportDate": "2024-01-01T00:00:00Z",
  "source": "RhinoCNC Material Manager",
  "materials": [
    {
      "id": "uuid",
      "name": "MDF 18mm",
      "category": "MDF",
      "type": "Sheet",
      "isInProjectPalette": true,
      "dimensions": {...},
      "properties": {...}
    }
  ]
}
```

### CSV Format
```csv
Name,Category,Type,Thickness,Width,Height,Color,InPalette
MDF 18mm,MDF,Sheet,18,2800,2070,#FF6B35,true
Plywood 12mm,SKLEJKA,Sheet,12,2500,1250,#FFD700,false
```

## Services do Implementacji

### IImportExportService
```csharp
public interface IImportExportService
{
    Task<ImportPreview> PreviewImportAsync(string filePath, ImportFormat format);
    Task<ImportResult> ImportMaterialsAsync(string filePath, ImportOptions options);
    Task<string> ExportMaterialsAsync(IEnumerable<Material> materials, ExportFormat format);
    Task<IEnumerable<string>> GetSupportedFormats();
}
```

### IBackupService
```csharp
public interface IBackupService
{
    Task<string> CreateBackupAsync(string description = null);
    Task<RestoreResult> RestoreBackupAsync(string backupPath);
    Task<IEnumerable<BackupInfo>> GetAvailableBackupsAsync();
    Task CleanupOldBackupsAsync(int keepCount = 10);
    Task<bool> ValidateBackupAsync(string backupPath);
}
```

### IDataMigrationService
```csharp
public interface IDataMigrationService
{
    Task<bool> IsUpgradeNeededAsync(string filePath);
    Task<MigrationResult> UpgradeDataAsync(string filePath, string targetVersion);
    Task<IEnumerable<string>> GetSupportedVersionsAsync();
}
```

## UI Components

### Import/Export Dialog
- Format selection dropdown
- File browser dla source/destination
- Preview grid z changes
- Options panel (merge strategy, validation level)
- Progress bar dla long operations

### Backup Manager
- List wszystkich backups z timestamps
- Create backup button
- Restore backup functionality  
- Backup validation status
- Cleanup old backups

## Error Handling

### Import Errors
- **File Format**: Invalid file structure
- **Data Validation**: Invalid material properties
- **Conflicts**: Duplicate materials handling
- **Permissions**: File access issues

### Export Errors
- **File Permissions**: Cannot write to destination
- **Data Corruption**: Invalid source data
- **Format Conversion**: Unsupported data types
- **Storage Space**: Insufficient disk space

### Backup Errors
- **Storage Issues**: Backup location unavailable
- **Corruption**: Backup file corruption
- **Version Mismatch**: Incompatible backup version
- **Restore Failures**: Cannot restore backup

## Performance Considerations

### Large Datasets
- **Streaming**: Process large files w chunks
- **Progress Reporting**: Real-time operation status
- **Memory Management**: Efficient memory usage
- **Cancellation**: Allow operation cancellation

### File Operations
- **Async I/O**: Non-blocking file operations
- **Compression**: Reduce backup file sizes
- **Validation**: Background validation operations
- **Caching**: Cache frequently accessed data

## Security Considerations

### File Access
- **Path Validation**: Prevent directory traversal
- **Permission Checks**: Verify file access rights
- **Backup Encryption**: Optional backup encryption
- **Audit Trail**: Log all import/export operations

### Data Integrity
- **Checksums**: File integrity verification
- **Validation**: Comprehensive data validation
- **Rollback**: Safe operation rollback
- **Backup Verification**: Automated backup testing

## Kryteria Ukończenia
- [ ] JSON import/export fully functional
- [ ] CSV import/export working
- [ ] Backup/restore system operational
- [ ] External sources integration basic level
- [ ] Error handling comprehensive
- [ ] Performance acceptable dla large datasets
- [ ] Security measures implemented

## Pliki do Stworzenia
- `src/Services/IImportExportService.cs`
- `src/Services/ImportExportService.cs`
- `src/Services/IBackupService.cs` 
- `src/Services/BackupService.cs`
- `ui/Dialogs/ImportExportDialog.cs`
- `ui/Dialogs/BackupManagerDialog.cs`
- Format converters i validators
- Migration utilities

## Future Enhancements
- **Cloud Sync**: Synchronization z cloud storage
- **Real-time Sync**: Live updates z external sources
- **Version Control**: Git-like versioning dla material changes
- **Collaborative Editing**: Multi-user material editing
- **API Integration**: REST API dla external integrations

Epic 6 zapewnia comprehensive data management capabilities, umożliwiając łatwy exchange materiałów między różnymi instalacjami i backup/restore functionality dla data safety. 