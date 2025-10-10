# Data Model: Preview Mode Fidelity Enhancement

**Feature**: 003-the-preview-must
**Date**: 2025-10-09

## Overview

This feature enhances the existing preview mode without introducing new persistent data entities. All changes are frontend-only, affecting rendering and interaction behavior.

## State Models

### PreviewState (Enhanced)

**Location**: `designer.js` - `FormDesigner.previewState` object

**Purpose**: Tracks preview mode state including form data, page navigation, and interaction state.

**Current Structure**:
```javascript
{
    currentPageIndex: Number,
    form: Object  // Form DSL
}
```

**Enhanced Structure**:
```javascript
{
    currentPageIndex: Number,
    form: Object,  // Form DSL
    formData: Object,  // NEW: Collected form data {fieldName: value}
    isDirty: Boolean,  // NEW: Tracks if user made changes
    validationErrors: Array  // NEW: Validation error messages
}
```

**Lifecycle**:
- **Created**: When user clicks "Preview" button
- **Updated**: On page navigation, form input changes, validation
- **Destroyed**: When preview modal is closed
- **Scope**: Session-only, never persisted

### Widget Rendering Configuration

**Purpose**: Defines which widgets support interactive features in preview mode.

**Structure**:
```javascript
const PREVIEW_CONFIG = {
    interactiveWidgets: {
        'field': { enabled: true, dataEntry: true },
        'table': { enabled: true, dataEntry: true, addRows: true, removeRows: true },
        'radiogroup': { enabled: true, dataEntry: true },
        'checkboxgroup': { enabled: true, dataEntry: true },
        'signature': { enabled: true, dataEntry: true },
        'hierarchicalchecklist': { enabled: true, dataEntry: true }
    },
    readonlyWidgets: {
        'formheader': { enabled: true, dataEntry: true },  // Allow editing header fields
        'notes': { enabled: false, dataEntry: false }  // Display only
    }
};
```

## CSS Class Model

### Runtime Styling Classes

**Purpose**: Ensure preview uses identical CSS classes as runtime mode.

**Core Classes** (from `runtime.css`):
```css
.runtime-section         /* Section container */
.runtime-section-title   /* Section heading */
.runtime-section-widgets /* Widget container */
.runtime-widget          /* Base widget wrapper */
.runtime-field           /* Field widget */
.runtime-table           /* Table widget */
.runtime-grid            /* Grid layout */
.runtime-checklist       /* Checklist widget */
.runtime-group           /* Group widget */
```

**Spacing Standards** (from `runtime.css`):
- Section margin-bottom: `30px`
- Section padding: `20px`
- Widget gap: `15px`
- Widget margin-bottom: `15px`

**No Inline Styles**: All styling must come from CSS classes, not inline `style` attributes.

## Event Handler Model

### Interactive Button Events

**Purpose**: Enable add/remove functionality in preview mode.

**Event Handlers** (to be added):

```javascript
// Table add row
$(document).on('click', '.btn-add-row', function(e) {
    const tableId = $(this).data('table-id');
    addPreviewTableRow(tableId);
});

// Table remove row
$(document).on('click', '.btn-delete-row', function(e) {
    $(this).closest('tr').remove();
    collectPreviewFormData();
});
```

**Scoping**: Event handlers only active when preview modal is open.

### Form Input Events

**Purpose**: Track form data changes and trigger calculations.

**Event Handler**:
```javascript
$(document).on('input change', '#preview-content input, #preview-content select, #preview-content textarea', function() {
    collectPreviewFormData();
    recalculatePreviewFormulas();
});
```

## Data Flow

### Preview Mode Data Flow

```
User Opens Preview
    ↓
Initialize PreviewState
    ↓
Render Form HTML
    ↓
Attach Event Handlers
    ↓
┌─────────────────────┐
│  User Interactions  │
│  - Input changes    │
│  - Add/Remove rows  │
│  - Page navigation  │
└─────────────────────┘
    ↓
Collect Form Data
    ↓
Update PreviewState.formData
    ↓
Recalculate Formulas (if any)
    ↓
Update Calculated Fields
    ↓
Display Updated Values
    ↓
User Closes Preview
    ↓
Destroy PreviewState (data discarded)
```

### Data Isolation

**Important**: Preview data is **never persisted**.

- No API calls to save preview data
- No modification to draft form
- No creation of form instances
- Data exists only in `PreviewState` object
- Destroyed when modal closes

## Configuration Model

### Preview Mode Configuration

**Purpose**: Toggle preview features on/off.

**Structure**:
```javascript
const PREVIEW_MODE_CONFIG = {
    enableDataEntry: true,        // Allow form input
    enableInteractiveButtons: true, // Enable add/remove
    enableFormulas: true,          // Calculate formulas
    enableValidation: false,       // Don't validate (future feature)
    enableConditionals: false,     // Don't process conditionals (future)
    showDebugInfo: false           // Show preview state (developer mode)
};
```

**Usage**: Feature flags to control preview behavior incrementally.

## Validation Model (Future)

**Not implemented in initial version**, but data model prepared:

```javascript
{
    validationRules: {
        'widgetId': {
            required: Boolean,
            minLength: Number,
            maxLength: Number,
            pattern: RegExp,
            custom: Function
        }
    },
    validationErrors: [
        {
            widgetId: String,
            fieldName: String,
            message: String,
            severity: 'error' | 'warning'
        }
    ]
}
```

## No Database Changes

**Important**: This feature requires **zero database changes**.

- No new tables
- No schema modifications
- No stored procedures
- No migrations

All changes are frontend JavaScript and CSS only.

## Performance Considerations

### Memory Model

**PreviewState Size Estimate**:
- Small form (~20 fields): ~5KB
- Medium form (~100 fields): ~25KB
- Large form (~500 fields): ~125KB

**Event Handlers**:
- Delegated event handlers (efficient)
- Cleaned up on modal close
- No memory leaks

**Rendering Performance**:
- Target: <500ms for typical form (100 fields)
- Incremental rendering not needed (modal-based)
- Reuse existing DOM elements on page navigation

## Data Relationships

### Relationships to Existing Entities

**Form DSL** (existing):
- Preview consumes form DSL
- No modifications to DSL structure
- Read-only relationship

**Runtime State** (existing):
- Preview mimics runtime state structure
- No shared state
- Parallel implementation

**Designer State** (existing):
- Preview is sub-state of designer
- Designer state unchanged by preview
- One-way data flow: Designer → Preview

## Summary

**Key Principles**:
1. **No Persistence**: All preview data is ephemeral
2. **CSS-Driven**: Styling via CSS classes, not inline styles
3. **Event-Driven**: jQuery event handlers for interactivity
4. **State Isolation**: Preview state separate from designer state
5. **Runtime Parity**: Mimic runtime behavior without runtime infrastructure

**Data Entities**:
- **Enhanced**: `PreviewState` (JavaScript object)
- **New**: `PREVIEW_CONFIG` (JavaScript constant)
- **Modified**: CSS class usage in preview renderers

**No Database Impact**: Frontend-only changes.
