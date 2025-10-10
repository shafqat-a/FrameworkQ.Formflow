# DSL Enhancement Proposal: Layout Table Widget

**Issue**: Paper forms like QF-GMD-14 have **bordered layout tables** where fields are positioned in specific cells for visual organization. The current DSL doesn't support this.

## Current State

**Paper Form Structure** (QF-GMD-14 Basic Info):
```
┌─────────────────────┬─────────────────────┬──────────────────────────┐
│ Grid Circle: ______ │ GMD: ________       │ Sub-station:             │
├─────────────────────┼─────────────────────┼──────────────────────────┤
│ Date:               │ Reference File:     │ Sub-Station ID No:       │
└─────────────────────┴─────────────────────┴──────────────────────────┘
Month: _______________________________
```

**Current YAML** (fields render vertically):
```yaml
- type: field
  id: grid-circle
  field: {name: grid-circle, label: "Grid Circle:", type: text}
- type: field
  id: gmd
  field: {name: gmd, label: "GMD:", type: text}
```

**Problem**: No way to specify that these should appear in a 2×3 bordered table layout.

## Proposed Solutions

### Option 1: Layout Table Widget (NEW widget type)

Add a new widget type specifically for layout:

```yaml
- type: layouttable
  id: info-layout
  layout_table:
    rows: 2
    columns: 3
    cells:
      - row: 0
        col: 0
        widget:
          type: field
          id: grid-circle
          field: {name: grid-circle, label: "Grid Circle:", type: text, required: true}

      - row: 0
        col: 1
        widget:
          type: field
          id: gmd
          field: {name: gmd, label: "GMD:", type: text, required: true}

      - row: 0
        col: 2
        widget:
          type: field
          id: sub-station
          field: {name: sub-station, label: "Sub-station:", type: text}

      - row: 1
        col: 0
        widget:
          type: field
          id: date
          field: {name: date, label: "Date:", type: date, required: true}

      - row: 1
        col: 1
        widget:
          type: field
          id: reference-file
          field: {name: reference-file, label: "Reference File:", type: text}

      - row: 1
        col: 2
        widget:
          type: field
          id: sub-station-id
          field: {name: sub-station-id, label: "Sub-Station ID No:", type: text}
```

**Pros**: Explicit, flexible positioning
**Cons**: Verbose, requires new widget implementation

### Option 2: Enhanced Group Widget with Table Layout

Extend Group widget to support table-style layout:

```yaml
- type: group
  id: info-group
  layout:
    style: table
    rows: 2
    columns: 3
  fields:
    # Fields automatically flow into cells
    - {name: grid-circle, label: "Grid Circle:", type: text, required: true}
    - {name: gmd, label: "GMD:", type: text, required: true}
    - {name: sub-station, label: "Sub-station:", type: text}
    - {name: date, label: "Date:", type: date, required: true}
    - {name: reference-file, label: "Reference File:", type: text}
    - {name: sub-station-id, label: "Sub-Station ID No:", type: text}
```

**Pros**: Simpler YAML, reuses Group widget
**Cons**: Less control over cell positioning

### Option 3: CSS-Based Rendering (NO DSL change)

Keep YAML simple, add rendering hints:

```yaml
- id: section-basic-info
  title: Basic Information
  rendering:
    layout: table-3col  # CSS class hint
  widgets:
    - type: field
      id: grid-circle
      field: {name: grid-circle, label: "Grid Circle:", type: text}
    # ... more fields
```

Renderer applies CSS to display fields in 3-column table grid.

**Pros**: Minimal DSL change, separation of content/presentation
**Cons**: Less explicit, CSS-dependent

## Recommended Approach

**Hybrid: Option 2 + Enhanced Rendering**

### DSL Enhancement:

Add `layout` property to **Group widget**:

```typescript
interface GroupLayout {
  style?: 'inline' | 'table' | 'grid';  // default: 'inline'
  columns?: number;  // For table/grid styles
  rows?: number;     // For table style
  bordered?: boolean; // default: true for table
  compact?: boolean;  // default: false
}
```

### Updated GroupSpec.cs:

```csharp
public class GroupSpec
{
    [JsonPropertyName("fields")]
    public List<FieldSpec> Fields { get; set; } = new();

    [JsonPropertyName("layout")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GroupLayout? Layout { get; set; }
}

public class GroupLayout
{
    [JsonPropertyName("style")]
    public string? Style { get; set; } = "inline";  // inline | table | grid

    [JsonPropertyName("columns")]
    public int? Columns { get; set; }

    [JsonPropertyName("rows")]
    public int? Rows { get; set; }

    [JsonPropertyName("bordered")]
    public bool Bordered { get; set; } = true;

    [JsonPropertyName("compact")]
    public bool Compact { get; set; } = false;
}
```

### QF-GMD-14 YAML with Layout Table:

```yaml
- id: section-basic-info
  title: Basic Information
  widgets:
    - type: group
      id: info-layout
      layout:
        style: table
        columns: 3
        rows: 2
        bordered: true
        compact: true
      fields:
        - {name: grid-circle, label: "Grid Circle:", type: text, required: true}
        - {name: gmd, label: "GMD:", type: text, required: true}
        - {name: sub-station, label: "Sub-station:", type: text}
        - {name: date, label: "Date:", type: date, required: true}
        - {name: reference-file, label: "Reference File:", type: text}
        - {name: sub-station-id, label: "Sub-Station ID No:", type: text}
```

### Rendering Logic:

```javascript
renderGroupWidget(widget) {
  const layout = widget.spec?.layout || {style: 'inline'};

  if (layout.style === 'table') {
    return renderAsLayoutTable(widget.spec.fields, layout);
  }
  // else render inline...
}

renderAsLayoutTable(fields, layout) {
  const {columns, rows, bordered} = layout;
  let html = '<table class="table table-bordered table-sm">';

  for (let r = 0; r < rows; r++) {
    html += '<tr>';
    for (let c = 0; c < columns; c++) {
      const fieldIndex = r * columns + c;
      const field = fields[fieldIndex];
      if (field) {
        html += `<td>${renderFieldInCell(field)}</td>`;
      }
    }
    html += '</tr>';
  }
  html += '</table>';
  return html;
}

renderFieldInCell(field) {
  return `
    <strong>${field.label}</strong><br>
    <input type="${field.type}" name="${field.name}" class="form-control form-control-sm">
  `;
}
```

## Implementation Tasks

1. **Backend** (1 hour):
   - Add `GroupLayout` class to GroupSpec.cs
   - Update Widget.cs to include Group spec

2. **Frontend - Runtime** (2 hours):
   - Update `renderGroupWidget()` in runtime.js
   - Add `renderAsLayoutTable()` method
   - Add CSS for bordered layout tables

3. **Frontend - Designer** (2 hours):
   - Add layout options to Group widget properties panel
   - Update `renderPreviewGroup()` to support table layout
   - Add preview of table layout in designer

4. **Testing** (1 hour):
   - Update QF-GMD-14 YAML with layout table
   - Test rendering in runtime and preview modes
   - Verify visual match with paper form

## Immediate Workaround

**For now**, to match the paper form visually:

1. Keep fields as individual widgets (correct semantically)
2. Add **CSS class** to the section for table-style rendering
3. Update runtime.css/enterprise-forms.css to style `.basic-info-fields` as a 3-column bordered table

Would you like me to implement Option 2 (Enhanced Group Widget) now?
