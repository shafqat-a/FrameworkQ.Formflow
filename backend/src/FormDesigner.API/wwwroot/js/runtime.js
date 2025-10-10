/**
 * Form Runtime - Runtime Mode Application
 * Handles form execution, data entry, and submission
 */

const FormRuntime = {
    // Application state
    state: {
        committedForms: [],
        currentForm: null,
        currentInstance: null,
        currentPageIndex: 0,
        formData: {},
        isDirty: false
    },

    // Initialize the application
    init() {
        console.log('Initializing Form Runtime...');

        // Set up event listeners
        this.setupEventListeners();

        console.log('Form Runtime initialized successfully');
    },

    // Set up all event listeners
    setupEventListeners() {
        // Select form buttons
        $('#btn-select-form, #btn-select-form-welcome').on('click', () => this.showFormSelection());

        // Form actions
        $('#btn-save-progress').on('click', () => this.saveProgress());
        $('#btn-submit').on('click', () => this.showSubmitConfirmation());
        $('#btn-confirm-submit').on('click', () => this.submitForm());
        $('#btn-cancel').on('click', () => this.cancelInstance());
        $('#btn-new-instance').on('click', () => this.showFormSelection());

        // Page navigation
        $('#btn-prev-page').on('click', () => this.previousPage());
        $('#btn-next-page').on('click', () => this.nextPage());

        // Track form data changes and trigger calculations
        $(document).on('input change', '.runtime-field input, .runtime-field select, .runtime-field textarea, input[type="number"], input[type="text"]', () => {
            this.state.isDirty = true;
            this.collectFormData();
            this.recalculateFormulas();
        });

        // Warn before leaving if unsaved changes
        $(window).on('beforeunload', (e) => {
            if (this.state.isDirty) {
                e.preventDefault();
                return 'You have unsaved changes. Are you sure you want to leave?';
            }
        });
    },

    // Show form selection modal
    async showFormSelection() {
        try {
            $('#committed-forms-list').html(`
                <div class="text-center text-muted">
                    <div class="spinner-border" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <p class="mt-2">Loading committed forms...</p>
                </div>
            `);

            new bootstrap.Modal($('#selectFormModal')[0]).show();

            const response = await fetch('/api/runtime/forms');
            if (!response.ok) throw new Error('Failed to load forms');

            const forms = await response.json();
            this.state.committedForms = forms;

            this.renderFormList(forms);
        } catch (error) {
            this.showMessage('Failed to load forms: ' + error.message, 'error');
            $('#committed-forms-list').html(`<p class="text-danger">Error loading forms</p>`);
        }
    },

    // Render list of committed forms
    renderFormList(forms) {
        if (forms.length === 0) {
            $('#committed-forms-list').html(`
                <p class="text-muted text-center">No committed forms available</p>
            `);
            return;
        }

        let html = '';
        forms.forEach(form => {
            const formData = typeof form.dsl_json === 'string'
                ? JSON.parse(form.dsl_json)
                : form.dsl_json;

            html += `
                <a href="#" class="list-group-item list-group-item-action form-list-item" data-form-id="${form.form_id}">
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <h6 class="mb-1">${formData.title || form.form_id}</h6>
                            <small class="text-muted">${form.form_id} v${form.version}</small>
                        </div>
                        <span class="badge bg-success">Committed</span>
                    </div>
                </a>
            `;
        });

        $('#committed-forms-list').html(html);

        // Attach click handlers
        $('.form-list-item').on('click', async (e) => {
            e.preventDefault();
            const formId = $(e.currentTarget).data('form-id');
            await this.startFormInstance(formId);
            bootstrap.Modal.getInstance($('#selectFormModal')[0]).hide();
        });
    },

    // Start a new form instance
    async startFormInstance(formId) {
        try {
            // Create instance
            const response = await fetch('/api/runtime/instances', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    form_id: formId,
                    user_id: 'user-1' // TODO: Get from authentication
                })
            });

            if (!response.ok) throw new Error('Failed to create instance');

            const instance = await response.json();
            this.state.currentInstance = instance;

            // Load form definition
            const formResponse = await fetch(`/api/forms/${formId}`);
            if (!formResponse.ok) throw new Error('Failed to load form');

            const formData = await formResponse.json();
            this.state.currentForm = formData.form;

            // Initialize state
            this.state.currentPageIndex = 0;
            this.state.formData = {};
            this.state.isDirty = false;

            // Render form
            this.renderForm();

            this.showMessage('Form instance created successfully');
        } catch (error) {
            this.showMessage('Failed to start form: ' + error.message, 'error');
        }
    },

    // Render the entire form
    renderForm() {
        // Hide welcome, show form
        $('#welcome-screen').hide();
        $('#form-container').show();

        // Update toolbar
        $('#form-title-display').text(this.state.currentForm.title);
        $('#btn-save-progress, #btn-submit, #btn-cancel').prop('disabled', false);

        // Render page navigation
        this.renderPageNavigation();

        // Render current page
        this.renderCurrentPage();
    },

    // Render page navigation buttons
    renderPageNavigation() {
        const pages = this.state.currentForm.pages;
        let html = '';

        pages.forEach((page, index) => {
            const activeClass = index === this.state.currentPageIndex ? 'active' : '';
            html += `
                <button class="page-nav-btn ${activeClass}" data-page-index="${index}">
                    ${page.title}
                </button>
            `;
        });

        $('#page-navigation').html(html);

        // Attach click handlers
        $('.page-nav-btn').on('click', (e) => {
            const pageIndex = parseInt($(e.currentTarget).data('page-index'));
            this.goToPage(pageIndex);
        });
    },

    // Render current page
    renderCurrentPage() {
        const page = this.state.currentForm.pages[this.state.currentPageIndex];
        if (!page) return;

        let html = '';

        page.sections.forEach(section => {
            html += `
                <div class="runtime-section" data-section-id="${section.id}">
                    <h3 class="runtime-section-title">${section.title}</h3>
                    <div class="runtime-section-widgets">
                        ${this.renderWidgets(section.widgets)}
                    </div>
                </div>
            `;
        });

        $('#form-content').html(html);

        // Update navigation buttons
        const totalPages = this.state.currentForm.pages.length;
        $('#btn-prev-page').prop('disabled', this.state.currentPageIndex === 0);
        $('#btn-next-page').prop('disabled', this.state.currentPageIndex === totalPages - 1);

        // Populate with existing data
        this.populateFormData();
    },

    // Render widgets
    renderWidgets(widgets) {
        let html = '';

        widgets.forEach(widget => {
            html += this.renderWidget(widget);
        });

        return html;
    },

    // Render a single widget
    renderWidget(widget) {
        switch (widget.type) {
            case 'field':
                return this.renderFieldWidget(widget);
            case 'table':
                return this.renderTableWidget(widget);
            case 'grid':
                return this.renderGridWidget(widget);
            case 'checklist':
                return this.renderChecklistWidget(widget);
            case 'group':
                return this.renderGroupWidget(widget);
            case 'formheader':
                return this.renderFormHeaderWidget(widget);
            case 'signature':
                return this.renderSignatureWidget(widget);
            case 'notes':
                return this.renderNotesWidget(widget);
            case 'hierarchicalchecklist':
                return this.renderHierarchicalChecklistWidget(widget);
            case 'radiogroup':
                return this.renderRadioGroupWidget(widget);
            case 'checkboxgroup':
                return this.renderCheckboxGroupWidget(widget);
            default:
                return `<div class="runtime-widget">Unknown widget type: ${widget.type}</div>`;
        }
    },

    // Render field widget
    renderFieldWidget(widget) {
        const field = widget.field || widget.spec || {};
        const requiredClass = field.required ? 'required' : '';
        const requiredAttr = field.required ? 'required' : '';
        const fieldType = field.type || 'text';
        const placeholder = field.placeholder || '';
        const defaultValue = field.default_value || '';
        const label = field.label || widget.label || 'Field';

        let inputHtml = '';

        if (fieldType === 'textarea') {
            inputHtml = `<textarea class="form-control" id="${widget.id}" name="${field.name || widget.id}" placeholder="${placeholder}" ${requiredAttr}>${defaultValue}</textarea>`;
        } else if (fieldType === 'select') {
            const options = field.options || [];
            const optionsHtml = options.map(opt => `<option value="${opt.value || opt}">${opt.label || opt}</option>`).join('');
            inputHtml = `<select class="form-control" id="${widget.id}" name="${field.name || widget.id}" ${requiredAttr}><option value="">-- Select --</option>${optionsHtml}</select>`;
        } else {
            inputHtml = `<input type="${fieldType}" class="form-control" id="${widget.id}" name="${field.name || widget.id}" placeholder="${placeholder}" value="${defaultValue}" ${requiredAttr}>`;
        }

        return `
            <div class="runtime-widget runtime-field ${requiredClass}" data-widget-id="${widget.id}">
                <label for="${widget.id}">${label}</label>
                ${inputHtml}
            </div>
        `;
    },

    // Render table widget
    renderTableWidget(widget) {
        const table = widget.table || widget.spec || {};
        const columns = table.columns || [];
        const allowAddRows = table.allow_add_rows !== false;
        const allowDeleteRows = table.allow_delete_rows !== false;
        const initialRows = table.initial_rows || [];
        const multiRowHeaders = table.multi_row_headers || null;
        const label = widget.label || 'Table';

        // Generate header HTML
        let headerHtml = '';
        if (multiRowHeaders && multiRowHeaders.length > 0) {
            // Multi-row headers
            multiRowHeaders.forEach(headerRow => {
                let rowHtml = '<tr>';
                headerRow.cells.forEach(cell => {
                    const colspan = cell.colspan > 1 ? `colspan="${cell.colspan}"` : '';
                    const rowspan = cell.rowspan > 1 ? `rowspan="${cell.rowspan}"` : '';
                    const cssClass = cell.css_class || '';
                    rowHtml += `<th ${colspan} ${rowspan} class="${cssClass}">${cell.label}</th>`;
                });
                rowHtml += '</tr>';
                headerHtml += rowHtml;
            });
        } else {
            // Simple single-row header
            let rowHtml = '<tr>';
            rowHtml += columns.map(col => `<th>${col.label || col.name}</th>`).join('');
            if (allowAddRows || allowDeleteRows) {
                rowHtml += '<th style="width: 80px;">Actions</th>';
            }
            rowHtml += '</tr>';
            headerHtml = rowHtml;
        }

        // Generate initial rows HTML
        let rowsHtml = '';
        if (initialRows.length > 0) {
            initialRows.forEach((rowData, index) => {
                rowsHtml += '<tr>';
                columns.forEach(col => {
                    const value = rowData[col.name] || '';
                    const readonly = col.readonly ? 'readonly' : '';
                    const fieldType = col.type === 'integer' || col.type === 'decimal' ? 'number' :
                                     col.type === 'date' ? 'date' :
                                     col.type === 'time' ? 'time' : 'text';

                    rowsHtml += `
                        <td>
                            <input type="${fieldType}"
                                   class="form-control form-control-sm"
                                   name="${col.name}_${index}"
                                   value="${value}"
                                   ${readonly}>
                        </td>
                    `;
                });

                if (allowDeleteRows) {
                    rowsHtml += `<td><button class="btn btn-sm btn-danger btn-delete-row">Remove</button></td>`;
                } else if (allowAddRows) {
                    rowsHtml += `<td></td>`;
                }
                rowsHtml += '</tr>';
            });
        }

        return `
            <div class="runtime-widget runtime-table" data-widget-id="${widget.id}">
                <h5>${label}</h5>
                <table class="table table-bordered">
                    <thead>
                        ${headerHtml}
                    </thead>
                    <tbody data-table-id="${widget.id}">
                        ${rowsHtml}
                    </tbody>
                </table>
                ${allowAddRows ? `<button class="btn btn-sm btn-primary btn-add-row" data-table-id="${widget.id}">+ Add Row</button>` : ''}
            </div>
        `;
    },

    // Render grid widget
    renderGridWidget(widget) {
        const columnsCount = widget.spec?.columns_count || 2;
        const childWidgets = widget.spec?.widgets || [];

        return `
            <div class="runtime-widget runtime-grid cols-${columnsCount}" data-widget-id="${widget.id}">
                ${this.renderWidgets(childWidgets)}
            </div>
        `;
    },

    // Render checklist widget
    renderChecklistWidget(widget) {
        const items = widget.spec?.items || [];

        const itemsHtml = items.map((item, index) => {
            const itemId = `${widget.id}_item_${index}`;
            return `
                <div class="runtime-checklist-item">
                    <input type="checkbox" id="${itemId}" name="${itemId}" value="${item.value || item}">
                    <label for="${itemId}">${item.label || item}</label>
                </div>
            `;
        }).join('');

        return `
            <div class="runtime-widget runtime-checklist" data-widget-id="${widget.id}">
                <h5>${widget.label}</h5>
                ${itemsHtml}
            </div>
        `;
    },

    // Render group widget
    renderGroupWidget(widget) {
        const spec = widget.spec || {};
        const layout = spec.layout || null;
        const cells = spec.cells || null;

        // If layout table with cells, render as bordered table
        if (layout && layout.style === 'table' && cells) {
            return this.renderGroupAsLayoutTable(widget, layout, cells);
        }

        // If layout table with fields (auto-flow), render in grid
        if (layout && layout.style === 'table' && spec.fields) {
            return this.renderGroupAsAutoLayoutTable(widget, layout, spec.fields);
        }

        // Default: render as simple group
        const childWidgets = spec.widgets || [];
        return `
            <div class="runtime-widget runtime-group" data-widget-id="${widget.id}">
                <h4 class="runtime-group-title">${widget.label}</h4>
                ${this.renderWidgets(childWidgets)}
            </div>
        `;
    },

    // Render group as layout table with explicit cell positioning
    renderGroupAsLayoutTable(widget, layout, cells) {
        const rows = layout.rows || this.calculateMaxRow(cells) + 1;
        const columns = layout.columns || this.calculateMaxCol(cells) + 1;
        const bordered = layout.bordered !== false;
        const compact = layout.compact || false;

        // Create 2D grid to track cell spans
        const grid = Array(rows).fill(null).map(() => Array(columns).fill(null));

        // Mark cells that are occupied by spans
        cells.forEach(cell => {
            for (let r = cell.row; r < cell.row + (cell.rowspan || 1); r++) {
                for (let c = cell.col; c < cell.col + (cell.colspan || 1); c++) {
                    if (grid[r] && grid[r][c] !== undefined) {
                        grid[r][c] = cell;
                    }
                }
            }
        });

        let tableHtml = `<table class="table ${bordered ? 'table-bordered' : ''} ${compact ? 'table-sm' : ''} runtime-layout-table">`;

        for (let r = 0; r < rows; r++) {
            tableHtml += '<tr>';
            for (let c = 0; c < columns; c++) {
                // Skip if this cell is part of a span from another cell
                const cellData = this.findCellAt(cells, r, c);
                if (cellData && cellData.row === r && cellData.col === c) {
                    // This is the origin cell
                    const rowspanAttr = cellData.rowspan > 1 ? `rowspan="${cellData.rowspan}"` : '';
                    const colspanAttr = cellData.colspan > 1 ? `colspan="${cellData.colspan}"` : '';
                    const alignClass = cellData.align ? `text-${cellData.align}` : '';
                    const valignClass = cellData.valign ? `align-${cellData.valign}` : '';
                    const cssClass = cellData.css_class || '';

                    tableHtml += `<td ${rowspanAttr} ${colspanAttr} class="${alignClass} ${valignClass} ${cssClass}">`;
                    tableHtml += this.renderFieldInCell(cellData.field, widget.id);
                    tableHtml += '</td>';
                } else if (!this.isCellSpanned(cells, r, c)) {
                    // Empty cell
                    tableHtml += '<td></td>';
                }
            }
            tableHtml += '</tr>';
        }

        tableHtml += '</table>';

        return `
            <div class="runtime-widget runtime-group runtime-group-table" data-widget-id="${widget.id}">
                ${widget.label ? `<h4 class="runtime-group-title">${widget.label}</h4>` : ''}
                ${tableHtml}
            </div>
        `;
    },

    // Render group as auto-layout table (fields flow into columns)
    renderGroupAsAutoLayoutTable(widget, layout, fields) {
        const columns = layout.columns || 2;
        const bordered = layout.bordered !== false;
        const compact = layout.compact || false;
        const rows = Math.ceil(fields.length / columns);

        let tableHtml = `<table class="table ${bordered ? 'table-bordered' : ''} ${compact ? 'table-sm' : ''} runtime-layout-table">`;

        for (let r = 0; r < rows; r++) {
            tableHtml += '<tr>';
            for (let c = 0; c < columns; c++) {
                const fieldIndex = r * columns + c;
                if (fieldIndex < fields.length) {
                    const field = fields[fieldIndex];
                    tableHtml += '<td>';
                    tableHtml += this.renderFieldInCell(field, widget.id);
                    tableHtml += '</td>';
                } else {
                    tableHtml += '<td></td>';
                }
            }
            tableHtml += '</tr>';
        }

        tableHtml += '</table>';

        return `
            <div class="runtime-widget runtime-group runtime-group-table" data-widget-id="${widget.id}">
                ${widget.label ? `<h4 class="runtime-group-title">${widget.label}</h4>` : ''}
                ${tableHtml}
            </div>
        `;
    },

    // Render a field inside a table cell
    renderFieldInCell(field, widgetId) {
        const fieldId = `${widgetId}_${field.name}`;
        const fieldType = field.type || 'text';
        const label = field.label || field.name;
        const required = field.required ? ' <span class="text-danger">*</span>' : '';
        const placeholder = field.placeholder || '';

        let inputHtml = '';
        if (fieldType === 'textarea') {
            inputHtml = `<textarea class="form-control form-control-sm" id="${fieldId}" name="${field.name}" placeholder="${placeholder}"></textarea>`;
        } else if (fieldType === 'select') {
            const options = field.enum || [];
            const optionsHtml = options.map(opt => `<option value="${opt}">${opt}</option>`).join('');
            inputHtml = `<select class="form-control form-control-sm" id="${fieldId}" name="${field.name}"><option value="">-- Select --</option>${optionsHtml}</select>`;
        } else {
            inputHtml = `<input type="${fieldType}" class="form-control form-control-sm" id="${fieldId}" name="${field.name}" placeholder="${placeholder}">`;
        }

        return `
            <div class="layout-cell-field">
                <strong>${label}${required}</strong>
                ${inputHtml}
            </div>
        `;
    },

    // Helper: Find cell at specific row/col
    findCellAt(cells, row, col) {
        return cells.find(cell => cell.row === row && cell.col === col);
    },

    // Helper: Check if cell is spanned by another cell
    isCellSpanned(cells, row, col) {
        return cells.some(cell => {
            return row >= cell.row && row < cell.row + (cell.rowspan || 1) &&
                   col >= cell.col && col < cell.col + (cell.colspan || 1) &&
                   !(cell.row === row && cell.col === col);  // Not the origin cell
        });
    },

    // Helper: Calculate max row from cells
    calculateMaxRow(cells) {
        return Math.max(...cells.map(c => c.row + (c.rowspan || 1) - 1));
    },

    // Helper: Calculate max column from cells
    calculateMaxCol(cells) {
        return Math.max(...cells.map(c => c.col + (c.colspan || 1) - 1));
    },

    // Render FormHeader widget
    renderFormHeaderWidget(widget) {
        const spec = widget.formheader || widget.form_header || widget.spec || {};
        return `
            <div class="runtime-widget runtime-form-header mb-4" data-widget-id="${widget.id}">
                <table class="table table-bordered table-sm">
                    <tr>
                        <td class="bg-light"><strong>Quality Management System</strong></td>
                        <td colspan="3" class="text-center"><strong>${spec.organization || 'ORGANIZATION'}</strong></td>
                        <td class="bg-light"><strong>${spec.category || 'QUALITY FORMS'}</strong></td>
                    </tr>
                    <tr>
                        <td><strong>Document No:</strong></td>
                        <td><input type="text" class="form-control form-control-sm" id="${widget.id}_doc_no" name="${widget.id}_doc_no" value="${spec.document_no || ''}"></td>
                        <td><strong>Revision No.:</strong></td>
                        <td><input type="text" class="form-control form-control-sm" id="${widget.id}_rev_no" name="${widget.id}_rev_no" value="${spec.revision_no || ''}"></td>
                        <td rowspan="2" class="text-center align-middle"><strong>Page:</strong><br>${spec.page_number || '1 of 1'}</td>
                    </tr>
                    <tr>
                        <td colspan="2" class="text-center"><strong>TITLE: ${spec.form_title || this.state.currentForm?.title || 'FORM TITLE'}</strong></td>
                        <td><strong>Effective Date:</strong></td>
                        <td><input type="date" class="form-control form-control-sm" id="${widget.id}_eff_date" name="${widget.id}_eff_date" value="${spec.effective_date || ''}"></td>
                    </tr>
                </table>
            </div>
        `;
    },

    // Render Signature widget
    renderSignatureWidget(widget) {
        const spec = widget.signature || widget.spec || {};
        return `
            <div class="runtime-widget runtime-signature mb-3" data-widget-id="${widget.id}">
                <div class="border p-3 bg-light">
                    <h6 class="mb-3">${spec.role || 'Signature'}</h6>
                    <div class="row">
                        <div class="col-md-6 mb-2">
                            <label class="form-label">${spec.name_label || 'Name'}${spec.name_required ? ' *' : ''}</label>
                            <input type="text" class="form-control" id="${widget.id}_name" name="${widget.id}_name" ${spec.name_required ? 'required' : ''}>
                        </div>
                        ${spec.show_designation ? `
                        <div class="col-md-6 mb-2">
                            <label class="form-label">${spec.designation_label || 'Designation'}</label>
                            <input type="text" class="form-control" id="${widget.id}_designation" name="${widget.id}_designation">
                        </div>
                        ` : ''}
                    </div>
                    ${spec.show_date ? `
                    <div class="row mt-2">
                        <div class="col-md-6">
                            <label class="form-label">${spec.date_label || 'Date'}</label>
                            <input type="date" class="form-control" id="${widget.id}_date" name="${widget.id}_date" ${spec.auto_date ? `value="${new Date().toISOString().split('T')[0]}"` : ''}>
                        </div>
                    </div>
                    ` : ''}
                    ${spec.require_signature_image ? `
                    <div class="mt-3">
                        <label class="form-label">Signature</label>
                        <div style="border: 2px solid #bdc3c7; background: white; width: ${spec.signature_width || 400}px; height: ${spec.signature_height || 100}px; display: flex; align-items: center; justify-content: center; color: #95a5a6;">
                            <em>Click to sign</em>
                        </div>
                    </div>
                    ` : ''}
                </div>
            </div>
        `;
    },

    // Render Notes widget
    renderNotesWidget(widget) {
        const spec = widget.notes || widget.spec || {};
        const styleClass = spec.style === 'warning' ? 'alert-warning' : spec.style === 'info' ? 'alert-info' : 'alert-primary';
        const content = spec.content || '';
        const formattedContent = content.replace(/\n/g, '<br>');
        return `
            <div class="runtime-widget runtime-notes mb-3" data-widget-id="${widget.id}">
                <div class="alert ${styleClass}">
                    ${spec.title ? `<h6 class="alert-heading">${spec.title}</h6>` : ''}
                    <div>${formattedContent}</div>
                </div>
            </div>
        `;
    },

    // Render HierarchicalChecklist widget
    renderHierarchicalChecklistWidget(widget) {
        const spec = widget.spec || {};
        const items = spec.items || [];

        const renderItems = (items, level = 0) => {
            return items.map((item, index) => {
                const indent = level * (spec.indent_size || 20);
                const number = item.number || `${level + 1}.${index}`;
                const numberDisplay = spec.show_numbering ? `${number} ` : '';

                let itemHtml = `
                    <div class="form-check" style="margin-left: ${indent}px;">
                        <input class="form-check-input" type="checkbox" id="${widget.id}_${item.key}" name="${widget.id}_${item.key}" ${item.required || spec.all_required ? 'required' : ''}>
                        <label class="form-check-label" for="${widget.id}_${item.key}">
                            ${numberDisplay}${item.label}
                        </label>
                    </div>
                `;

                if (item.children && item.children.length > 0) {
                    itemHtml += renderItems(item.children, level + 1);
                }

                return itemHtml;
            }).join('');
        };

        return `
            <div class="runtime-widget runtime-hierarchical-checklist mb-3" data-widget-id="${widget.id}">
                <h5 class="mb-3">${widget.label}</h5>
                ${renderItems(items)}
            </div>
        `;
    },

    // Render RadioGroup widget
    renderRadioGroupWidget(widget) {
        const spec = widget.spec || {};
        const options = spec.options || [];
        const orientation = spec.orientation === 'horizontal' ? 'd-flex gap-3' : '';
        const requiredAttr = spec.required ? 'required' : '';

        const optionsHtml = options.map((opt, index) => `
            <div class="form-check">
                <input class="form-check-input" type="radio" name="${widget.id}" id="${widget.id}_${index}" value="${opt.value || opt}" ${requiredAttr}>
                <label class="form-check-label" for="${widget.id}_${index}">
                    ${opt.label || opt}
                </label>
            </div>
        `).join('');

        return `
            <div class="runtime-widget runtime-radio-group mb-3" data-widget-id="${widget.id}">
                <label class="form-label"><strong>${widget.label}</strong>${spec.required ? ' <span class="text-danger">*</span>' : ''}</label>
                <div class="${orientation}">
                    ${optionsHtml}
                </div>
            </div>
        `;
    },

    // Render CheckboxGroup widget
    renderCheckboxGroupWidget(widget) {
        const spec = widget.spec || {};
        const options = spec.options || [];
        const orientation = spec.orientation === 'horizontal' ? 'd-flex gap-3' : spec.orientation === 'grid' ? `row row-cols-${spec.grid_columns || 2}` : '';

        const optionsHtml = options.map((opt, index) => `
            <div class="form-check ${spec.orientation === 'grid' ? 'col' : ''}">
                <input class="form-check-input" type="checkbox" name="${widget.id}_${index}" id="${widget.id}_${index}" value="${opt.value || opt}">
                <label class="form-check-label" for="${widget.id}_${index}">
                    ${opt.label || opt}
                </label>
            </div>
        `).join('');

        return `
            <div class="runtime-widget runtime-checkbox-group mb-3" data-widget-id="${widget.id}">
                <label class="form-label"><strong>${widget.label}</strong></label>
                ${spec.min_selections || spec.max_selections ? `<small class="text-muted d-block mb-2">Select ${spec.min_selections ? `at least ${spec.min_selections}` : ''}${spec.min_selections && spec.max_selections ? ' and ' : ''}${spec.max_selections ? `at most ${spec.max_selections}` : ''}</small>` : ''}
                <div class="${orientation}">
                    ${optionsHtml}
                </div>
            </div>
        `;
    },

    // Populate form with existing data
    populateFormData() {
        Object.keys(this.state.formData).forEach(key => {
            const $field = $(`[name="${key}"]`);
            if ($field.length) {
                if ($field.attr('type') === 'checkbox') {
                    $field.prop('checked', this.state.formData[key]);
                } else {
                    $field.val(this.state.formData[key]);
                }
            }
        });
    },

    // Collect form data from inputs
    collectFormData() {
        this.state.formData = {};

        $('#form-content input, #form-content select, #form-content textarea').each((i, el) => {
            const $el = $(el);
            const name = $el.attr('name');

            if (!name) return;

            if ($el.attr('type') === 'checkbox') {
                this.state.formData[name] = $el.is(':checked');
            } else if ($el.attr('type') === 'radio') {
                if ($el.is(':checked')) {
                    this.state.formData[name] = $el.val();
                }
            } else {
                this.state.formData[name] = $el.val();
            }
        });

        return this.state.formData;
    },

    // Recalculate all formulas in the form
    recalculateFormulas() {
        if (!this.state.currentForm) return;

        // Find all fields with formulas
        this.state.currentForm.pages.forEach(page => {
            page.sections.forEach(section => {
                section.widgets.forEach(widget => {
                    if (widget.type === 'field' && widget.spec?.formula) {
                        this.calculateField(widget);
                    } else if (widget.type === 'table') {
                        this.calculateTableAggregates(widget);
                    }
                });
            });
        });
    },

    // Calculate a single field with formula
    calculateField(widget) {
        const formula = widget.spec?.formula;
        if (!formula) return;

        const result = FormulaEvaluator.evaluate(formula, this.state.formData);

        if (result.error) {
            console.warn(`Formula error in ${widget.id}: ${result.error}`);
            return;
        }

        // Update the field value
        const $field = $(`#${widget.id}`);
        if ($field.length) {
            $field.val(result.value);
            // Mark as readonly
            $field.attr('readonly', true);
            $field.addClass('bg-light');
        }

        // Update form data
        this.state.formData[widget.id] = result.value;
    },

    // Calculate table aggregates
    calculateTableAggregates(widget) {
        const aggregates = widget.spec?.aggregates;
        if (!aggregates || aggregates.length === 0) return;

        // Get table rows data
        const tableId = widget.id;
        const rows = [];

        $(`[data-table-id="${tableId}"] tr`).each((i, row) => {
            const rowData = {};
            $(row).find('input, select').each((j, input) => {
                const name = $(input).attr('name');
                if (name) {
                    rowData[name] = $(input).val();
                }
            });
            if (Object.keys(rowData).length > 0) {
                rows.push(rowData);
            }
        });

        // Calculate each aggregate
        aggregates.forEach(agg => {
            const column = agg.column;
            const type = agg.type || 'SUM';
            const targetField = agg.target_field;

            if (targetField) {
                const result = FormulaEvaluator.evaluateAggregate(rows, column, type);

                // Update target field
                const $target = $(`#${targetField}`);
                if ($target.length) {
                    $target.val(result);
                    $target.attr('readonly', true);
                    $target.addClass('bg-light');
                }

                this.state.formData[targetField] = result;
            }
        });
    },

    // Navigation
    goToPage(pageIndex) {
        this.state.currentPageIndex = pageIndex;
        this.renderPageNavigation();
        this.renderCurrentPage();
    },

    previousPage() {
        if (this.state.currentPageIndex > 0) {
            this.goToPage(this.state.currentPageIndex - 1);
        }
    },

    nextPage() {
        if (this.state.currentPageIndex < this.state.currentForm.pages.length - 1) {
            this.goToPage(this.state.currentPageIndex + 1);
        }
    },

    // Save progress
    async saveProgress() {
        try {
            const data = this.collectFormData();

            const response = await fetch(`/api/runtime/instances/${this.state.currentInstance.instance_id}/save`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    data_json: JSON.stringify(data),
                    user_id: 'user-1' // TODO: Get from authentication
                })
            });

            if (!response.ok) throw new Error('Failed to save progress');

            this.state.isDirty = false;
            $('#instance-id-display').text(this.state.currentInstance.instance_id);
            new bootstrap.Modal($('#saveProgressModal')[0]).show();
        } catch (error) {
            this.showMessage('Failed to save progress: ' + error.message, 'error');
        }
    },

    // Show submit confirmation
    showSubmitConfirmation() {
        new bootstrap.Modal($('#submitConfirmModal')[0]).show();
    },

    // Submit form
    async submitForm() {
        try {
            const data = this.collectFormData();

            // Validate all widgets
            const validationErrors = this.validateForm();

            if (validationErrors.length > 0) {
                bootstrap.Modal.getInstance($('#submitConfirmModal')[0]).hide();
                this.showMessage(validationErrors[0], 'error');
                return;
            }

            const response = await fetch(`/api/runtime/instances/${this.state.currentInstance.instance_id}/submit`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    data_json: JSON.stringify(data)
                })
            });

            if (!response.ok) throw new Error('Failed to submit form');

            this.state.isDirty = false;
            bootstrap.Modal.getInstance($('#submitConfirmModal')[0]).hide();
            $('#submit-instance-id-display').text(this.state.currentInstance.instance_id);
            new bootstrap.Modal($('#submitSuccessModal')[0]).show();

            // Disable form actions
            $('#btn-save-progress, #btn-submit, #btn-cancel').prop('disabled', true);
        } catch (error) {
            this.showMessage('Failed to submit form: ' + error.message, 'error');
        }
    },

    // Cancel instance
    async cancelInstance() {
        if (!confirm('Cancel this form? All unsaved progress will be lost.')) return;

        try {
            const response = await fetch(`/api/runtime/instances/${this.state.currentInstance.instance_id}`, {
                method: 'DELETE'
            });

            if (!response.ok) throw new Error('Failed to delete instance');

            this.resetState();
            this.showMessage('Form instance cancelled');
        } catch (error) {
            this.showMessage('Failed to cancel instance: ' + error.message, 'error');
        }
    },

    // Reset to initial state
    resetState() {
        this.state.currentForm = null;
        this.state.currentInstance = null;
        this.state.currentPageIndex = 0;
        this.state.formData = {};
        this.state.isDirty = false;

        $('#form-container').hide();
        $('#welcome-screen').show();
        $('#form-title-display').text('Form Runtime');
    },

    // Validate entire form
    validateForm() {
        const errors = [];

        // Clear all previous validation states
        $('.is-invalid').removeClass('is-invalid');

        // Validate each widget type
        this.state.currentForm.pages.forEach(page => {
            page.sections.forEach(section => {
                section.widgets.forEach(widget => {
                    const widgetErrors = this.validateWidget(widget);
                    errors.push(...widgetErrors);
                });
            });
        });

        return errors;
    },

    // Validate a single widget
    validateWidget(widget) {
        const errors = [];

        switch (widget.type) {
            case 'field':
                if (widget.required) {
                    const value = this.state.formData[widget.id];
                    if (!value || value === '') {
                        $(`#${widget.id}`).addClass('is-invalid');
                        errors.push(`${widget.label} is required`);
                    }
                }
                break;

            case 'radiogroup':
                if (widget.spec?.required) {
                    const selected = $(`input[name="${widget.id}"]:checked`).length > 0;
                    if (!selected) {
                        errors.push(`${widget.label} requires a selection`);
                    }
                }
                break;

            case 'checkboxgroup':
                const checked = $(`input[name^="${widget.id}_"]:checked`).length;
                const spec = widget.spec || {};

                if (spec.min_selections && checked < spec.min_selections) {
                    errors.push(`${widget.label} requires at least ${spec.min_selections} selection(s)`);
                }

                if (spec.max_selections && checked > spec.max_selections) {
                    errors.push(`${widget.label} allows at most ${spec.max_selections} selection(s)`);
                }
                break;

            case 'signature':
                if (widget.spec?.name_required) {
                    const name = this.state.formData[`${widget.id}_name`];
                    if (!name || name === '') {
                        $(`#${widget.id}_name`).addClass('is-invalid');
                        errors.push(`${widget.label} - Name is required`);
                    }
                }
                break;

            case 'hierarchicalchecklist':
                if (widget.spec?.all_required) {
                    const checkboxes = $(`input[id^="${widget.id}_"]`);
                    const unchecked = checkboxes.filter((i, el) => !$(el).is(':checked'));
                    if (unchecked.length > 0) {
                        errors.push(`${widget.label} - All items must be checked`);
                    }
                }
                break;
        }

        return errors;
    },

    // Show status message
    showMessage(message, type = 'success') {
        const $msg = $(`<div class="status-message ${type}">${message}</div>`);
        $('body').append($msg);

        setTimeout(() => {
            $msg.fadeOut(() => $msg.remove());
        }, 3000);
    }
};

// Initialize when DOM is ready
$(document).ready(() => {
    FormRuntime.init();
});
