---
name: Task
about: Individual task within an Epic for RhinoAI Production Suite
title: '[TASK] Task Name'
labels: task, enhancement
assignees: ''

---

## 🎯 Task Overview

**Epic**: #epic_number
**Component**: [ ] Material Manager | [ ] Element Outliner | [ ] CNC Automation | [ ] Local AI Models
**Priority**: [ ] Critical | [ ] High | [ ] Medium | [ ] Low
**Estimated Effort**: [ ] XS (1-2h) | [ ] S (3-6h) | [ ] M (1-2d) | [ ] L (3-5d) | [ ] XL (1w+)
**Assignee**: @username

## 📋 Task Description

Clear description of what needs to be implemented, modified, or fixed.

## 🎪 User Story

**As a** [user type]
**I want** [action/functionality]
**So that** [benefit/value]

## ⚙️ Technical Requirements

### Implementation Details
- [ ] Specific class/method to create/modify
- [ ] Integration points with existing code
- [ ] Dependencies on other components

### Code Structure (.NET 8.0 + WPF)
```csharp
// Przykład struktury kodu do implementacji
namespace RhinoCncSuite.Components.MaterialManager
{
    // Klasy do utworzenia/zmodyfikowania
}
```

### MVVM Pattern Requirements
- [ ] Model classes needed
- [ ] ViewModel implementation
- [ ] View/UI components
- [ ] Command bindings

## 🔗 Dependencies

### Blocking Issues
- [ ] #issue_number - Description

### Related Issues
- [ ] #issue_number - Description

### File Dependencies
- [ ] Existing file to modify: `path/to/file.cs`
- [ ] New file to create: `path/to/newfile.cs`

## 📝 Acceptance Criteria

### Functional Requirements
- [ ] Specific functionality works as expected
- [ ] Integration with existing components successful
- [ ] Edge cases handled properly

### Technical Requirements
- [ ] Code follows project conventions (English names, Polish comments)
- [ ] Programmatic UI creation (no XAML)
- [ ] Thread-safe Rhino API calls using `RhinoApp.InvokeOnUiThread()`
- [ ] Proper error handling and logging

### Quality Requirements
- [ ] Unit tests written and passing
- [ ] Integration tests if applicable
- [ ] Performance requirements met
- [ ] Memory usage within limits

## 🧪 Testing Approach

### Unit Tests
```csharp
[TestFixture]
public class ComponentNameTests
{
    [Test]
    public void ShouldPerformExpectedAction_WhenConditionMet()
    {
        // Test implementation
    }
}
```

### Integration Tests
- [ ] Rhino API integration test
- [ ] Component interaction test
- [ ] Data persistence test

### Manual Testing Steps
1. Step 1: Action to perform
2. Step 2: Expected result
3. Step 3: Verification method

## 📊 Success Metrics

### Performance Targets
- [ ] Execution time: < X ms
- [ ] Memory usage: < X MB
- [ ] Response time: < X ms

### Quality Metrics
- [ ] Test coverage: >80%
- [ ] No critical bugs
- [ ] Code review approved

## 🏗️ Implementation Plan

### Phase 1: Setup
- [ ] Create basic class structure
- [ ] Set up test framework
- [ ] Define interfaces

### Phase 2: Core Implementation
- [ ] Implement main functionality
- [ ] Add error handling
- [ ] Write unit tests

### Phase 3: Integration
- [ ] Integrate with existing components
- [ ] Test end-to-end functionality
- [ ] Performance optimization

### Phase 4: Polish
- [ ] Code review and cleanup
- [ ] Documentation updates
- [ ] Final testing

## 🔧 Technical Notes

### Rhino Integration
- Kill Rhino.exe process before building to avoid DLL locking
- Use `RhinoApp.InvokeOnUiThread()` for UI operations
- Store metadata in User Attributes

### Material Manager Specific
- JSON serialization with Newtonsoft.Json
- Single material_catalog.json database
- Flag-based filtering (IsInProjectPalette)

### WPF/UI Considerations
- Programmatic UI creation only
- MVVM pattern with BaseViewModel
- RelayCommand for command binding

## 📚 Documentation

### Code Documentation
- [ ] XML documentation comments (in Polish)
- [ ] Inline comments for complex logic
- [ ] README updates if needed

### Architecture Documentation
- [ ] Update technical architecture docs
- [ ] Add API documentation
- [ ] Update development guide

## 🚦 Definition of Done

- [ ] Code implemented and tested
- [ ] Unit tests passing (>80% coverage)
- [ ] Integration tests passing
- [ ] Code review completed
- [ ] Documentation updated
- [ ] Manual testing completed
- [ ] Performance benchmarks met
- [ ] No blocking bugs
- [ ] Merged to main branch

## 💬 Additional Notes

Any specific implementation details, gotchas, or context needed for this task.
