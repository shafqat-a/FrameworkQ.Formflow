# Visual Form Designer Specification

## Overview

The Visual Form Designer is a web-based drag-and-drop interface for creating form definitions without writing YAML manually. It consists of:

- **Frontend**: HTML5 + jQuery + jQuery UI (drag-drop)
- **Backend**: .NET Core Web API
- **Output**: YAML form definition compliant with DSL v0.1

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Browser (Frontend)                      │
│  ┌───────────────┬──────────────────┬────────────────────┐  │
│  │  Widget       │   Canvas         │   Inspector        │  │
│  │  Palette      │   (Form Builder) │   Panel            │  │
│  │  (Draggable)  │   (Droppable)    │   (Properties)     │  │
│  └───────────────┴──────────────────┴────────────────────┘  │
│                          ▲    │                              │
│                          │    ▼                              │
│                    jQuery + AJAX                             │
└──────────────────────────┼────┼─────────────────────────────┘
                           │    │
                    ┌──────┘    └──────┐
                    │                  │
                    ▼                  ▼
┌─────────────────────────────────────────────────────────────┐
│              .NET Core Web API (Backend)                     │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Controllers                                          │   │
│  │  - FormDefinitionController                          │   │
│  │  - WidgetController                                  │   │
│  │  - ExportController                                  │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Services                                             │   │
│  │  - FormBuilderService (business logic)               │   │
│  │  - YamlExportService (DSL → YAML)                    │   │
│  │  - ValidationService (JSON Schema validation)        │   │
│  │  - SqlGeneratorService (YAML → SQL DDL)              │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Data Access                                          │   │
│  │  - FormDefinitionRepository (EF Core)                │   │
│  │  - Database (PostgreSQL/SQL Server)                  │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Frontend Specification

### 1. Page Layout

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Form Designer</title>
    <link rel="stylesheet" href="/css/designer.css">
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.13.2/themes/base/jquery-ui.css">
</head>
<body>
    <!-- Top Toolbar -->
    <div id="toolbar">
        <div class="toolbar-left">
            <button id="btn-new" class="btn btn-primary">New Form</button>
            <button id="btn-open" class="btn btn-secondary">Open</button>
            <button id="btn-save" class="btn btn-success">Save</button>
        </div>
        <div class="toolbar-center">
            <input type="text" id="form-title" placeholder="Form Title" />
            <input type="text" id="form-id" placeholder="form-id" pattern="[a-z0-9_-]+" />
            <input type="text" id="form-version" placeholder="1.0" />
        </div>
        <div class="toolbar-right">
            <button id="btn-preview" class="btn btn-info">Preview</button>
            <button id="btn-export-yaml" class="btn btn-warning">Export YAML</button>
            <button id="btn-export-sql" class="btn btn-warning">Generate SQL</button>
        </div>
    </div>

    <div id="main-container">
        <!-- Left Panel: Widget Palette -->
        <div id="palette" class="panel-left">
            <h3>Widgets</h3>
            <div class="widget-item" data-type="field" draggable="true">
                <i class="icon-field"></i>
                <span>Field</span>
            </div>
            <div class="widget-item" data-type="group" draggable="true">
                <i class="icon-group"></i>
                <span>Group</span>
            </div>
            <div class="widget-item" data-type="table" draggable="true">
                <i class="icon-table"></i>
                <span>Table</span>
            </div>
            <div class="widget-item" data-type="grid" draggable="true">
                <i class="icon-grid"></i>
                <span>Grid</span>
            </div>
            <div class="widget-item" data-type="checklist" draggable="true">
                <i class="icon-checklist"></i>
                <span>Checklist</span>
            </div>
        </div>

        <!-- Center Panel: Canvas (Form Builder) -->
        <div id="canvas" class="panel-center">
            <div id="page-tabs">
                <ul id="page-tab-list">
                    <li data-page-id="page-1" class="active">Page 1</li>
                    <li id="add-page-btn">+ Add Page</li>
                </ul>
            </div>

            <div id="page-content">
                <div class="page-view" data-page-id="page-1">
                    <!-- Sections will be added here -->
                    <div class="section-container">
                        <button class="btn-add-section">+ Add Section</button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Right Panel: Inspector (Properties) -->
        <div id="inspector" class="panel-right">
            <h3>Properties</h3>
            <div id="properties-container">
                <p class="no-selection">Select a widget to edit properties</p>
            </div>
        </div>
    </div>

    <!-- Modals -->
    <div id="modal-container"></div>

    <script src="https://code.jquery.com/jquery-3.7.0.min.js"></script>
    <script src="https://code.jquery.com/ui/1.13.2/jquery-ui.min.js"></script>
    <script src="/js/designer.js"></script>
</body>
</html>
```

### 2. CSS Structure

```css
/* designer.css */

/* Layout */
body {
    margin: 0;
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    overflow: hidden;
}

#toolbar {
    height: 60px;
    background: #2c3e50;
    color: white;
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0 20px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

#main-container {
    display: flex;
    height: calc(100vh - 60px);
}

.panel-left {
    width: 250px;
    background: #ecf0f1;
    border-right: 1px solid #bdc3c7;
    overflow-y: auto;
    padding: 20px;
}

.panel-center {
    flex: 1;
    background: #ffffff;
    overflow-y: auto;
    padding: 20px;
}

.panel-right {
    width: 350px;
    background: #ecf0f1;
    border-left: 1px solid #bdc3c7;
    overflow-y: auto;
    padding: 20px;
}

/* Widget Palette */
.widget-item {
    background: white;
    border: 2px solid #3498db;
    border-radius: 4px;
    padding: 15px;
    margin-bottom: 10px;
    cursor: move;
    transition: all 0.3s;
}

.widget-item:hover {
    background: #3498db;
    color: white;
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(0,0,0,0.2);
}

.widget-item.dragging {
    opacity: 0.5;
}

/* Canvas */
#page-tabs {
    border-bottom: 2px solid #3498db;
    margin-bottom: 20px;
}

#page-tab-list {
    list-style: none;
    padding: 0;
    margin: 0;
    display: flex;
}

#page-tab-list li {
    padding: 10px 20px;
    cursor: pointer;
    background: #ecf0f1;
    margin-right: 5px;
    border-radius: 4px 4px 0 0;
}

#page-tab-list li.active {
    background: #3498db;
    color: white;
}

.section-container {
    min-height: 100px;
    margin-bottom: 20px;
}

.section {
    background: #f8f9fa;
    border: 2px dashed #bdc3c7;
    border-radius: 4px;
    padding: 15px;
    margin-bottom: 15px;
    position: relative;
}

.section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 10px;
    padding-bottom: 10px;
    border-bottom: 1px solid #bdc3c7;
}

.section-title {
    font-weight: bold;
    font-size: 16px;
}

.section-actions {
    display: flex;
    gap: 5px;
}

.widget-drop-zone {
    min-height: 80px;
    border: 2px dashed #95a5a6;
    border-radius: 4px;
    padding: 10px;
    background: white;
}

.widget-drop-zone.drag-over {
    background: #e8f5e9;
    border-color: #4caf50;
}

.widget-instance {
    background: white;
    border: 1px solid #bdc3c7;
    border-radius: 4px;
    padding: 10px;
    margin-bottom: 10px;
    cursor: pointer;
    position: relative;
}

.widget-instance.selected {
    border-color: #3498db;
    box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.2);
}

.widget-instance:hover {
    border-color: #3498db;
}

.widget-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.widget-type-badge {
    background: #3498db;
    color: white;
    padding: 2px 8px;
    border-radius: 3px;
    font-size: 11px;
    text-transform: uppercase;
}

.widget-actions {
    display: flex;
    gap: 5px;
}

/* Inspector Panel */
.form-group {
    margin-bottom: 15px;
}

.form-group label {
    display: block;
    font-weight: 600;
    margin-bottom: 5px;
    font-size: 13px;
}

.form-group input,
.form-group select,
.form-group textarea {
    width: 100%;
    padding: 8px;
    border: 1px solid #bdc3c7;
    border-radius: 4px;
    font-size: 13px;
}

.form-group input[type="checkbox"] {
    width: auto;
    margin-right: 5px;
}

/* Buttons */
.btn {
    padding: 8px 16px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 14px;
    transition: all 0.3s;
}

.btn-primary { background: #3498db; color: white; }
.btn-secondary { background: #95a5a6; color: white; }
.btn-success { background: #27ae60; color: white; }
.btn-warning { background: #f39c12; color: white; }
.btn-danger { background: #e74c3c; color: white; }
.btn-info { background: #16a085; color: white; }

.btn:hover { opacity: 0.9; transform: translateY(-1px); }

.btn-icon {
    padding: 5px 8px;
    font-size: 12px;
}

/* Table Editor */
.column-list {
    list-style: none;
    padding: 0;
    margin: 10px 0;
}

.column-item {
    background: white;
    border: 1px solid #bdc3c7;
    padding: 10px;
    margin-bottom: 5px;
    border-radius: 4px;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.column-item:hover {
    background: #f8f9fa;
}
```

### 3. JavaScript Structure

```javascript
// designer.js

(function($) {
    'use strict';

    // API Configuration
    const API_BASE = '/api';
    const API = {
        forms: {
            list: () => `${API_BASE}/forms`,
            get: (id) => `${API_BASE}/forms/${id}`,
            save: () => `${API_BASE}/forms`,
            update: (id) => `${API_BASE}/forms/${id}`,
            delete: (id) => `${API_BASE}/forms/${id}`
        },
        export: {
            yaml: (id) => `${API_BASE}/export/${id}/yaml`,
            sql: (id) => `${API_BASE}/export/${id}/sql`
        }
    };

    // State Management
    const DesignerState = {
        formDefinition: {
            form: {
                id: '',
                title: '',
                version: '1.0',
                pages: []
            }
        },
        currentPageId: 'page-1',
        selectedWidget: null,
        isDirty: false
    };

    // Widget Templates
    const WidgetTemplates = {
        field: {
            type: 'field',
            id: '',
            field: {
                name: '',
                label: 'New Field',
                type: 'string',
                required: false
            }
        },
        group: {
            type: 'group',
            id: '',
            fields: []
        },
        table: {
            type: 'table',
            id: '',
            table: {
                row_mode: 'infinite',
                min: 1,
                columns: []
            }
        },
        grid: {
            type: 'grid',
            id: '',
            grid: {
                rows: { mode: 'finite' },
                columns: { generator: {} },
                cell: { type: 'string' }
            }
        },
        checklist: {
            type: 'checklist',
            id: '',
            checklist: {
                items: []
            }
        }
    };

    // Initialize Designer
    function initDesigner() {
        initDragDrop();
        initEventHandlers();
        initPageManagement();
        loadFormIfExists();
    }

    // Drag and Drop
    function initDragDrop() {
        // Make palette widgets draggable
        $('.widget-item').draggable({
            helper: 'clone',
            cursor: 'move',
            revert: 'invalid',
            start: function() {
                $(this).addClass('dragging');
            },
            stop: function() {
                $(this).removeClass('dragging');
            }
        });

        // Make widget drop zones droppable
        $(document).on('dragenter', '.widget-drop-zone', function() {
            $(this).addClass('drag-over');
        });

        $(document).on('dragleave', '.widget-drop-zone', function() {
            $(this).removeClass('drag-over');
        });

        // Initialize droppable on sections
        updateDroppableZones();
    }

    function updateDroppableZones() {
        $('.widget-drop-zone').droppable({
            accept: '.widget-item',
            drop: function(event, ui) {
                const widgetType = ui.draggable.data('type');
                const sectionId = $(this).closest('.section').data('section-id');
                addWidgetToSection(widgetType, sectionId);
                $(this).removeClass('drag-over');
            }
        });
    }

    // Add Widget to Section
    function addWidgetToSection(widgetType, sectionId) {
        const widget = $.extend(true, {}, WidgetTemplates[widgetType]);
        widget.id = generateId(widgetType);

        // Add to state
        const page = getCurrentPage();
        const section = page.sections.find(s => s.id === sectionId);
        if (!section.widgets) section.widgets = [];
        section.widgets.push(widget);

        // Update UI
        renderSection(sectionId);
        markDirty();
    }

    // Section Management
    function addSection() {
        const page = getCurrentPage();
        const sectionId = generateId('section');

        const section = {
            id: sectionId,
            title: 'New Section',
            widgets: []
        };

        page.sections.push(section);
        renderPage(page.id);
        markDirty();
    }

    function renderSection(sectionId) {
        const page = getCurrentPage();
        const section = page.sections.find(s => s.id === sectionId);

        const sectionHtml = `
            <div class="section" data-section-id="${section.id}">
                <div class="section-header">
                    <input type="text" class="section-title-input" value="${section.title}"
                           onchange="updateSectionTitle('${section.id}', this.value)" />
                    <div class="section-actions">
                        <button class="btn btn-icon btn-danger" onclick="deleteSection('${section.id}')">
                            🗑️
                        </button>
                    </div>
                </div>
                <div class="widget-drop-zone" data-section-id="${section.id}">
                    ${renderWidgets(section.widgets)}
                </div>
            </div>
        `;

        $(`[data-section-id="${sectionId}"]`).replaceWith(sectionHtml);
        updateDroppableZones();
    }

    function renderWidgets(widgets) {
        if (!widgets || widgets.length === 0) {
            return '<p style="text-align:center; color:#95a5a6;">Drop widgets here</p>';
        }

        return widgets.map(widget => `
            <div class="widget-instance" data-widget-id="${widget.id}"
                 onclick="selectWidget('${widget.id}')">
                <div class="widget-header">
                    <div>
                        <span class="widget-type-badge">${widget.type}</span>
                        <span class="widget-label">${getWidgetLabel(widget)}</span>
                    </div>
                    <div class="widget-actions">
                        <button class="btn btn-icon btn-danger"
                                onclick="event.stopPropagation(); deleteWidget('${widget.id}')">
                            ✕
                        </button>
                    </div>
                </div>
            </div>
        `).join('');
    }

    // Inspector Panel (Properties)
    function selectWidget(widgetId) {
        $('.widget-instance').removeClass('selected');
        $(`[data-widget-id="${widgetId}"]`).addClass('selected');

        DesignerState.selectedWidget = widgetId;
        renderInspector(widgetId);
    }

    function renderInspector(widgetId) {
        const widget = findWidget(widgetId);
        if (!widget) return;

        let html = `
            <h4>Widget Properties</h4>
            <div class="form-group">
                <label>Widget ID</label>
                <input type="text" value="${widget.id}"
                       pattern="[a-z0-9_-]+"
                       onchange="updateWidgetProperty('${widgetId}', 'id', this.value)" />
            </div>
            <div class="form-group">
                <label>Title</label>
                <input type="text" value="${widget.title || ''}"
                       onchange="updateWidgetProperty('${widgetId}', 'title', this.value)" />
            </div>
        `;

        // Type-specific properties
        if (widget.type === 'field') {
            html += renderFieldProperties(widgetId, widget);
        } else if (widget.type === 'group') {
            html += renderGroupProperties(widgetId, widget);
        } else if (widget.type === 'table') {
            html += renderTableProperties(widgetId, widget);
        } else if (widget.type === 'grid') {
            html += renderGridProperties(widgetId, widget);
        } else if (widget.type === 'checklist') {
            html += renderChecklistProperties(widgetId, widget);
        }

        $('#properties-container').html(html);
    }

    function renderFieldProperties(widgetId, widget) {
        const field = widget.field;
        return `
            <div class="form-group">
                <label>Field Name</label>
                <input type="text" value="${field.name}"
                       pattern="[a-z0-9_-]+"
                       onchange="updateFieldProperty('${widgetId}', 'name', this.value)" />
            </div>
            <div class="form-group">
                <label>Label</label>
                <input type="text" value="${field.label}"
                       onchange="updateFieldProperty('${widgetId}', 'label', this.value)" />
            </div>
            <div class="form-group">
                <label>Type</label>
                <select onchange="updateFieldProperty('${widgetId}', 'type', this.value)">
                    ${renderTypeOptions(field.type)}
                </select>
            </div>
            <div class="form-group">
                <label>
                    <input type="checkbox" ${field.required ? 'checked' : ''}
                           onchange="updateFieldProperty('${widgetId}', 'required', this.checked)" />
                    Required
                </label>
            </div>
            <div class="form-group">
                <label>
                    <input type="checkbox" ${field.readonly ? 'checked' : ''}
                           onchange="updateFieldProperty('${widgetId}', 'readonly', this.checked)" />
                    Read Only
                </label>
            </div>
            <div class="form-group">
                <label>Placeholder</label>
                <input type="text" value="${field.placeholder || ''}"
                       onchange="updateFieldProperty('${widgetId}', 'placeholder', this.value)" />
            </div>
            <div class="form-group">
                <label>Unit</label>
                <input type="text" value="${field.unit || ''}"
                       onchange="updateFieldProperty('${widgetId}', 'unit', this.value)" />
            </div>
            ${field.type === 'enum' ? `
                <div class="form-group">
                    <label>Enum Values (comma-separated)</label>
                    <input type="text" value="${(field.enum || []).join(',')}"
                           onchange="updateFieldProperty('${widgetId}', 'enum', this.value.split(','))" />
                </div>
            ` : ''}
            <div class="form-group">
                <label>Formula</label>
                <input type="text" value="${field.compute || ''}"
                       placeholder="e.g., forced + scheduled"
                       onchange="updateFieldProperty('${widgetId}', 'compute', this.value)" />
            </div>
        `;
    }

    function renderTableProperties(widgetId, widget) {
        return `
            <div class="form-group">
                <label>Row Mode</label>
                <select onchange="updateTableProperty('${widgetId}', 'row_mode', this.value)">
                    <option value="infinite" ${widget.table.row_mode === 'infinite' ? 'selected' : ''}>Infinite</option>
                    <option value="finite" ${widget.table.row_mode === 'finite' ? 'selected' : ''}>Finite</option>
                </select>
            </div>
            <div class="form-group">
                <label>Columns</label>
                <button class="btn btn-primary" onclick="addTableColumn('${widgetId}')">+ Add Column</button>
                <ul class="column-list">
                    ${renderColumnList(widgetId, widget.table.columns)}
                </ul>
            </div>
        `;
    }

    function renderColumnList(widgetId, columns) {
        if (!columns || columns.length === 0) {
            return '<li style="color:#95a5a6;">No columns yet</li>';
        }

        return columns.map((col, idx) => `
            <li class="column-item">
                <span>${col.name} (${col.type})</span>
                <div>
                    <button class="btn btn-icon btn-info" onclick="editColumn('${widgetId}', ${idx})">✎</button>
                    <button class="btn btn-icon btn-danger" onclick="deleteColumn('${widgetId}', ${idx})">✕</button>
                </div>
            </li>
        `).join('');
    }

    function renderTypeOptions(selectedType) {
        const types = ['string', 'text', 'integer', 'decimal', 'date', 'time', 'datetime',
                       'bool', 'enum', 'attachment', 'signature'];
        return types.map(type =>
            `<option value="${type}" ${type === selectedType ? 'selected' : ''}>${type}</option>`
        ).join('');
    }

    // API Calls
    function saveForm() {
        const formData = DesignerState.formDefinition;

        // Update metadata from toolbar
        formData.form.id = $('#form-id').val();
        formData.form.title = $('#form-title').val();
        formData.form.version = $('#form-version').val();

        $.ajax({
            url: API.forms.save(),
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                alert('Form saved successfully!');
                DesignerState.isDirty = false;
            },
            error: function(xhr) {
                alert('Error saving form: ' + xhr.responseText);
            }
        });
    }

    function exportYaml() {
        const formId = $('#form-id').val();
        if (!formId) {
            alert('Please set a form ID first');
            return;
        }

        window.open(API.export.yaml(formId), '_blank');
    }

    function exportSql() {
        const formId = $('#form-id').val();
        if (!formId) {
            alert('Please set a form ID first');
            return;
        }

        window.open(API.export.sql(formId), '_blank');
    }

    // Utility Functions
    function generateId(prefix) {
        return `${prefix}-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    }

    function getCurrentPage() {
        return DesignerState.formDefinition.form.pages.find(
            p => p.id === DesignerState.currentPageId
        );
    }

    function findWidget(widgetId) {
        for (const page of DesignerState.formDefinition.form.pages) {
            for (const section of page.sections) {
                const widget = section.widgets?.find(w => w.id === widgetId);
                if (widget) return widget;
            }
        }
        return null;
    }

    function markDirty() {
        DesignerState.isDirty = true;
    }

    function getWidgetLabel(widget) {
        if (widget.type === 'field') return widget.field.label;
        if (widget.title) return widget.title;
        return widget.id;
    }

    // Event Handlers
    function initEventHandlers() {
        $('#btn-save').on('click', saveForm);
        $('#btn-export-yaml').on('click', exportYaml);
        $('#btn-export-sql').on('click', exportSql);

        $(document).on('click', '.btn-add-section', addSection);

        // Warn on unsaved changes
        window.addEventListener('beforeunload', function(e) {
            if (DesignerState.isDirty) {
                e.preventDefault();
                e.returnValue = '';
            }
        });
    }

    // Page Management
    function initPageManagement() {
        // Initialize first page if empty
        if (DesignerState.formDefinition.form.pages.length === 0) {
            DesignerState.formDefinition.form.pages.push({
                id: 'page-1',
                title: 'Page 1',
                sections: []
            });
        }
    }

    // Expose global functions
    window.DesignerAPI = {
        selectWidget,
        addSection,
        deleteSection: function(sectionId) { /* implementation */ },
        deleteWidget: function(widgetId) { /* implementation */ },
        updateWidgetProperty: function(widgetId, prop, value) { /* implementation */ },
        updateFieldProperty: function(widgetId, prop, value) { /* implementation */ },
        updateTableProperty: function(widgetId, prop, value) { /* implementation */ },
        addTableColumn: function(widgetId) { /* implementation */ },
        editColumn: function(widgetId, idx) { /* implementation */ },
        deleteColumn: function(widgetId, idx) { /* implementation */ }
    };

    // Initialize on document ready
    $(document).ready(initDesigner);

})(jQuery);
```

## Backend Specification (.NET Core)

### 1. Project Structure

```
FormDesigner.API/
├── Controllers/
│   ├── FormDefinitionController.cs
│   ├── ExportController.cs
│   └── ValidationController.cs
├── Models/
│   ├── FormDefinition.cs
│   ├── Page.cs
│   ├── Section.cs
│   ├── Widget.cs
│   └── DTOs/
├── Services/
│   ├── IFormBuilderService.cs
│   ├── FormBuilderService.cs
│   ├── IYamlExportService.cs
│   ├── YamlExportService.cs
│   ├── ISqlGeneratorService.cs
│   └── SqlGeneratorService.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Repositories/
├── Validators/
│   └── FormDefinitionValidator.cs
└── appsettings.json
```

### 2. Models

```csharp
// Models/FormDefinition.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FormDesigner.API.Models
{
    public class FormDefinitionRoot
    {
        public FormDefinition Form { get; set; }
    }

    public class FormDefinition
    {
        [Required]
        [RegularExpression(@"^[a-z0-9_-]+$")]
        public string Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Version { get; set; }

        public List<string> Locale { get; set; }
        public Dictionary<string, string> Labels { get; set; }
        public FormMetadata Meta { get; set; }
        public FormOptions Options { get; set; }
        public StorageOptions Storage { get; set; }

        [Required]
        public List<Page> Pages { get; set; } = new List<Page>();
    }

    public class FormMetadata
    {
        public string Organization { get; set; }
        public string DocumentNo { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string RevisionNo { get; set; }
        public string Reference { get; set; }
        public List<string> Tags { get; set; }
    }

    public class FormOptions
    {
        public PrintOptions Print { get; set; }
        public PermissionOptions Permissions { get; set; }
    }

    public class PrintOptions
    {
        public string PageSize { get; set; }
        public Margins MarginsMm { get; set; }
    }

    public class Margins
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
    }

    public class PermissionOptions
    {
        public List<string> Roles { get; set; }
        public string Visibility { get; set; }
    }

    public class StorageOptions
    {
        public string Mode { get; set; }
        public List<string> CopyHeader { get; set; }
        public List<List<string>> Indexes { get; set; }
    }

    public class Page
    {
        [Required]
        [RegularExpression(@"^[a-z0-9_-]+$")]
        public string Id { get; set; }

        [Required]
        public string Title { get; set; }

        public Dictionary<string, string> Labels { get; set; }

        [Required]
        public List<Section> Sections { get; set; } = new List<Section>();
    }

    public class Section
    {
        [Required]
        [RegularExpression(@"^[a-z0-9_-]+$")]
        public string Id { get; set; }

        [Required]
        public string Title { get; set; }

        public Dictionary<string, string> Labels { get; set; }

        [Required]
        public List<Widget> Widgets { get; set; } = new List<Widget>();
    }

    public class Widget
    {
        [Required]
        public string Type { get; set; } // field, group, table, grid, checklist

        [Required]
        [RegularExpression(@"^[a-z0-9_-]+$")]
        public string Id { get; set; }

        public string Title { get; set; }
        public Dictionary<string, string> Labels { get; set; }
        public string When { get; set; }
        public string Help { get; set; }

        // Type-specific properties
        public FieldSpec Field { get; set; }
        public List<FieldSpec> Fields { get; set; }
        public LayoutSpec Layout { get; set; }
        public TableSpec Table { get; set; }
        public GridSpec Grid { get; set; }
        public ChecklistSpec Checklist { get; set; }
    }

    public class FieldSpec
    {
        [Required]
        [RegularExpression(@"^[a-z0-9_-]+$")]
        public string Name { get; set; }

        [Required]
        public string Label { get; set; }

        [Required]
        public string Type { get; set; }

        public bool? Required { get; set; }
        public bool? Readonly { get; set; }
        public string Placeholder { get; set; }
        public object Default { get; set; }
        public string Unit { get; set; }
        public string Pattern { get; set; }
        public object Min { get; set; }
        public object Max { get; set; }
        public List<string> Enum { get; set; }
        public string Format { get; set; }
        public string Compute { get; set; }
        public bool? Override { get; set; }
    }

    public class LayoutSpec
    {
        public int? Columns { get; set; }
    }

    public class TableSpec
    {
        public string RowMode { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
        public List<string> RowKey { get; set; }
        public List<ColumnSpec> Columns { get; set; } = new List<ColumnSpec>();
        public List<RowGenerator> RowGenerators { get; set; }
        public List<AggregateSpec> Aggregates { get; set; }
    }

    public class ColumnSpec
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Label { get; set; }

        [Required]
        public string Type { get; set; }

        public bool? Required { get; set; }
        public bool? Readonly { get; set; }
        public string Unit { get; set; }
        public string Pattern { get; set; }
        public object Min { get; set; }
        public object Max { get; set; }
        public List<string> Enum { get; set; }
        public object Default { get; set; }
        public string Formula { get; set; }
        public string Format { get; set; }
    }

    public class RowGenerator
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public int? From { get; set; }
        public int? To { get; set; }
        public int? Step { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public int? StepMinutes { get; set; }
        public List<string> Values { get; set; }
    }

    public class AggregateSpec
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Label { get; set; }

        [Required]
        public string Expr { get; set; }

        public string Format { get; set; }
    }

    public class GridSpec
    {
        public GridRowSpec Rows { get; set; }
        public GridColumnSpec Columns { get; set; }
        public GridCellSpec Cell { get; set; }
    }

    public class GridRowSpec
    {
        [Required]
        public string Mode { get; set; }
        public GridRowGenerator Generator { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
    }

    public class GridRowGenerator
    {
        public string Type { get; set; }
        public List<string> Values { get; set; }
    }

    public class GridColumnSpec
    {
        public GridColumnGenerator Generator { get; set; }
    }

    public class GridColumnGenerator
    {
        public string Type { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public int? StepMinutes { get; set; }
        public List<object> Values { get; set; }
    }

    public class GridCellSpec
    {
        [Required]
        public string Type { get; set; }
        public List<string> Enum { get; set; }
        public object Default { get; set; }
        public bool? Required { get; set; }
        public string Help { get; set; }
    }

    public class ChecklistSpec
    {
        public List<ChecklistItem> Items { get; set; } = new List<ChecklistItem>();
    }

    public class ChecklistItem
    {
        [Required]
        public string Key { get; set; }

        [Required]
        public string Label { get; set; }

        [Required]
        public string Type { get; set; }

        public List<string> Enum { get; set; }
        public object Default { get; set; }
        public bool? Required { get; set; }
    }
}
```

### 3. Controllers

```csharp
// Controllers/FormDefinitionController.cs
using Microsoft.AspNetCore.Mvc;
using FormDesigner.API.Models;
using FormDesigner.API.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FormDesigner.API.Controllers
{
    [ApiController]
    [Route("api/forms")]
    public class FormDefinitionController : ControllerBase
    {
        private readonly IFormBuilderService _formBuilderService;

        public FormDefinitionController(IFormBuilderService formBuilderService)
        {
            _formBuilderService = formBuilderService;
        }

        // GET: api/forms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FormDefinition>>> GetAll()
        {
            var forms = await _formBuilderService.GetAllFormsAsync();
            return Ok(forms);
        }

        // GET: api/forms/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<FormDefinitionRoot>> GetById(string id)
        {
            var form = await _formBuilderService.GetFormByIdAsync(id);
            if (form == null)
                return NotFound();

            return Ok(form);
        }

        // POST: api/forms
        [HttpPost]
        public async Task<ActionResult<FormDefinitionRoot>> Create([FromBody] FormDefinitionRoot formRoot)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _formBuilderService.CreateFormAsync(formRoot);
            return CreatedAtAction(nameof(GetById), new { id = created.Form.Id }, created);
        }

        // PUT: api/forms/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<FormDefinitionRoot>> Update(string id, [FromBody] FormDefinitionRoot formRoot)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != formRoot.Form.Id)
                return BadRequest("ID mismatch");

            var updated = await _formBuilderService.UpdateFormAsync(formRoot);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        // DELETE: api/forms/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var success = await _formBuilderService.DeleteFormAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        // POST: api/forms/{id}/validate
        [HttpPost("{id}/validate")]
        public async Task<ActionResult<ValidationResult>> Validate(string id)
        {
            var result = await _formBuilderService.ValidateFormAsync(id);
            return Ok(result);
        }
    }
}
```

```csharp
// Controllers/ExportController.cs
using Microsoft.AspNetCore.Mvc;
using FormDesigner.API.Services;
using System.Threading.Tasks;

namespace FormDesigner.API.Controllers
{
    [ApiController]
    [Route("api/export")]
    public class ExportController : ControllerBase
    {
        private readonly IYamlExportService _yamlExportService;
        private readonly ISqlGeneratorService _sqlGeneratorService;

        public ExportController(
            IYamlExportService yamlExportService,
            ISqlGeneratorService sqlGeneratorService)
        {
            _yamlExportService = yamlExportService;
            _sqlGeneratorService = sqlGeneratorService;
        }

        // GET: api/export/{id}/yaml
        [HttpGet("{id}/yaml")]
        public async Task<ActionResult> ExportYaml(string id)
        {
            var yaml = await _yamlExportService.ExportToYamlAsync(id);
            if (yaml == null)
                return NotFound();

            return File(
                System.Text.Encoding.UTF8.GetBytes(yaml),
                "application/x-yaml",
                $"{id}.yaml"
            );
        }

        // GET: api/export/{id}/sql
        [HttpGet("{id}/sql")]
        public async Task<ActionResult> ExportSql(string id)
        {
            var sql = await _sqlGeneratorService.GenerateSqlAsync(id);
            if (sql == null)
                return NotFound();

            return File(
                System.Text.Encoding.UTF8.GetBytes(sql),
                "text/plain",
                $"{id}_schema.sql"
            );
        }
    }
}
```

### 4. Services

```csharp
// Services/IYamlExportService.cs
using System.Threading.Tasks;

namespace FormDesigner.API.Services
{
    public interface IYamlExportService
    {
        Task<string> ExportToYamlAsync(string formId);
    }
}
```

```csharp
// Services/YamlExportService.cs
using FormDesigner.API.Models;
using FormDesigner.API.Data.Repositories;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FormDesigner.API.Services
{
    public class YamlExportService : IYamlExportService
    {
        private readonly IFormRepository _formRepository;
        private readonly ISerializer _yamlSerializer;

        public YamlExportService(IFormRepository formRepository)
        {
            _formRepository = formRepository;

            _yamlSerializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();
        }

        public async Task<string> ExportToYamlAsync(string formId)
        {
            var formRoot = await _formRepository.GetByIdAsync(formId);
            if (formRoot == null)
                return null;

            return _yamlSerializer.Serialize(formRoot);
        }
    }
}
```

```csharp
// Services/ISqlGeneratorService.cs
using System.Threading.Tasks;

namespace FormDesigner.API.Services
{
    public interface ISqlGeneratorService
    {
        Task<string> GenerateSqlAsync(string formId);
    }
}
```

```csharp
// Services/SqlGeneratorService.cs
using FormDesigner.API.Models;
using FormDesigner.API.Data.Repositories;
using System.Threading.Tasks;
using System.Text;
using System.Linq;

namespace FormDesigner.API.Services
{
    public class SqlGeneratorService : ISqlGeneratorService
    {
        private readonly IFormRepository _formRepository;

        public SqlGeneratorService(IFormRepository formRepository)
        {
            _formRepository = formRepository;
        }

        public async Task<string> GenerateSqlAsync(string formId)
        {
            var formRoot = await _formRepository.GetByIdAsync(formId);
            if (formRoot == null)
                return null;

            var sql = new StringBuilder();

            // Generate CREATE TABLE statements for each table/grid widget
            foreach (var page in formRoot.Form.Pages)
            {
                foreach (var section in page.Sections)
                {
                    foreach (var widget in section.Widgets)
                    {
                        if (widget.Type == "table" && widget.Table != null)
                        {
                            sql.AppendLine(GenerateTableSql(formRoot.Form.Id, widget));
                            sql.AppendLine();
                        }
                        else if (widget.Type == "grid" && widget.Grid != null)
                        {
                            sql.AppendLine(GenerateGridSql(formRoot.Form.Id, widget));
                            sql.AppendLine();
                        }
                    }
                }
            }

            return sql.ToString();
        }

        private string GenerateTableSql(string formId, Widget widget)
        {
            var tableName = $"{formId}__{widget.Id}";
            var sql = new StringBuilder();

            sql.AppendLine($"-- Table for widget: {widget.Id}");
            sql.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (");

            // Base columns
            sql.AppendLine("    instance_id UUID NOT NULL REFERENCES form_instances(instance_id),");
            sql.AppendLine("    page_id TEXT NOT NULL,");
            sql.AppendLine("    section_id TEXT NOT NULL,");
            sql.AppendLine("    widget_id TEXT NOT NULL,");
            sql.AppendLine("    row_id BIGSERIAL PRIMARY KEY,");
            sql.AppendLine("    recorded_at TIMESTAMPTZ DEFAULT NOW(),");

            // Data columns
            foreach (var col in widget.Table.Columns)
            {
                var sqlType = MapDslTypeToSql(col.Type);
                var nullable = col.Required == true ? "NOT NULL" : "";

                if (!string.IsNullOrEmpty(col.Formula))
                {
                    // Generated column
                    sql.AppendLine($"    {col.Name} {sqlType} GENERATED ALWAYS AS ({TranslateFormula(col.Formula)}) STORED,");
                }
                else
                {
                    sql.AppendLine($"    {col.Name} {sqlType} {nullable},");
                }
            }

            // Remove trailing comma
            sql.Length -= 3;
            sql.AppendLine();
            sql.AppendLine(");");

            // Indexes
            sql.AppendLine($"CREATE INDEX IF NOT EXISTS ix_{tableName}_instance ON {tableName}(instance_id);");

            return sql.ToString();
        }

        private string GenerateGridSql(string formId, Widget widget)
        {
            // Similar to GenerateTableSql but for grid widgets
            var tableName = $"{formId}__{widget.Id}";
            var sql = new StringBuilder();

            sql.AppendLine($"-- Grid table for widget: {widget.Id}");
            sql.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (");
            sql.AppendLine("    instance_id UUID NOT NULL REFERENCES form_instances(instance_id),");
            sql.AppendLine("    row_id BIGSERIAL PRIMARY KEY,");
            sql.AppendLine("    row_key TEXT NOT NULL,");
            sql.AppendLine("    col_key TEXT NOT NULL,");
            sql.AppendLine($"    cell_value {MapDslTypeToSql(widget.Grid.Cell.Type)},");
            sql.AppendLine("    recorded_at TIMESTAMPTZ DEFAULT NOW()");
            sql.AppendLine(");");

            return sql.ToString();
        }

        private string MapDslTypeToSql(string dslType)
        {
            return dslType switch
            {
                "string" => "TEXT",
                "text" => "TEXT",
                "integer" => "INTEGER",
                "decimal" => "NUMERIC(18,6)",
                "date" => "DATE",
                "time" => "TIME",
                "datetime" => "TIMESTAMPTZ",
                "bool" => "BOOLEAN",
                "enum" => "TEXT",
                "attachment" => "TEXT",
                "signature" => "TEXT",
                _ => "TEXT"
            };
        }

        private string TranslateFormula(string formula)
        {
            // Simple translation - in production, use proper expression parser
            return formula
                .Replace(" + ", " + COALESCE(")
                .Replace(",", ", 0) + COALESCE(")
                + ", 0)";
        }
    }
}
```

### 5. Data Access

```csharp
// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using FormDesigner.API.Models;

namespace FormDesigner.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FormDefinitionEntity> FormDefinitions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FormDefinitionEntity>(entity =>
            {
                entity.HasKey(e => e.FormId);
                entity.Property(e => e.DslJson).HasColumnType("jsonb");
                entity.HasIndex(e => e.IsActive);
            });
        }
    }

    public class FormDefinitionEntity
    {
        public string FormId { get; set; }
        public string Version { get; set; }
        public string DslJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
```

### 6. Configuration

```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=formdesigner;Username=postgres;Password=yourpassword"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173", "https://yourdomain.com"]
  }
}
```

```csharp
// Program.cs
using FormDesigner.API.Data;
using FormDesigner.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IFormBuilderService, FormBuilderService>();
builder.Services.AddScoped<IYamlExportService, YamlExportService>();
builder.Services.AddScoped<ISqlGeneratorService, SqlGeneratorService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>())
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
```

## Workflow Examples

### 1. Creating a New Form

1. User clicks "New Form"
2. Enters form metadata (ID, title, version)
3. Clicks "+ Add Section"
4. Drags "Table" widget from palette into section
5. Selects table widget
6. In inspector panel:
   - Sets table ID: `substation-perf`
   - Clicks "+ Add Column"
   - Configures each column
7. Clicks "Save"
8. Frontend POSTs JSON to `/api/forms`
9. Backend validates, stores in DB
10. User clicks "Export YAML"
11. Backend serializes to YAML and downloads

### 2. Editing Existing Form

1. User clicks "Open"
2. Selects form from list
3. Frontend GETs `/api/forms/{id}`
4. Backend returns JSON
5. Frontend reconstructs canvas from JSON
6. User makes changes
7. Clicks "Save"
8. Frontend PUTs to `/api/forms/{id}`

## Testing Strategy

### Frontend
- Unit tests: jQuery plugin functions
- Integration tests: API calls
- E2E tests: Cypress for drag-drop workflows

### Backend
- Unit tests: Services (YamlExportService, SqlGeneratorService)
- Integration tests: Controllers with in-memory DB
- Validation tests: Model validation rules

## Security Considerations

1. **Input Validation**: All IDs must match `[a-z0-9_-]+` pattern
2. **SQL Injection**: Use parameterized queries for generated SQL
3. **XSS Prevention**: Sanitize HTML in inspector inputs
4. **CSRF Protection**: Add anti-forgery tokens
5. **Authentication**: Add JWT/OAuth for production
6. **Authorization**: Role-based access control

## Future Enhancements

1. **Real-time Collaboration**: SignalR for multi-user editing
2. **Version Control**: Git-like diff/merge for form definitions
3. **Template Library**: Pre-built form templates
4. **Expression Builder**: Visual formula editor
5. **Preview Mode**: Live form preview with sample data
6. **Import**: Upload existing YAML files
7. **Undo/Redo**: Command pattern for history
8. **Keyboard Shortcuts**: Power user features
9. **Mobile Support**: Touch-friendly drag-drop
10. **Localization**: Multi-language UI

## Dependencies

### Frontend
- jQuery 3.7.0
- jQuery UI 1.13.2
- Bootstrap 5 (optional, for styling)

### Backend
- .NET 8.0
- Entity Framework Core
- Npgsql (PostgreSQL provider)
- YamlDotNet (YAML serialization)
- Swashbuckle (Swagger/OpenAPI)

## Deployment

### Frontend
- Static files served from `/wwwroot`
- CDN for jQuery/jQuery UI in production

### Backend
- Docker container
- Reverse proxy (nginx)
- PostgreSQL database
- CI/CD pipeline (GitHub Actions)
