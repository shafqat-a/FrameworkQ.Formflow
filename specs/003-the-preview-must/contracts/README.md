# API Contracts: Preview Mode Fidelity Enhancement

**Feature**: 003-the-preview-must
**Date**: 2025-10-09

## Overview

This feature does **not introduce new API endpoints** or modify existing API contracts. All changes are frontend-only (JavaScript and CSS).

## Existing API Dependencies

The preview mode relies on existing APIs for form loading, but does not call them during preview:

### 1. GET /api/forms/{formId}

**Used by**: Runtime mode only
**Preview behavior**: Uses in-memory form from designer state

**Contract** (unchanged):
```http
GET /api/forms/{formId}
Response: 200 OK
{
    "form": {
        "form_id": "string",
        "title": "string",
        "pages": [...],
        "version": "number"
    }
}
```

**Preview impact**: None. Preview uses `FormDesigner.state.currentForm` directly.

### 2. POST /api/runtime/instances

**Used by**: Runtime mode only
**Preview behavior**: Not called. Preview does not create instances.

**Contract** (unchanged):
```http
POST /api/runtime/instances
Request:
{
    "form_id": "string",
    "user_id": "string"
}
Response: 201 Created
{
    "instance_id": "string",
    "form_id": "string",
    "user_id": "string",
    "status": "in_progress",
    "created_at": "timestamp"
}
```

**Preview impact**: None. Preview is read-only and ephemeral.

### 3. PUT /api/runtime/instances/{instanceId}/save

**Used by**: Runtime mode only
**Preview behavior**: Not called. Preview data never persisted.

**Preview impact**: None.

### 4. POST /api/runtime/instances/{instanceId}/submit

**Used by**: Runtime mode only
**Preview behavior**: Not called. Preview cannot submit.

**Preview impact**: None.

## Frontend Contract Changes

While no HTTP APIs are affected, the JavaScript API (internal) is enhanced:

### FormDesigner.previewForm()

**Current signature**:
```javascript
previewForm(): void
```

**Behavior changes**:
- Still returns void
- Still shows modal
- **New**: Attaches event handlers for interactive buttons
- **New**: Enables form inputs (removes `disabled` attribute)
- **New**: Initializes `formData` object in preview state

**No breaking changes**: Existing callers unaffected.

### FormDesigner.renderPreview()

**Current signature**:
```javascript
renderPreview(): void
```

**Behavior changes**:
- Still returns void
- Still renders preview HTML
- **New**: Uses CSS classes instead of inline styles
- **New**: Renders functional buttons (not decorative)

**No breaking changes**: Internal method, no external callers.

### New Internal Methods

These are new but internal to FormDesigner:

```javascript
// Collect form data from preview inputs
collectPreviewFormData(): Object

// Add table row in preview mode
addPreviewTableRow(tableId: string): void

// Remove table row in preview mode
removePreviewTableRow(rowElement: HTMLElement): void

// Recalculate formulas in preview (future)
recalculatePreviewFormulas(): void
```

**Visibility**: Private to FormDesigner object, not exposed externally.

## Data Contracts

### PreviewState Object

**Structure**:
```typescript
interface PreviewState {
    currentPageIndex: number;
    form: FormDSL;          // Existing
    formData?: {            // NEW (optional)
        [fieldName: string]: any;
    };
    isDirty?: boolean;      // NEW (optional)
}
```

**Backward compatibility**: New fields are optional, old code unaffected.

## Event Contracts

### New DOM Events

**Table Row Add**:
```javascript
// Event: click
// Target: .btn-add-row
// Data attribute: data-table-id
$(document).on('click', '.btn-add-row', handler);
```

**Table Row Remove**:
```javascript
// Event: click
// Target: .btn-delete-row
$(document).on('click', '.btn-delete-row', handler);
```

**Form Input Change**:
```javascript
// Event: input, change
// Target: #preview-content input|select|textarea
$(document).on('input change', '#preview-content input, ...', handler);
```

**Scope**: Only active when `#previewModal` is visible.

## CSS Contracts

### Class Name Consistency

Preview mode now guarantees use of runtime CSS classes:

**Contract**:
```
Preview HTML must use identical CSS class names as runtime HTML for corresponding widgets.
```

**Examples**:
- Section: `.runtime-section`
- Field: `.runtime-field`
- Table: `.runtime-table`

**Enforcement**: Inline styles removed, only class-based styling allowed.

## Breaking Changes

**None**. This is a non-breaking enhancement.

## Deprecations

**None**. No APIs or features deprecated.

## Version Compatibility

**Minimum versions**:
- Bootstrap: 5.3.0 (unchanged)
- jQuery: 3.7.1 (unchanged)
- Browser: Modern browsers with CSS Grid support (unchanged)

**No new dependencies added**.

## Testing Contract

### Manual Testing

Testers should verify:

1. **Visual Parity**: Preview matches runtime appearance
2. **Button Functionality**: Add/Remove buttons work in preview
3. **Form Input**: Can type/select in preview fields
4. **Data Isolation**: Closing preview doesn't affect draft

### Automated Testing

**No new test endpoints required**. Frontend testing only:

```javascript
// Example QUnit test
test('Preview table add row', function(assert) {
    FormDesigner.previewForm();
    const initialRows = $('#preview-content tbody tr').length;
    $('.btn-add-row').first().click();
    const newRows = $('#preview-content tbody tr').length;
    assert.equal(newRows, initialRows + 1, 'Row added');
});
```

## Documentation

**User-facing changes**:
- Preview mode now supports interactive testing
- Add/remove buttons work in preview
- Form fields are editable in preview

**Developer-facing changes**:
- Preview rendering uses CSS classes (maintainability)
- Preview state includes formData object
- New internal methods for data collection

## Migration Guide

**For users**: No migration needed. Enhancement is automatic.

**For developers**:
- If custom widgets were added, ensure they use CSS classes not inline styles
- If custom preview renderers exist, align with new approach

## Summary

**API Impact**: Zero. No HTTP API changes.

**Frontend Contract Impact**: Minor enhancements, backward compatible.

**Data Contract Impact**: Optional new fields in PreviewState.

**Breaking Changes**: None.
