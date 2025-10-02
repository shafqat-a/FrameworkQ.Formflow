# Sample Enterprise Forms

This directory contains sample YAML form definitions based on real-world Power Grid Company of Bangladesh quality management forms.

## Forms Included

### 1. Surveillance Visit of Sub-Station (QF-GMD-17)
**File**: `surveillance-visit-checklist.yaml`

**Features Demonstrated:**
- FormHeader widget with document metadata
- Hierarchical checklist with nested items (1.0, 1.1, 1.2, etc.)
- Multiple signature blocks (Reviewed by, Approved by)
- Basic information fields
- Notes widget for instructions

**Based on**: Power Grid Company screenshot - Surveillance visit form

### 2. Monthly Shift Duty Roaster (QF-GMD-14)
**File**: `monthly-shift-duty-roaster.yaml`

**Features Demonstrated:**
- Large table with 31 day columns (full month roster)
- Multiple signature workflow (5 signature blocks)
- Notes with shift timings
- Legend notes for abbreviations
- Grid circle and GMD metadata

**Based on**: Power Grid Company screenshot - Monthly shift duty roaster

## How to Use These Samples

### Import via API

```bash
# Import a sample form
curl -X POST http://localhost:5099/api/import/yaml \
  -F "file=@surveillance-visit-checklist.yaml"
```

### Import via Designer UI

1. Open designer at http://localhost:5099/index.html
2. Click "Import YAML" button
3. Select the YAML file
4. Form will be loaded into designer
5. Click "Save Draft" to persist
6. Click "Commit" to make available for runtime

### Use in Runtime Mode

1. Commit the form in designer mode
2. Open runtime at http://localhost:5099/runtime.html
3. Click "Select Form"
4. Choose the imported form
5. Fill in the inspection data
6. Submit the form

## Form Structure Overview

All sample forms follow this pattern:

```yaml
form:
  id: qf-gmd-xx
  title: Form Title
  version: "01"

  pages:
    - id: page-1
      title: Page Title
      sections:
        - id: section-header
          title: Form Header
          widgets:
            - type: formheader
              id: header-widget
              formheader:
                document_no: QF-GMD-XX
                # ... metadata

        - id: section-content
          title: Content Section
          widgets:
            # ... various widgets

        - id: section-signatures
          title: Signatures
          widgets:
            - type: signature
              # ... signature widgets
```

## Widget Types Used

### FormHeader
Document metadata display matching Power Grid forms:
- Document number
- Revision number
- Effective date
- Page numbers
- Organization name
- Form title

### HierarchicalChecklist
Inspection items with hierarchical numbering:
- Nested structure (parent → children)
- Auto-numbering (1.0, 1.1, 1.2, 2.0, etc.)
- Different item types (checkbox, select, text)
- Options for select items (Good/Acceptable/Poor, Yes/No, Healthy/Defective)

### Signature
Approval workflow signatures:
- Role/title
- Name (required)
- Designation (optional)
- Date (optional)
- Auto-date support

### Notes
Instructions and legends:
- Shift timings
- Abbreviation legends
- Important instructions

### Table
Large data tables:
- 31 columns for daily roster
- Add/delete rows
- Name + daily duty assignments

### Field
Standard inputs:
- Text fields (names, locations)
- Date fields (inspection date)
- DateTime fields (inspection timestamp)

## Testing Scenarios

Use these forms to test:
1. **Import/Export**: Import YAML → Edit in designer → Export YAML
2. **Preview**: Load form → Click Preview → Verify layout
3. **Commit**: Save draft → Commit → Verify in runtime
4. **Runtime**: Create instance → Fill data → Save progress → Submit
5. **Validation**: Try submitting with missing required fields
6. **Calculations**: Add formula fields and verify auto-calculation
7. **Print**: Print preview to verify print styling

## Additional Sample Forms To Create

Based on the 6 Power Grid screenshots, create:
- [ ] Sub-Station Performance Report (QF-GMD-06)
- [ ] Log Sheet (QF-GMD-01) - with hourly time columns
- [ ] Daily Inspection Check Sheet (QF-GMD-19) - with Bengali text
- [ ] Transformer Inspection & Maintenance (QF-GMD-22)

See `localdoc/` for the original form screenshots.
