# Tasks: Visual Form Designer

**Input**: Design documents from `/specs/001-designer-implement-please/`
**Prerequisites**: plan.md, research.md, data-model.md, contracts/forms-api.yaml, quickstart.md

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions
- **Backend**: `backend/src/FormDesigner.API/`
- **Frontend**: `backend/src/FormDesigner.API/wwwroot/`
- **Tests**: `backend/tests/`

---

## Phase 3.1: Setup & Infrastructure

- [ ] **T001** Create backend project structure
  - Create `backend/src/FormDesigner.API/` directory
  - Initialize .NET 8.0 Web API project: `dotnet new webapi -n FormDesigner.API`
  - Create directory structure: Controllers/, Services/, Data/, Models/, Validators/
  - Create `backend/src/FormDesigner.API/wwwroot/` for frontend files (css/, js/)

- [ ] **T002** [P] Create test project structure
  - Create `backend/tests/FormDesigner.Tests.Contract/` directory
  - Initialize xUnit project: `dotnet new xunit`
  - Create `backend/tests/FormDesigner.Tests.Integration/` directory
  - Initialize xUnit project: `dotnet new xunit`
  - Create `backend/tests/FormDesigner.Tests.Unit/` directory
  - Initialize xUnit project: `dotnet new xunit`

- [ ] **T003** Configure NuGet packages in `backend/src/FormDesigner.API/FormDesigner.API.csproj`
  - Add EntityFrameworkCore 8.0.0
  - Add Npgsql.EntityFrameworkCore.PostgreSQL 8.0.0
  - Add YamlDotNet 13.7.1
  - Add Swashbuckle.AspNetCore 6.5.0
  - Add FluentValidation.AspNetCore 11.3.0

- [ ] **T004** [P] Configure test packages in test projects
  - Add xUnit 2.6.2
  - Add Moq 4.20.70
  - Add FluentAssertions 6.12.0
  - Add Microsoft.AspNetCore.Mvc.Testing 8.0.0 (integration tests only)

- [ ] **T005** Configure database connection in `backend/src/FormDesigner.API/appsettings.json`
  - Add ConnectionStrings:DefaultConnection for PostgreSQL
  - Configure logging levels
  - Configure CORS allowed origins

- [ ] **T006** Create EF Core migrations setup script
  - Create `backend/scripts/setup-database.sh`
  - Script to create PostgreSQL database `formdesigner`
  - Script to run: `dotnet ef migrations add InitialCreate`
  - Script to run: `dotnet ef database update`

---

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3

**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**

### Contract Tests (API Endpoints)

- [ ] **T007** [P] Contract test GET /api/forms in `backend/tests/FormDesigner.Tests.Contract/FormsApiGetListTests.cs`
  - Test: Returns 200 with empty array when no forms exist
  - Test: Returns 200 with forms array when forms exist
  - Test: Returns 500 on database error
  - Assert response schema matches OpenAPI spec

- [ ] **T008** [P] Contract test POST /api/forms in `backend/tests/FormDesigner.Tests.Contract/FormsApiPostTests.cs`
  - Test: Returns 201 with Location header on success
  - Test: Returns 400 on invalid form ID pattern
  - Test: Returns 400 on missing required fields
  - Test: Returns 409 when form ID already exists
  - Assert request/response schemas match OpenAPI spec

- [ ] **T009** [P] Contract test GET /api/forms/{id} in `backend/tests/FormDesigner.Tests.Contract/FormsApiGetByIdTests.cs`
  - Test: Returns 200 with form definition
  - Test: Returns 404 when form not found
  - Assert response schema matches OpenAPI spec

- [ ] **T010** [P] Contract test PUT /api/forms/{id} in `backend/tests/FormDesigner.Tests.Contract/FormsApiPutTests.cs`
  - Test: Returns 200 on successful update
  - Test: Returns 400 on ID mismatch (path vs body)
  - Test: Returns 404 when form not found
  - Assert request/response schemas match OpenAPI spec

- [ ] **T011** [P] Contract test DELETE /api/forms/{id} in `backend/tests/FormDesigner.Tests.Contract/FormsApiDeleteTests.cs`
  - Test: Returns 204 on successful delete
  - Test: Returns 404 when form not found
  - Verify soft delete (is_active=false)

- [ ] **T012** [P] Contract test GET /api/export/{id}/yaml in `backend/tests/FormDesigner.Tests.Contract/ExportYamlTests.cs`
  - Test: Returns 200 with application/x-yaml content type
  - Test: Returns Content-Disposition header with filename
  - Test: Returns 404 when form not found

- [ ] **T013** [P] Contract test GET /api/export/{id}/sql in `backend/tests/FormDesigner.Tests.Contract/ExportSqlTests.cs`
  - Test: Returns 200 with text/plain content type
  - Test: Returns Content-Disposition header with filename
  - Test: Returns 404 when form not found

- [ ] **T013a** [P] Contract test POST /api/import/yaml in `backend/tests/FormDesigner.Tests.Contract/ImportYamlTests.cs`
  - Test: Returns 201 on successful import
  - Test: Returns 400 on invalid YAML format
  - Test: Returns 200 when updating existing form

- [ ] **T013b** [P] Contract test POST /api/forms/{id}/commit in `backend/tests/FormDesigner.Tests.Contract/CommitFormTests.cs`
  - Test: Returns 200 on successful commit
  - Test: Returns 404 when form not found
  - Test: Returns 409 when already committed

- [ ] **T013c** [P] Contract test GET /api/runtime/forms in `backend/tests/FormDesigner.Tests.Contract/RuntimeFormsTests.cs`
  - Test: Returns 200 with array of committed forms
  - Test: Returns empty array when no committed forms

- [ ] **T013d** [P] Contract test POST /api/runtime/instances in `backend/tests/FormDesigner.Tests.Contract/RuntimeInstancesTests.cs`
  - Test: Returns 201 with instance ID
  - Test: Returns 404 when form not found
  - Test: Returns 400 when form not committed

- [ ] **T013e** [P] Contract test PUT /api/runtime/instances/{id}/save in `backend/tests/FormDesigner.Tests.Contract/RuntimeSaveProgressTests.cs`
  - Test: Returns 200 on successful save
  - Test: Returns 404 when instance not found

- [ ] **T013f** [P] Contract test POST /api/runtime/instances/{id}/submit in `backend/tests/FormDesigner.Tests.Contract/RuntimeSubmitTests.cs`
  - Test: Returns 200 on successful submission
  - Test: Returns 400 on validation failure
  - Test: Returns 404 when instance not found
  - Test: Verifies data persisted to SQL tables

### Integration Tests (User Scenarios from spec.md)

- [ ] **T014** [P] Integration test: Create form with single field in `backend/tests/FormDesigner.Tests.Integration/CreateFormWithFieldTests.cs`
  - Maps to Scenario 1 in quickstart.md
  - Test full workflow: POST form → GET form → Verify field widget

- [ ] **T015** [P] Integration test: Create multi-page form with table in `backend/tests/FormDesigner.Tests.Integration/MultiPageFormWithTableTests.cs`
  - Maps to Scenario 2 in quickstart.md
  - Test: Create form with 2 pages, table widget with columns
  - Test: Navigate between pages, verify persistence

- [ ] **T016** [P] Integration test: Export YAML and validate DSL compliance in `backend/tests/FormDesigner.Tests.Integration/YamlExportValidationTests.cs`
  - Maps to Scenario 3 in quickstart.md
  - Test: Create form → Export YAML → Parse YAML → Validate structure
  - Assert ID pattern compliance `^[a-z0-9_-]+$`
  - Assert snake_case property names

- [ ] **T017** [P] Integration test: Generate SQL DDL in `backend/tests/FormDesigner.Tests.Integration/SqlGenerationTests.cs`
  - Maps to Scenario 4 in quickstart.md
  - Test: Create form with table widget → Generate SQL
  - Test: Verify CREATE TABLE statement
  - Test: Verify computed column with GENERATED ALWAYS AS
  - Test: Verify indexes created

- [ ] **T018** [P] Integration test: Form validation in `backend/tests/FormDesigner.Tests.Integration/FormValidationTests.cs`
  - Maps to Scenario 6 in quickstart.md
  - Test: Invalid form ID (uppercase) rejected
  - Test: Invalid widget ID rejected
  - Test: Empty form ID rejected
  - Test: Duplicate widget IDs rejected

- [ ] **T019** [P] Integration test: Delete widget and section in `backend/tests/FormDesigner.Tests.Integration/DeleteOperationsTests.cs`
  - Maps to Scenario 9 in quickstart.md
  - Test: DELETE widget → Save → Reload → Verify deletion
  - Test: DELETE section → Verify all widgets removed

- [ ] **T019a** [P] Integration test: YAML export/import workflow in `backend/tests/FormDesigner.Tests.Integration/YamlImportExportTests.cs`
  - Test: Create form → Export YAML → Import YAML → Verify identical
  - Test: Resume design from imported YAML
  - Maps to design mode scenarios 5-6

- [ ] **T019b** [P] Integration test: Commit form workflow in `backend/tests/FormDesigner.Tests.Integration/CommitFormTests.cs`
  - Test: Create form → Commit → Verify locked for editing
  - Test: Committed form appears in runtime forms list
  - Maps to design mode scenario 8

- [ ] **T019c** [P] Integration test: Runtime form execution in `backend/tests/FormDesigner.Tests.Integration/RuntimeExecutionTests.cs`
  - Test: Create instance → Enter data → Save progress → Resume → Submit
  - Test: Query submitted data from generated SQL tables
  - Maps to runtime mode scenarios 13-17

---

## Phase 3.3: Data Layer (ONLY after tests are failing)

### Models

- [ ] **T020** [P] Create FormDefinitionEntity in `backend/src/FormDesigner.API/Models/Entities/FormDefinitionEntity.cs`
  - Properties: FormId (PK), Version, DslJson (JSONB), IsCommitted (bool), CreatedAt, UpdatedAt, IsActive
  - Validation attributes: [Key], [Required], [RegularExpression] for FormId

- [ ] **T020a** [P] Create FormInstanceEntity in `backend/src/FormDesigner.API/Models/Entities/FormInstanceEntity.cs`
  - Properties: InstanceId (PK, GUID), FormId (FK), Status (draft/submitted), DataJson (JSONB), CreatedAt, SubmittedAt
  - Represents runtime form instance with temporary or submitted data

- [ ] **T020b** [P] Create TemporaryStateEntity in `backend/src/FormDesigner.API/Models/Entities/TemporaryStateEntity.cs`
  - Properties: StateId (PK, GUID), InstanceId (FK), DataJson (JSONB), SavedAt, UserId (optional)
  - Stores temporary progress during data entry

- [ ] **T021** [P] Create FormDefinitionRoot DTO in `backend/src/FormDesigner.API/Models/DTOs/FormDefinitionRoot.cs`
  - Property: FormDefinition Form
  - JSON serialization attributes with snake_case

- [ ] **T022** [P] Create FormDefinition model in `backend/src/FormDesigner.API/Models/DTOs/FormDefinition.cs`
  - Properties: Id, Title, Version, Locale, Labels, Meta, Options, Storage, Pages
  - Validation: [Required], [RegularExpression] for Id
  - JSON property names with snake_case

- [ ] **T023** [P] Create Page model in `backend/src/FormDesigner.API/Models/DTOs/Page.cs`
  - Properties: Id, Title, Labels, Sections
  - JSON serialization configuration

- [ ] **T024** [P] Create Section model in `backend/src/FormDesigner.API/Models/DTOs/Section.cs`
  - Properties: Id, Title, Labels, Widgets
  - JSON serialization configuration

- [ ] **T025** [P] Create Widget model in `backend/src/FormDesigner.API/Models/DTOs/Widget.cs`
  - Properties: Type, Id, Title, Labels, When, Help
  - Type-specific properties: Field, Fields, Layout, Table, Grid, Checklist
  - JSON serialization configuration

- [ ] **T026** [P] Create FieldSpec model in `backend/src/FormDesigner.API/Models/DTOs/Specs/FieldSpec.cs`
  - Properties: Name, Label, Type, Required, Readonly, Placeholder, Default, Unit, Pattern, Min, Max, Enum, Format, Compute, Override
  - JSON serialization configuration

- [ ] **T027** [P] Create TableSpec model in `backend/src/FormDesigner.API/Models/DTOs/Specs/TableSpec.cs`
  - Properties: RowMode, Min, Max, RowKey, Columns, RowGenerators, Aggregates
  - JSON serialization configuration

- [ ] **T028** [P] Create ColumnSpec model in `backend/src/FormDesigner.API/Models/DTOs/Specs/ColumnSpec.cs`
  - Properties: Name, Label, Type, Required, Readonly, Unit, Pattern, Min, Max, Enum, Default, Formula, Format
  - JSON serialization configuration

- [ ] **T029** [P] Create GridSpec models in `backend/src/FormDesigner.API/Models/DTOs/Specs/GridSpec.cs`
  - Classes: GridSpec, GridRowSpec, GridColumnSpec, GridCellSpec
  - JSON serialization configuration

- [ ] **T030** [P] Create ChecklistSpec models in `backend/src/FormDesigner.API/Models/DTOs/Specs/ChecklistSpec.cs`
  - Classes: ChecklistSpec, ChecklistItem
  - JSON serialization configuration

- [ ] **T031** [P] Create metadata models in `backend/src/FormDesigner.API/Models/DTOs/FormMetadata.cs`
  - Classes: FormMetadata, FormOptions, PrintOptions, Margins, PermissionOptions, StorageOptions
  - JSON serialization configuration

### Database Context & Repository

- [ ] **T032** Create ApplicationDbContext in `backend/src/FormDesigner.API/Data/ApplicationDbContext.cs`
  - DbSet<FormDefinitionEntity> FormDefinitions
  - DbSet<FormInstanceEntity> FormInstances
  - DbSet<TemporaryStateEntity> TemporaryStates
  - Configure JSONB column type for DslJson and DataJson
  - Configure indexes (IsActive, IsCommitted, CreatedAt, GIN on DslJson)
  - Configure relationships (FormInstance -> FormDefinition)
  - OnModelCreating with fluent API configuration

- [ ] **T033** Create IFormRepository interface in `backend/src/FormDesigner.API/Data/Repositories/IFormRepository.cs`
  - Methods: GetAllAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsAsync

- [ ] **T034** Create FormRepository in `backend/src/FormDesigner.API/Data/Repositories/FormRepository.cs`
  - Implement IFormRepository
  - CRUD operations using EF Core
  - JSON serialization/deserialization for DslJson
  - Soft delete implementation (set IsActive=false)

- [ ] **T034a** Create IFormInstanceRepository interface in `backend/src/FormDesigner.API/Data/Repositories/IFormInstanceRepository.cs`
  - Methods: CreateInstanceAsync, GetInstanceAsync, SaveProgressAsync, SubmitInstanceAsync

- [ ] **T034b** Create FormInstanceRepository in `backend/src/FormDesigner.API/Data/Repositories/FormInstanceRepository.cs`
  - Implement IFormInstanceRepository
  - Manage form instances and temporary state
  - Handle data submission to generated SQL tables

---

## Phase 3.4: Service Layer

- [ ] **T035** Create IFormBuilderService interface in `backend/src/FormDesigner.API/Services/IFormBuilderService.cs`
  - Methods: GetAllFormsAsync, GetFormByIdAsync, CreateFormAsync, UpdateFormAsync, DeleteFormAsync, ValidateFormAsync

- [ ] **T036** Create FormBuilderService in `backend/src/FormDesigner.API/Services/FormBuilderService.cs`
  - Implement IFormBuilderService
  - Business logic: validation, duplicate checking, timestamps
  - Call FormRepository methods
  - Exception handling

- [ ] **T037** Create IYamlExportService interface in `backend/src/FormDesigner.API/Services/IYamlExportService.cs`
  - Method: ExportToYamlAsync(string formId)

- [ ] **T038** Create YamlExportService in `backend/src/FormDesigner.API/Services/YamlExportService.cs`
  - Implement IYamlExportService
  - Configure YamlDotNet serializer (snake_case, omit nulls)
  - Load form from repository
  - Serialize to YAML string

- [ ] **T039** Create ISqlGeneratorService interface in `backend/src/FormDesigner.API/Services/ISqlGeneratorService.cs`
  - Method: GenerateSqlAsync(string formId)

- [ ] **T040** Create SqlGeneratorService in `backend/src/FormDesigner.API/Services/SqlGeneratorService.cs`
  - Implement ISqlGeneratorService
  - Load form from repository
  - Iterate table/grid widgets
  - Generate CREATE TABLE statements with:
    - instance_id, page_id, section_id, widget_id, row_id, recorded_at
    - Data columns from widget definition
    - GENERATED ALWAYS AS for computed columns
    - CREATE INDEX statements
  - Map DSL types to PostgreSQL types
  - Translate formulas to SQL expressions (COALESCE for nulls)

- [ ] **T040a** Create IYamlImportService interface in `backend/src/FormDesigner.API/Services/IYamlImportService.cs`
  - Method: ImportFromYamlAsync(Stream yamlStream)

- [ ] **T040b** Create YamlImportService in `backend/src/FormDesigner.API/Services/YamlImportService.cs`
  - Implement IYamlImportService
  - Parse YAML file using YamlDotNet
  - Validate form definition
  - Create or update form in repository

- [ ] **T040c** Create IRuntimeService interface in `backend/src/FormDesigner.API/Services/IRuntimeService.cs`
  - Methods: GetCommittedFormsAsync, CreateInstanceAsync, GetInstanceAsync, SaveProgressAsync, SubmitInstanceAsync

- [ ] **T040d** Create RuntimeService in `backend/src/FormDesigner.API/Services/RuntimeService.cs`
  - Implement IRuntimeService
  - Manage form instances
  - Handle temporary state saving
  - Validate and submit data to generated SQL tables

---

## Phase 3.5: Validation

- [ ] **T041** Create FormDefinitionValidator in `backend/src/FormDesigner.API/Validators/FormDefinitionValidator.cs`
  - Use FluentValidation
  - Validate form ID pattern `^[a-z0-9_-]+$`
  - Validate required fields (id, title, version)
  - Validate pages array not empty
  - Validate unique IDs across pages/sections/widgets
  - Cascade validators for nested objects (Page, Section, Widget)

---

## Phase 3.6: API Controllers

- [ ] **T042** Create FormDefinitionController in `backend/src/FormDesigner.API/Controllers/FormDefinitionController.cs`
  - GET /api/forms → GetAll() → Call FormBuilderService.GetAllFormsAsync()
  - GET /api/forms/{id} → GetById(string id) → Call FormBuilderService.GetFormByIdAsync()
  - POST /api/forms → Create(FormDefinitionRoot) → Validate → Call FormBuilderService.CreateFormAsync()
  - PUT /api/forms/{id} → Update(string id, FormDefinitionRoot) → Validate ID match → Call FormBuilderService.UpdateFormAsync()
  - DELETE /api/forms/{id} → Delete(string id) → Call FormBuilderService.DeleteFormAsync()
  - POST /api/forms/{id}/validate → Validate(string id) → Call FormBuilderService.ValidateFormAsync()
  - Return appropriate HTTP status codes (200, 201, 204, 400, 404, 409, 500)

- [ ] **T043** Create ExportController in `backend/src/FormDesigner.API/Controllers/ExportController.cs`
  - GET /api/export/{id}/yaml → ExportYaml(string id) → Call YamlExportService → Return File() with application/x-yaml
  - POST /api/import/yaml → ImportYaml(IFormFile) → Parse YAML → Call FormBuilderService.CreateOrUpdateFormAsync()
  - GET /api/export/{id}/sql → ExportSql(string id) → Call SqlGeneratorService → Return File() with text/plain
  - Set Content-Disposition headers with filenames

- [ ] **T043a** Add commit form endpoint in FormDefinitionController
  - POST /api/forms/{id}/commit → CommitForm(string id) → Call FormBuilderService.CommitFormAsync()
  - Set is_committed=true, prevent further edits
  - Return 200 on success, 404 if not found, 409 if already committed

- [ ] **T043b** Create RuntimeController in `backend/src/FormDesigner.API/Controllers/RuntimeController.cs`
  - GET /api/runtime/forms → GetCommittedForms() → List all committed forms available for runtime
  - POST /api/runtime/instances → CreateInstance(formId) → Create new form instance for data entry
  - GET /api/runtime/instances/{instanceId} → GetInstance(instanceId) → Get form instance with data
  - PUT /api/runtime/instances/{instanceId}/save → SaveProgress(instanceId, data) → Save temporary state
  - POST /api/runtime/instances/{instanceId}/submit → Submit(instanceId, data) → Validate and persist to database

---

## Phase 3.7: Program.cs Configuration

- [ ] **T044** Configure Program.cs in `backend/src/FormDesigner.API/Program.cs`
  - Configure services: AddControllers, AddDbContext<ApplicationDbContext>, AddScoped services
  - Configure JSON options: snake_case naming, omit nulls
  - Configure CORS: Add allowed origins from appsettings
  - Configure Swagger/OpenAPI: AddSwaggerGen
  - Configure middleware: UseCors, UseStaticFiles, UseRouting, MapControllers
  - Configure SwaggerUI for development

---

## Phase 3.8: Frontend - HTML Structure

- [ ] **T045** Create index.html in `backend/src/FormDesigner.API/wwwroot/index.html`
  - DOCTYPE html5
  - Meta tags (charset, viewport)
  - Title: Form Designer
  - Link CDN stylesheets: jQuery UI, Bootstrap 5
  - Toolbar div: buttons (New, Open, Save, Preview, Export YAML, Export SQL), inputs (form-title, form-id, form-version)
  - Main container div with 3 panels:
    - Left palette: Widget items (Field, Group, Table, Grid, Checklist) with draggable=true
    - Center canvas: Page tabs, page content, section containers
    - Right inspector: Properties container
  - Script tags: jQuery 3.7, jQuery UI 1.13, Bootstrap 5, designer.js

---

## Phase 3.9: Frontend - CSS Styling

- [ ] **T046** Create designer.css in `backend/src/FormDesigner.API/wwwroot/css/designer.css`
  - Reset: margin 0, box-sizing
  - Layout: toolbar (fixed top, 60px), main-container (flexbox), panel-left (250px), panel-center (flex:1), panel-right (350px)
  - Toolbar: background #2c3e50, flex justify-content space-between
  - Palette: widget-item styling (border, hover, dragging states)
  - Canvas: page-tabs styling, section styling (border, header, drop-zone)
  - Widget instance: border, selected state (blue), hover state
  - Inspector: form-group styling, input/select/textarea styling
  - Buttons: btn classes (primary, secondary, success, warning, danger, info)
  - Drag-drop states: drag-over (green background, border)

---

## Phase 3.10: Frontend - JavaScript Core (Design Mode)

- [ ] **T047** Create designer.js skeleton in `backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - IIFE pattern: (function($) { ... })(jQuery)
  - API configuration object: base URL, endpoint methods
  - DesignerState object: formDefinition, currentPageId, selectedWidget, isDirty, isCommitted
  - WidgetTemplates object: field, group, table, grid, checklist
  - initDesigner() function: entry point
  - $(document).ready(initDesigner)

- [ ] **T048** Implement drag-drop in designer.js
  - initDragDrop() function
  - Make palette widgets draggable using jQuery UI .draggable()
  - Make section drop zones droppable using jQuery UI .droppable()
  - Handle drop event: addWidgetToSection(widgetType, sectionId)
  - Update DOM and state on drop
  - Visual feedback: drag-over class, dragging class

- [ ] **T049** Implement widget management in designer.js
  - addWidgetToSection(widgetType, sectionId): clone template, generate ID, add to state, render
  - deleteWidget(widgetId): remove from state, update DOM
  - selectWidget(widgetId): set selectedWidget, highlight border, render inspector
  - generateId(prefix): return unique ID with timestamp + random

- [ ] **T050** Implement section management in designer.js
  - addSection(): create section object, add to current page, render
  - deleteSection(sectionId): remove from state, update DOM
  - updateSectionTitle(sectionId, title): update state and DOM
  - renderSection(sectionId): generate HTML, update DOM

- [ ] **T051** Implement page management in designer.js
  - initPageManagement(): initialize first page if empty
  - addPage(): create page object, add tab, render
  - switchPage(pageId): set currentPageId, show/hide page content
  - getCurrentPage(): return page object from state

- [ ] **T052** Implement inspector panel in designer.js
  - renderInspector(widgetId): generate property form based on widget type
  - renderFieldProperties(widgetId, widget): field-specific properties
  - renderTableProperties(widgetId, widget): table columns, row mode
  - renderGridProperties(widgetId, widget): row/column generators, cell type
  - renderChecklistProperties(widgetId, widget): checklist items
  - updateWidgetProperty(widgetId, prop, value): update state, re-render
  - updateFieldProperty(widgetId, prop, value): update nested field property

- [ ] **T053** Implement form CRUD operations in designer.js
  - saveForm(): collect metadata, POST /api/forms or PUT /api/forms/{id}
  - openForm(): GET /api/forms, show modal, load selected form
  - loadForm(formData): populate state, render all pages/sections/widgets
  - newForm(): confirm unsaved changes, clear state, reset UI
  - commitForm(): POST /api/forms/{id}/commit, lock form for editing

- [ ] **T054** Implement export/import operations in designer.js
  - exportYaml(): GET /api/export/{id}/yaml, trigger download
  - importYaml(): POST /api/import/yaml with file upload
  - exportSql(): GET /api/export/{id}/sql, trigger download

- [ ] **T055** Implement validation in designer.js
  - validateFormId(formId): check pattern `^[a-z0-9_-]+$`, show error
  - validateWidgetId(widgetId): check pattern, show error
  - validateRequiredFields(): check form ID, title, version not empty
  - checkDuplicateIds(): scan all widgets, highlight duplicates
  - markDirty(): set isDirty=true
  - unsavedChangesWarning(): beforeunload event, show warning if isDirty

---

## Phase 3.10a: Frontend - JavaScript Runtime Mode

- [ ] **T055a** Create runtime.html in `backend/src/FormDesigner.API/wwwroot/runtime.html`
  - HTML structure for form execution
  - Form selector dropdown (committed forms)
  - Form rendering area
  - Save Progress and Submit buttons

- [ ] **T055b** Create runtime.js in `backend/src/FormDesigner.API/wwwroot/js/runtime.js`
  - RuntimeState object: currentInstance, formDefinition, formData
  - loadCommittedForms(): GET /api/runtime/forms
  - createInstance(formId): POST /api/runtime/instances
  - renderForm(formDefinition): generate form HTML from definition
  - saveProgress(): PUT /api/runtime/instances/{id}/save
  - submitForm(): POST /api/runtime/instances/{id}/submit

- [ ] **T055c** Create runtime.css in `backend/src/FormDesigner.API/wwwroot/css/runtime.css`
  - Styles for runtime form rendering
  - Different from designer (no drag-drop, simpler layout)
  - Form field styles, table input styles

---

## Phase 3.11: Unit Tests (Services)

- [ ] **T056** [P] Unit tests for YamlExportService in `backend/tests/FormDesigner.Tests.Unit/Services/YamlExportServiceTests.cs`
  - Test: Exports form with correct YAML structure
  - Test: Uses snake_case for property names
  - Test: Omits null properties
  - Test: Returns null when form not found
  - Mock IFormRepository

- [ ] **T057** [P] Unit tests for SqlGeneratorService in `backend/tests/FormDesigner.Tests.Unit/Services/SqlGeneratorServiceTests.cs`
  - Test: Generates CREATE TABLE for table widget
  - Test: Maps DSL types to PostgreSQL types correctly
  - Test: Generates computed column with GENERATED ALWAYS AS
  - Test: Includes indexes (instance_id)
  - Test: Translates formula with COALESCE
  - Mock IFormRepository

- [ ] **T058** [P] Unit tests for FormBuilderService in `backend/tests/FormDesigner.Tests.Unit/Services/FormBuilderServiceTests.cs`
  - Test: CreateFormAsync sets CreatedAt timestamp
  - Test: UpdateFormAsync sets UpdatedAt timestamp
  - Test: DeleteFormAsync sets IsActive=false (soft delete)
  - Test: ValidateFormAsync calls FluentValidation
  - Test: CommitFormAsync sets IsCommitted=true
  - Test: CommitFormAsync prevents editing committed forms
  - Mock IFormRepository

- [ ] **T058a** [P] Unit tests for YamlImportService in `backend/tests/FormDesigner.Tests.Unit/Services/YamlImportServiceTests.cs`
  - Test: Imports valid YAML and creates form
  - Test: Handles invalid YAML format
  - Test: Updates existing form if ID matches
  - Mock IFormRepository

- [ ] **T058b** [P] Unit tests for RuntimeService in `backend/tests/FormDesigner.Tests.Unit/Services/RuntimeServiceTests.cs`
  - Test: CreateInstanceAsync creates new instance
  - Test: SaveProgressAsync stores temporary state
  - Test: SubmitInstanceAsync validates and persists data
  - Test: Cannot submit incomplete required fields
  - Mock IFormInstanceRepository

---

## Phase 3.12: Integration & End-to-End

- [ ] **T059** Create EF Core migration in `backend/src/FormDesigner.API/`
  - Run: `dotnet ef migrations add InitialCreate`
  - Verify migration file creates form_definitions table
  - Verify indexes created

- [ ] **T060** Run database migration
  - Start PostgreSQL Docker container
  - Run: `dotnet ef database update`
  - Verify schema created successfully

- [ ] **T061** Manual testing: Run backend and verify Swagger
  - Run: `dotnet run` from backend/src/FormDesigner.API/
  - Navigate to http://localhost:5000/swagger
  - Verify all 6 endpoints listed
  - Test endpoints via Swagger UI

- [ ] **T062** Manual testing: Run design mode scenarios
  - Execute all 12 design mode scenarios from spec.md
  - Test YAML export/import workflow
  - Test commit functionality
  - Document results in test log
  - Fix any failures found

- [ ] **T062a** Manual testing: Run runtime mode scenarios
  - Execute all 5 runtime mode scenarios from spec.md
  - Test form instance creation
  - Test save progress and resume
  - Test data submission to database
  - Query submitted data from generated SQL tables
  - Document results in test log
  - Fix any failures found

---

## Phase 3.13: Polish & Documentation

- [ ] **T063** [P] Add XML documentation comments to controllers
  - FormDefinitionController: XML comments for each action
  - ExportController: XML comments for each action
  - Enable Swagger XML comments in Program.cs

- [ ] **T064** [P] Add error handling middleware in `backend/src/FormDesigner.API/Middleware/ErrorHandlingMiddleware.cs`
  - Catch unhandled exceptions
  - Log errors
  - Return consistent error response format

- [ ] **T065** [P] Add logging throughout services
  - Use ILogger<T> in services
  - Log: Info (CRUD operations), Warning (validation failures), Error (exceptions)

- [ ] **T066** [P] Performance testing
  - Create form with 500 widgets
  - Measure API response times (<100ms CRUD, <500ms YAML, <1s SQL)
  - Measure UI drag-drop time (<200ms)
  - Document results

- [ ] **T067** [P] Code cleanup and refactoring
  - Remove unused imports
  - Apply consistent code style
  - Extract magic strings to constants
  - Add defensive null checks

- [ ] **T068** Create README.md in `backend/`
  - Prerequisites: .NET 8, PostgreSQL 15+
  - Setup instructions
  - Build commands
  - Run commands
  - Test commands

---

## Dependencies

### Critical Path
- T001 (Setup) blocks T003-T006
- T003-T004 (Packages) block T007-T019 (Tests)
- T007-T019 (Tests) must complete before T020-T055 (Implementation)
- T020-T031 (Models) block T032-T034 (Database)
- T032-T034 (Database) block T035-T040 (Services)
- T035-T040 (Services) block T042-T043 (Controllers)
- T041 (Validation) blocks T042 (Controller validation)
- T044 (Program.cs) blocks T061 (Run backend)
- T045-T055 (Frontend) blocks T062 (Manual testing)
- T059-T060 (Migrations) block T061 (Run backend)

### Parallel Groups
- **Group 1** (T007-T019): All test files can be written in parallel
- **Group 2** (T020-T031): All model files can be created in parallel
- **Group 3** (T056-T058): All unit test files can be written in parallel
- **Group 4** (T063-T067): All polish tasks can be done in parallel

---

## Parallel Execution Examples

### Write All Contract Tests Together
```bash
# Launch 7 tasks in parallel (different test files)
# T007-T013: All contract test files
```

### Write All Models Together
```bash
# Launch 12 tasks in parallel (different model files)
# T020-T031: All entity and DTO files
```

### Write All Unit Tests Together
```bash
# Launch 3 tasks in parallel (different test files)
# T056-T058: All service unit tests
```

---

## Notes

- Mark tasks complete after tests pass (not when code written)
- Commit after each logical group (e.g., all contract tests)
- Create GitHub issues for major phases:
  - Issue #1: Setup & Infrastructure (T001-T006) ✅
  - Issue #2: Contract Tests (T007-T013f) ✅
  - Issue #3: Integration Tests (T014-T019c) ✅
  - Issue #4: Data Models (T020-T031, T020a-T020b)
  - Issue #5: Database Layer (T032-T034, T034a-T034b)
  - Issue #6: Service Layer (T035-T040, T040a-T040d)
  - Issue #7: Validation (T041)
  - Issue #8: Controllers (T042-T043, T043a-T043b)
  - Issue #9: Configuration (T044)
  - Issue #10: Frontend HTML/CSS (T045-T046)
  - Issue #11: Frontend JavaScript - Design Mode (T047-T055)
  - Issue #11a: Frontend JavaScript - Runtime Mode (T055a-T055c)
  - Issue #12: Unit Tests (T056-T058, T058a-T058b)
  - Issue #13: Integration & Deployment (T059-T062, T062a)
  - Issue #14: Polish & Documentation (T063-T068)

- Use conventional commits:
  - `test: add contract tests for GET /api/forms` (T007)
  - `feat: add FormDefinitionEntity model` (T020)
  - `feat: implement YamlExportService` (T038)
  - `feat: create FormDefinitionController` (T042)
  - `feat: add frontend drag-drop functionality` (T048)
  - `docs: add backend README` (T068)

- Verify tests fail before implementing (TDD)
- Run full test suite after each implementation task
- Update CLAUDE.md if tech stack changes

---

## Task Generation Rules Applied

✅ Each contract endpoint → separate contract test [P]
✅ Each entity in data-model → separate model file [P]
✅ Each service → separate unit test file [P]
✅ Each quickstart scenario → integration test [P]
✅ Different files marked [P], same file sequential
✅ Tests before implementation (TDD enforced)
✅ Setup → Tests → Models → Services → Controllers → Frontend → Polish

---

**Total Tasks**: 88 tasks (68 original + 20 new for design/runtime modes)
**New Tasks Added**:
- 6 contract tests for YAML import, commit, runtime endpoints (T013a-T013f)
- 3 integration tests for YAML import/export, commit, runtime (T019a-T019c)
- 3 runtime entities (T020a-T020b)
- 2 runtime repositories (T034a-T034b)
- 4 runtime services (T040a-T040d)
- 2 runtime controllers (T043a-T043b)
- 3 runtime frontend (T055a-T055c)
- 2 runtime unit tests (T058a-T058b)
- 1 runtime manual testing (T062a)

**Estimated Parallel Groups**: 5 groups (42 tasks can run in parallel across groups)
**Estimated Effort**: 100-150 hours (depending on developer familiarity with stack)
