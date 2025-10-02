# Quickstart: Visual Form Designer

## Purpose

This quickstart guide provides step-by-step manual testing scenarios to validate the Visual Form Designer implementation. Each scenario maps to acceptance criteria from the feature specification and serves as integration test validation.

## Prerequisites

- PostgreSQL 15+ running on localhost:5432
- .NET 8.0 SDK installed
- Modern web browser (Chrome 90+, Firefox 88+, Edge 90+)
- Backend API running at http://localhost:5000
- Database `formdesigner` created

## Setup

```bash
# 1. Start PostgreSQL
docker run -d --name postgres-formdesigner \
  -e POSTGRES_DB=formdesigner \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=yourpassword \
  -p 5432:5432 \
  postgres:15

# 2. Build and run backend
cd backend/src/FormDesigner.API
dotnet restore
dotnet ef database update
dotnet run

# 3. Open browser
open http://localhost:5000
```

## Scenario 1: Create New Form with Single Field

**Maps to**: FR-001, FR-008, FR-009, FR-015, FR-018

### Steps

1. **Open Designer**
   - Navigate to http://localhost:5000
   - Verify toolbar displays with "New Form", "Open", "Save" buttons
   - Verify left palette shows 5 widget types: Field, Group, Table, Grid, Checklist
   - Verify center canvas is empty
   - Verify right inspector shows "Select a widget to edit properties"

2. **Enter Form Metadata**
   - Click in "Form Title" input, type: `Employee Information Form`
   - Click in "form-id" input, type: `employee-info`
   - Verify pattern validation (lowercase, alphanumeric, hyphens only)
   - Click in "form-version" input, type: `1.0`

3. **Add First Section**
   - Click "+ Add Section" button on canvas
   - Verify section appears with header "New Section"
   - Click section title, change to: `Personal Details`
   - Verify section has empty drop zone

4. **Drag Field Widget**
   - Click and hold "Field" widget in palette
   - Drag to section drop zone
   - Verify drop zone highlights with green border during drag
   - Release mouse
   - Verify field widget appears in section with badge "FIELD"

5. **Configure Field Properties**
   - Click field widget to select
   - Verify widget highlights with blue border
   - Verify inspector panel shows field properties
   - In inspector:
     - Widget ID: `employee-name`
     - Field Name: `employee_name`
     - Label: `Full Name`
     - Type: Select `string`
     - Check "Required" checkbox
     - Placeholder: `Enter full name`

6. **Save Form**
   - Click "Save" button in toolbar
   - Verify success message or confirmation
   - Verify form persists (no error in console)

7. **Verify State Persistence**
   - Refresh browser (F5)
   - Click "Open" button
   - Select `employee-info` from list
   - Verify form loads with:
     - Title: "Employee Information Form"
     - Section: "Personal Details"
     - Widget: employee-name field with all properties

**Expected Result**: ✅ Form created, saved, and reopened successfully

---

## Scenario 2: Create Multi-Page Form with Table Widget

**Maps to**: FR-006, FR-011, FR-019, FR-027

### Steps

1. **Create New Form**
   - Click "New Form"
   - Form ID: `inspection-checklist`
   - Title: `Safety Inspection Checklist`
   - Version: `1.0`

2. **Add First Page**
   - Verify "Page 1" tab exists by default
   - Add section: `Inspector Information`
   - Add 2 field widgets:
     - `inspector_name` (string, required)
     - `inspection_date` (date, required)

3. **Add Second Page**
   - Click "+ Add Page" button
   - Verify "Page 2" tab appears
   - Click "Page 2" tab to switch
   - Verify canvas clears (Page 1 widgets hidden)
   - Rename page title to: `Equipment Checks`

4. **Add Section to Page 2**
   - Click "+ Add Section"
   - Section title: `Substation Performance`

5. **Add Table Widget**
   - Drag "Table" widget from palette to section
   - Verify table widget appears
   - Click table widget to select

6. **Configure Table Properties**
   - In inspector:
     - Widget ID: `substation-perf`
     - Row Mode: Select `infinite`
     - Click "+ Add Column" button (3 times)

7. **Configure Columns**
   - Column 1:
     - Name: `substation_id`
     - Label: `Substation ID`
     - Type: `string`
     - Required: true
   - Column 2:
     - Name: `forced_outage`
     - Label: `Forced Outage (hrs)`
     - Type: `decimal`
     - Required: true
   - Column 3:
     - Name: `scheduled_outage`
     - Label: `Scheduled Outage (hrs)`
     - Type: `decimal`
     - Required: true

8. **Add Computed Column**
   - Click "+ Add Column"
   - Column 4:
     - Name: `total_outage`
     - Label: `Total Outage (hrs)`
     - Type: `decimal`
     - Formula: `forced_outage + scheduled_outage`
     - Readonly: true

9. **Save and Navigate**
   - Click "Save"
   - Click "Page 1" tab
   - Verify inspector widgets still visible
   - Click "Page 2" tab
   - Verify table widget still configured

**Expected Result**: ✅ Multi-page form with table widget created successfully

---

## Scenario 3: Export YAML and Validate DSL Compliance

**Maps to**: FR-026, FR-043, FR-044

### Steps

1. **Open Existing Form**
   - Click "Open"
   - Select `inspection-checklist`
   - Verify form loads

2. **Export YAML**
   - Click "Export YAML" button
   - Verify browser downloads file: `inspection-checklist.yaml`

3. **Validate YAML Structure**
   - Open downloaded file in text editor
   - Verify top-level structure:
     ```yaml
     form:
       id: inspection-checklist
       title: Safety Inspection Checklist
       version: '1.0'
       pages:
         - id: page-1
           title: Page 1
           sections: ...
         - id: page-2
           title: Equipment Checks
           sections: ...
     ```

4. **Validate Table Widget in YAML**
   - Locate `substation-perf` widget
   - Verify structure:
     ```yaml
     - type: table
       id: substation-perf
       table:
         row_mode: infinite
         columns:
           - name: substation_id
             label: Substation ID
             type: string
             required: true
           - name: forced_outage
             label: Forced Outage (hrs)
             type: decimal
             required: true
           - name: scheduled_outage
             label: Scheduled Outage (hrs)
             type: decimal
             required: true
           - name: total_outage
             label: Total Outage (hrs)
             type: decimal
             formula: forced_outage + scheduled_outage
             readonly: true
     ```

5. **Validate Field Properties**
   - Verify `inspector_name` field:
     ```yaml
     - type: field
       id: inspector-name
       field:
         name: inspector_name
         label: Inspector Name
         type: string
         required: true
     ```

6. **Validate ID Pattern Compliance**
   - Search for all `id:` entries
   - Verify all match pattern `^[a-z0-9_-]+$`
   - No uppercase letters, no special characters except - and _

**Expected Result**: ✅ YAML exports correctly with DSL v0.1 compliant structure

---

## Scenario 4: Generate SQL DDL

**Maps to**: FR-027, FR-029, FR-030, FR-031, FR-045

### Steps

1. **Open Form with Table Widget**
   - Open `inspection-checklist`
   - Verify table widget `substation-perf` exists

2. **Generate SQL**
   - Click "Generate SQL" button
   - Verify browser downloads: `inspection-checklist_schema.sql`

3. **Validate SQL DDL**
   - Open SQL file in text editor
   - Verify table creation statement:
     ```sql
     -- Table for widget: substation-perf
     CREATE TABLE IF NOT EXISTS inspection_checklist__substation_perf (
         instance_id UUID NOT NULL REFERENCES form_instances(instance_id),
         page_id TEXT NOT NULL,
         section_id TEXT NOT NULL,
         widget_id TEXT NOT NULL,
         row_id BIGSERIAL PRIMARY KEY,
         recorded_at TIMESTAMPTZ DEFAULT NOW(),
         substation_id TEXT NOT NULL,
         forced_outage NUMERIC(18,6) NOT NULL,
         scheduled_outage NUMERIC(18,6) NOT NULL,
         total_outage NUMERIC(18,6) GENERATED ALWAYS AS (
           COALESCE(forced_outage, 0) + COALESCE(scheduled_outage, 0)
         ) STORED
     );

     CREATE INDEX IF NOT EXISTS ix_inspection_checklist__substation_perf_instance
       ON inspection_checklist__substation_perf(instance_id);
     ```

4. **Validate Data Type Mapping**
   - string → TEXT ✓
   - decimal → NUMERIC(18,6) ✓
   - date → DATE ✓
   - datetime → TIMESTAMPTZ ✓

5. **Validate Computed Column**
   - Verify `total_outage` has `GENERATED ALWAYS AS` clause
   - Verify formula translation with COALESCE for null handling
   - Verify `STORED` keyword for materialized computation

6. **Test SQL Execution** (Optional)
   - Connect to PostgreSQL
   - Execute generated SQL
   - Verify tables created without errors
   - Verify indexes created

**Expected Result**: ✅ SQL DDL generates correctly with PostgreSQL-compatible syntax

---

## Scenario 5: Drag-Drop UX Validation

**Maps to**: FR-038, FR-039, FR-040

### Steps

1. **Create New Form**
   - Form ID: `ux-test`
   - Add section: `Test Section`

2. **Test Valid Drag-Drop**
   - Drag "Field" widget from palette
   - Hover over section drop zone
   - **Verify**: Drop zone background changes to light green
   - **Verify**: Dashed border changes to solid green
   - Drop widget
   - **Verify**: Widget appears immediately
   - **Verify**: Drop zone returns to normal state

3. **Test Invalid Drag-Drop**
   - Drag "Table" widget from palette
   - Hover over toolbar (invalid drop target)
   - **Verify**: No drop zone highlight
   - **Verify**: Cursor shows "not allowed" icon
   - Release mouse
   - **Verify**: Widget does NOT appear

4. **Test Widget Selection**
   - Click first widget
   - **Verify**: Widget border changes to blue (#3498db)
   - **Verify**: Box shadow appears around widget
   - **Verify**: Inspector panel updates
   - Click second widget
   - **Verify**: First widget deselected (border returns to gray)
   - **Verify**: Second widget selected (blue border)
   - **Verify**: Inspector updates to second widget properties

5. **Test Property Update Feedback**
   - Select a field widget
   - Change "Label" in inspector to: `Updated Label`
   - **Verify**: Widget label updates in real-time on canvas
   - Change "Type" to: `date`
   - **Verify**: Type badge updates

**Expected Result**: ✅ All visual feedback working correctly

---

## Scenario 6: Validation and Error Handling

**Maps to**: FR-005, FR-024, FR-025

### Steps

1. **Test Form ID Validation**
   - Create new form
   - Form ID: `INVALID_ID` (uppercase)
   - Attempt to save
   - **Verify**: Error message displays
   - **Verify**: Form ID input shows red border
   - Change to: `valid-id-123`
   - **Verify**: Error clears, green border

2. **Test Widget ID Validation**
   - Add field widget
   - In inspector, Widget ID: `Invalid@Widget!`
   - Tab out of input
   - **Verify**: Validation error shows
   - Change to: `valid-widget-id`
   - **Verify**: Error clears

3. **Test Required Field Validation**
   - Create new form
   - Leave Form ID empty
   - Click "Save"
   - **Verify**: Error message: "Form ID is required"
   - **Verify**: Cannot save until filled

4. **Test Duplicate ID Detection**
   - Create form: `duplicate-test`
   - Add two widgets with same ID: `field-1`
   - Click "Save"
   - **Verify**: Error message: "Duplicate widget ID detected"
   - **Verify**: Both widgets highlighted in red

5. **Test Empty Form Validation**
   - Create form with no pages
   - **Verify**: Cannot save (Pages required)
   - Add page with no sections
   - **Verify**: Cannot save (Sections required)

**Expected Result**: ✅ All validation rules enforced correctly

---

## Scenario 7: Complex Form End-to-End

**Maps to**: All functional requirements integrated

### Steps

1. **Create Comprehensive Form**
   - Form ID: `comprehensive-test`
   - Title: `Comprehensive Test Form`
   - Version: `2.0`

2. **Page 1: Mixed Widgets**
   - Section 1: `Basic Info`
     - Field: `name` (string, required)
     - Field: `age` (integer, min=18, max=100)
     - Field: `email` (string, pattern for email)
   - Section 2: `Preferences`
     - Group widget with 3 fields (layout: 2 columns)
     - Checklist widget with 5 items

3. **Page 2: Table Widget**
   - Section: `Data Entry`
   - Table widget: 5 columns (including 1 computed)
   - Row mode: finite, min=1, max=10

4. **Page 3: Grid Widget**
   - Section: `Matrix Input`
   - Grid widget: 10 rows x 7 columns
   - Cell type: enum with 3 values

5. **Save Form**
   - Click "Save"
   - **Verify**: Success message

6. **Export YAML**
   - Click "Export YAML"
   - **Verify**: File downloads
   - **Verify**: YAML validates against DSL v0.1

7. **Generate SQL**
   - Click "Generate SQL"
   - **Verify**: SQL generates for table and grid widgets

8. **Reopen Form**
   - Close browser
   - Reopen http://localhost:5000
   - Click "Open"
   - Select `comprehensive-test`
   - **Verify**: All 3 pages load correctly
   - **Verify**: All widgets preserved
   - Navigate between pages
   - **Verify**: No data loss

9. **Modify and Re-export**
   - Add new field to Page 1
   - Export YAML again
   - **Verify**: New field present in YAML
   - **Verify**: Version still 2.0 (or increment if changed)

**Expected Result**: ✅ Complex form created, saved, exported, and reopened successfully

---

## Scenario 8: Unsaved Changes Warning

**Maps to**: FR-004

### Steps

1. **Create Form**
   - Form ID: `unsaved-test`
   - Add section and field widget

2. **Attempt Navigation Without Save**
   - Click "New Form" button
   - **Verify**: Warning modal appears: "You have unsaved changes. Discard?"
   - Click "Cancel"
   - **Verify**: Stays on current form

3. **Attempt Browser Close Without Save**
   - Click browser close button (X)
   - **Verify**: Browser warning: "Changes you made may not be saved"
   - Click "Stay on page"

4. **Save and Verify Warning Clears**
   - Click "Save"
   - Click "New Form"
   - **Verify**: No warning (navigates immediately)

**Expected Result**: ✅ Unsaved changes warning working correctly

---

## Scenario 9: Delete Widget

**Maps to**: FR-013

### Steps

1. **Open Form**
   - Open any existing form with widgets

2. **Delete Widget**
   - Click widget to select
   - Click "✕" button on widget header
   - **Verify**: Widget disappears from canvas
   - **Verify**: Inspector shows "Select a widget..."

3. **Delete Section**
   - Click "🗑️" button on section header
   - **Verify**: Section and all widgets removed
   - **Verify**: Canvas updates

4. **Save and Verify Deletion Persisted**
   - Click "Save"
   - Refresh browser
   - Reopen form
   - **Verify**: Deleted widgets/sections do not reappear

**Expected Result**: ✅ Deletion working correctly

---

## Scenario 10: Performance Validation

**Maps to**: Performance goals from spec

### Steps

1. **Create Large Form**
   - Form ID: `performance-test`
   - Add 10 pages
   - Add 5 sections per page
   - Add 10 widgets per section
   - Total: 500 widgets

2. **Test Drag-Drop Performance**
   - Open browser DevTools → Performance tab
   - Start recording
   - Drag widget to section
   - Stop recording
   - **Verify**: Drop operation completes in <200ms

3. **Test Save Performance**
   - Open Network tab
   - Click "Save"
   - **Verify**: POST request completes in <100ms
   - **Verify**: Response 201 Created

4. **Test Export Performance**
   - Click "Export YAML"
   - **Verify**: Download starts in <500ms
   - Check file size (should be reasonable, <1MB)

5. **Test SQL Generation Performance**
   - Add 10 table widgets with 10 columns each
   - Click "Generate SQL"
   - **Verify**: Generation completes in <1s
   - **Verify**: SQL file downloads

**Expected Result**: ✅ All operations meet performance targets

---

## Validation Checklist

After completing all scenarios, verify:

- [ ] All 10 scenarios passed without errors
- [ ] Forms saved and reopened correctly
- [ ] YAML exports are DSL v0.1 compliant
- [ ] SQL DDL is PostgreSQL-compatible and executes without errors
- [ ] UI provides appropriate visual feedback
- [ ] Validation rules enforced
- [ ] Performance targets met (<200ms drag-drop, <100ms API, <500ms export)
- [ ] No console errors in browser DevTools
- [ ] No database errors in backend logs

## Troubleshooting

### Issue: Form doesn't save

- Check browser console for errors
- Verify backend API is running (http://localhost:5000/api/forms)
- Check PostgreSQL connection in appsettings.json
- Verify database migrations ran successfully

### Issue: YAML export fails

- Check backend logs for serialization errors
- Verify YamlDotNet package installed
- Check form has at least one page/section/widget

### Issue: Drag-drop not working

- Verify jQuery UI script loaded (check browser console)
- Check for JavaScript errors
- Test in different browser (Chrome recommended)

### Issue: SQL generation produces errors

- Check PostgreSQL logs when executing SQL
- Verify formula syntax is valid
- Ensure all column types are supported

## Success Criteria

All scenarios must pass for the feature to be considered complete. This quickstart serves as the acceptance test suite and should be executed:
1. Before marking implementation complete
2. After any major refactoring
3. Before creating pull request
4. As part of release validation
