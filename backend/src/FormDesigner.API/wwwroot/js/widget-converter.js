/**
 * Widget Converter
 * Converts between API DTO format and Designer internal format
 */

const WidgetConverter = {
    /**
     * Convert from API DTO format to Designer internal format
     * API format: {type, id, field/table/grid/...}
     * Designer format: {type, id, label, required, spec}
     */
    fromDTO(widget) {
        const internal = {
            id: widget.id,
            type: widget.type,
            label: widget.title || this.getDefaultLabel(widget),
            required: false,
            spec: {}
        };

        // Convert type-specific properties
        switch (widget.type) {
            case 'field':
                if (widget.field) {
                    internal.label = widget.field.label || widget.field.name || widget.title || internal.label;
                    internal.required = widget.field.required || false;
                    internal.spec = {
                        type: widget.field.type || 'text',
                        placeholder: widget.field.placeholder || '',
                        default_value: widget.field.default_value || '',
                        formula: widget.field.formula || ''
                    };
                }
                break;

            case 'table':
                if (widget.table) {
                    internal.label = widget.title || 'Table Widget';
                    internal.spec = {
                        columns: widget.table.columns || [],
                        allow_add_rows: widget.table.allow_add_rows !== false,
                        allow_delete_rows: widget.table.allow_delete_rows !== false,
                        min_rows: widget.table.min || 1,
                        max_rows: widget.table.max || null,
                        multi_row_headers: widget.table.multi_row_headers || null,
                        merged_cells: widget.table.merged_cells || null,
                        row_mode: widget.table.row_mode || 'finite',
                        initial_rows: widget.table.initial_rows || null
                    };
                }
                break;

            case 'grid':
                if (widget.grid) {
                    internal.spec = {
                        columns_count: widget.grid.columns_count || 2,
                        widgets: widget.grid.widgets || []
                    };
                }
                break;

            case 'checklist':
                if (widget.checklist) {
                    internal.spec = {
                        items: widget.checklist.items || []
                    };
                }
                break;

            case 'group':
                // Support new group spec format with layout and cells
                if (widget.group) {
                    internal.spec = {
                        fields: widget.group.fields || [],
                        layout: widget.group.layout || null,
                        cells: widget.group.cells || null,
                        widgets: widget.group.fields || []  // Alias for backward compat
                    };
                } else {
                    // Backward compatibility: old format with widget.fields directly
                    internal.spec = {
                        widgets: widget.fields || [],
                        fields: widget.fields || []
                    };
                }
                break;

            case 'formheader':
                if (widget.form_header || widget.formheader) {
                    internal.label = widget.title || 'Form Header';
                    internal.spec = widget.form_header || widget.formheader || {};
                }
                break;

            case 'signature':
                if (widget.signature) {
                    internal.label = widget.title || widget.signature.role || 'Signature';
                    internal.spec = widget.signature;
                }
                break;

            case 'notes':
                if (widget.notes) {
                    internal.label = widget.title || widget.notes.title || 'Notes';
                    internal.spec = widget.notes;
                }
                break;

            case 'hierarchicalchecklist':
                if (widget.hierarchical_checklist || widget.hierarchicalchecklist) {
                    internal.label = widget.title || 'Hierarchical Checklist';
                    internal.spec = widget.hierarchical_checklist || widget.hierarchicalchecklist || {};
                }
                break;

            case 'radiogroup':
                if (widget.radio_group || widget.radiogroup) {
                    internal.label = widget.title || 'Radio Group';
                    internal.spec = widget.radio_group || widget.radiogroup || {};
                }
                break;

            case 'checkboxgroup':
                if (widget.checkbox_group || widget.checkboxgroup) {
                    internal.label = widget.title || 'Checkbox Group';
                    internal.spec = widget.checkbox_group || widget.checkboxgroup || {};
                }
                break;
        }

        return internal;
    },

    /**
     * Convert from Designer internal format to API DTO format
     * Designer format: {type, id, label, required, spec}
     * API format: {type, id, field/table/grid/...}
     */
    toDTO(widget) {
        const dto = {
            type: widget.type,
            id: widget.id,
            title: widget.label
        };

        // Convert type-specific properties
        switch (widget.type) {
            case 'field':
                dto.field = {
                    name: widget.id,
                    label: widget.label,
                    type: widget.spec.type || 'text',
                    required: widget.required || false,
                    placeholder: widget.spec.placeholder || '',
                    default_value: widget.spec.default_value || '',
                    formula: widget.spec.formula || null
                };
                break;

            case 'table':
                dto.table = {
                    columns: widget.spec.columns || [],
                    row_mode: 'finite',
                    min: widget.spec.min_rows || 1,
                    max: widget.spec.max_rows || null,
                    allow_add_rows: widget.spec.allow_add_rows !== false,
                    allow_delete_rows: widget.spec.allow_delete_rows !== false,
                    multi_row_headers: widget.spec.multi_row_headers || null,
                    merged_cells: widget.spec.merged_cells || null,
                    initial_rows: widget.spec.initial_rows || null
                };
                break;

            case 'grid':
                dto.grid = {
                    columns_count: widget.spec.columns_count || 2,
                    widgets: widget.spec.widgets || []
                };
                break;

            case 'checklist':
                dto.checklist = {
                    items: widget.spec.items || []
                };
                break;

            case 'group':
                // Support new group spec format
                if (widget.spec.layout || widget.spec.cells) {
                    dto.group = {
                        fields: widget.spec.fields || widget.spec.widgets || [],
                        layout: widget.spec.layout || null,
                        cells: widget.spec.cells || null
                    };
                } else {
                    // Backward compatibility: old format
                    dto.fields = widget.spec.widgets || widget.spec.fields || [];
                    dto.layout = widget.spec.layout || null;
                }
                break;

            case 'formheader':
                dto.formheader = widget.spec;
                break;

            case 'signature':
                dto.signature = widget.spec;
                break;

            case 'notes':
                dto.notes = widget.spec;
                break;

            case 'hierarchicalchecklist':
                dto.hierarchicalchecklist = widget.spec;
                break;

            case 'radiogroup':
                dto.radiogroup = widget.spec;
                break;

            case 'checkboxgroup':
                dto.checkboxgroup = widget.spec;
                break;
        }

        return dto;
    },

    /**
     * Get default label for a widget based on type
     */
    getDefaultLabel(widget) {
        const type = widget.type.charAt(0).toUpperCase() + widget.type.slice(1);
        return `${type} Widget`;
    },

    /**
     * Convert entire form from API to Designer format
     */
    formFromDTO(apiForm) {
        const form = {
            id: apiForm.id,
            title: apiForm.title,
            version: apiForm.version,
            pages: []
        };

        apiForm.pages.forEach(apiPage => {
            const page = {
                id: apiPage.id,
                title: apiPage.title,
                sections: []
            };

            apiPage.sections.forEach(apiSection => {
                const section = {
                    id: apiSection.id,
                    title: apiSection.title,
                    widgets: apiSection.widgets.map(w => this.fromDTO(w))
                };

                page.sections.push(section);
            });

            form.pages.push(page);
        });

        return form;
    },

    /**
     * Convert entire form from Designer to API format
     */
    formToDTO(designerForm) {
        const form = {
            id: designerForm.id,
            title: designerForm.title,
            version: designerForm.version,
            pages: []
        };

        designerForm.pages.forEach(designerPage => {
            const page = {
                id: designerPage.id,
                title: designerPage.title,
                sections: []
            };

            designerPage.sections.forEach(designerSection => {
                const section = {
                    id: designerSection.id,
                    title: designerSection.title,
                    widgets: designerSection.widgets.map(w => this.toDTO(w))
                };

                page.sections.push(section);
            });

            form.pages.push(page);
        });

        return { form };
    }
};

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = WidgetConverter;
}
