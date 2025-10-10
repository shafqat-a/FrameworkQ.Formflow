# QF-GMD-14: Monthly Shift Duty Roaster

**Form ID**: qf-gmd-14
**Document No**: QF-GMD-14
**Title**: MONTHLY SHIFT DUTY ROASTER
**Organization**: Power Grid Company of Bangladesh Ltd.
**Revision**: 01
**Effective Date**: 11/11/12

## Form Purpose

Monthly tracking form for shift duty assignments across 31 days. Used by Grid Maintenance Division (GMD) to schedule and track employee shifts at sub-stations.

## Form Structure

### 1. Standard QMS Header (FormHeader Widget)
- Document No: QF-GMD-14
- Revision No: 01
- Effective Date: 11/11/12
- Page: 1 of 1
- Organization: POWER GRID COMPANY OF BANGLADESH LTD.
- Form Title: MONTHLY SHIFT DUTY ROASTER
- Category: QUALITY FORMS

### 2. Location Information (3-Column Grid)

**Row 1:**
- Grid Circle (text, required)
- GMD (text, required)
- Sub-station (text, optional)

**Row 2:**
- Date (date, required)
- Reference File (text, optional)
- Sub-Station Identification No (text, optional)

### 3. Period

- Month (text, required) - Full width field

### 4. Monthly Shift Duty Roster (Table Widget)

**Columns**: 33 total
- Sl. No. (number, required)
- Name (text, required)
- Days 1-31 (text, optional) - For shift codes

**Features**:
- Add/Remove rows enabled
- Initial rows: 0 (dynamic)
- Each day cell accepts shift codes: A, B, C, G, F, Ad

### 5. Shift Legend (Notes Widget)

```
Note: A shift: 06:00 – 14:00 hr. B shift: 14:00 – 22:00 hr. C shift: 22:00 – 06:00 hr
```

### 6. Shift Codes (Notes Widget)

```
NOTE:
1. Govt. Holiday : G
2. Weekly Holiday : F
3. Additional Duty : Ad
```

### 7. Approval Signatures

**First Row (3-Column Grid):**
1. Junior Assistant Manager
   - Designation: "............................ Sub-station GMD. ................, PGCB."
   - Date: No

2. Deputy Manager / Assistant Manager
   - Designation: "............................ Sub-station GMD. ..............., PGCB."
   - Date: No

3. Manager
   - Designation: "GMD. .................., PGCB"
   - Date: No

**Second Row (2-Column Grid):**
1. Reviewed by (GMT-1)
   - Designation: No
   - Date: Yes

2. Approved by (DGM)
   - Designation: No
   - Date: Yes

## Shift Codes

| Code | Meaning |
|------|---------|
| A | 06:00 – 14:00 hr (Morning shift) |
| B | 14:00 – 22:00 hr (Evening shift) |
| C | 22:00 – 06:00 hr (Night shift) |
| G | Govt. Holiday |
| F | Weekly Holiday (Friday) |
| Ad | Additional Duty |

## Data Entry Workflow

1. Fill in Grid Circle, GMD, Sub-station
2. Enter Date, Reference File, Sub-Station ID
3. Specify Month
4. For each employee:
   - Click "Add Row"
   - Enter Sl. No. and Name
   - Enter shift codes for each day (A, B, C, G, F, or Ad)
5. Review shift legend and codes
6. Signatures:
   - Junior Assistant Manager signs
   - Deputy Manager / Assistant Manager signs
   - Manager signs
   - GMT-1 reviews and dates
   - DGM approves and dates

## Layout Features

### Grid Layouts
- **Top fields**: 3-column grid for horizontal layout
- **Signatures row 1**: 3-column grid (JAM | DM/AM | Manager)
- **Signatures row 2**: 2-column grid (Reviewed | Approved)

### Table Features
- **33 columns**: Sl.No + Name + 31 days
- **Dynamic rows**: Add/Remove as needed
- **Compact cells**: Small input fields for shift codes
- **Horizontal scroll**: Required for 31 day columns

## Preview Mode Testing

This form is ideal for testing the enhanced preview mode (Feature 003):

**Visual Parity Tests:**
- ✅ 3-column grid layout renders correctly
- ✅ Table with 33 columns displays properly
- ✅ Signature grids show correct column counts
- ✅ Notes sections styled appropriately

**Interactive Tests:**
- ✅ Click "Add Row" to add employee
- ✅ Click "Remove" to delete employee
- ✅ Enter shift codes in table cells
- ✅ Fill in header information
- ✅ Sign signature fields

**Data Collection:**
- ✅ Form data collected in `previewState.formData`
- ✅ Console logs show all field values
- ✅ Grid layouts preserve data structure

## Sample Data

**Example Shift Roster:**

| Sl. No. | Name | 1 | 2 | 3 | 4 | 5 | ... |
|---------|------|---|---|---|---|---|-----|
| 1 | John Doe | A | A | A | A | A | ... |
| 2 | Jane Smith | B | B | B | B | B | ... |
| 3 | Bob Johnson | C | C | C | C | F | ... |

## Form Validation Rules

- Grid Circle: Required
- GMD: Required
- Date: Required
- Month: Required
- Table Sl. No.: Required per row
- Table Name: Required per row
- Day columns: Optional (allow empty if no assignment)

## SQL Generation

```sql
CREATE TABLE monthly_shift_duty_roaster (
    id UUID PRIMARY KEY,
    grid_circle VARCHAR(255) NOT NULL,
    gmd VARCHAR(255) NOT NULL,
    sub_station VARCHAR(255),
    date DATE NOT NULL,
    reference_file VARCHAR(255),
    sub_station_id VARCHAR(255),
    month VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE shift_roster_entries (
    id UUID PRIMARY KEY,
    form_id UUID REFERENCES monthly_shift_duty_roaster(id),
    sl_no INTEGER NOT NULL,
    name VARCHAR(255) NOT NULL,
    day_1 VARCHAR(10),
    day_2 VARCHAR(10),
    -- ... days 3-30
    day_31 VARCHAR(10),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

## Print Layout

**Paper Size**: A4 Landscape
**Orientation**: Landscape (required for 31 day columns)
**Margins**: Standard (1 inch)
**Font**: Arial 10pt for data, 12pt for headers
**Table Borders**: 1px solid black

## Notes

- Form designed for landscape printing
- Table will require horizontal scrolling in web view
- Shift codes are single letters for space efficiency
- Month field accepts text (e.g., "January 2025")
- Multiple employees can be added dynamically
- Signatures can be filled electronically or printed for manual signing
