# Research: Preview Mode Fidelity Enhancement

**Feature**: 003-the-preview-must
**Date**: 2025-10-09

## Current Implementation Analysis

### Preview Mode Location

Preview mode is currently implemented in the **designer application** (`index.html` + `designer.js`), not in the runtime application. The preview is shown in a Bootstrap modal (`#previewModal`) within the designer interface.

**Key Files**:
- `/backend/src/FormDesigner.API/wwwroot/index.html` - Contains preview modal HTML (lines 153-172)
- `/backend/src/FormDesigner.API/wwwroot/js/designer.js` - Preview rendering logic (lines 707-1067)
- `/backend/src/FormDesigner.API/wwwroot/css/runtime.css` - Runtime styling (used as reference)

### Current Preview Rendering Approach

**Preview Method** (`designer.js:707-728`):
1. Validates form has content
2. Initializes `previewState` with current form
3. Calls `renderPreview()` to generate HTML
4. Shows Bootstrap modal

**Rendering Methods**:
- `renderPreview()` - Main preview renderer
- `renderPreviewSection()` - Section renderer with **inline styles** (line 769)
- `renderPreviewWidget()` - Widget dispatcher
- Widget-specific renderers: `renderPreviewField()`, `renderPreviewTable()`, etc.

### Issues Identified

#### 1. Inline Styling Creating Visual Discrepancy

**Problem**: Preview section rendering uses inline styles that differ from runtime:

```javascript
// designer.js:769-770
<div class="runtime-section" style="margin-bottom: 20px; padding: 20px; background: #f8f9fa; border-radius: 6px; border-left: 4px solid #3498db;">
```

**Runtime CSS** (`runtime.css:139-145`):
```css
.runtime-section {
    margin-bottom: 30px;  /* Preview uses 20px */
    padding: 20px;         /* Match */
    background: #f8f9fa;   /* Match */
    border-radius: 6px;    /* Match */
    border-left: 4px solid #3498db; /* Match */
}
```

**Discrepancy**: `margin-bottom` differs (20px vs 30px).

Similar issue with section titles (designer.js:770-772):
```javascript
<h3 style="font-size: 1.5rem; font-weight: 600; color: #2c3e50; margin-bottom: 20px; padding-bottom: 10px; border-bottom: 2px solid #ecf0f1;">
```

#### 2. Non-Functional Interactive Elements

**Table Add/Remove Buttons**:
- `renderPreviewTable()` (designer.js:839-887) renders add/remove buttons but **no event handlers attached**
- Buttons are purely decorative in preview mode
- Runtime has full functionality in `runtime.js` with dynamic row management

**Current Preview Table Rendering** (designer.js:879-884):
```javascript
${allowAddRows ? `
    <button class="btn btn-sm btn-primary btn-add-row" data-table-id="${widget.id}">+ Add Row</button>
    ${widget.spec?.initial_rows ? `...sample rows...` : ''}
` : ''}
```

**Runtime Table Rendering** (runtime.js:305-330):
```javascript
${allowAddRows ? `<button class="btn btn-sm btn-primary btn-add-row" data-table-id="${widget.id}">+ Add Row</button>` : ''}
```

Both render similar HTML but runtime has event handlers.

#### 3. Disabled Form Inputs

**Radio/Checkbox Groups**: Preview renders inputs as `disabled`:

```javascript
// designer.js:1017
<input class="form-check-input" type="radio" name="preview-${widget.id}" disabled>
```

This prevents testing of conditional logic and validation rules.

### Runtime vs Preview Comparison

| Aspect | Runtime (`runtime.js`) | Preview (`designer.js`) |
|--------|------------------------|-------------------------|
| **Styling** | External CSS classes | Mix of CSS + inline styles |
| **Add/Remove Buttons** | Fully functional with event handlers | Rendered but non-functional |
| **Form Inputs** | Enabled and functional | Disabled (radio/checkbox) |
| **Data Entry** | Full form data collection | No data collection |
| **Validation** | Active validation on submit | No validation |
| **Formulas** | Real-time calculation | Not supported |
| **Mode Switching** | Dedicated runtime.html page | Modal in designer |

### Architecture Insights

**Runtime Architecture** (`runtime.js:6-880`):
- **State Management**: `FormRuntime.state` object tracks form, instance, page, data
- **Event-Driven**: jQuery event handlers for all interactions
- **Modular Rendering**: Separate methods for each widget type
- **Data Collection**: `collectFormData()` and `populateFormData()` methods
- **Formula Support**: `recalculateFormulas()`, `calculateField()`, `calculateTableAggregates()`

**Preview Architecture** (`designer.js:707-1067`):
- **Simplified State**: Only `previewState` with form and page index
- **Static Rendering**: No event handlers for interactive elements
- **Modal-Based**: Embedded in designer, not standalone
- **No Data Handling**: Pure visual representation

### Dependencies

**Current Stack**:
- **Bootstrap 5.3**: Modal, form controls, layout
- **jQuery 3.7.1**: DOM manipulation, event handling
- **Formula Engine**: `formula-parser.js`, `formula-evaluator.js` (runtime only)

**CSS Files**:
- `runtime.css` - Runtime-specific styles
- `enterprise-forms.css` - Form styling
- `widgets-enhanced.css` - Enhanced widget styles
- `designer.css` - Designer-specific styles

### Gaps Analysis

**Gap 1: Visual Fidelity**
- Inline styles override CSS classes
- Inconsistent spacing/padding values
- Preview doesn't use same rendering pipeline as runtime

**Gap 2: Functional Interactivity**
- No event handlers for add/remove buttons
- Disabled form inputs prevent interaction
- No formula evaluation
- No validation testing

**Gap 3: Data Persistence**
- Preview doesn't track form data
- No way to test data flow
- Can't verify conditional logic

**Gap 4: Mode Indication**
- Modal title is only indicator of preview mode
- No visual distinction between designer and preview contexts

## Recommendations

### Approach 1: Code Reuse (Recommended)

**Strategy**: Reuse runtime rendering methods in preview mode.

**Benefits**:
- Guaranteed visual parity
- Single source of truth for rendering
- Automatic updates when runtime changes
- Less code duplication

**Implementation**:
1. Extract runtime rendering methods to shared module
2. Preview mode imports and uses shared renderers
3. Add mode flag to enable/disable persistence features
4. Keep preview in modal but with runtime rendering

**Complexity**: Medium (refactoring required)

### Approach 2: Style Alignment

**Strategy**: Remove inline styles, use CSS classes exclusively.

**Benefits**:
- Simpler code
- Easier to maintain
- CSS-driven consistency

**Implementation**:
1. Remove all inline styles from preview renderers
2. Ensure preview uses same CSS classes as runtime
3. Add event handlers for interactive elements
4. Implement data collection (without persistence)

**Complexity**: Low (targeted fixes)

### Approach 3: Embedded Runtime

**Strategy**: Load runtime.html in iframe within preview modal.

**Benefits**:
- Complete runtime environment
- Zero code duplication
- Automatic feature parity

**Drawbacks**:
- Iframe complexity
- Cross-frame communication needed
- Potential performance impact

**Complexity**: Medium-High

## Technical Constraints

**Existing Constraints**:
- Bootstrap 5 modal architecture
- jQuery-based event handling
- Existing designer workflow (can't break)
- Backward compatibility with existing forms

**New Constraints**:
- Preview changes must not persist to draft
- Must work with unsaved forms
- Performance: preview should load <1 second

## Related Code Locations

**Preview Entry Points**:
- Button: `index.html` - `#btn-preview`
- Handler: `designer.js:38` - Event listener setup
- Method: `designer.js:707` - `previewForm()`

**Runtime Entry Points**:
- Page: `runtime.html`
- Script: `runtime.js:6` - `FormRuntime` object
- Init: `runtime.js:883` - jQuery ready handler

**Shared Rendering Targets**:
- Table widget: Both render add/remove buttons
- Form header: Both render organization/document info
- Signature: Both render name/date/designation fields
- All field types: Both use Bootstrap form controls

## Next Steps

Based on this research, **Approach 2 (Style Alignment)** is recommended as the MVP:

1. **Phase 1**: Remove inline styles, use CSS classes
2. **Phase 2**: Add event handlers for interactive elements
3. **Phase 3**: Implement data collection (preview-only, no persistence)
4. **Phase 4**: Enable formula evaluation in preview
5. **Phase 5**: Add validation testing capability

**Rationale**:
- Minimal refactoring risk
- Delivers visual parity immediately (P1 requirement)
- Enables functional testing (P1 requirement)
- Can iterate to Approach 1 later if needed
