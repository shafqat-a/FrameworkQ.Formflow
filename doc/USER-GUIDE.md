# Visual Form Designer - User Guide

## Quick Start

**Application URL**: http://localhost:5099

**Two Modes:**
- **Designer Mode**: Create and edit forms → http://localhost:5099/index.html
- **Runtime Mode**: Fill and submit forms → http://localhost:5099/runtime.html

---

## Designer Mode

### Creating a New Form

1. Open http://localhost:5099/index.html
2. Fill in form metadata:
   - Form Title: "My Form"
   - Form ID: "my-form" (lowercase, hyphens, underscores only)
   - Version: "1.0"
3. Drag widgets from palette to canvas
4. Configure widget properties in the Properties panel
5. Click "Save Draft" to save
6. Click "Commit" when ready for runtime use

### Available Widgets

**Basic Widgets:**
1. **Field** - Text, number, date, time, email, tel, select, textarea
2. **Group** - Container for related fields
3. **Table** - Repeating rows with columns
4. **Grid** - Multi-column layout
5. **Checklist** - Simple checkbox list

**Enterprise Widgets:**
6. **Form Header** - Document metadata (Doc No, Revision, Date, Organization)
7. **Signature** - Approval signatures with name, designation, date
8. **Notes** - Instructions or warnings (info/warning/note styles)
9. **Hierarchical Checklist** - Nested items with numbering (1.0, 1.1, 1.2)
10. **Radio Group** - Single selection from options
11. **Checkbox Group** - Multiple selection from options

### Configuring Widgets

**Field Widget:**
- Label, Type (text/number/date/time/datetime/email/tel/select/textarea)
- Placeholder, Required
- **Formula** (for calculated fields): e.g., `quantity * unit_price`

**Table Widget:**
- Click widget → Properties panel → "+ Add Column"
- Configure: Name, Label, Type (string/integer/decimal/date/checkbox)
- Set min/max rows, allow add/delete

**Radio/Checkbox Groups:**
- Click widget → Properties panel → "+ Add Option"
- Configure: Label, Value
- Set orientation (horizontal/vertical/grid)

**Form Header:**
- Document No, Revision No, Effective Date
- Organization, Form Title, Category

**Signature:**
- Role (e.g., "Reviewed by (GMT-1)")
- Show/hide designation and date fields
- Auto-date option

**Notes:**
- Title, Content
- Style: info (blue), warning (orange), note (green)

### Preview

Click **Preview** button to see how the form will look in runtime mode.

### Import/Export

**Export YAML:**
1. Enter Form ID
2. Click "Export YAML"
3. Downloads form definition in DSL v0.1 format

**Import YAML:**
1. Click "Import YAML"
2. Select YAML file
3. Form loads into designer

**Export SQL:**
1. Enter Form ID
2. Click "Export SQL"
3. Downloads PostgreSQL DDL for form tables

---

## Runtime Mode

### Filling Out Forms

1. Open http://localhost:5099/runtime.html
2. Click "Select Form"
3. Choose a committed form
4. Fill in the form fields
5. Click "Save Progress" to save without submitting
6. Click "Submit" when complete

### Form Features

**Formulas:**
- Calculated fields auto-update as you type
- Shows results in real-time

**Validation:**
- Required fields marked with *
- Min/max selection constraints for checkbox groups
- Validation runs before submit

**Multi-Page:**
- Use Previous/Next buttons to navigate
- Progress saved across pages

---

## Sample Forms

**Located in**: `backend/tests/FormDesigner.Tests.Integration/SampleForms/`

**Power Grid Company Forms:**
1. **surveillance-complete.yaml** - Surveillance Visit (QF-GMD-17)
2. **qf-gmd-22-complete.yaml** - Transformer Inspection
3. **qf-gmd-19-complete.yaml** - Daily Inspection (Bengali)
4. **qf-gmd-14-shift-roster.yaml** - Monthly Shift Roster
5. **qf-gmd-06-performance.yaml** - Performance Report

**Test Forms:**
6. **enterprise-demo.yaml** - Showcases ALL 11 widget types
7. **simple-test-form.yaml** - Basic 3-field form

### Importing Sample Forms

**Via Designer UI:**
1. Click "Import YAML"
2. Navigate to `SampleForms/` folder
3. Select any `.yaml` file
4. Form loads immediately

**Via API:**
```bash
cd backend/tests/FormDesigner.Tests.Integration/SampleForms

curl -X POST http://localhost:5099/api/import/yaml \
  -F "file=@surveillance-complete.yaml"
```

---

## YAML Format

Forms use DSL v0.1 with **snake_case** properties:

```yaml
form:
  id: my-form
  title: My Form Title
  version: "1.0"
  pages:
    - id: page-1
      title: Page 1
      sections:
        - id: section-1
          title: Section 1
          widgets:
            - type: formheader
              id: header-1
              form_header:           # snake_case!
                document_no: DOC-001
                organization: My Organization

            - type: field
              id: field-1
              field:
                name: field-1
                label: Field Label
                type: text
                required: true

            - type: table
              id: table-1
              table:
                columns:
                  - name: col1
                    label: Column 1
                    type: string
                allow_add_rows: true

            - type: signature
              id: sig-1
              signature:
                role: Approved by
                show_date: true
```

**Important:** Widget properties use snake_case:
- `form_header` (not `formHeader`)
- `radio_group` (not `radioGroup`)
- `checkbox_group` (not `checkboxGroup`)
- `hierarchical_checklist` (not `hierarchicalChecklist`)

---

## Formulas

Calculated fields support:

**Operators:** `+`, `-`, `*`, `/`, `()`

**Functions:**
- `SUM(field1, field2, field3)`
- `AVG(field1, field2)`
- `MIN(field1, field2)`
- `MAX(field1, field2)`
- `COUNT(field1, field2)`
- `ROUND(value, decimals)`
- `ABS(value)`
- `CEIL(value)`
- `FLOOR(value)`

**Example:**
```yaml
- type: field
  id: total
  field:
    name: total
    label: Total
    type: number
    formula: quantity * unit_price
```

---

## Troubleshooting

**Form doesn't appear in Open dialog:**
- Check if it was saved (click "Save Draft")
- Verify Form ID is valid (lowercase, alphanumeric, hyphens, underscores)

**Import fails:**
- Verify YAML uses snake_case property names
- Check YAML syntax is valid
- Ensure file has `.yaml` or `.yml` extension

**Table shows "No columns defined":**
- Select table widget
- Click "+ Add Column" in Properties panel
- Add at least one column

**Preview shows "undefined":**
- Hard refresh browser (Ctrl+Shift+R)
- Check widget has label or field.label property

---

## API Endpoints

**Forms:**
- `GET /api/forms` - List all forms
- `POST /api/forms` - Create form
- `GET /api/forms/{id}` - Get form
- `PUT /api/forms/{id}` - Update form
- `DELETE /api/forms/{id}` - Delete form
- `POST /api/forms/{id}/commit` - Commit form

**Export/Import:**
- `GET /api/export/{id}/yaml` - Export YAML
- `POST /api/import/yaml` - Import YAML
- `GET /api/export/{id}/sql` - Export SQL

**Runtime:**
- `GET /api/runtime/forms` - List committed forms
- `POST /api/runtime/instances` - Create instance
- `GET /api/runtime/instances/{id}` - Get instance
- `PUT /api/runtime/instances/{id}/save` - Save progress
- `POST /api/runtime/instances/{id}/submit` - Submit
- `DELETE /api/runtime/instances/{id}` - Delete instance

---

## Database Setup

```bash
# Create database
psql -h localhost -p 5400 -U postgres
CREATE DATABASE formflow;

# Run migrations
cd backend/src/FormDesigner.API
dotnet ef database update
```

**Connection String:**
```
Host=localhost;Port=5400;Database=formflow;Username=postgres;Password=orion@123
```

---

## Support

For issues or questions, see the GitHub repository:
https://github.com/shafqat-a/FrameworkQ.Formflow

Application developed with Claude Code.
