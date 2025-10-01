# Feature Specification: Visual Form Designer

**Feature Branch**: `001-designer-implement-please`
**Created**: 2025-10-01
**Status**: Draft
**Input**: User description: "designer-implement - Please read doc/spec/designer.md and implement"

## Execution Flow (main)
```
1. Parse user description from Input
   → Identified: "Implement visual form designer from specification"
2. Extract key concepts from description
   → Actors: Form designers, users creating forms
   → Actions: Drag-drop widgets, configure properties, export YAML/SQL
   → Data: Form definitions, widgets, pages, sections
   → Constraints: DSL v0.1 compliance
3. For each unclear aspect:
   → [NEEDS CLARIFICATION: Should users be authenticated/authorized?]
   → [NEEDS CLARIFICATION: Multi-tenant support - single database or isolated?]
   → [NEEDS CLARIFICATION: Maximum form size/complexity limits?]
4. Fill User Scenarios & Testing section
   → Primary flow: Create form → drag widgets → configure → export
5. Generate Functional Requirements
   → Each requirement mapped to designer capabilities
6. Identify Key Entities
   → Form, Page, Section, Widget, Field specs
7. Run Review Checklist
   → WARN "Spec has uncertainties in auth, multi-tenancy, and limits"
8. Return: SUCCESS (spec ready for planning with clarifications)
```

---

## ⚡ Quick Guidelines
- ✅ Focus on WHAT users need and WHY
- ❌ Avoid HOW to implement (no tech stack, APIs, code structure)
- 👥 Written for business stakeholders, not developers

---

## User Scenarios & Testing *(mandatory)*

### Primary User Story
A form designer needs to create complex data collection forms without writing YAML manually. They use a visual drag-and-drop interface to:
1. Create form structure with pages and sections
2. Add widgets (fields, tables, grids, checklists, groups) by dragging from a palette
3. Configure widget properties through an inspector panel
4. Preview the form layout
5. Export the form definition as YAML compliant with DSL v0.1
6. Generate SQL schema for data storage

### Acceptance Scenarios

1. **Given** an empty designer canvas, **When** user creates a new form and adds a section, **Then** the section appears on the canvas ready to receive widgets

2. **Given** a section on the canvas, **When** user drags a "Field" widget from the palette and drops it into the section, **Then** the field widget appears in the section with default properties

3. **Given** a field widget is selected, **When** user modifies properties (name, label, type, required) in the inspector panel, **Then** the widget updates to reflect the new properties

4. **Given** a form with multiple widgets configured, **When** user clicks "Save", **Then** the form definition is persisted and can be reopened later

5. **Given** a saved form, **When** user clicks "Export YAML", **Then** a YAML file compliant with DSL v0.1 is generated and downloaded

6. **Given** a form with table widgets, **When** user clicks "Generate SQL", **Then** SQL DDL statements for data storage are generated

7. **Given** a form with unsaved changes, **When** user attempts to leave or close, **Then** system warns about unsaved changes

8. **Given** a table widget, **When** user adds columns with formulas, **Then** the column properties correctly store the formula expressions for later SQL generation

9. **Given** a multi-page form, **When** user adds/removes pages via page tabs, **Then** page navigation works correctly and widgets persist on their respective pages

10. **Given** an existing form, **When** user clicks "Open", **Then** the form loads with all pages, sections, and widgets restored to their saved state

### Edge Cases
- What happens when a user drags a widget to an invalid drop target?
  - Widget should not be added, drop zone should not accept it

- How does the system handle invalid property values (e.g., non-lowercase IDs)?
  - System should validate on input and show error messages

- What happens when exporting a form with no widgets?
  - System should generate valid but minimal YAML structure

- How are computed fields (formulas) validated?
  - [NEEDS CLARIFICATION: Should formula syntax be validated in real-time?]

- What is the maximum number of widgets, pages, or table columns?
  - [NEEDS CLARIFICATION: Performance limits not specified]

- How are conflicting widget IDs handled?
  - System should enforce unique IDs or auto-rename conflicts

## Requirements *(mandatory)*

### Functional Requirements

**Form Management**
- **FR-001**: System MUST allow users to create new form definitions with ID, title, and version
- **FR-002**: System MUST allow users to save form definitions for later editing
- **FR-003**: System MUST allow users to open previously saved forms
- **FR-004**: System MUST warn users before discarding unsaved changes
- **FR-005**: System MUST validate form IDs against pattern `[a-z0-9_-]+`
- **FR-006**: System MUST support multi-page forms with user-controlled page addition/removal
- **FR-007**: System MUST [NEEDS CLARIFICATION: user authentication/authorization not specified - who can create/edit forms?]

**Widget Palette & Canvas**
- **FR-008**: System MUST provide a palette with draggable widgets: Field, Group, Table, Grid, Checklist
- **FR-009**: System MUST support drag-and-drop from palette to canvas sections
- **FR-010**: System MUST allow users to organize widgets within sections on each page
- **FR-011**: System MUST support section creation and deletion
- **FR-012**: System MUST allow section titles to be edited inline
- **FR-013**: System MUST support widget deletion from sections
- **FR-014**: System MUST generate unique IDs for new widgets

**Property Inspector**
- **FR-015**: System MUST display widget properties when a widget is selected
- **FR-016**: System MUST allow editing of common widget properties (ID, title)
- **FR-017**: System MUST provide type-specific property editors for each widget type
- **FR-018**: System MUST support field property configuration: name, label, type, required, readonly, placeholder, unit, enum values, formula
- **FR-019**: System MUST support table property configuration: row mode, columns with types and formulas
- **FR-020**: System MUST support grid property configuration: row/column generators, cell type
- **FR-021**: System MUST support checklist property configuration: items with keys, labels, types
- **FR-022**: System MUST support group property configuration: nested fields and layout

**Data Types & Validation**
- **FR-023**: System MUST support field types: string, text, integer, decimal, date, time, datetime, bool, enum, attachment, signature
- **FR-024**: System MUST validate property values according to their expected format
- **FR-025**: System MUST enforce naming patterns for IDs (lowercase, alphanumeric, hyphens, underscores)

**Export & Generation**
- **FR-026**: System MUST export form definitions as YAML compliant with DSL v0.1 specification
- **FR-027**: System MUST generate SQL DDL statements for table widgets
- **FR-028**: System MUST generate SQL DDL statements for grid widgets
- **FR-029**: System MUST include appropriate indexes in generated SQL
- **FR-030**: System MUST translate formula expressions to SQL computed columns
- **FR-031**: System MUST map DSL data types to appropriate SQL types

**Preview**
- **FR-032**: System MUST provide a preview mode to visualize the form layout [NEEDS CLARIFICATION: Should preview be interactive or static?]

**Data Persistence**
- **FR-033**: System MUST persist form definitions in a database
- **FR-034**: System MUST store form definitions in a structured format (JSON)
- **FR-035**: System MUST track form metadata: created date, updated date, active status
- **FR-036**: System MUST [NEEDS CLARIFICATION: version control for forms not specified - how are form versions managed?]
- **FR-037**: System MUST [NEEDS CLARIFICATION: multi-tenancy not specified - are forms isolated per organization/user?]

**User Experience**
- **FR-038**: System MUST provide visual feedback during drag-and-drop operations
- **FR-039**: System MUST highlight selected widgets
- **FR-040**: System MUST show drag-over state on valid drop zones
- **FR-041**: System MUST maintain form state during page navigation
- **FR-042**: System MUST provide undo/redo capabilities [NEEDS CLARIFICATION: Is undo/redo required or future enhancement?]

**Compliance & Standards**
- **FR-043**: System MUST generate YAML that validates against DSL v0.1 JSON schema
- **FR-044**: System MUST ensure exported YAML can be consumed by form rendering engines
- **FR-045**: System MUST generate PostgreSQL-compatible SQL DDL

### Key Entities

- **Form Definition**: Represents a complete form with ID, title, version, locale settings, metadata (organization, document number, effective date, revision, reference, tags), options (print settings, permissions), storage configuration, and collection of pages

- **Page**: A logical grouping within a form containing ID, title, multilingual labels, and collection of sections. Forms can have multiple pages for complex workflows

- **Section**: A container within a page with ID, title, multilingual labels, and collection of widgets. Sections organize related widgets visually

- **Widget**: The base unit of form interaction. Types include:
  - **Field Widget**: Single data input with name, label, type, validation rules, placeholder, default value, unit, formula for computed values
  - **Group Widget**: Container for related fields with optional layout configuration (columns)
  - **Table Widget**: Repeating rows with defined columns, row mode (finite/infinite), min/max rows, row generators, aggregates
  - **Grid Widget**: Two-dimensional data entry with row/column generators and cell type specification
  - **Checklist Widget**: List of checkbox items with keys, labels, and types

- **Field Specification**: Data type definition including type (string, integer, decimal, date, time, datetime, bool, enum, etc.), constraints (required, readonly, min, max, pattern), enum values, default value, formula expression, formatting rules

- **Table Column**: Column definition within table widget with name, label, type, validation rules, formula expression, formatting

- **Form Metadata**: Organizational information including document number, effective date, revision number, reference links, tags for categorization

- **Storage Configuration**: Defines how form data is persisted including storage mode, header fields to copy to detail tables, index specifications

- **Row/Column Generators**: Mechanisms to dynamically generate table rows or grid dimensions based on ranges (integer, date/time) or value lists

---

## Review & Acceptance Checklist

### Content Quality
- [x] No implementation details (languages, frameworks, APIs)
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed

### Requirement Completeness
- [ ] No [NEEDS CLARIFICATION] markers remain - **6 clarifications identified**
- [x] Requirements are testable and unambiguous (except clarified items)
- [x] Success criteria are measurable
- [x] Scope is clearly bounded
- [x] Dependencies and assumptions identified

**Outstanding Clarifications:**
1. User authentication and authorization model
2. Multi-tenant data isolation strategy
3. Maximum form complexity limits (pages, widgets, columns)
4. Formula validation approach (real-time vs. export-time)
5. Form version control and history management
6. Preview mode interactivity level
7. Undo/redo requirement priority

---

## Execution Status

- [x] User description parsed
- [x] Key concepts extracted
- [x] Ambiguities marked (6 clarifications needed)
- [x] User scenarios defined
- [x] Requirements generated (45 functional requirements)
- [x] Entities identified (10 key entities)
- [ ] Review checklist passed - **Blocked on clarifications**

---

## Dependencies & Assumptions

**Dependencies:**
- DSL v0.1 specification must be finalized and accessible
- Form rendering engine must exist to consume exported YAML
- Database infrastructure for persistence (specification suggests PostgreSQL)

**Assumptions:**
- Users have basic understanding of form design concepts
- Forms are primarily for data collection workflows
- YAML export is the primary integration mechanism
- SQL generation targets PostgreSQL dialect
- Browser environment supports HTML5 drag-and-drop
- Forms are designed once and rendered many times (design-time vs. runtime separation)

---

## Success Criteria

**User can successfully:**
1. Create a complete multi-page form definition in under 30 minutes without writing code
2. Configure complex table widgets with 10+ columns including formulas
3. Export form as valid DSL v0.1 YAML that renders correctly in form engine
4. Generate SQL DDL that creates working database tables
5. Save and reopen forms without data loss
6. Understand form structure through visual representation alone

**System quality:**
- Generated YAML passes DSL v0.1 schema validation 100% of the time
- Generated SQL executes without errors on PostgreSQL
- UI responds to drag-drop operations within 200ms [NEEDS CLARIFICATION: performance requirements not specified]
- No data loss on save/load operations
