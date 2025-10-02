/**
 * Form Designer - Main Application
 * Handles initialization and coordination of all designer modules
 */

const FormDesigner = {
    // Application state
    state: {
        currentForm: null,
        selectedWidget: null,
        currentPageId: 'page-1',
        pageCounter: 1,
        sectionCounter: 1,
        widgetCounter: 1,
        isDirty: false
    },

    // Initialize the application
    init() {
        console.log('Initializing Form Designer...');

        // Set up event listeners
        this.setupEventListeners();

        // Initialize default state
        this.initializeDefaultForm();

        console.log('Form Designer initialized successfully');
    },

    // Set up all event listeners
    setupEventListeners() {
        // Toolbar buttons
        $('#btn-new').on('click', () => this.newForm());
        $('#btn-open').on('click', () => this.openForm());
        $('#btn-save').on('click', () => this.saveDraft());
        $('#btn-commit').on('click', () => this.commitForm());
        $('#btn-preview').on('click', () => this.previewForm());
        $('#btn-export-yaml').on('click', () => this.exportYaml());
        $('#btn-import-yaml').on('click', () => this.importYaml());
        $('#btn-export-sql').on('click', () => this.exportSql());

        // Form metadata inputs
        $('#form-title, #form-id, #form-version').on('input', () => {
            this.state.isDirty = true;
        });

        // Page management
        $('#btn-add-page').on('click', () => this.addPage());
        $(document).on('click', '.page-tab', (e) => this.switchPage($(e.currentTarget).data('page-id')));

        // Section management
        $(document).on('click', '.btn-add-section', (e) => {
            const pageId = $(e.currentTarget).closest('.page-container').data('page-id');
            this.addSection(pageId);
        });
        $(document).on('click', '.btn-delete-section', (e) => {
            const sectionId = $(e.currentTarget).closest('.section').data('section-id');
            this.deleteSection(sectionId);
        });
        $(document).on('input', '.section-title-input', () => {
            this.state.isDirty = true;
        });

        // Widget management
        $(document).on('click', '.widget-instance', (e) => {
            const widgetId = $(e.currentTarget).data('widget-id');
            this.selectWidget(widgetId);
        });
        $(document).on('click', '.btn-delete-widget', (e) => {
            e.stopPropagation();
            const widgetId = $(e.currentTarget).closest('.widget-instance').data('widget-id');
            this.deleteWidget(widgetId);
        });

        // Drag and drop
        this.setupDragAndDrop();

        // Warn before leaving if unsaved changes
        $(window).on('beforeunload', (e) => {
            if (this.state.isDirty) {
                e.preventDefault();
                return 'You have unsaved changes. Are you sure you want to leave?';
            }
        });
    },

    // Initialize with default blank form
    initializeDefaultForm() {
        this.state.currentForm = {
            id: '',
            title: '',
            version: '1.0',
            pages: [{
                id: 'page-1',
                title: 'Page 1',
                sections: [{
                    id: 'section-1',
                    title: 'Section 1',
                    widgets: []
                }]
            }]
        };
    },

    // Setup drag and drop for widgets
    setupDragAndDrop() {
        // Make widget palette items draggable
        $('.widget-item').attr('draggable', true);

        $('.widget-item').on('dragstart', function(e) {
            const widgetType = $(this).data('type');
            e.originalEvent.dataTransfer.setData('widget-type', widgetType);
            $(this).addClass('dragging');
        });

        $('.widget-item').on('dragend', function() {
            $(this).removeClass('dragging');
        });

        // Make drop zones accept drops
        $(document).on('dragover', '.drop-zone', function(e) {
            e.preventDefault();
            $(this).addClass('drag-over');
        });

        $(document).on('dragleave', '.drop-zone', function() {
            $(this).removeClass('drag-over');
        });

        $(document).on('drop', '.drop-zone', function(e) {
            e.preventDefault();
            $(this).removeClass('drag-over');

            const widgetType = e.originalEvent.dataTransfer.getData('widget-type');
            const sectionId = $(this).data('section-id');

            FormDesigner.addWidget(sectionId, widgetType);
        });
    },

    // Add a new widget to a section
    addWidget(sectionId, widgetType) {
        const widgetId = `widget-${this.state.widgetCounter++}`;

        const widget = {
            id: widgetId,
            type: widgetType,
            label: `${widgetType.charAt(0).toUpperCase() + widgetType.slice(1)} ${this.state.widgetCounter - 1}`,
            required: false,
            spec: this.getDefaultWidgetSpec(widgetType)
        };

        // Add to state
        const section = this.findSection(sectionId);
        if (section) {
            section.widgets.push(widget);
            this.state.isDirty = true;
            this.renderWidget(sectionId, widget);
        }
    },

    // Get default spec for widget type
    getDefaultWidgetSpec(widgetType) {
        switch (widgetType) {
            case 'field':
                return {
                    type: 'text',
                    placeholder: '',
                    default_value: ''
                };
            case 'table':
                return {
                    columns: [],
                    allow_add_rows: true,
                    allow_delete_rows: true,
                    min_rows: 1,
                    max_rows: null
                };
            case 'grid':
                return {
                    columns_count: 2,
                    widgets: []
                };
            case 'checklist':
                return {
                    items: []
                };
            case 'group':
                return {
                    widgets: []
                };
            case 'formheader':
                return {
                    document_no: '',
                    revision_no: '',
                    effective_date: '',
                    page_number: '',
                    organization: '',
                    form_title: '',
                    category: 'QUALITY FORMS',
                    show_on_all_pages: true
                };
            case 'signature':
                return {
                    role: '',
                    name_label: 'Name',
                    name_required: true,
                    designation_label: 'Designation',
                    show_designation: true,
                    date_label: 'Date',
                    show_date: true,
                    auto_date: false,
                    require_signature_image: false,
                    signature_type: 'draw',
                    signature_width: 400,
                    signature_height: 100
                };
            case 'notes':
                return {
                    content: '',
                    style: 'info',
                    title: '',
                    markdown: false,
                    collapsible: false,
                    collapsed: false
                };
            case 'hierarchicalchecklist':
                return {
                    items: [],
                    numbering_style: 'decimal',
                    show_numbering: true,
                    indent_size: 20,
                    all_required: false
                };
            case 'radiogroup':
                return {
                    options: [],
                    orientation: 'horizontal',
                    required: false,
                    default_value: null,
                    button_style: false,
                    spacing: 10
                };
            case 'checkboxgroup':
                return {
                    options: [],
                    orientation: 'vertical',
                    min_selections: null,
                    max_selections: null,
                    default_values: [],
                    grid_columns: 2,
                    show_select_all: false,
                    spacing: 10
                };
            default:
                return {};
        }
    },

    // Render a widget in the UI
    renderWidget(sectionId, widget) {
        const $dropZone = $(`.drop-zone[data-section-id="${sectionId}"]`);

        const $widget = $(`
            <div class="widget-instance" data-widget-id="${widget.id}">
                <button class="btn-delete-widget btn btn-sm btn-danger">×</button>
                <div class="widget-instance-header">
                    <span class="widget-instance-type">${widget.type}</span>
                    <span class="widget-instance-id">${widget.id}</span>
                </div>
                <div class="widget-instance-content">
                    <strong>${widget.label}</strong>
                    ${widget.required ? '<span class="text-danger">*</span>' : ''}
                </div>
            </div>
        `);

        $dropZone.append($widget);
    },

    // Select a widget
    selectWidget(widgetId) {
        $('.widget-instance').removeClass('selected');
        $(`.widget-instance[data-widget-id="${widgetId}"]`).addClass('selected');

        this.state.selectedWidget = widgetId;
        this.showWidgetProperties(widgetId);
    },

    // Show widget properties in inspector
    showWidgetProperties(widgetId) {
        const widget = this.findWidget(widgetId);
        if (!widget) return;

        let html = `
            <h5>Widget Properties</h5>
            <div class="form-group">
                <label>Widget ID</label>
                <input type="text" class="form-control" id="prop-widget-id" value="${widget.id}" data-prop="id">
            </div>
            <div class="form-group">
                <label>Label</label>
                <input type="text" class="form-control" id="prop-label" value="${widget.label}" data-prop="label">
            </div>
            <div class="form-check">
                <input type="checkbox" class="form-check-input" id="prop-required" ${widget.required ? 'checked' : ''} data-prop="required">
                <label class="form-check-label" for="prop-required">Required</label>
            </div>
        `;

        // Add type-specific properties
        if (widget.type === 'field') {
            html += `
                <div class="form-group">
                    <label>Field Type</label>
                    <select class="form-control" id="prop-field-type" data-prop="spec.type">
                        <option value="text" ${widget.spec.type === 'text' ? 'selected' : ''}>Text</option>
                        <option value="number" ${widget.spec.type === 'number' ? 'selected' : ''}>Number</option>
                        <option value="date" ${widget.spec.type === 'date' ? 'selected' : ''}>Date</option>
                        <option value="time" ${widget.spec.type === 'time' ? 'selected' : ''}>Time</option>
                        <option value="datetime" ${widget.spec.type === 'datetime' ? 'selected' : ''}>Date & Time</option>
                        <option value="email" ${widget.spec.type === 'email' ? 'selected' : ''}>Email</option>
                        <option value="tel" ${widget.spec.type === 'tel' ? 'selected' : ''}>Phone</option>
                        <option value="select" ${widget.spec.type === 'select' ? 'selected' : ''}>Select</option>
                        <option value="textarea" ${widget.spec.type === 'textarea' ? 'selected' : ''}>Textarea</option>
                    </select>
                </div>
                <div class="form-group">
                    <label>Placeholder</label>
                    <input type="text" class="form-control" id="prop-placeholder" value="${widget.spec.placeholder || ''}" data-prop="spec.placeholder">
                </div>
                <div class="form-group">
                    <label>Formula (for computed fields)</label>
                    <input type="text" class="form-control" id="prop-formula" value="${widget.spec.formula || ''}" data-prop="spec.formula" placeholder="e.g., quantity * unit_price">
                    <small class="form-text text-muted" id="formula-help"></small>
                </div>
            `;
        }

        // Add formula validation on change
        if (widget.type === 'field') {
            setTimeout(() => {
                $('#prop-formula').on('input', (e) => {
                    const formula = $(e.target).val();
                    if (formula) {
                        const parsed = FormulaParser.parse(formula);
                        const helpText = FormulaParser.describe(parsed);
                        $('#formula-help').text(helpText);
                        $('#formula-help').removeClass('text-danger text-success');
                        $('#formula-help').addClass(parsed.valid ? 'text-success' : 'text-danger');
                    } else {
                        $('#formula-help').text('');
                    }
                });
                // Trigger initial validation
                if (widget.spec.formula) {
                    $('#prop-formula').trigger('input');
                }
            }, 100);
        }

        if (widget.type === 'table') {
            html += this.renderTableEditor(widget);
        } else if (widget.type === 'radiogroup' || widget.type === 'checkboxgroup') {
            html += this.renderOptionsEditor(widget);
        } else if (widget.type === 'formheader') {
            html += this.renderFormHeaderEditor(widget);
        } else if (widget.type === 'signature') {
            html += this.renderSignatureEditor(widget);
        } else if (widget.type === 'notes') {
            html += this.renderNotesEditor(widget);
        }

        $('#inspector-content').html(html);

        // Attach change handlers
        $('#inspector-content input, #inspector-content select').on('change', (e) => {
            const $el = $(e.currentTarget);
            if ($el.data('column-prop') !== undefined) {
                this.updateColumnProperty(widgetId, $el.data('column-prop'), $el.val());
            } else if ($el.data('option-prop') !== undefined) {
                this.updateOptionProperty(widgetId, $el.data('option-prop'), $el.val());
            } else if ($el.data('prop')) {
                this.updateWidgetProperty(widgetId, $el.data('prop'), $el.val());
            }
        });
        $('#inspector-content input[type="checkbox"]').on('change', (e) => {
            this.updateWidgetProperty(widgetId, $(e.currentTarget).data('prop'), $(e.currentTarget).is(':checked'));
        });

        // Table column management
        $('#btn-add-column').on('click', () => this.addTableColumn(widgetId));
        $(document).on('click', '.btn-delete-column', (e) => {
            const index = parseInt($(e.currentTarget).data('index'));
            this.deleteTableColumn(widgetId, index);
        });

        // Options management
        $('#btn-add-option').on('click', () => this.addOption(widgetId));
        $(document).on('click', '.btn-delete-option', (e) => {
            const index = parseInt($(e.currentTarget).data('index'));
            this.deleteOption(widgetId, index);
        });
    },

    // Update widget property
    updateWidgetProperty(widgetId, propPath, value) {
        const widget = this.findWidget(widgetId);
        if (!widget) return;

        const parts = propPath.split('.');
        let target = widget;

        for (let i = 0; i < parts.length - 1; i++) {
            target = target[parts[i]];
        }

        target[parts[parts.length - 1]] = value;
        this.state.isDirty = true;

        // Re-render widget
        const section = this.findSectionByWidget(widgetId);
        if (section) {
            $(`.widget-instance[data-widget-id="${widgetId}"] .widget-instance-content`).html(`
                <strong>${widget.label}</strong>
                ${widget.required ? '<span class="text-danger">*</span>' : ''}
            `);
        }
    },

    // Delete a widget
    deleteWidget(widgetId) {
        if (!confirm('Delete this widget?')) return;

        const section = this.findSectionByWidget(widgetId);
        if (section) {
            section.widgets = section.widgets.filter(w => w.id !== widgetId);
            $(`.widget-instance[data-widget-id="${widgetId}"]`).remove();
            this.state.isDirty = true;

            if (this.state.selectedWidget === widgetId) {
                this.state.selectedWidget = null;
                $('#inspector-content').html('<p class="text-muted">Select a widget to view properties</p>');
            }
        }
    },

    // Add a new page
    addPage() {
        const pageId = `page-${++this.state.pageCounter}`;
        const pageTitle = `Page ${this.state.pageCounter}`;

        const page = {
            id: pageId,
            title: pageTitle,
            sections: [{
                id: `section-${++this.state.sectionCounter}`,
                title: 'Section 1',
                widgets: []
            }]
        };

        this.state.currentForm.pages.push(page);
        this.state.isDirty = true;

        this.renderPage(page);
        this.switchPage(pageId);
    },

    // Render a page
    renderPage(page) {
        // Add tab
        const $tab = $(`<button class="page-tab" data-page-id="${page.id}">${page.title}</button>`);
        $tab.insertBefore('#btn-add-page');

        // Add page content
        const $pageContainer = $(`
            <div class="page-container" data-page-id="${page.id}">
                <h2>${page.title}</h2>
                <div class="sections-container"></div>
                <button class="btn-add-section btn btn-sm btn-secondary">+ Add Section</button>
            </div>
        `);

        $('#page-content').append($pageContainer);

        // Render sections
        page.sections.forEach(section => this.renderSection(page.id, section));
    },

    // Switch to a page
    switchPage(pageId) {
        $('.page-tab').removeClass('active');
        $(`.page-tab[data-page-id="${pageId}"]`).addClass('active');

        $('.page-container').removeClass('active').hide();
        $(`.page-container[data-page-id="${pageId}"]`).addClass('active').show();

        this.state.currentPageId = pageId;
    },

    // Add a new section
    addSection(pageId) {
        const sectionId = `section-${++this.state.sectionCounter}`;
        const sectionTitle = `Section ${this.state.sectionCounter}`;

        const section = {
            id: sectionId,
            title: sectionTitle,
            widgets: []
        };

        const page = this.state.currentForm.pages.find(p => p.id === pageId);
        if (page) {
            page.sections.push(section);
            this.state.isDirty = true;
            this.renderSection(pageId, section);
        }
    },

    // Render a section
    renderSection(pageId, section) {
        const $section = $(`
            <div class="section" data-section-id="${section.id}">
                <div class="section-header">
                    <input type="text" class="section-title-input" value="${section.title}" placeholder="Section Title">
                    <button class="btn-delete-section btn btn-sm btn-danger">×</button>
                </div>
                <div class="section-widgets drop-zone" data-section-id="${section.id}">
                </div>
            </div>
        `);

        $(`.page-container[data-page-id="${pageId}"] .sections-container`).append($section);

        // Render widgets
        section.widgets.forEach(widget => this.renderWidget(section.id, widget));
    },

    // Delete a section
    deleteSection(sectionId) {
        if (!confirm('Delete this section and all its widgets?')) return;

        const page = this.state.currentForm.pages.find(p =>
            p.sections.some(s => s.id === sectionId)
        );

        if (page) {
            page.sections = page.sections.filter(s => s.id !== sectionId);
            $(`.section[data-section-id="${sectionId}"]`).remove();
            this.state.isDirty = true;
        }
    },

    // Find helpers
    findSection(sectionId) {
        for (const page of this.state.currentForm.pages) {
            const section = page.sections.find(s => s.id === sectionId);
            if (section) return section;
        }
        return null;
    },

    findWidget(widgetId) {
        for (const page of this.state.currentForm.pages) {
            for (const section of page.sections) {
                const widget = section.widgets.find(w => w.id === widgetId);
                if (widget) return widget;
            }
        }
        return null;
    },

    findSectionByWidget(widgetId) {
        for (const page of this.state.currentForm.pages) {
            for (const section of page.sections) {
                if (section.widgets.some(w => w.id === widgetId)) {
                    return section;
                }
            }
        }
        return null;
    },

    // Form operations
    newForm() {
        if (this.state.isDirty && !confirm('Discard unsaved changes?')) return;

        location.reload();
    },

    async openForm() {
        try {
            const response = await fetch('/api/forms');
            const forms = await response.json();

            let html = '';
            forms.forEach(form => {
                const badge = form.is_committed
                    ? '<span class="badge bg-success">Committed</span>'
                    : '<span class="badge bg-warning">Draft</span>';

                html += `
                    <a href="#" class="list-group-item list-group-item-action" data-form-id="${form.form_id}">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <h6 class="mb-1">${form.dsl_json.title || form.form_id}</h6>
                                <small class="text-muted">${form.form_id} v${form.version}</small>
                            </div>
                            ${badge}
                        </div>
                    </a>
                `;
            });

            $('#form-list').html(html || '<p class="text-muted">No forms found</p>');

            $('#form-list a').on('click', async (e) => {
                e.preventDefault();
                const formId = $(e.currentTarget).data('form-id');
                await this.loadForm(formId);
                bootstrap.Modal.getInstance($('#openFormModal')[0]).hide();
            });

            new bootstrap.Modal($('#openFormModal')[0]).show();
        } catch (error) {
            this.showMessage('Failed to load forms: ' + error.message, 'error');
        }
    },

    async loadForm(formId) {
        try {
            const response = await fetch(`/api/forms/${formId}`);
            const data = await response.json();

            // Convert from API DTO format to Designer internal format
            this.state.currentForm = WidgetConverter.formFromDTO(data.form);
            this.state.isDirty = false;

            // Update UI
            $('#form-title').val(this.state.currentForm.title);
            $('#form-id').val(this.state.currentForm.id);
            $('#form-version').val(this.state.currentForm.version);

            // Clear and rebuild UI
            $('#page-tabs .page-tab').remove();
            $('#page-content .page-container').remove();

            this.state.currentForm.pages.forEach(page => this.renderPage(page));
            this.switchPage(this.state.currentForm.pages[0].id);

            this.showMessage('Form loaded successfully');
        } catch (error) {
            this.showMessage('Failed to load form: ' + error.message, 'error');
        }
    },

    async saveDraft() {
        try {
            const formData = this.collectFormData();
            const formId = formData.form.id;

            if (!formId) {
                this.showMessage('Please enter a Form ID', 'error');
                return;
            }

            const response = await fetch(`/api/forms/${formId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(formData)
            });

            if (!response.ok) throw new Error('Save failed');

            this.state.isDirty = false;
            this.showMessage('Draft saved successfully');
        } catch (error) {
            this.showMessage('Failed to save draft: ' + error.message, 'error');
        }
    },

    async commitForm() {
        if (!confirm('Commit this form? It will become read-only and available for runtime.')) return;

        try {
            const formId = $('#form-id').val();
            if (!formId) {
                this.showMessage('Please enter a Form ID', 'error');
                return;
            }

            const response = await fetch(`/api/forms/${formId}/commit`, {
                method: 'POST'
            });

            if (!response.ok) throw new Error('Commit failed');

            this.state.isDirty = false;
            this.showMessage('Form committed successfully');
        } catch (error) {
            this.showMessage('Failed to commit form: ' + error.message, 'error');
        }
    },

    previewForm() {
        const formData = this.collectFormData();

        if (!formData.form.pages || formData.form.pages.length === 0) {
            this.showMessage('Form is empty. Add some widgets to preview.', 'warning');
            return;
        }

        // Initialize preview state
        this.previewState = {
            currentPageIndex: 0,
            form: formData.form
        };

        // Render preview
        this.renderPreview();

        // Show modal
        new bootstrap.Modal($('#previewModal')[0]).show();

        // Setup preview navigation
        $('#preview-btn-prev').off('click').on('click', () => this.previewPreviousPage());
        $('#preview-btn-next').off('click').on('click', () => this.previewNextPage());
    },

    renderPreview() {
        const form = this.previewState.form;
        const currentPageIndex = this.previewState.currentPageIndex;
        const page = form.pages[currentPageIndex];

        // Render page navigation
        let navHtml = '';
        form.pages.forEach((p, index) => {
            const activeClass = index === currentPageIndex ? 'active' : '';
            navHtml += `
                <button class="page-nav-btn ${activeClass}" data-page-index="${index}">
                    ${p.title}
                </button>
            `;
        });
        $('#preview-page-nav').html(navHtml);

        // Render page content
        let contentHtml = `<h2>${page.title}</h2>`;

        page.sections.forEach(section => {
            contentHtml += this.renderPreviewSection(section);
        });

        $('#preview-content').html(contentHtml);

        // Update navigation buttons
        $('#preview-btn-prev').prop('disabled', currentPageIndex === 0);
        $('#preview-btn-next').prop('disabled', currentPageIndex === form.pages.length - 1);

        // Page nav buttons
        $('.page-nav-btn').off('click').on('click', (e) => {
            this.previewState.currentPageIndex = parseInt($(e.currentTarget).data('page-index'));
            this.renderPreview();
        });
    },

    renderPreviewSection(section) {
        let html = `
            <div class="runtime-section" style="margin-bottom: 20px; padding: 20px; background: #f8f9fa; border-radius: 6px; border-left: 4px solid #3498db;">
                <h3 style="font-size: 1.5rem; font-weight: 600; color: #2c3e50; margin-bottom: 20px; padding-bottom: 10px; border-bottom: 2px solid #ecf0f1;">
                    ${section.title}
                </h3>
                <div class="runtime-section-widgets">
        `;

        section.widgets.forEach(widget => {
            html += this.renderPreviewWidget(widget);
        });

        html += `
                </div>
            </div>
        `;

        return html;
    },

    renderPreviewWidget(widget) {
        switch (widget.type) {
            case 'field':
                return this.renderPreviewField(widget);
            case 'table':
                return this.renderPreviewTable(widget);
            case 'grid':
                return this.renderPreviewGrid(widget);
            case 'checklist':
                return this.renderPreviewChecklist(widget);
            case 'group':
                return this.renderPreviewGroup(widget);
            case 'formheader':
                return this.renderPreviewFormHeader(widget);
            case 'signature':
                return this.renderPreviewSignature(widget);
            case 'notes':
                return this.renderPreviewNotes(widget);
            case 'hierarchicalchecklist':
                return this.renderPreviewHierarchicalChecklist(widget);
            case 'radiogroup':
                return this.renderPreviewRadioGroup(widget);
            case 'checkboxgroup':
                return this.renderPreviewCheckboxGroup(widget);
            default:
                return `<div class="mb-3"><em>Widget type: ${widget.type}</em></div>`;
        }
    },

    renderPreviewField(widget) {
        const requiredStar = widget.required ? '<span class="text-danger">*</span>' : '';
        const fieldType = widget.spec?.type || 'text';
        const placeholder = widget.spec?.placeholder || '';

        let inputHtml = '';
        if (fieldType === 'textarea') {
            inputHtml = `<textarea class="form-control" placeholder="${placeholder}" disabled></textarea>`;
        } else if (fieldType === 'select') {
            inputHtml = `<select class="form-control" disabled><option>-- Select --</option></select>`;
        } else {
            inputHtml = `<input type="${fieldType}" class="form-control" placeholder="${placeholder}" disabled>`;
        }

        return `
            <div class="mb-3">
                <label class="form-label"><strong>${widget.label}</strong> ${requiredStar}</label>
                ${inputHtml}
            </div>
        `;
    },

    renderPreviewTable(widget) {
        const columns = widget.spec?.columns || [];
        const allowAddRows = widget.spec?.allow_add_rows !== false;

        if (columns.length === 0) {
            return `
                <div class="mb-3 p-3 border rounded bg-light">
                    <h5>${widget.label}</h5>
                    <p class="text-muted"><em>No columns defined. Select the table widget and add columns in the properties panel.</em></p>
                </div>
            `;
        }

        let headerHtml = columns.map(col => `<th>${col.label || col.name}</th>`).join('');
        if (allowAddRows) {
            headerHtml += '<th style="width: 80px;">Actions</th>';
        }

        let rowHtml = columns.map(col => {
            if (col.type === 'checkbox') {
                return '<td><input type="checkbox" class="form-check-input" disabled></td>';
            } else if (col.type === 'integer' || col.type === 'decimal') {
                return '<td><input type="number" class="form-control form-control-sm" disabled></td>';
            } else if (col.type === 'date') {
                return '<td><input type="date" class="form-control form-control-sm" disabled></td>';
            } else {
                return '<td><input type="text" class="form-control form-control-sm" disabled></td>';
            }
        }).join('');

        if (allowAddRows) {
            rowHtml += '<td><button class="btn btn-sm btn-danger" disabled>×</button></td>';
        }

        return `
            <div class="mb-3">
                <h5>${widget.label}</h5>
                <table class="table table-bordered">
                    <thead class="table-light">
                        <tr>${headerHtml}</tr>
                    </thead>
                    <tbody>
                        <tr>${rowHtml}</tr>
                    </tbody>
                </table>
                ${allowAddRows ? '<button class="btn btn-sm btn-primary" disabled>+ Add Row</button>' : ''}
            </div>
        `;
    },

    renderPreviewGrid(widget) {
        const cols = widget.spec?.columns_count || 2;
        return `
            <div class="mb-3">
                <div class="row">
                    <div class="col-md-${12/cols}">
                        <input type="text" class="form-control" placeholder="Grid Cell" disabled>
                    </div>
                    <div class="col-md-${12/cols}">
                        <input type="text" class="form-control" placeholder="Grid Cell" disabled>
                    </div>
                </div>
            </div>
        `;
    },

    renderPreviewChecklist(widget) {
        return `
            <div class="mb-3">
                <h5>${widget.label}</h5>
                <div class="form-check">
                    <input class="form-check-input" type="checkbox" disabled>
                    <label class="form-check-label">Checklist item example</label>
                </div>
            </div>
        `;
    },

    renderPreviewGroup(widget) {
        return `
            <div class="mb-3 p-3 border rounded">
                <h5>${widget.label}</h5>
                <p class="text-muted">Group contains nested widgets</p>
            </div>
        `;
    },

    renderPreviewFormHeader(widget) {
        const spec = widget.spec || {};
        return `
            <div class="mb-4 p-3 border rounded bg-light">
                <table class="table table-sm table-bordered mb-0">
                    <tr>
                        <td><strong>Document No:</strong> ${spec.document_no || '[Document No]'}</td>
                        <td><strong>Revision:</strong> ${spec.revision_no || '[Rev]'}</td>
                        <td><strong>Effective Date:</strong> ${spec.effective_date || '[Date]'}</td>
                        <td><strong>Page:</strong> ${spec.page_number || '[Page]'}</td>
                    </tr>
                    <tr>
                        <td colspan="4" class="text-center">
                            <strong>${spec.organization || '[Organization]'}</strong><br>
                            ${spec.form_title || '[Form Title]'}
                        </td>
                    </tr>
                </table>
            </div>
        `;
    },

    renderPreviewSignature(widget) {
        const spec = widget.spec || {};
        return `
            <div class="mb-3 p-3 border rounded">
                <h6>${spec.role || 'Signature'}</h6>
                <div class="row">
                    <div class="col-md-6">
                        <label class="form-label">${spec.name_label || 'Name'}</label>
                        <input type="text" class="form-control" disabled>
                    </div>
                    ${spec.show_designation ? `
                    <div class="col-md-6">
                        <label class="form-label">${spec.designation_label || 'Designation'}</label>
                        <input type="text" class="form-control" disabled>
                    </div>
                    ` : ''}
                </div>
                ${spec.show_date ? `
                <div class="mt-2">
                    <label class="form-label">${spec.date_label || 'Date'}</label>
                    <input type="date" class="form-control" disabled>
                </div>
                ` : ''}
                ${spec.require_signature_image ? `
                <div class="mt-2">
                    <label class="form-label">Signature</label>
                    <div style="border: 2px dashed #ccc; height: ${spec.signature_height || 100}px; width: ${spec.signature_width || 400}px; display: flex; align-items: center; justify-content: center; color: #999;">
                        Signature area
                    </div>
                </div>
                ` : ''}
            </div>
        `;
    },

    renderPreviewNotes(widget) {
        const spec = widget.spec || {};
        const styleClass = spec.style === 'warning' ? 'alert-warning' : spec.style === 'note' ? 'alert-info' : 'alert-primary';
        return `
            <div class="mb-3 alert ${styleClass}">
                ${spec.title ? `<h6 class="alert-heading">${spec.title}</h6>` : ''}
                <p class="mb-0">${spec.content || 'Note content'}</p>
            </div>
        `;
    },

    renderPreviewHierarchicalChecklist(widget) {
        return `
            <div class="mb-3">
                <h5>${widget.label}</h5>
                <div class="form-check">
                    <input class="form-check-input" type="checkbox" disabled>
                    <label class="form-check-label">1.0 Parent item</label>
                </div>
                <div class="form-check ms-4">
                    <input class="form-check-input" type="checkbox" disabled>
                    <label class="form-check-label">1.1 Child item</label>
                </div>
            </div>
        `;
    },

    renderPreviewRadioGroup(widget) {
        const spec = widget.spec || {};
        const orientation = spec.orientation === 'horizontal' ? 'd-flex gap-3' : '';
        const options = spec.options || [{label: 'Option 1', value: '1'}, {label: 'Option 2', value: '2'}];

        let optionsHtml = options.map(opt => `
            <div class="form-check">
                <input class="form-check-input" type="radio" name="preview-${widget.id}" disabled>
                <label class="form-check-label">${opt.label}</label>
            </div>
        `).join('');

        return `
            <div class="mb-3">
                <label class="form-label"><strong>${widget.label}</strong></label>
                <div class="${orientation}">
                    ${optionsHtml}
                </div>
            </div>
        `;
    },

    renderPreviewCheckboxGroup(widget) {
        const spec = widget.spec || {};
        const orientation = spec.orientation === 'horizontal' ? 'd-flex gap-3' : '';
        const options = spec.options || [{label: 'Option 1', value: '1'}, {label: 'Option 2', value: '2'}];

        let optionsHtml = options.map(opt => `
            <div class="form-check">
                <input class="form-check-input" type="checkbox" disabled>
                <label class="form-check-label">${opt.label}</label>
            </div>
        `).join('');

        return `
            <div class="mb-3">
                <label class="form-label"><strong>${widget.label}</strong></label>
                <div class="${orientation}">
                    ${optionsHtml}
                </div>
            </div>
        `;
    },

    previewPreviousPage() {
        if (this.previewState.currentPageIndex > 0) {
            this.previewState.currentPageIndex--;
            this.renderPreview();
        }
    },

    previewNextPage() {
        if (this.previewState.currentPageIndex < this.previewState.form.pages.length - 1) {
            this.previewState.currentPageIndex++;
            this.renderPreview();
        }
    },

    async exportYaml() {
        try {
            const formId = $('#form-id').val();
            if (!formId) {
                this.showMessage('Please enter a Form ID', 'error');
                return;
            }

            window.location.href = `/api/export/${formId}/yaml`;
        } catch (error) {
            this.showMessage('Failed to export YAML: ' + error.message, 'error');
        }
    },

    importYaml() {
        $('#file-import-yaml').click();
        $('#file-import-yaml').off('change').on('change', async (e) => {
            const file = e.target.files[0];
            if (!file) return;

            const formData = new FormData();
            formData.append('file', file);

            try {
                const response = await fetch('/api/import/yaml', {
                    method: 'POST',
                    body: formData
                });

                if (!response.ok) throw new Error('Import failed');

                const data = await response.json();
                await this.loadForm(data.form.form.id);
                this.showMessage('YAML imported successfully');
            } catch (error) {
                this.showMessage('Failed to import YAML: ' + error.message, 'error');
            }
        });
    },

    async exportSql() {
        try {
            const formId = $('#form-id').val();
            if (!formId) {
                this.showMessage('Please enter a Form ID', 'error');
                return;
            }

            window.location.href = `/api/export/${formId}/sql`;
        } catch (error) {
            this.showMessage('Failed to export SQL: ' + error.message, 'error');
        }
    },

    // Collect form data from UI
    collectFormData() {
        const formId = $('#form-id').val();
        const formTitle = $('#form-title').val();
        const formVersion = $('#form-version').val();

        // Update section titles from inputs
        $('.section').each((i, el) => {
            const sectionId = $(el).data('section-id');
            const newTitle = $(el).find('.section-title-input').val();
            const section = this.findSection(sectionId);
            if (section) {
                section.title = newTitle;
            }
        });

        const designerForm = {
            id: formId,
            title: formTitle,
            version: formVersion,
            pages: this.state.currentForm.pages
        };

        // Convert to API DTO format
        return WidgetConverter.formToDTO(designerForm);
    },

    // Render table column editor
    renderTableEditor(widget) {
        const columns = widget.spec.columns || [];

        let columnsHtml = '';
        columns.forEach((col, index) => {
            columnsHtml += `
                <div class="border p-2 mb-2 bg-light" data-column-index="${index}">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <strong>Column ${index + 1}</strong>
                        <button class="btn btn-sm btn-danger btn-delete-column" data-index="${index}">×</button>
                    </div>
                    <input type="text" class="form-control form-control-sm mb-1" placeholder="Name" value="${col.name || ''}" data-column-prop="${index}.name">
                    <input type="text" class="form-control form-control-sm mb-1" placeholder="Label" value="${col.label || ''}" data-column-prop="${index}.label">
                    <select class="form-control form-control-sm" data-column-prop="${index}.type">
                        <option value="string" ${col.type === 'string' ? 'selected' : ''}>String</option>
                        <option value="integer" ${col.type === 'integer' ? 'selected' : ''}>Integer</option>
                        <option value="decimal" ${col.type === 'decimal' ? 'selected' : ''}>Decimal</option>
                        <option value="date" ${col.type === 'date' ? 'selected' : ''}>Date</option>
                        <option value="checkbox" ${col.type === 'checkbox' ? 'selected' : ''}>Checkbox</option>
                    </select>
                </div>
            `;
        });

        return `
            <div class="form-group">
                <label>Table Columns</label>
                <div id="table-columns-list">
                    ${columnsHtml}
                </div>
                <button type="button" class="btn btn-sm btn-primary mt-2" id="btn-add-column">+ Add Column</button>
            </div>
        `;
    },

    // Render options editor for radio/checkbox groups
    renderOptionsEditor(widget) {
        const options = widget.spec.options || [];

        let optionsHtml = '';
        options.forEach((opt, index) => {
            const label = opt.label || opt;
            const value = opt.value || opt;
            optionsHtml += `
                <div class="border p-2 mb-2 bg-light" data-option-index="${index}">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <strong>Option ${index + 1}</strong>
                        <button class="btn btn-sm btn-danger btn-delete-option" data-index="${index}">×</button>
                    </div>
                    <input type="text" class="form-control form-control-sm mb-1" placeholder="Label" value="${label}" data-option-prop="${index}.label">
                    <input type="text" class="form-control form-control-sm" placeholder="Value" value="${value}" data-option-prop="${index}.value">
                </div>
            `;
        });

        return `
            <div class="form-group">
                <label>Options</label>
                <div id="options-list">
                    ${optionsHtml}
                </div>
                <button type="button" class="btn btn-sm btn-primary mt-2" id="btn-add-option">+ Add Option</button>
            </div>
            <div class="form-group">
                <label>Orientation</label>
                <select class="form-control" data-prop="spec.orientation">
                    <option value="horizontal" ${widget.spec.orientation === 'horizontal' ? 'selected' : ''}>Horizontal</option>
                    <option value="vertical" ${widget.spec.orientation === 'vertical' ? 'selected' : ''}>Vertical</option>
                </select>
            </div>
        `;
    },

    // Render FormHeader editor
    renderFormHeaderEditor(widget) {
        const spec = widget.spec || {};
        return `
            <div class="form-group">
                <label>Document No</label>
                <input type="text" class="form-control" value="${spec.document_no || ''}" data-prop="spec.document_no">
            </div>
            <div class="form-group">
                <label>Revision No</label>
                <input type="text" class="form-control" value="${spec.revision_no || ''}" data-prop="spec.revision_no">
            </div>
            <div class="form-group">
                <label>Effective Date</label>
                <input type="text" class="form-control" value="${spec.effective_date || ''}" data-prop="spec.effective_date">
            </div>
            <div class="form-group">
                <label>Organization</label>
                <input type="text" class="form-control" value="${spec.organization || ''}" data-prop="spec.organization">
            </div>
        `;
    },

    // Render Signature editor
    renderSignatureEditor(widget) {
        const spec = widget.spec || {};
        return `
            <div class="form-group">
                <label>Role</label>
                <input type="text" class="form-control" value="${spec.role || ''}" data-prop="spec.role" placeholder="e.g., Reviewed by (GMT-1)">
            </div>
            <div class="form-check">
                <input type="checkbox" class="form-check-input" ${spec.show_designation ? 'checked' : ''} data-prop="spec.show_designation">
                <label class="form-check-label">Show Designation</label>
            </div>
            <div class="form-check">
                <input type="checkbox" class="form-check-input" ${spec.show_date ? 'checked' : ''} data-prop="spec.show_date">
                <label class="form-check-label">Show Date</label>
            </div>
            <div class="form-check">
                <input type="checkbox" class="form-check-input" ${spec.require_signature_image ? 'checked' : ''} data-prop="spec.require_signature_image">
                <label class="form-check-label">Require Signature Image</label>
            </div>
        `;
    },

    // Render Notes editor
    renderNotesEditor(widget) {
        const spec = widget.spec || {};
        return `
            <div class="form-group">
                <label>Title</label>
                <input type="text" class="form-control" value="${spec.title || ''}" data-prop="spec.title">
            </div>
            <div class="form-group">
                <label>Content</label>
                <textarea class="form-control" rows="4" data-prop="spec.content">${spec.content || ''}</textarea>
            </div>
            <div class="form-group">
                <label>Style</label>
                <select class="form-control" data-prop="spec.style">
                    <option value="info" ${spec.style === 'info' ? 'selected' : ''}>Info</option>
                    <option value="warning" ${spec.style === 'warning' ? 'selected' : ''}>Warning</option>
                    <option value="note" ${spec.style === 'note' ? 'selected' : ''}>Note</option>
                </select>
            </div>
        `;
    },

    // Add table column
    addTableColumn(widgetId) {
        const widget = this.findWidget(widgetId);
        if (!widget || !widget.spec.columns) {
            widget.spec.columns = [];
        }

        widget.spec.columns.push({
            name: `column_${widget.spec.columns.length + 1}`,
            label: `Column ${widget.spec.columns.length + 1}`,
            type: 'string'
        });

        this.state.isDirty = true;
        this.showWidgetProperties(widgetId);
    },

    // Delete table column
    deleteTableColumn(widgetId, index) {
        const widget = this.findWidget(widgetId);
        if (!widget || !widget.spec.columns) return;

        widget.spec.columns.splice(index, 1);
        this.state.isDirty = true;
        this.showWidgetProperties(widgetId);
    },

    // Update column property
    updateColumnProperty(widgetId, propPath, value) {
        const widget = this.findWidget(widgetId);
        if (!widget) return;

        const [index, prop] = propPath.split('.');
        const columnIndex = parseInt(index);

        if (widget.spec.columns && widget.spec.columns[columnIndex]) {
            widget.spec.columns[columnIndex][prop] = value;
            this.state.isDirty = true;
        }
    },

    // Add option to radio/checkbox group
    addOption(widgetId) {
        const widget = this.findWidget(widgetId);
        if (!widget || !widget.spec.options) {
            widget.spec.options = [];
        }

        widget.spec.options.push({
            label: `Option ${widget.spec.options.length + 1}`,
            value: `option_${widget.spec.options.length + 1}`
        });

        this.state.isDirty = true;
        this.showWidgetProperties(widgetId);
    },

    // Delete option
    deleteOption(widgetId, index) {
        const widget = this.findWidget(widgetId);
        if (!widget || !widget.spec.options) return;

        widget.spec.options.splice(index, 1);
        this.state.isDirty = true;
        this.showWidgetProperties(widgetId);
    },

    // Update option property
    updateOptionProperty(widgetId, propPath, value) {
        const widget = this.findWidget(widgetId);
        if (!widget) return;

        const [index, prop] = propPath.split('.');
        const optionIndex = parseInt(index);

        if (widget.spec.options && widget.spec.options[optionIndex]) {
            widget.spec.options[optionIndex][prop] = value;
            this.state.isDirty = true;
        }
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
    FormDesigner.init();
});
