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
            `;
        }

        $('#inspector-content').html(html);

        // Attach change handlers
        $('#inspector-content input, #inspector-content select').on('change', (e) => {
            this.updateWidgetProperty(widgetId, $(e.currentTarget).data('prop'), $(e.currentTarget).val());
        });
        $('#inspector-content input[type="checkbox"]').on('change', (e) => {
            this.updateWidgetProperty(widgetId, $(e.currentTarget).data('prop'), $(e.currentTarget).is(':checked'));
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

            this.state.currentForm = data.form;
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
        this.showMessage('Preview functionality coming soon', 'warning');
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

        return {
            form: {
                id: formId,
                title: formTitle,
                version: formVersion,
                pages: this.state.currentForm.pages
            }
        };
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
