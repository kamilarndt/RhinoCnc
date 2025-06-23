# 🦏 RhinoAI Production Suite

**Professional-grade Rhino 3D Plugin Suite for Manufacturing & CNC Operations**

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4)]()
[![Rhino 8.20+](https://img.shields.io/badge/Rhino-8.20+-FF6B35)]()
[![Windows 11](https://img.shields.io/badge/Windows-11-0078D4)]()
[![License](https://img.shields.io/badge/License-Commercial-blue)]()

## 🎯 Overview

RhinoAI Production Suite to komprehensywny system składający się z 4 głównych komponentów zaprojektowanych do maksymalizacji efektywności workflow od modelu 3D do plików produkcyjnych:

### 📊 Core Components

| Component | Status | Description | Features |
|-----------|--------|-------------|----------|
| **🎨 Material Manager** | 🚧 **In Development** | Material management & assignment | Drag&drop, AI auto-assignment, 0.01mm duplicate detection |
| **📋 Element Outliner** | 📝 Planned | Hierarchical organization | Smart grouping, auto-naming, batch operations |
| **⚙️ CNC Automation Engine** | 📝 Planned | Precision unfolding & optimization | 6 strategies, AI optimization, hole detection |
| **🤖 Local AI Models** | 📝 Planned | On-device intelligence | Material Classifier 95%, Duplicate Detector 90% |

### 🚀 Key Benefits
- **⚡ 80% time savings** - From 3D model to DXF files in 45 minutes
- **💰 0 PLN/month** - No recurring API costs (local AI models)
- **🎯 99.9% accuracy** - Precision unfolding with intelligent validation
- **🔒 Private & Secure** - All processing happens locally

---

## 🎨 Material Manager (Current Focus)

**Status**: 🚧 Active Development | **Target**: v1.0 Q3 2025

### Features
- **Drag & Drop Material Assignment** - Intuitive material application
- **AI-Powered Auto-Assignment** - Smart material detection and suggestion
- **0.01mm Duplicate Detection** - Prevent identical geometry duplication
- **Advanced Material Catalog** - Comprehensive material library management
- **Color-Coded Categories** - Visual organization (MDF-orange, SKLEJKA-yellow, etc.)

### Technical Stack
- **.NET 8.0** - Latest framework supporting Rhino 8.20+
- **WPF + MVVM** - Modern UI architecture with clean separation
- **RhinoCommon API** - Direct Rhino 3D integration
- **JSON Serialization** - Efficient data management
- **Programmatic UI** - No XAML dependencies for maximum compatibility

### 🔧 Architecture
```
Material Manager/
├── 📁 Models/           # Data structures (Material, MaterialCollection)
├── 📁 ViewModels/       # MVVM business logic (MaterialPaletteViewModel)
├── 📁 Services/         # Core services (MaterialCatalogService)
├── 📁 UI/              # WPF controls and panels
└── 📁 Commands/        # Rhino command implementations
```

---

## 📈 Development Progress

### Current Epic Status

| Epic | Tasks | Progress | Target |
|------|-------|----------|---------|
| **Epic 1: Core Infrastructure** | 3/3 | 🟢 100% | Q2 2025 |
| **Epic 2: Data Models** | 0/4 | 🔴 0% | Q2 2025 |
| **Epic 3: Material Palette UI** | 0/5 | 🔴 0% | Q3 2025 |
| **Epic 4: Rhino Integration** | 0/4 | 🔴 0% | Q3 2025 |
| **Epic 5: Material Catalog** | 0/6 | 🔴 0% | Q3 2025 |
| **Epic 6: Import/Export** | 0/4 | 🔴 0% | Q4 2025 |
| **Epic 7: Advanced Features** | 0/5 | 🔴 0% | Q4 2025 |

### Recent Activity
- ✅ **Core Infrastructure** - BaseViewModel, RelayCommand, Service Locator
- ✅ **Project Setup** - .NET 8.0 migration, documentation structure
- 🚧 **Data Models** - Material and collection classes in progress

---

## 🛠️ Development Setup

### Prerequisites
- **Windows 11** (64-bit)
- **Rhino 8.20+** with .NET 8.0 runtime
- **Visual Studio 2022** with .NET 8.0 SDK
- **.NET 8.0 Desktop Runtime**

### Quick Start
```bash
# Clone repository
git clone https://github.com/your-org/rhinocnc.git
cd rhinocnc

# Restore packages
dotnet restore

# Build solution
dotnet build --configuration Release

# IMPORTANT: Close Rhino before building to prevent DLL locking!
# Kill Rhino process if needed: taskkill /F /IM "Rhino.exe"
```

### 🧪 Testing
```bash
# Run unit tests
dotnet test --configuration Release

# Run specific component tests
dotnet test --filter "Category=MaterialManager"

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📚 Documentation

### 📖 Core Documentation
- **[Material Manager README](docs/material-manager/README.md)** - Component overview
- **[Product Requirements](docs/material-manager/material_manager_prd.md)** - Detailed specifications
- **[Technical Architecture](docs/material-manager/technical_architecture.md)** - System design
- **[Development Guide](docs/material-manager/development_guide.md)** - Developer handbook
- **[Testing Strategy](docs/material-manager/testing_strategy.md)** - QA approach

### 🎯 Task Documentation
Detailed task breakdown available in [Epic Documentation](docs/material-manager/tasks/):
- [Epic 1: Core Infrastructure](docs/material-manager/tasks/epic-1-core-infrastructure/)
- [Epic 2: Data Models](docs/material-manager/tasks/epic-2-data-models/)
- [Epic 3: Material Palette UI](docs/material-manager/tasks/epic-3-material-palette-ui/)
- [Epic 4: Rhino Integration](docs/material-manager/tasks/epic-4-rhino-integration/)
- [Epic 5: Material Catalog](docs/material-manager/tasks/epic-5-material-catalog/)
- [Epic 6: Import/Export](docs/material-manager/tasks/epic-6-import-export/)
- [Epic 7: Advanced Features](docs/material-manager/tasks/epic-7-advanced-features/)

---

## 🤝 Contributing

### GitHub Workflow
1. **Create Issue** using appropriate template ([Epic](../../issues/new?template=epic.md) | [Task](../../issues/new?template=task.md) | [Bug](../../issues/new?template=bug.md))
2. **Create Feature Branch** following naming convention: `feature/component-description`
3. **Implement with TDD** - Write tests first, then code
4. **Submit Pull Request** using [PR template](../../compare)
5. **Code Review** - Ensure quality and architecture compliance

### Development Standards
- **🏗️ Architecture**: MVVM pattern with clear separation of concerns
- **🧪 Testing**: TDD approach with 80%+ unit test coverage
- **📝 Documentation**: Polish comments, English code, comprehensive docs
- **🔧 Code Quality**: Microsoft C# conventions, meaningful names
- **⚡ Performance**: Optimized for large material libraries and complex geometries

### Labels & Organization
We use a comprehensive [labeling system](.github/labels.yml) for organization:
- **Components**: `material-manager`, `element-outliner`, `cnc-automation`, `local-ai`
- **Priority**: `priority:critical/high/medium/low`
- **Size**: `size:xs/s/m/l/xl` (time estimates)
- **Status**: `status:ready/in-progress/blocked/review/testing/done`

---

## 🔮 Future Roadmap

### Phase 1: Material Manager (2025 Q2-Q4)
- Core material management functionality
- Basic UI and Rhino integration
- Import/export capabilities
- Advanced features (themes, shortcuts, analytics)

### Phase 2: Element Outliner (2025 Q4-2026 Q1)
- Hierarchical element organization
- Smart grouping and naming
- Batch operations
- Integration with Material Manager

### Phase 3: CNC Automation Engine (2026 Q1-Q2)
- Precision unfolding algorithms
- AI-powered optimization
- Multiple unfolding strategies
- Quality validation

### Phase 4: Local AI Models (2026 Q2-Q3)
- Material classification model
- Duplicate detection algorithm
- User-trained models
- Performance optimization

### Phase 5: Integration & Polish (2026 Q3-Q4)
- Full suite integration
- Performance optimization
- Advanced AI features
- Production release

---

## 📞 Support & Contact

- **Documentation**: [docs/material-manager/](docs/material-manager/)
- **Issues**: [GitHub Issues](../../issues)
- **Discussions**: [GitHub Discussions](../../discussions)
- **Email**: support@rhinoai-suite.com

---

## 📄 License

**Commercial License** - Contact for licensing information.

---

<div align="center">
  <strong>🦏 Made with ❤️ for the Rhino 3D Community</strong><br>
  <em>Professional Manufacturing Tools for Modern Workflows</em>
</div> 