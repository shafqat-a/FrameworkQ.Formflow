# Form DSL Specification v0.1

## Overview

This document defines a YAML/JSON-based Domain Specific Language (DSL) for digitizing paper forms. The DSL enables:

1. **Rendering** forms for user input (web/mobile UI)
2. **Storage** of form data in databases
3. **Reporting** on collected data

## Design Principles

The DSL is organized in four layers:

- **form** → metadata + global options
- **pages[]** → groups the form into logical pages
- **sections[]** → groups each page into blocks
- **widgets[]** → input elements or tables inside sections
- **fields/columns** → actual data points (typed, validated, computed)

## Core Data Types

| DSL Type | Description | SQL Mapping |
|----------|-------------|-------------|
| `string` | Short labels, names | `text` |
| `text` | Long notes, multiline | `text` |
| `integer` | Whole numbers | `integer` |
| `decimal` | Floating point numbers | `numeric(18,6)` |
| `date` | Date (YYYY-MM-DD) | `date` |
| `time` | Time (HH:mm:ss) | `time` |
| `datetime` | Timestamp with timezone | `timestamptz` |
| `bool` | Boolean true/false | `boolean` |
| `enum` | Enumerated values | `text + check constraint` |
| `attachment` | File upload | `text` (URI/key) |
| `signature` | Digital signature | `text` (URI/key) |

## ID Naming Conventions

- All IDs (`form.id`, `page.id`, `section.id`, `widget.id`, `field.name`) must be `[a-z0-9_-]+`
- IDs are stable across versions
- Use kebab-case or snake_case consistently

## Top-Level Structure

```yaml
form:
  id: <slug>                    # required, unique identifier
  title: <string>               # required
  version: <semver|string>      # required, e.g., "1.0"
  locale?: [<bcp47>, ...]       # optional, e.g., ["en", "bn"]
  labels?:                      # optional i18n display titles
    <lang>: <string>
  meta?:                        # optional header metadata
    organization?: <string>
    document_no?: <string>
    effective_date?: <date>
    revision_no?: <string>
    reference?: <string>
    tags?: [<string>, ...]
  options?:                     # renderer/runtime hints
    print?:
      page_size?: A4|Letter
      margins_mm?: {top:10, left:10, right:10, bottom:10}
    permissions?:
      roles?: [<string>]
      visibility?: "public"|"private"
  storage?:
    mode?: "jsonb"|"normalized"
  pages:                        # required (>=1)
    - (Page)
```

## Page Structure

```yaml
id: <slug>                      # required
title: <string>                 # required
labels?:                        # optional i18n
  <lang>: <string>
sections:                       # required (>=1)
  - (Section)
```

## Section Structure

```yaml
id: <slug>                      # required
title: <string>                 # required
labels?:                        # optional i18n
  <lang>: <string>
widgets:                        # required (>=1)
  - (Widget)
```

## Widget Types

All widgets share common properties:

```yaml
type: field|group|table|grid|checklist   # required
id: <slug>                               # required
title?: <string>
labels?: {<lang>: <string>}
when?: <expression>                      # optional visibility rule
help?: <string>                          # optional help text
```

### 1. Field Widget (Single Input)

```yaml
type: field
id: <slug>
field:
  name: <slug>                  # required, becomes column/key
  label: <string>               # required
  type: string|text|integer|decimal|date|time|datetime|bool|enum|attachment|signature
  required?: <bool>
  readonly?: <bool>
  placeholder?: <string>
  default?: <value>
  unit?: <string>               # e.g., "MkWh", "°C", "kV"
  pattern?: <regex>             # for string validation
  min?: <number|date|time>      # minimum value
  max?: <number|date|time>      # maximum value
  enum?: [<string>, ...]        # when type: enum
  format?: <string>             # display format, e.g., "0.000"
  compute?: <expression>        # computed value
  override?: <bool>             # allow manual override of computed value
```

### 2. Group Widget (Multiple Fields)

```yaml
type: group
id: <slug>
fields:                         # required (>=1)
  - name: <slug>
    label: <string>
    type: <data-type>
    # ... same properties as field.field
layout?:
  columns?: <int>               # UI hint (1-6 columns)
```

### 3. Table Widget (Row List)

```yaml
type: table
id: <slug>
table:
  row_mode?: infinite|finite    # default: infinite
  min?: <int>                   # initial rows for infinite
  max?: <int>                   # max rows for finite
  row_key?: [<column-name>, ...] # optional natural key
  columns:                      # required (>=1)
    - name: <slug>              # required
      label: <string>           # required
      type: <data-type>
      required?: <bool>
      readonly?: <bool>
      unit?: <string>
      pattern?: <regex>
      min?: <number|date|time>
      max?: <number|date|time>
      enum?: [<string>, ...]
      default?: <value>
      formula?: <expression>    # computed per-row
      format?: <string>
  row_generators?:              # for finite or prefill
    - type: range
      name: <slug>
      from: <int>
      to: <int>
      step?: <int>              # default 1
    - type: times
      name: <slug>
      start: "HH:mm"
      end: "HH:mm"
      step_minutes: <int>
    - type: enum
      name: <slug>
      values: [<string>, ...]
    - type: days-of-month
      name: <slug>              # derives from context month field
  aggregates?:                  # footer totals
    - name: <slug>
      label: <string>
      expr: <aggregate-expression>  # sum(col), avg(col), etc.
      format?: <string>
```

### 4. Grid Widget (2D Matrix)

```yaml
type: grid
id: <slug>
grid:
  rows:
    mode: finite|infinite       # required
    generator?:                 # required when mode=finite
      type: names               # editable list
      # OR
      values: [<string>, ...]   # explicit list
    min?: <int>                 # for infinite mode
    max?: <int>                 # for finite mode
  columns:
    generator:                  # required
      type: days-of-month       # derives from context month
      # OR
      type: times
      start: "HH:mm"
      end: "HH:mm"
      step_minutes: <int>
      # OR
      values: [<string|date|time>, ...]
  cell:
    type: string|integer|decimal|enum|bool
    enum?: [<string>, ...]
    default?: <value>
    required?: <bool>
    help?: <string>
```

### 5. Checklist Widget

```yaml
type: checklist
id: <slug>
checklist:
  items:
    - key: <slug>               # required
      label: <string>           # required
      type: bool|enum
      enum?: [<string>, ...]
      default?: <value>
      required?: <bool>
```

## Expressions

Expressions are used for:
- **Formulas**: `formula: "forced + scheduled"`
- **Aggregates**: `expr: "sum(energy_mwh)"`
- **Visibility**: `when: "header.month != null"`

### Context

- Same-row columns: refer by `name`
- Header/context fields: refer via `ctx.<field>` or `header.<field>`

### Operators

- Arithmetic: `+`, `-`, `*`, `/`, `%`
- Comparison: `==`, `!=`, `<`, `>`, `<=`, `>=`
- Logical: `and`, `or`, `not`

### Functions

- **Aggregates**: `sum(col)`, `avg(col)`, `min(col)`, `max(col)`, `count()`, `countif(expr)`
- **Utilities**: `coalesce(a, b)`, `abs(x)`, `round(x, n)`, `to_number(s)`
- **Date**: `date_trunc(unit, date)`

## Storage Model

### Canonical Tables

```sql
-- Form definitions
form_definitions (
  form_id text primary key,
  version text,
  dsl_jsonb jsonb,
  created_at timestamptz,
  is_active boolean
)

-- Form instances (submissions)
form_instances (
  instance_id uuid primary key,
  form_id text references form_definitions(form_id),
  version text,
  submitted_at timestamptz,
  submitted_by text,
  header_ctx jsonb,           -- header fields for quick access
  raw_data jsonb,             -- complete submission
  checksum text
)
```

### Reporting Tables (Auto-Generated)

For each `table` or `grid` widget, generate:

**Table name**: `<form_id>__<widget_id>`

**Base columns** (always present):

```sql
instance_id uuid not null references form_instances(instance_id),
page_id text not null,
section_id text not null,
widget_id text not null,
row_id bigserial primary key,
recorded_at timestamptz default now()
```

**Data columns**: Derived from DSL columns with type mapping

**Computed columns**: From `formula` expressions

```sql
total integer generated always as (coalesce(forced,0) + coalesce(scheduled,0)) stored
```

## Validation Rules

- **required**: Field cannot be null/empty
- **min/max**: Numeric, date, time bounds
- **pattern**: Regex validation for strings
- **enum**: Value must be in allowed list
- **formula**: Computed columns are read-only (unless override=true)

## Example: Substation Performance Form

```yaml
form:
  id: substation-performance
  title: "Consolidated statement of Sub-Station & Transmission Line Performance"
  version: "1.0"
  meta:
    organization: "Power Grid Company of Bangladesh Ltd."
    document_no: "QF-GMD-06"
    effective_date: "2007-02-15"
    revision_no: "00"

  pages:
    - id: p1
      title: "Header & Sub-Station"
      sections:
        - id: hdr
          title: "Header"
          widgets:
            - type: group
              id: header-fields
              fields:
                - {name: substation, label: "Substation", type: string, required: true}
                - {name: month, label: "Month", type: date, required: true}

        - id: a-substation
          title: "A) Sub-Station Performance"
          widgets:
            - type: table
              id: substation-perf
              table:
                row_mode: infinite
                min: 1
                columns:
                  - {name: sl_no, label: "Sl", type: integer}
                  - {name: capacity_mva, label: "Capacity (MVA)", type: decimal, min: 0}
                  - {name: forced, label: "Forced", type: integer, min: 0}
                  - {name: scheduled, label: "Scheduled", type: integer, min: 0}
                  - {name: total, label: "Total", type: integer, formula: "forced + scheduled"}
                  - {name: energy_mwh, label: "Energy (MkWh)", type: decimal, min: 0}
                  - {name: remarks, label: "Remarks", type: text}
                aggregates:
                  - {name: sum_energy, label: "Total Energy", expr: "sum(energy_mwh)"}
```

## Example: Monthly Shift Duty Roster

```yaml
form:
  id: monthly-shift-duty-roster
  title: "Monthly Shift Duty Roster"
  version: "1.0"

  pages:
    - id: roster
      title: "Shift Roster"
      sections:
        - id: header
          title: "Context"
          widgets:
            - type: group
              id: ctx
              fields:
                - {name: sub_station, label: "Sub-station", type: string, required: true}
                - {name: month, label: "Month", type: date, required: true}

        - id: duty-roster
          title: "Roster"
          widgets:
            - type: grid
              id: shift-grid
              grid:
                rows:
                  mode: finite
                  generator: {type: names}
                columns:
                  generator: {type: days-of-month}
                cell:
                  type: enum
                  enum: ["A", "B", "C", "G", "F", "Ad", ""]
                  help: "A=06-14, B=14-22, C=22-06, G=Govt Holiday, F=Weekly, Ad=Additional"
```

## Migration & Versioning

- Increment `form.version` when changing structure
- Store version in `form_instances` to track which DSL version was used
- For schema changes:
  - **Add columns**: ALTER TABLE ADD COLUMN (backward compatible)
  - **Rename columns**: Use migration with `old_name` mapping
  - Keep old instances queryable by version

## Storage Hints

```yaml
storage:
  mode: normalized              # or jsonb
  copy_header: [substation, month]  # replicate header fields into rows
  indexes:
    - [month]
    - [substation, month]
```

## Quality of Life Features

### Defaults

```yaml
default: "today()"              # set default values dynamically
```

### Constraints

```yaml
constraints:
  - expr: "total == forced + scheduled"
    message: "Total must equal forced + scheduled"
```

### Internationalization

```yaml
labels:
  en: "Substation"
  bn: "সাবস্টেশন"
```

### Permissions

```yaml
permissions:
  roles: ["operator", "supervisor"]
  edit_sections: ["header"]     # restrict editing by section
```

## JSON Schema

A JSON Schema is provided for validation. Use libraries like AJV (JavaScript) or Newtonsoft.Json.Schema (C#) to validate YAML after parsing to JSON.

## Design Rationale

1. **YAML/JSON format**: Human-readable, tooling support, no custom parser needed
2. **Four-layer hierarchy**: Matches mental model of forms (page → section → widget → field)
3. **Widget discrimination**: Single schema with type-based variants
4. **Expression language**: Simple, safe, covers 90% of use cases
5. **Stable IDs**: Enable schema evolution without breaking existing data
6. **Storage flexibility**: Support both JSONB (fast iteration) and normalized (analytics)

## Implementation Notes

- Parse YAML → validate against JSON Schema → render UI
- For formulas, use expression evaluator (NCalc for .NET, or safe JS eval)
- Generate SQL DDL from DSL for reporting tables
- Keep DSL versions in sync with database migrations

## Future Extensions

- Conditional sections (show/hide entire sections)
- Repeating sections (not just rows in tables)
- File attachments with size/type validation
- Digital signatures with PKI integration
- Workflow states (draft, submitted, approved)
- Multi-step wizards with progress tracking
