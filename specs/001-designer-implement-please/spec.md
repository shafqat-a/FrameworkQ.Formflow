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

### System Modes

The system operates in two distinct modes:

#### 1. Design Mode (Designer)
In design mode, users create and edit form definitions:
- User can create a new blank form design
- User can save the form design state without committing (draft state)
- User can export the draft design to YAML file
- User can import a design from YAML file to resume editing
- User can commit the form when design is complete, making it available for runtime
- Draft forms are editable and not available for data entry

#### 2. Runtime Mode (Form Execution)
Once a form is committed, it enters runtime mode for data collection:
- User can open a committed form for data entry
- User can save temporary form state (data entered but not submitted)
- User can resume a saved temporary state to continue data entry
- User can submit the form, which persists the data to the database
- Submitted form data is permanent and stored in database tables
- Runtime does not allow modifying the form structure, only data entry

### Primary User Story
A form designer needs to create complex data collection forms without writing YAML manually. They use a visual drag-and-drop interface to:
1. Create form structure with pages and sections
2. Add widgets (fields, tables, grids, checklists, groups) by dragging from a palette
3. Configure widget properties through an inspector panel
4. Preview the form layout
5. Save draft state and export/import via YAML during design
6. Commit the form definition to make it available for runtime
7. Generate SQL schema for data storage

### Acceptance Scenarios

#### Design Mode Scenarios

1. **Given** an empty designer canvas, **When** user creates a new form and adds a section, **Then** the section appears on the canvas ready to receive widgets

2. **Given** a section on the canvas, **When** user drags a "Field" widget from the palette and drops it into the section, **Then** the field widget appears in the section with default properties

3. **Given** a field widget is selected, **When** user modifies properties (name, label, type, required) in the inspector panel, **Then** the widget updates to reflect the new properties

4. **Given** a form with multiple widgets configured, **When** user clicks "Save Draft", **Then** the form definition is persisted as draft and can be reopened for editing

5. **Given** a draft form, **When** user clicks "Export YAML", **Then** a YAML file compliant with DSL v0.1 is generated and downloaded, preserving draft state

6. **Given** a YAML file from a previous session, **When** user clicks "Import YAML", **Then** the form design is restored to the canvas for continued editing

7. **Given** a draft form with table widgets, **When** user clicks "Generate SQL", **Then** SQL DDL statements for data storage are generated

8. **Given** a completed form design, **When** user clicks "Commit", **Then** the form is marked as committed and becomes available for runtime data entry

9. **Given** a form with unsaved changes, **When** user attempts to leave or close, **Then** system warns about unsaved changes

10. **Given** a table widget, **When** user adds columns with formulas, **Then** the column properties correctly store the formula expressions for later SQL generation

11. **Given** a multi-page form, **When** user adds/removes pages via page tabs, **Then** page navigation works correctly and widgets persist on their respective pages

12. **Given** a saved draft form, **When** user clicks "Open", **Then** the form loads with all pages, sections, and widgets restored to their saved state for editing

#### Runtime Mode Scenarios

13. **Given** a committed form, **When** user opens the form for data entry, **Then** the form renders all fields and widgets ready for input

14. **Given** a form with data partially entered, **When** user clicks "Save Progress", **Then** the current data state is saved as temporary state without submitting to database

15. **Given** a saved temporary state, **When** user reopens the form instance, **Then** all previously entered data is restored for continued entry

16. **Given** a form with all required fields completed, **When** user clicks "Submit", **Then** the data is validated and persisted to the database

17. **Given** submitted form data, **When** user queries the database, **Then** the data appears in the generated SQL tables

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

**Form Management - Design Mode**
- **FR-001**: System MUST allow users to create new blank form definitions with ID, title, and version
- **FR-002**: System MUST allow users to save form definitions as drafts for later editing
- **FR-003**: System MUST allow users to open previously saved draft forms for editing
- **FR-004**: System MUST allow users to export draft form definitions to YAML files
- **FR-005**: System MUST allow users to import form definitions from YAML files to resume editing
- **FR-006**: System MUST allow users to commit form definitions, making them available for runtime
- **FR-007**: System MUST prevent editing of committed forms (design is locked after commit)
- **FR-008**: System MUST warn users before discarding unsaved changes
- **FR-009**: System MUST validate form IDs against pattern `[a-z0-9_-]+`
- **FR-010**: System MUST support multi-page forms with user-controlled page addition/removal
- **FR-011**: System MUST distinguish between draft and committed forms in storage

**Form Management - Runtime Mode**
- **FR-012**: System MUST allow users to open committed forms for data entry
- **FR-013**: System MUST allow users to save temporary data entry state without submitting
- **FR-014**: System MUST allow users to resume data entry from saved temporary state
- **FR-015**: System MUST validate data against form constraints before submission
- **FR-016**: System MUST persist submitted form data to database tables
- **FR-017**: System MUST prevent modification of form structure during runtime
- **FR-018**: System MUST associate runtime data instances with the committed form definition
- **FR-019**: System MUST [NEEDS CLARIFICATION: user authentication/authorization not specified - who can create/edit/submit forms?]

**Widget Palette & Canvas**
- **FR-020**: System MUST provide a palette with draggable widgets: Field, Group, Table, Grid, Checklist
- **FR-021**: System MUST support drag-and-drop from palette to canvas sections
- **FR-022**: System MUST allow users to organize widgets within sections on each page
- **FR-023**: System MUST support section creation and deletion
- **FR-024**: System MUST allow section titles to be edited inline
- **FR-025**: System MUST support widget deletion from sections
- **FR-026**: System MUST generate unique IDs for new widgets

**Property Inspector**
- **FR-027**: System MUST display widget properties when a widget is selected
- **FR-028**: System MUST allow editing of common widget properties (ID, title)
- **FR-029**: System MUST provide type-specific property editors for each widget type
- **FR-030**: System MUST support field property configuration: name, label, type, required, readonly, placeholder, unit, enum values, formula
- **FR-031**: System MUST support table property configuration: row mode, columns with types and formulas
- **FR-032**: System MUST support grid property configuration: row/column generators, cell type
- **FR-033**: System MUST support checklist property configuration: items with keys, labels, types
- **FR-034**: System MUST support group property configuration: nested fields and layout

**Data Types & Validation**
- **FR-035**: System MUST support field types: string, text, integer, decimal, date, time, datetime, bool, enum, attachment, signature
- **FR-036**: System MUST validate property values according to their expected format
- **FR-037**: System MUST enforce naming patterns for IDs (lowercase, alphanumeric, hyphens, underscores)

**Export & Generation**
- **FR-038**: System MUST export form definitions as YAML compliant with DSL v0.1 specification
- **FR-039**: System MUST import form definitions from YAML files
- **FR-040**: System MUST generate SQL DDL statements for table widgets
- **FR-041**: System MUST generate SQL DDL statements for grid widgets
- **FR-042**: System MUST include appropriate indexes in generated SQL
- **FR-043**: System MUST translate formula expressions to SQL computed columns
- **FR-044**: System MUST map DSL data types to appropriate SQL types

**Preview**
- **FR-045**: System MUST provide a preview mode to visualize the form layout [NEEDS CLARIFICATION: Should preview be interactive or static?]

**Data Persistence - Design Mode**
- **FR-046**: System MUST persist form definitions (draft and committed) in a database
- **FR-047**: System MUST store form definitions in a structured format (JSON)
- **FR-048**: System MUST track form metadata: created date, updated date, is_committed flag, active status
- **FR-049**: System MUST [NEEDS CLARIFICATION: version control for forms not specified - how are form versions managed?]
- **FR-050**: System MUST [NEEDS CLARIFICATION: multi-tenancy not specified - are forms isolated per organization/user?]

**Data Persistence - Runtime Mode**
- **FR-051**: System MUST persist temporary runtime data state separately from submitted data
- **FR-052**: System MUST associate temporary state with form instance and user session
- **FR-053**: System MUST store submitted form data in SQL tables generated from form definition
- **FR-054**: System MUST maintain referential integrity between form instances and form definitions
- **FR-055**: System MUST support querying submitted data via standard SQL

**User Experience**
- **FR-056**: System MUST provide visual feedback during drag-and-drop operations
- **FR-057**: System MUST highlight selected widgets
- **FR-058**: System MUST show drag-over state on valid drop zones
- **FR-059**: System MUST maintain form state during page navigation
- **FR-060**: System MUST provide undo/redo capabilities [NEEDS CLARIFICATION: Is undo/redo required or future enhancement?]

**Compliance & Standards**
- **FR-061**: System MUST generate YAML that validates against DSL v0.1 JSON schema
- **FR-062**: System MUST ensure exported YAML can be consumed by form rendering engines
- **FR-063**: System MUST generate PostgreSQL-compatible SQL DDL

### Key Entities

#### Design Mode Entities

- **Form Definition**: Represents a complete form with ID, title, version, locale settings, metadata (organization, document number, effective date, revision, reference, tags), options (print settings, permissions), storage configuration, is_committed flag, and collection of pages

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

#### Runtime Mode Entities

- **Form Instance**: A specific runtime session where a user is entering data into a committed form. Links to the committed Form Definition. Has a unique instance ID and status (draft/submitted)

- **Temporary Data State**: Saved progress of data entry in a form instance. Contains partial field values, table row data, and user session information. Can be resumed later

- **Submitted Data Record**: Finalized form data stored in database tables. Immutable after submission. Stored in SQL tables generated from the form definition

- **Form Instance Metadata**: Tracks instance creation time, submission time, user information, and status transitions

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
- Form rendering engine must exist to consume exported YAML (for runtime mode)
- Database infrastructure for persistence (specification suggests PostgreSQL)
- SQL table schema must be created from generated DDL before form data submission

**Assumptions:**
- Users have basic understanding of form design concepts
- Forms are primarily for data collection workflows
- YAML export/import is the mechanism for saving and resuming design work
- SQL generation targets PostgreSQL dialect
- Browser environment supports HTML5 drag-and-drop
- Clear separation between design-time (designer) and runtime (form execution)
- Draft forms can be edited; committed forms are locked for editing
- Runtime data is stored separately from form definitions
- Temporary state and submitted data are distinct storage layers

---

## Success Criteria

**Design Mode - User can successfully:**
1. Create a complete multi-page form definition in under 30 minutes without writing code
2. Configure complex table widgets with 10+ columns including formulas
3. Save form as draft and resume editing later
4. Export form as valid DSL v0.1 YAML file
5. Import form from YAML file to continue editing
6. Commit form to make it available for runtime
7. Generate SQL DDL that creates working database tables
8. Understand form structure through visual representation alone

**Runtime Mode - User can successfully:**
9. Open a committed form for data entry
10. Enter data into all field types and table widgets
11. Save progress without submitting (temporary state)
12. Resume data entry from saved temporary state
13. Submit completed form data to database
14. Query submitted data from generated SQL tables

**System quality:**
- Generated YAML passes DSL v0.1 schema validation 100% of the time
- Generated SQL executes without errors on PostgreSQL
- Imported YAML correctly reconstructs form in designer
- UI responds to drag-drop operations within 200ms [NEEDS CLARIFICATION: performance requirements not specified]
- No data loss on save/load operations
- Draft and committed forms are clearly distinguished
- Temporary runtime state and submitted data are properly isolated
