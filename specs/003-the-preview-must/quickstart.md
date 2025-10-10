# Quickstart Guide: Preview Mode Enhancement

**Feature**: 003-the-preview-must
**Target Audience**: Developers implementing the feature
**Estimated Time**: 15 minutes to understand, 4-6 hours to implement

## Overview

This guide helps you implement high-fidelity preview mode with functional interactive elements. The implementation involves:

1. **Removing inline styles** from preview renderers
2. **Adding event handlers** for interactive buttons
3. **Enabling form inputs** for testing
4. **Collecting form data** in preview state

**Files to modify**:
- `/backend/src/FormDesigner.API/wwwroot/js/designer.js` (primary)
- `/backend/src/FormDesigner.API/wwwroot/css/runtime.css` (verify only, no changes)

**No backend changes required**.

## Prerequisites

**Knowledge Required**:
- JavaScript ES6+
- jQuery 3.x
- Bootstrap 5 modals
- CSS class-based styling

**Development Environment**:
- Text editor with JavaScript support
- Modern browser with DevTools
- Local dev server running

**Test Data**:
- Sample form with tables (e.g., `qf-gmd-14-shift-roster.yaml`)
- Multi-page form for navigation testing

## Step-by-Step Implementation

### Step 1: Enhance PreviewState (5 min)

**File**: `designer.js`

**Location**: `previewForm()` method (line 707)

**Current code**:
```javascript
this.previewState = {
    currentPageIndex: 0,
    form: this.state.currentForm
};
```

**Enhanced code**:
```javascript
this.previewState = {
    currentPageIndex: 0,
    form: this.state.currentForm,
    formData: {},           // NEW: Store form data
    isDirty: false          // NEW: Track changes
};
```

**Purpose**: Add data tracking to preview state.

### Step 2: Remove Inline Styles from Sections (10 min)

**File**: `designer.js`

**Location**: `renderPreviewSection()` method (line 778)

**Current code**:
```javascript
renderPreviewSection(section) {
    let html = `
        <div class="runtime-section" style="margin-bottom: 20px; padding: 20px; background: #f8f9fa; border-radius: 6px; border-left: 4px solid #3498db;">
            <h3 style="font-size: 1.5rem; font-weight: 600; color: #2c3e50; margin-bottom: 20px; padding-bottom: 10px; border-bottom: 2px solid #ecf0f1;">
                ${section.title}
            </h3>
            <div class="runtime-section-widgets">
                ${this.renderWidgets(section.widgets)}
            </div>
        </div>
    `;
    return html;
}
```

**Enhanced code**:
```javascript
renderPreviewSection(section) {
    let html = `
        <div class="runtime-section">
            <h3 class="runtime-section-title">${section.title}</h3>
            <div class="runtime-section-widgets">
                ${this.renderPreviewWidgets(section.widgets)}
            </div>
        </div>
    `;
    return html;
}
```

**Changes**:
- ❌ Remove `style="..."` from section div
- ❌ Remove `style="..."` from h3
- ✅ Use `.runtime-section-title` class
- ✅ Fix method name: `renderWidgets` → `renderPreviewWidgets`

**Result**: Sections now match runtime spacing (30px margin vs 20px).

### Step 3: Enable Form Inputs (5 min)

**File**: `designer.js`

**Location**: `renderPreviewRadioGroup()` method (around line 1010)

**Current code**:
```javascript
<input class="form-check-input" type="radio" name="preview-${widget.id}" disabled>
```

**Enhanced code**:
```javascript
<input class="form-check-input" type="radio" name="preview-${widget.id}" value="${opt.value || opt}">
```

**Changes**:
- ❌ Remove `disabled` attribute
- ✅ Keep `value` attribute

**Repeat for**:
- `renderPreviewCheckboxGroup()` - Remove `disabled`
- `renderPreviewField()` - Already enabled, verify no `disabled` added
- `renderPreviewChecklist()` - Remove `disabled` if present

**Result**: Users can interact with form inputs in preview.

### Step 4: Add Table Row Management (30 min)

**File**: `designer.js`

**Location**: After `renderPreviewCheckboxGroup()` method (around line 1052)

**Add new methods**:

```javascript
// Add table row in preview mode
addPreviewTableRow(tableId) {
    const widget = this.findWidgetById(tableId);
    if (!widget || widget.type !== 'table') return;

    const columns = widget.spec?.columns || [];
    let rowHtml = '<tr>';

    columns.forEach((col, index) => {
        const fieldType = col.type || 'text';
        rowHtml += `
            <td>
                <input type="${fieldType}"
                       class="form-control form-control-sm"
                       name="preview_${tableId}_${Date.now()}_${index}"
                       placeholder="${col.label || col.name}">
            </td>
        `;
    });

    rowHtml += `
        <td>
            <button class="btn btn-sm btn-danger btn-delete-row">Remove</button>
        </td>
    </tr>`;

    $(`#preview-content tbody[data-table-id="${tableId}"]`).append(rowHtml);
    this.previewState.isDirty = true;
},

// Helper: Find widget by ID in current preview form
findWidgetById(widgetId) {
    const form = this.previewState.form;
    for (const page of form.pages) {
        for (const section of page.sections) {
            const widget = section.widgets.find(w => w.id === widgetId);
            if (widget) return widget;
        }
    }
    return null;
}
```

**Purpose**: Dynamically add table rows when user clicks "Add Row".

### Step 5: Attach Event Handlers (15 min)

**File**: `designer.js`

**Location**: Inside `renderPreview()` method, after rendering content (around line 754)

**Add after `$('#preview-content').html(contentHtml);`**:

```javascript
// Attach event handlers for interactive elements
this.attachPreviewEventHandlers();
```

**Then create new method after `previewNextPage()`**:

```javascript
// Attach event handlers for preview interactions
attachPreviewEventHandlers() {
    // Add table row
    $(document).off('click', '#preview-content .btn-add-row');
    $(document).on('click', '#preview-content .btn-add-row', (e) => {
        const tableId = $(e.currentTarget).data('table-id');
        this.addPreviewTableRow(tableId);
    });

    // Remove table row
    $(document).off('click', '#preview-content .btn-delete-row');
    $(document).on('click', '#preview-content .btn-delete-row', (e) => {
        $(e.currentTarget).closest('tr').remove();
        this.previewState.isDirty = true;
    });

    // Track form data changes
    $(document).off('input change', '#preview-content input, #preview-content select, #preview-content textarea');
    $(document).on('input change', '#preview-content input, #preview-content select, #preview-content textarea', () => {
        this.collectPreviewFormData();
    });
}
```

**Note**: Using `.off()` before `.on()` prevents duplicate handlers.

### Step 6: Implement Data Collection (15 min)

**File**: `designer.js`

**Location**: After `attachPreviewEventHandlers()` method

**Add new method**:

```javascript
// Collect form data from preview inputs
collectPreviewFormData() {
    this.previewState.formData = {};

    $('#preview-content input, #preview-content select, #preview-content textarea').each((i, el) => {
        const $el = $(el);
        const name = $el.attr('name');

        if (!name) return;

        if ($el.attr('type') === 'checkbox') {
            this.previewState.formData[name] = $el.is(':checked');
        } else if ($el.attr('type') === 'radio') {
            if ($el.is(':checked')) {
                this.previewState.formData[name] = $el.val();
            }
        } else {
            this.previewState.formData[name] = $el.val();
        }
    });

    this.previewState.isDirty = true;

    // Optional: Log to console for debugging
    console.log('Preview form data:', this.previewState.formData);
}
```

**Purpose**: Track user inputs for potential formula calculations (future).

### Step 7: Clean Up on Modal Close (5 min)

**File**: `designer.js`

**Location**: In `previewForm()` method, after modal show (around line 723)

**Add modal cleanup handler**:

```javascript
// Show modal
const previewModal = new bootstrap.Modal($('#previewModal')[0]);
previewModal.show();

// Clean up when modal closes
$('#previewModal').off('hidden.bs.modal').on('hidden.bs.modal', () => {
    this.cleanupPreviewHandlers();
});
```

**Then create cleanup method**:

```javascript
// Clean up preview event handlers
cleanupPreviewHandlers() {
    $(document).off('click', '#preview-content .btn-add-row');
    $(document).off('click', '#preview-content .btn-delete-row');
    $(document).off('input change', '#preview-content input, #preview-content select, #preview-content textarea');

    // Optional: Clear preview state
    this.previewState = null;
}
```

**Purpose**: Prevent memory leaks from event handlers.

## Testing Checklist

### Manual Testing

**Test 1: Visual Parity** ✅
1. Open designer with sample form
2. Click "Preview"
3. Compare spacing/padding to runtime mode
4. **Expected**: Identical layout, no extra gaps

**Test 2: Add Row Functionality** ✅
1. Preview form with table widget
2. Click "Add Row" button
3. **Expected**: New row appears with input fields
4. Enter data in new row
5. **Expected**: Data entry works

**Test 3: Remove Row Functionality** ✅
1. In table with multiple rows
2. Click "Remove" button on a row
3. **Expected**: Row is deleted

**Test 4: Form Input** ✅
1. Preview form with various field types
2. Enter text, select options, check boxes
3. **Expected**: All inputs are enabled and functional

**Test 5: Page Navigation** ✅
1. Preview multi-page form
2. Navigate between pages
3. **Expected**: Navigation works, data persists (if on same page)

**Test 6: Data Isolation** ✅
1. Enter data in preview
2. Close preview modal
3. **Expected**: Draft form unchanged

### Browser Testing

Test in:
- ✅ Chrome/Edge (Chromium)
- ✅ Firefox
- ✅ Safari

### Performance Testing

**Metrics**:
- Preview open time: <500ms for 100-field form
- Add row: <50ms
- Page navigation: <200ms

**Tool**: Browser DevTools Performance tab

## Verification

**After implementation, verify**:

```javascript
// 1. PreviewState has new fields
FormDesigner.previewForm();
console.log(FormDesigner.previewState);
// Should show: { currentPageIndex, form, formData, isDirty }

// 2. No inline styles in sections
$('#preview-content .runtime-section[style]').length;
// Should return: 0

// 3. Event handlers attached
$._data($('#preview-content')[0], 'events');
// Should show delegated handlers

// 4. Inputs not disabled
$('#preview-content input:disabled, #preview-content select:disabled').length;
// Should return: 0 (or very few for calculated fields)
```

## Troubleshooting

### Issue: "Add Row" button doesn't work

**Cause**: Event handler not attached
**Fix**: Verify `attachPreviewEventHandlers()` is called after rendering
**Check**: Console for errors

### Issue: Preview looks different from runtime

**Cause**: Inline styles still present
**Fix**: Search for `style="` in preview render methods
**Solution**: Replace with CSS classes

### Issue: Data doesn't persist across pages

**Cause**: `formData` object cleared on page change
**Fix**: Don't clear `formData` in `renderPreview()`, only update current page data

### Issue: Memory leak / slow performance

**Cause**: Event handlers not cleaned up
**Fix**: Ensure `cleanupPreviewHandlers()` called on modal close
**Check**: Chrome DevTools Memory profiler

## Next Steps

**After basic implementation**:

1. **Add formula support** (optional):
   - Import formula evaluator in preview
   - Call `recalculateFormulas()` after data changes

2. **Add validation testing** (optional):
   - Implement `validatePreviewForm()` method
   - Show validation errors in preview

3. **Add conditional logic testing** (future):
   - Process conditional visibility rules
   - Show/hide fields based on inputs

## Resources

**Key Files Reference**:
- Form DSL structure: `backend/tests/FormDesigner.Tests.Integration/SampleForms/*.yaml`
- Runtime implementation: `wwwroot/js/runtime.js`
- Runtime CSS: `wwwroot/css/runtime.css`
- Designer: `wwwroot/js/designer.js`

**Testing**:
- Sample forms: `backend/tests/FormDesigner.Tests.Integration/SampleForms/`
- Integration tests: `backend/tests/FormDesigner.Tests.Integration/*Tests.cs`

## Estimated Timeline

| Task | Estimated Time |
|------|----------------|
| Step 1-2: State & Styles | 30 min |
| Step 3: Enable Inputs | 15 min |
| Step 4: Table Management | 45 min |
| Step 5-6: Events & Data | 30 min |
| Step 7: Cleanup | 15 min |
| Testing & Debugging | 90 min |
| **Total** | **3.5-4 hours** |

**Note**: Times assume familiarity with codebase.

## Success Criteria

Implementation is complete when:

- ✅ Preview sections have no inline styles
- ✅ Add/Remove buttons work in tables
- ✅ Form inputs are enabled and functional
- ✅ Form data is collected in `previewState.formData`
- ✅ Event handlers cleaned up on modal close
- ✅ Visual parity with runtime mode (±2px)
- ✅ No console errors
- ✅ All manual tests pass

**Ready for**: Code review and integration testing.
