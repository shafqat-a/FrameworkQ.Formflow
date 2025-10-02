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
        const requiredClass = widget.required ? 'required' : '';
        const requiredAttr = widget.required ? 'required' : '';
        const fieldType = widget.spec?.type || 'text';
        const placeholder = widget.spec?.placeholder || '';
        const defaultValue = widget.spec?.default_value || '';

        let inputHtml = '';

        if (fieldType === 'textarea') {
            inputHtml = `<textarea class="form-control" id="${widget.id}" name="${widget.id}" placeholder="${placeholder}" ${requiredAttr}>${defaultValue}</textarea>`;
        } else if (fieldType === 'select') {
            const options = widget.spec?.options || [];
            const optionsHtml = options.map(opt => `<option value="${opt.value || opt}">${opt.label || opt}</option>`).join('');
            inputHtml = `<select class="form-control" id="${widget.id}" name="${widget.id}" ${requiredAttr}><option value="">-- Select --</option>${optionsHtml}</select>`;
        } else {
            inputHtml = `<input type="${fieldType}" class="form-control" id="${widget.id}" name="${widget.id}" placeholder="${placeholder}" value="${defaultValue}" ${requiredAttr}>`;
        }

        return `
            <div class="runtime-widget runtime-field ${requiredClass}" data-widget-id="${widget.id}">
                <label for="${widget.id}">${widget.label}</label>
                ${inputHtml}
            </div>
        `;
    },

    // Render table widget
    renderTableWidget(widget) {
        const columns = widget.spec?.columns || [];
        const allowAddRows = widget.spec?.allow_add_rows !== false;

        let headerHtml = columns.map(col => `<th>${col.label || col.name}</th>`).join('');
        if (allowAddRows) {
            headerHtml += '<th style="width: 80px;">Actions</th>';
        }

        return `
            <div class="runtime-widget runtime-table" data-widget-id="${widget.id}">
                <h5>${widget.label}</h5>
                <table class="table">
                    <thead>
                        <tr>${headerHtml}</tr>
                    </thead>
                    <tbody data-table-id="${widget.id}">
                        <!-- Rows will be added dynamically -->
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
        const childWidgets = widget.spec?.widgets || [];

        return `
            <div class="runtime-widget runtime-group" data-widget-id="${widget.id}">
                <h4 class="runtime-group-title">${widget.label}</h4>
                ${this.renderWidgets(childWidgets)}
            </div>
        `;
    },

    // Render FormHeader widget
    renderFormHeaderWidget(widget) {
        const spec = widget.spec || {};
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
        const spec = widget.spec || {};
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
        const spec = widget.spec || {};
        const styleClass = spec.style === 'warning' ? 'alert-warning' : spec.style === 'note' ? 'alert-info' : 'alert-primary';
        return `
            <div class="runtime-widget runtime-notes mb-3" data-widget-id="${widget.id}">
                <div class="alert ${styleClass}">
                    ${spec.title ? `<h6 class="alert-heading">${spec.title}</h6>` : ''}
                    <div>${spec.content || ''}</div>
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

            // Validate required fields
            let isValid = true;
            $('.runtime-field.required input, .runtime-field.required select, .runtime-field.required textarea').each((i, el) => {
                if (!$(el).val()) {
                    $(el).addClass('is-invalid');
                    isValid = false;
                } else {
                    $(el).removeClass('is-invalid');
                }
            });

            if (!isValid) {
                bootstrap.Modal.getInstance($('#submitConfirmModal')[0]).hide();
                this.showMessage('Please fill in all required fields', 'error');
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
