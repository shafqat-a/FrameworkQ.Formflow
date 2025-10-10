# Tasks: Preview Mode Fidelity Enhancement

**Feature**: 003-the-preview-must
**Input**: Design documents from `/specs/003-the-preview-must/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, quickstart.md ✅

**Target File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js` (single file modification)

**Tests**: Manual browser testing as specified in plan.md. No automated tests required (frontend visual/interactive changes).

**Organization**: Tasks are organized by implementation phase, following the quickstart guide steps. All tasks modify the same file (designer.js), so parallelization is limited.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1 = P1, US2 = P1, US3 = P2)

---

## Phase 0: Prerequisites & Setup

**Purpose**: GitHub issue creation and environment verification

- [X] T001 Create GitHub issue for feature tracking with title "Preview Mode Fidelity Enhancement" and labels: `enhancement`, `frontend`, `preview-mode`
  - **Description**: Create issue with acceptance criteria from spec.md
  - **File**: GitHub issue tracker
  - **Story**: N/A (infrastructure)
  - **Dependencies**: None
  - **Commit**: Not applicable (GitHub operation)

- [X] T002 Verify development environment and test data availability
  - **Description**: Confirm local dev server running, browser DevTools available, and sample form `qf-gmd-14-shift-roster.yaml` exists for testing
  - **File**: Environment check
  - **Story**: N/A (infrastructure)
  - **Dependencies**: None
  - **Commit**: Not applicable (verification only)

---

## Phase 1: Visual Parity (User Story 1 - P1) 🎯

**Goal**: Remove inline styles to match runtime appearance exactly

**Independent Test**: Load form in preview, inspect elements with DevTools, verify no `style="..."` attributes and spacing matches runtime

### Implementation Tasks

- [X] T003 [US1] Enhance PreviewState object structure in `previewForm()` method
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Location**: `previewForm()` method (~line 713)
  - **Changes**:
    ```javascript
    // Add to previewState initialization:
    formData: {},     // NEW: Store form data
    isDirty: false    // NEW: Track changes
    ```
  - **Dependencies**: None
  - **Validation**: Check `FormDesigner.previewState` object in console has new fields
  - **Commit**: `feat(preview): enhance preview state with data tracking (#<issue>)`

- [X] T004 [US1] Remove inline styles from `renderPreviewSection()` method
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Location**: `renderPreviewSection()` method (~line 767)
  - **Changes**:
    - Remove `style="..."` attribute from `.runtime-section` div
    - Remove `style="..."` attribute from `<h3>` tag
    - Add CSS class `.runtime-section-title` to `<h3>`
    - Fix method call: `renderWidgets()` → `renderPreviewWidgets()`
  - **Dependencies**: T003 (code organization)
  - **Validation**: Inspect preview section HTML, verify no inline styles
  - **Commit**: `feat(preview): remove inline styles from sections for runtime parity (#<issue>)`

- [X] T005 [US1] Verify and document CSS class alignment with runtime.css
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Location**: All `renderPreview*()` methods
  - **Changes**: Review all preview render methods, ensure they use CSS classes from runtime.css:
    - `.runtime-section`, `.runtime-section-title`, `.runtime-section-widgets`
    - `.runtime-field`, `.runtime-table`, `.runtime-grid`, etc.
    - Add inline comments noting CSS source
  - **Dependencies**: T004
  - **Validation**: Visual comparison of preview vs runtime spacing (should be ≤2px difference)
  - **Commit**: `docs(preview): add CSS class alignment comments (#<issue>)`

---

## Phase 2: Interactive Buttons (User Story 2 - P1) 🎯

**Goal**: Enable add/remove row functionality in preview mode

**Independent Test**: Preview form with table, click "Add Row", verify new row appears; click "Remove", verify row deleted

### Helper Methods

- [X] T006 [US2] Implement `findWidgetById()` helper method
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Location**: After `renderPreviewCheckboxGroup()` method (~line 1052)
  - **Implementation**:
    ```javascript
    /**
     * Find widget by ID in current preview form
     * @param {string} widgetId - Widget ID to find
     * @returns {Object|null} Widget object or null if not found
     */
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
  - **Dependencies**: T003 (uses previewState)
  - **Validation**: Test in console: `FormDesigner.findWidgetById('test-id')`
  - **Commit**: `feat(preview): add widget lookup helper method (#<issue>)`

- [X] T007 [US2] Implement `addPreviewTableRow()` method
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Location**: After `findWidgetById()` method
  - **Implementation**:
    ```javascript
    /**
     * Add a new row to table in preview mode
     * @param {string} tableId - Table widget ID
     */
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
    }
    ```
  - **Dependencies**: T006
  - **Validation**: Call method in console, verify row appears
  - **Commit**: `feat(preview): implement add table row functionality (#<issue>)`

### Event Handler Infrastructure

- [X] T008 [US2] Implement `attachPreviewEventHandlers()` method
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Location**: After `previewNextPage()` method
  - **Implementation**:
    ```javascript
    /**
     * Attach event handlers for preview interactions
     */
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

        // Track form data changes (will implement data collection in next phase)
        $(document).off('input change', '#preview-content input, #preview-content select, #preview-content textarea');
        $(document).on('input change', '#preview-content input, #preview-content select, #preview-content textarea', () => {
            // Placeholder for data collection (T011)
            this.previewState.isDirty = true;
        });
    }
    ```
  - **Dependencies**: T007
  - **Validation**: Verify event handlers in console with `$._data($('#preview-content')[0], 'events')`
  - **Commit**: `feat(preview): add event handlers for interactive elements (#<issue>)`

- [X] T009 [US2] Implement `cleanupPreviewHandlers()` method
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Location**: After `attachPreviewEventHandlers()` method
  - **Implementation**:
    ```javascript
    /**
     * Clean up preview event handlers to prevent memory leaks
     */
    cleanupPreviewHandlers() {
        $(document).off('click', '#preview-content .btn-add-row');
        $(document).off('click', '#preview-content .btn-delete-row');
        $(document).off('input change', '#preview-content input, #preview-content select, #preview-content textarea');

        // Clear preview state
        this.previewState = null;
    }
    ```
  - **Dependencies**: T008
  - **Validation**: Open/close preview, check for memory leaks in DevTools Memory profiler
  - **Commit**: `feat(preview): add cleanup handlers for memory management (#<issue>)`

### Integration

- [X] T010 [US2] Integrate event handlers into `renderPreview()` method
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Location**: `renderPreview()` method (~line 730), after `$('#preview-content').html(contentHtml);`
  - **Changes**: Add `this.attachPreviewEventHandlers();` after rendering content
  - **Dependencies**: T008
  - **Validation**: Preview form, click add/remove buttons, verify functionality
  - **Commit**: `feat(preview): integrate event handlers into render pipeline (#<issue>)`

- [X] T011 [US2] Add cleanup handler to `previewForm()` method
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Location**: `previewForm()` method (~line 723), after modal show
  - **Changes**:
    ```javascript
    // Show modal
    const previewModal = new bootstrap.Modal($('#previewModal')[0]);
    previewModal.show();

    // Clean up when modal closes
    $('#previewModal').off('hidden.bs.modal').on('hidden.bs.modal', () => {
        this.cleanupPreviewHandlers();
    });
    ```
  - **Dependencies**: T009
  - **Validation**: Close preview modal, verify handlers removed in console
  - **Commit**: `feat(preview): add modal cleanup on close (#<issue>)`

---

## Phase 3: Form Data Entry (User Story 3 - P2)

**Goal**: Enable form inputs and collect data

**Independent Test**: Enter data in preview inputs, check console for `previewState.formData`, verify data collected

### Enable Inputs

- [X] T012 [US3] Remove `disabled` attribute from radio group inputs
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Location**: `renderPreviewRadioGroup()` method (~line 1010)
  - **Changes**:
    - Change `<input ... disabled>` to `<input ... value="${opt.value || opt}">`
    - Remove `disabled` attribute, ensure `value` attribute present
  - **Dependencies**: None (independent modification)
  - **Validation**: Preview form, verify radio buttons are selectable
  - **Commit**: `feat(preview): enable radio group inputs for testing (#<issue>)`

- [X] T013 [US3] Remove `disabled` attribute from checkbox group inputs
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Location**: `renderPreviewCheckboxGroup()` method (~line 1032)
  - **Changes**: Remove any `disabled` attributes from checkbox inputs
  - **Dependencies**: None (independent modification)
  - **Validation**: Preview form, verify checkboxes are selectable
  - **Commit**: `feat(preview): enable checkbox group inputs for testing (#<issue>)`

### Data Collection

- [X] T014 [US3] Implement `collectPreviewFormData()` method
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Location**: After `cleanupPreviewHandlers()` method
  - **Implementation**:
    ```javascript
    /**
     * Collect form data from preview inputs
     * @returns {Object} Collected form data
     */
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

        // Log to console for debugging
        console.log('Preview form data:', this.previewState.formData);

        return this.previewState.formData;
    }
    ```
  - **Dependencies**: T003 (uses previewState)
  - **Validation**: Enter data in preview, check console for logged data
  - **Commit**: `feat(preview): implement form data collection (#<issue>)`

- [X] T015 [US3] Wire data collection to input change events
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Location**: `attachPreviewEventHandlers()` method, update input/change handler
  - **Changes**: Replace placeholder comment with `this.collectPreviewFormData();`
  - **Dependencies**: T014
  - **Validation**: Enter data, verify `previewState.formData` updates in real-time
  - **Commit**: `feat(preview): wire data collection to input events (#<issue>)`

---

## Phase 4: Testing & Validation

**Purpose**: Manual browser testing and bug fixes

### Manual Testing Tasks

- [ ] T016 Manual Test: Visual Parity Validation
  - **Description**: Compare preview mode appearance to runtime mode
  - **Test Steps**:
    1. Open designer with sample form (qf-gmd-14-shift-roster.yaml)
    2. Click "Preview" button
    3. Inspect section elements with DevTools
    4. Verify no `style="..."` attributes
    5. Measure spacing with DevTools (should match runtime: 30px section margin-bottom)
    6. Compare visually to runtime.html rendering
  - **Success Criteria**: ≤2px deviation, no inline styles
  - **Dependencies**: T005
  - **File**: Manual testing record
  - **Commit**: Not applicable (testing only)

- [ ] T017 Manual Test: Add Row Functionality
  - **Description**: Test table row addition in preview mode
  - **Test Steps**:
    1. Preview form with table widget
    2. Click "Add Row" button
    3. Verify new row appears with input fields
    4. Enter data in new row
    5. Verify data entry works
    6. Add multiple rows, verify all work
  - **Success Criteria**: Rows added, inputs functional, no console errors
  - **Dependencies**: T010, T011
  - **File**: Manual testing record
  - **Commit**: Not applicable (testing only)

- [ ] T018 Manual Test: Remove Row Functionality
  - **Description**: Test table row removal in preview mode
  - **Test Steps**:
    1. Preview form with table (add 3+ rows)
    2. Click "Remove" button on middle row
    3. Verify row deleted
    4. Verify remaining rows intact
    5. Remove all but one row
  - **Success Criteria**: Rows removed correctly, no errors
  - **Dependencies**: T010, T011
  - **File**: Manual testing record
  - **Commit**: Not applicable (testing only)

- [ ] T019 Manual Test: Form Input Functionality
  - **Description**: Test form data entry in preview mode
  - **Test Steps**:
    1. Preview form with various field types (text, radio, checkbox, select)
    2. Enter text in text fields
    3. Select radio buttons
    4. Check checkboxes
    5. Select dropdown options
    6. Verify all inputs enabled and functional
    7. Check console for `formData` object
  - **Success Criteria**: All inputs work, data collected in `previewState.formData`
  - **Dependencies**: T012, T013, T014, T015
  - **File**: Manual testing record
  - **Commit**: Not applicable (testing only)

- [ ] T020 Manual Test: Page Navigation Persistence
  - **Description**: Test multi-page form navigation
  - **Test Steps**:
    1. Preview multi-page form
    2. Enter data on page 1
    3. Navigate to page 2
    4. Enter data on page 2
    5. Navigate back to page 1
    6. Verify page 1 data still present (should persist within session)
  - **Success Criteria**: Page navigation works, data persists within preview session
  - **Dependencies**: T015
  - **File**: Manual testing record
  - **Commit**: Not applicable (testing only)

- [ ] T021 Manual Test: Data Isolation Validation
  - **Description**: Verify preview data doesn't affect draft
  - **Test Steps**:
    1. Open form in designer
    2. Click "Preview"
    3. Enter data in preview
    4. Add table rows
    5. Close preview modal
    6. Verify draft form unchanged (no data, no extra rows)
  - **Success Criteria**: Preview data discarded, draft unaffected
  - **Dependencies**: T011 (cleanup)
  - **File**: Manual testing record
  - **Commit**: Not applicable (testing only)

### Cross-Browser Testing

- [ ] T022 [P] Cross-Browser Test: Chrome/Edge
  - **Description**: Test all functionality in Chrome/Edge (Chromium)
  - **Test Steps**: Run T016-T021 in Chrome
  - **Success Criteria**: All tests pass
  - **Dependencies**: T016-T021
  - **File**: Browser compatibility record
  - **Commit**: Not applicable (testing only)

- [ ] T023 [P] Cross-Browser Test: Firefox
  - **Description**: Test all functionality in Firefox
  - **Test Steps**: Run T016-T021 in Firefox
  - **Success Criteria**: All tests pass
  - **Dependencies**: T016-T021
  - **File**: Browser compatibility record
  - **Commit**: Not applicable (testing only)

- [ ] T024 [P] Cross-Browser Test: Safari
  - **Description**: Test all functionality in Safari (macOS)
  - **Test Steps**: Run T016-T021 in Safari
  - **Success Criteria**: All tests pass
  - **Dependencies**: T016-T021
  - **File**: Browser compatibility record
  - **Commit**: Not applicable (testing only)

### Performance Testing

- [ ] T025 Performance Test: Preview Modal Open Time
  - **Description**: Measure preview modal open performance
  - **Test Steps**:
    1. Open form with ~100 fields
    2. Open DevTools Performance tab
    3. Click "Preview" button
    4. Stop recording when modal fully rendered
    5. Measure time from click to interactive
  - **Success Criteria**: <500ms for 100-field form
  - **Dependencies**: All implementation complete
  - **File**: Performance metrics record
  - **Commit**: Not applicable (testing only)

- [ ] T026 Performance Test: Add Row Latency
  - **Description**: Measure add row operation speed
  - **Test Steps**:
    1. Open DevTools Performance tab
    2. Click "Add Row" button
    3. Measure time to row appearance
  - **Success Criteria**: <50ms per row
  - **Dependencies**: All implementation complete
  - **File**: Performance metrics record
  - **Commit**: Not applicable (testing only)

### Bug Fixes (if needed)

- [ ] T027 Bug Fix: Address issues found in testing
  - **Description**: Fix any bugs discovered during T016-T026
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Dependencies**: T016-T026 (testing)
  - **Validation**: Re-run failing tests
  - **Commit**: `fix(preview): <description of bug fix> (#<issue>)`

---

## Phase 5: Documentation & Code Quality

**Purpose**: Finalize code quality and documentation

- [X] T028 Add JSDoc comments to new methods
  - **Description**: Ensure all new methods have proper JSDoc comments
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Methods**: `findWidgetById`, `addPreviewTableRow`, `attachPreviewEventHandlers`, `cleanupPreviewHandlers`, `collectPreviewFormData`
  - **Dependencies**: All implementation tasks
  - **Validation**: Review code, verify all methods documented
  - **Commit**: `docs(preview): add JSDoc comments to new methods (#<issue>)`

- [X] T029 Add inline comments for complex logic
  - **Description**: Add explanatory comments for non-obvious code sections
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Focus Areas**: Event handler cleanup, data collection logic, row generation
  - **Dependencies**: All implementation tasks
  - **Validation**: Code review
  - **Commit**: `docs(preview): add inline comments for clarity (#<issue>)`

- [X] T030 Final code review and cleanup
  - **Description**: Review all changes, remove console.logs (except debug-only), ensure consistent style
  - **File**: `/backend/src/FormDesigner.API/wwwroot/js/designer.js`
  - **Dependencies**: T028, T029
  - **Validation**: Full code review
  - **Commit**: `refactor(preview): code cleanup and style consistency (#<issue>)`

---

## Phase 6: Deployment Preparation

**Purpose**: Prepare for merge and deployment

- [X] T031 Update quickstart.md with actual line numbers (if changed)
  - **Description**: Update quickstart guide if line numbers shifted during implementation
  - **File**: `/specs/003-the-preview-must/quickstart.md`
  - **Dependencies**: All implementation complete
  - **Validation**: Follow quickstart, verify accuracy
  - **Commit**: `docs(preview): update quickstart with final line numbers (#<issue>)`

- [ ] T032 Create pull request
  - **Description**: Create PR from `003-the-preview-must` to `main`
  - **PR Description**: Include summary, testing evidence, screenshots
  - **Dependencies**: T001-T031
  - **Validation**: CI passes (if applicable), manual review
  - **Commit**: Not applicable (GitHub operation)

- [ ] T033 Address PR review feedback
  - **Description**: Make changes requested in code review
  - **File**: As needed based on feedback
  - **Dependencies**: T032
  - **Validation**: Re-request review
  - **Commit**: `fix(preview): address PR feedback - <description> (#<issue>)`

---

## Dependencies & Execution Order

### Phase Dependencies

```
Phase 0: Prerequisites (T001-T002)
    ↓
Phase 1: Visual Parity (T003-T005) - User Story 1
    ↓
Phase 2: Interactive Buttons (T006-T011) - User Story 2
    ↓
Phase 3: Form Data Entry (T012-T015) - User Story 3
    ↓
Phase 4: Testing & Validation (T016-T027)
    ↓
Phase 5: Documentation (T028-T030)
    ↓
Phase 6: Deployment (T031-T033)
```

### Critical Path

**Sequential Tasks** (same file, must be ordered):
- T003 → T004 → T005 (Visual parity foundation)
- T006 → T007 → T008 → T009 (Event handler infrastructure)
- T010 → T011 (Event handler integration)
- T014 → T015 (Data collection)

**Parallel Opportunities**:
- T012, T013 [P] - Different render methods, can be done in parallel
- T022, T023, T024 [P] - Different browsers, independent testing
- T016-T021 testing tasks can be done in any order after implementation complete

### Within Each Phase

- **Phase 1**: Must complete T003 before T004 (state structure needed)
- **Phase 2**: Linear dependency chain T006→T007→T008→T009→T010→T011
- **Phase 3**: T012 and T013 can be parallel [P], then T014→T015
- **Phase 4**: Testing tasks independent after all implementation done

### Task-Level Dependencies

```
T001, T002 [Prerequisites] - No dependencies
T003 [Enhance state] - After T001
T004 [Remove styles] - After T003
T005 [CSS alignment] - After T004
T006 [Helper method] - After T003
T007 [Add row method] - After T006
T008 [Event handlers] - After T007
T009 [Cleanup method] - After T008
T010 [Integrate handlers] - After T008
T011 [Cleanup integration] - After T009
T012 [P] [Enable radios] - After T005
T013 [P] [Enable checkboxes] - After T005
T014 [Data collection] - After T003
T015 [Wire data collection] - After T014
T016-T021 [Manual tests] - After relevant implementation
T022-T024 [P] [Browser tests] - After T016-T021
T025-T026 [Performance] - After all implementation
T027 [Bug fixes] - After T016-T026
T028-T030 [Documentation] - After all implementation
T031-T033 [Deployment] - After everything
```

---

## Parallel Execution Examples

Since this feature modifies a single file (designer.js), true parallel execution is limited. However, independent tasks can be done by different developers or in separate branches:

### Example 1: Parallel Input Enablement
```bash
# Task T012 and T013 can run in parallel (different methods):
Developer A: "Remove disabled from radio groups in renderPreviewRadioGroup()"
Developer B: "Remove disabled from checkbox groups in renderPreviewCheckboxGroup()"
# Merge both changes
```

### Example 2: Parallel Browser Testing
```bash
# After implementation complete, test in parallel:
Tester A: "Run all manual tests in Chrome (T022)"
Tester B: "Run all manual tests in Firefox (T023)"
Tester C: "Run all manual tests in Safari (T024)"
```

---

## Implementation Strategy

### Recommended Approach: Sequential by Phase

**Rationale**: Single file modification, dependencies between tasks, manual testing required.

**Timeline** (estimated):
- Phase 0 (Prerequisites): 15 minutes (T001-T002)
- Phase 1 (Visual Parity): 45 minutes (T003-T005)
- Phase 2 (Interactive Buttons): 2 hours (T006-T011)
- Phase 3 (Form Data Entry): 1 hour (T012-T015)
- Phase 4 (Testing): 2 hours (T016-T027)
- Phase 5 (Documentation): 30 minutes (T028-T030)
- Phase 6 (Deployment): 1 hour (T031-T033)

**Total**: ~7.5 hours

### MVP Checkpoints

**Checkpoint 1 - Visual Parity** (After T005):
- Preview appearance matches runtime
- Can validate visual fidelity independently
- **Stop here** if only visual parity is needed

**Checkpoint 2 - Interactive Buttons** (After T011):
- Add/remove buttons work
- Can test dynamic table functionality
- **Stop here** if data entry not needed yet

**Checkpoint 3 - Full Feature** (After T015):
- All user stories complete (US1, US2, US3)
- Full preview functionality available
- Ready for comprehensive testing

---

## Notes

- **Single File Modification**: All implementation tasks modify `designer.js`, limiting parallelization
- **Manual Testing**: No automated tests (per constitution exception), comprehensive manual test suite required
- **GitHub Issue**: Create issue (T001) before starting implementation (constitution requirement)
- **Commit Frequency**: Commit after each task or logical group with format: `feat(preview): <description> (#<issue>)`
- **Testing Evidence**: Take screenshots during T016-T026 for PR documentation
- **Browser DevTools**: Essential for validation and debugging throughout
- **Console Logging**: Keep `console.log` in `collectPreviewFormData()` for debugging (can be useful post-deployment)

---

## Success Criteria

Implementation is complete when:

- ✅ All implementation tasks (T003-T015) completed and committed
- ✅ All manual tests (T016-T021) pass
- ✅ Cross-browser tests (T022-T024) pass in Chrome, Firefox, Safari
- ✅ Performance tests (T025-T026) meet targets (<500ms preview, <50ms add row)
- ✅ No console errors in any browser
- ✅ Visual parity achieved (≤2px deviation from runtime)
- ✅ Add/Remove buttons functional
- ✅ Form inputs enabled and data collected
- ✅ Event handlers cleaned up (no memory leaks)
- ✅ Code documented (JSDoc + inline comments)
- ✅ PR created and approved
- ✅ GitHub issue closed with reference to merged PR

**Ready for**: Merge to main and deployment to production.
