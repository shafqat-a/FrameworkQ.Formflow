using FluentValidation;
using FormDesigner.API.Models.DTOs;

namespace FormDesigner.API.Validators;

/// <summary>
/// FluentValidation validator for FormDefinition.
/// Validates form structure, IDs, and ensures compliance with DSL v0.1.
/// </summary>
public class FormDefinitionValidator : AbstractValidator<FormDefinitionRoot>
{
    private static readonly string IdPattern = @"^[a-z0-9_-]+$";

    public FormDefinitionValidator()
    {
        RuleFor(x => x.Form)
            .NotNull()
            .WithMessage("Form is required");

        RuleFor(x => x.Form.Id)
            .NotEmpty()
            .WithMessage("Form ID is required")
            .Matches(IdPattern)
            .WithMessage("Form ID must contain only lowercase letters, numbers, hyphens, and underscores");

        RuleFor(x => x.Form.Title)
            .NotEmpty()
            .WithMessage("Form title is required");

        RuleFor(x => x.Form.Version)
            .NotEmpty()
            .WithMessage("Form version is required");

        RuleFor(x => x.Form.Pages)
            .NotNull()
            .WithMessage("Form must have pages array")
            .NotEmpty()
            .WithMessage("Form must have at least one page");

        RuleForEach(x => x.Form.Pages)
            .SetValidator(new PageValidator());

        // Validate unique widget IDs across entire form
        RuleFor(x => x.Form)
            .Must(HaveUniqueWidgetIds)
            .WithMessage("All widget IDs must be unique across the form");
    }

    private bool HaveUniqueWidgetIds(FormDefinition form)
    {
        var widgetIds = new HashSet<string>();

        foreach (var page in form.Pages ?? new List<Page>())
        {
            foreach (var section in page.Sections ?? new List<Section>())
            {
                foreach (var widget in section.Widgets ?? new List<Widget>())
                {
                    if (!string.IsNullOrEmpty(widget.Id))
                    {
                        if (!widgetIds.Add(widget.Id))
                        {
                            return false; // Duplicate found
                        }
                    }
                }
            }
        }

        return true;
    }
}

/// <summary>
/// Validator for Page.
/// </summary>
public class PageValidator : AbstractValidator<Page>
{
    private static readonly string IdPattern = @"^[a-z0-9_-]+$";

    public PageValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Page ID is required")
            .Matches(IdPattern)
            .WithMessage("Page ID must contain only lowercase letters, numbers, hyphens, and underscores");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Page title is required");

        RuleFor(x => x.Sections)
            .NotNull()
            .WithMessage("Page must have sections array");

        RuleForEach(x => x.Sections)
            .SetValidator(new SectionValidator());
    }
}

/// <summary>
/// Validator for Section.
/// </summary>
public class SectionValidator : AbstractValidator<Section>
{
    private static readonly string IdPattern = @"^[a-z0-9_-]+$";

    public SectionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Section ID is required")
            .Matches(IdPattern)
            .WithMessage("Section ID must contain only lowercase letters, numbers, hyphens, and underscores");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Section title is required");

        RuleFor(x => x.Widgets)
            .NotNull()
            .WithMessage("Section must have widgets array");

        RuleForEach(x => x.Widgets)
            .SetValidator(new WidgetValidator());
    }
}

/// <summary>
/// Validator for Widget.
/// </summary>
public class WidgetValidator : AbstractValidator<Widget>
{
    private static readonly string IdPattern = @"^[a-z0-9_-]+$";
    private static readonly HashSet<string> ValidWidgetTypes = new()
    {
        "field", "group", "table", "grid", "checklist"
    };

    public WidgetValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Widget type is required")
            .Must(type => ValidWidgetTypes.Contains(type ?? ""))
            .WithMessage("Widget type must be one of: field, group, table, grid, checklist");

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Widget ID is required")
            .Matches(IdPattern)
            .WithMessage("Widget ID must contain only lowercase letters, numbers, hyphens, and underscores");

        // Type-specific validation
        When(x => x.Type == "field", () =>
        {
            RuleFor(x => x.Field)
                .NotNull()
                .WithMessage("Field widget must have 'field' property");
        });

        When(x => x.Type == "table", () =>
        {
            RuleFor(x => x.Table)
                .NotNull()
                .WithMessage("Table widget must have 'table' property");

            RuleFor(x => x.Table!.Columns)
                .NotNull()
                .NotEmpty()
                .WithMessage("Table must have at least one column")
                .When(x => x.Table != null);
        });

        When(x => x.Type == "grid", () =>
        {
            RuleFor(x => x.Grid)
                .NotNull()
                .WithMessage("Grid widget must have 'grid' property");
        });

        When(x => x.Type == "checklist", () =>
        {
            RuleFor(x => x.Checklist)
                .NotNull()
                .WithMessage("Checklist widget must have 'checklist' property");

            RuleFor(x => x.Checklist!.Items)
                .NotNull()
                .NotEmpty()
                .WithMessage("Checklist must have at least one item")
                .When(x => x.Checklist != null);
        });

        When(x => x.Type == "group", () =>
        {
            RuleFor(x => x.Fields)
                .NotNull()
                .NotEmpty()
                .WithMessage("Group widget must have at least one field")
                .When(x => x.Fields != null);
        });
    }
}
