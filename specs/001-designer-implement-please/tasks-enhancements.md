# Tasks: Form Designer Enhancements (Enterprise Forms Support)

**Based on**: Power Grid Company of Bangladesh form requirements
**Prerequisites**: Core implementation complete (T001-T063)

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions
- **Backend**: `backend/src/FormDesigner.API/`
- **Frontend**: `backend/src/FormDesigner.API/wwwroot/`
- **Tests**: `backend/tests/`

---

## Phase 4.1: Enhanced Widgets - Backend Models

- [ ] **T100** [P] Create FormHeader widget DTO in `backend/src/FormDesigner.API/Models/DTOs/Widgets/FormHeaderSpec.cs`
  - Properties: document_no, revision_no, effective_date, page_number, organization, form_title
  - JSON serialization with snake_case

- [ ] **T101** [P] Create Signature widget DTO in `backend/src/FormDesigner.API/Models/DTOs/Widgets/SignatureSpec.cs`
  - Properties: role, name, designation, date, signature_image (optional)
  - Support multiple signatures per form

- [ ] **T102** [P] Create Notes widget DTO in `backend/src/FormDesigner.API/Models/DTOs/Widgets/NotesSpec.cs`
  - Properties: content (text), formatting (markdown/plain), style (info/warning/note)

- [ ] **T103** [P] Create HierarchicalChecklist widget DTO in `backend/src/FormDesigner.API/Models/DTOs/Widgets/HierarchicalChecklistSpec.cs`
  - Properties: items (nested structure), numbering_style (1.0/1.1 or a/b)
  - Support parent-child relationships

- [ ] **T104** [P] Create RadioGroup widget DTO in `backend/src/FormDesigner.API/Models/DTOs/Widgets/RadioGroupSpec.cs`
  - Properties: options (array of {label, value}), orientation (horizontal/vertical)

- [ ] **T105** [P] Create CheckboxGroup widget DTO in `backend/src/FormDesigner.API/Models/DTOs/Widgets/CheckboxGroupSpec.cs`
  - Properties: options (array of {label, value}), min_selections, max_selections

- [ ] **T106** [P] Create TimePicker field spec in `backend/src/FormDesigner.API/Models/DTOs/TimePickerSpec.cs`
  - Properties: format (12h/24h), min_time, max_time, step_minutes

- [ ] **T107** Extend TableSpec for advanced features in `backend/src/FormDesigner.API/Models/DTOs/TableSpec.cs`
  - Add multi_row_headers property (array of header rows)
  - Add merged_cells property (array of {row, col, rowspan, colspan})
  - Add checkbox_columns and radio_columns arrays

- [ ] **T108** Create Section hierarchy support in `backend/src/FormDesigner.API/Models/DTOs/Section.cs`
  - Add parent_section_id property (nullable)
  - Add numbering_style property (auto, manual, none)
  - Add level property (0 for root, 1+ for nested)

---

## Phase 4.2: Enhanced Widgets - Frontend Designer

- [ ] **T109** Add FormHeader widget to palette in `backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - Add to widget palette with icon
  - Implement drag-drop handler
  - Add to getDefaultWidgetSpec()

- [ ] **T110** Add Signature widget to palette in `backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - Add to widget palette with icon
  - Implement drag-drop handler
  - Add to getDefaultWidgetSpec()

- [ ] **T111** Add Notes widget to palette in `backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - Add to widget palette with icon
  - Implement drag-drop handler
  - Add to getDefaultWidgetSpec()

- [ ] **T112** Add HierarchicalChecklist widget to palette in `backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - Add to widget palette with icon
  - Implement drag-drop handler with nesting support
  - Add to getDefaultWidgetSpec()

- [ ] **T113** Add RadioGroup widget to palette in `backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - Add to widget palette with icon
  - Implement drag-drop handler
  - Add to getDefaultWidgetSpec()

- [ ] **T114** Add CheckboxGroup widget to palette in `backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - Add to widget palette with icon
  - Implement drag-drop handler
  - Add to getDefaultWidgetSpec()

- [ ] **T115** Add TimePicker field type in `backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - Add to field type dropdown in inspector
  - Add time-specific properties (format, min, max)

- [ ] **T116** Implement advanced table editor in `backend/src/FormDesigner.API/wwwroot/js/table-editor.js`
  - Visual editor for merged cells (colspan/rowspan)
  - Multi-row header configuration
  - Checkbox/radio column type support
  - Preview of table structure

- [ ] **T117** Implement nested section support in `backend/src/FormDesigner.API/wwwroot/js/section-manager.js`
  - Add "Add Sub-section" button to sections
  - Implement hierarchical numbering (1.0, 1.1, 1.2)
  - Visual indentation for nested sections
  - Drag-drop widgets to nested sections

---

## Phase 4.3: Enhanced Widgets - Property Inspectors

- [ ] **T118** [P] Create FormHeader inspector in `backend/src/FormDesigner.API/wwwroot/js/inspectors/form-header-inspector.js`
  - Properties: document_no, revision_no, effective_date, page_number
  - Organization and form_title fields

- [ ] **T119** [P] Create Signature inspector in `backend/src/FormDesigner.API/wwwroot/js/inspectors/signature-inspector.js`
  - Properties: role, name, designation, date
  - Multiple signature configuration

- [ ] **T120** [P] Create Notes inspector in `backend/src/FormDesigner.API/wwwroot/js/inspectors/notes-inspector.js`
  - Content editor (textarea)
  - Formatting style dropdown
  - Preview area

- [ ] **T121** [P] Create HierarchicalChecklist inspector in `backend/src/FormDesigner.API/wwwroot/js/inspectors/hierarchical-checklist-inspector.js`
  - Item tree editor
  - Numbering style selector
  - Add/remove/nest items

- [ ] **T122** [P] Create RadioGroup inspector in `backend/src/FormDesigner.API/wwwroot/js/inspectors/radio-group-inspector.js`
  - Options editor (add/remove/reorder)
  - Orientation selector

- [ ] **T123** [P] Create CheckboxGroup inspector in `backend/src/FormDesigner.API/wwwroot/js/inspectors/checkbox-group-inspector.js`
  - Options editor (add/remove/reorder)
  - Min/max selections

- [ ] **T124** [P] Create TimePicker inspector in `backend/src/FormDesigner.API/wwwroot/js/inspectors/time-picker-inspector.js`
  - Format selector (12h/24h)
  - Min/max time range
  - Step minutes

- [ ] **T125** Enhance table inspector in `backend/src/FormDesigner.API/wwwroot/js/inspectors/table-inspector.js`
  - Add merged cells configuration
  - Add multi-row headers editor
  - Add checkbox/radio column type option

---

## Phase 4.4: Enhanced Widgets - Runtime Rendering

- [ ] **T126** [P] Implement FormHeader rendering in `backend/src/FormDesigner.API/wwwroot/js/runtime.js`
  - Render document header with metadata
  - Format: table layout with document info
  - Page number display

- [ ] **T127** [P] Implement Signature rendering in `backend/src/FormDesigner.API/wwwroot/js/runtime.js`
  - Signature capture area
  - Role/name/designation display
  - Date auto-fill
  - Optional image upload

- [ ] **T128** [P] Implement Notes rendering in `backend/src/FormDesigner.API/wwwroot/js/runtime.js`
  - Styled note box
  - Different styles (info/warning/note)
  - Markdown support (optional)

- [ ] **T129** [P] Implement HierarchicalChecklist rendering in `backend/src/FormDesigner.API/wwwroot/js/runtime.js`
  - Nested checkbox structure
  - Hierarchical numbering display
  - Indentation for nested items

- [ ] **T130** [P] Implement RadioGroup rendering in `backend/src/FormDesigner.API/wwwroot/js/runtime.js`
  - Radio button group
  - Horizontal/vertical layout
  - Single selection validation

- [ ] **T131** [P] Implement CheckboxGroup rendering in `backend/src/FormDesigner.API/wwwroot/js/runtime.js`
  - Checkbox group
  - Multi-selection
  - Min/max validation

- [ ] **T132** [P] Implement TimePicker rendering in `backend/src/FormDesigner.API/wwwroot/js/runtime.js`
  - Time input field
  - 12h/24h format
  - Time validation

- [ ] **T133** Enhance table rendering in `backend/src/FormDesigner.API/wwwroot/js/runtime.js`
  - Multi-row headers rendering
  - Merged cells (colspan/rowspan)
  - Checkbox/radio columns

- [ ] **T134** Implement nested section rendering in `backend/src/FormDesigner.API/wwwroot/js/runtime.js`
  - Hierarchical section display
  - Automatic numbering (1.0, 1.1, 1.2)
  - Visual indentation

---

## Phase 4.5: Calculation Engine

- [ ] **T135** Create formula parser in `backend/src/FormDesigner.API/wwwroot/js/formula-parser.js`
  - Parse formula expressions
  - Support arithmetic operators (+, -, *, /)
  - Support field references
  - Support functions (SUM, AVG, etc.)

- [ ] **T136** Create formula evaluator in `backend/src/FormDesigner.API/wwwroot/js/formula-evaluator.js`
  - Evaluate formulas with current form data
  - Handle null/undefined values
  - Type coercion (string to number)
  - Error handling

- [ ] **T137** Implement auto-calculation in runtime in `backend/src/FormDesigner.API/wwwroot/js/runtime.js`
  - Listen to field changes
  - Re-calculate dependent fields
  - Update computed values in real-time
  - Support table column totals

- [ ] **T138** Add calculation preview in designer in `backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - Show formula in inspector
  - Validate formula syntax
  - Preview calculated value (with sample data)

---

## Phase 4.6: Page Layout & Print Support

- [ ] **T139** Add page layout configuration in `backend/src/FormDesigner.API/Models/DTOs/PageLayout.cs`
  - Properties: orientation (portrait/landscape)
  - Page size (A4, Letter, Legal, custom)
  - Margins configuration

- [ ] **T140** Implement page layout UI in `backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - Page settings button
  - Orientation selector
  - Page size dropdown
  - Margins editor

- [ ] **T141** Create print stylesheet in `backend/src/FormDesigner.API/wwwroot/css/print.css`
  - Print-friendly layouts
  - Page break control
  - Hide non-printable elements
  - Landscape/portrait support

- [ ] **T142** Add print preview in runtime in `backend/src/FormDesigner.API/wwwroot/js/runtime.js`
  - Print preview button
  - Generate print-friendly HTML
  - Show page breaks
  - Export to PDF (optional)

---

## Phase 4.7: Enhanced Styling

- [ ] **T143** Create enterprise form stylesheet in `backend/src/FormDesigner.API/wwwroot/css/enterprise-forms.css`
  - Table borders and styling (matching screenshots)
  - Form header styling
  - Signature block styling
  - Hierarchical numbering styles
  - Merged cell borders

- [ ] **T144** Add custom widget styling in `backend/src/FormDesigner.API/wwwroot/css/widgets-enhanced.css`
  - FormHeader widget styles
  - Signature widget styles
  - Notes widget styles (info/warning/note)
  - HierarchicalChecklist styles
  - RadioGroup/CheckboxGroup styles
  - TimePicker styles

---

## Phase 4.8: DSL v0.1 Extensions

- [ ] **T145** Extend YAML export for new widgets in `backend/src/FormDesigner.API/Services/YamlExportService.cs`
  - Export FormHeader spec
  - Export Signature spec
  - Export Notes spec
  - Export HierarchicalChecklist spec
  - Export RadioGroup spec
  - Export CheckboxGroup spec
  - Export TimePicker spec
  - Export advanced table features

- [ ] **T146** Extend YAML import for new widgets in `backend/src/FormDesigner.API/Services/YamlImportService.cs`
  - Import FormHeader spec
  - Import Signature spec
  - Import Notes spec
  - Import HierarchicalChecklist spec
  - Import RadioGroup spec
  - Import CheckboxGroup spec
  - Import TimePicker spec
  - Import advanced table features

- [ ] **T147** Update SQL generator for new widgets in `backend/src/FormDesigner.API/Services/SqlGeneratorService.cs`
  - Generate schema for FormHeader
  - Generate schema for Signature
  - Generate schema for Notes
  - Generate schema for HierarchicalChecklist
  - Generate schema for RadioGroup/CheckboxGroup
  - Generate schema for TimePicker
  - Handle merged cells in DDL
  - Generate multi-row header comments

---

## Phase 4.9: Validation Extensions

- [ ] **T148** Extend FluentValidation for new widgets in `backend/src/FormDesigner.API/Validators/`
  - FormHeaderValidator.cs
  - SignatureValidator.cs
  - NotesValidator.cs
  - HierarchicalChecklistValidator.cs
  - RadioGroupValidator.cs
  - CheckboxGroupValidator.cs
  - TimePickerValidator.cs

- [ ] **T149** Add runtime validation for new widgets in `backend/src/FormDesigner.API/wwwroot/js/validators/`
  - Validate time format
  - Validate radio selection
  - Validate checkbox min/max
  - Validate signature required fields
  - Validate hierarchical checklist

---

## Phase 4.10: Testing & Documentation

- [ ] **T150** Create sample enterprise forms in `backend/tests/FormDesigner.Tests.Integration/SampleForms/`
  - Power Grid Sub-Station Performance Report
  - Monthly Shift Duty Roaster
  - Log Sheet (Hourly readings)
  - Surveillance Visit Checklist
  - Daily Inspection Check Sheet
  - Transformer Inspection & Maintenance

- [ ] **T151** Write unit tests for new widgets in `backend/tests/FormDesigner.Tests.Unit/`
  - FormHeader widget tests
  - Signature widget tests
  - Notes widget tests
  - HierarchicalChecklist tests
  - RadioGroup/CheckboxGroup tests
  - TimePicker tests
  - Formula parser tests
  - Formula evaluator tests

- [ ] **T152** Write integration tests for enhanced features in `backend/tests/FormDesigner.Tests.Integration/`
  - Create form with new widgets
  - Export YAML with new widgets
  - Import YAML with new widgets
  - Generate SQL with new widgets
  - Runtime data entry with new widgets
  - Formula calculation tests

- [ ] **T153** Create user documentation in `doc/user-guide/`
  - Enterprise Forms Guide (enterprise-forms.md)
  - Widget Reference (widget-reference.md)
  - Formula Guide (formulas.md)
  - Print & Layout Guide (print-guide.md)
  - Sample Forms Gallery (samples.md)

- [ ] **T154** Update API documentation in `doc/api/`
  - New widget endpoints
  - Enhanced table API
  - Formula calculation API
  - Page layout API

---

## Summary

**Total New Tasks**: 55 (T100-T154)
**Estimated Effort**: 60-90 hours
**Dependencies**: Core implementation (T001-T063) must be complete

### Phases Overview:
- **Phase 4.1**: Backend Models (9 tasks, 10-12h)
- **Phase 4.2**: Frontend Designer (9 tasks, 12-15h)
- **Phase 4.3**: Property Inspectors (8 tasks, 8-10h)
- **Phase 4.4**: Runtime Rendering (9 tasks, 10-12h)
- **Phase 4.5**: Calculation Engine (4 tasks, 6-8h)
- **Phase 4.6**: Page Layout (4 tasks, 4-6h)
- **Phase 4.7**: Enhanced Styling (2 tasks, 3-4h)
- **Phase 4.8**: DSL Extensions (3 tasks, 4-6h)
- **Phase 4.9**: Validation (2 tasks, 3-4h)
- **Phase 4.10**: Testing & Docs (5 tasks, 10-15h)

### Priority Order:
1. **HIGH**: T100-T108 (Backend Models) - Foundation
2. **HIGH**: T109-T117 (Frontend Designer) - User-facing
3. **HIGH**: T126-T134 (Runtime Rendering) - Core functionality
4. **MEDIUM**: T118-T125 (Inspectors) - Configuration
5. **MEDIUM**: T135-T138 (Calculations) - Enterprise feature
6. **LOW**: T139-T154 (Layout, Styling, Testing, Docs) - Polish
