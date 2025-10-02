using FluentValidation;
using FormDesigner.API.Models.DTOs.Widgets;

namespace FormDesigner.API.Validators;

/// <summary>
/// Validator for FormHeader widget specification
/// </summary>
public class FormHeaderSpecValidator : AbstractValidator<FormHeaderSpec>
{
    public FormHeaderSpecValidator()
    {
        // Optional fields - no strict validation needed
        RuleFor(x => x.DocumentNo)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.DocumentNo));

        RuleFor(x => x.RevisionNo)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.RevisionNo));

        RuleFor(x => x.Organization)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Organization));
    }
}

/// <summary>
/// Validator for Signature widget specification
/// </summary>
public class SignatureSpecValidator : AbstractValidator<SignatureSpec>
{
    public SignatureSpecValidator()
    {
        RuleFor(x => x.Role)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Role));

        RuleFor(x => x.SignatureWidth)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000)
            .WithMessage("Signature width must be between 1 and 1000 pixels");

        RuleFor(x => x.SignatureHeight)
            .GreaterThan(0)
            .LessThanOrEqualTo(500)
            .WithMessage("Signature height must be between 1 and 500 pixels");

        RuleFor(x => x.SignatureType)
            .Must(type => type == "draw" || type == "upload" || type == "both")
            .WithMessage("Signature type must be 'draw', 'upload', or 'both'");
    }
}

/// <summary>
/// Validator for Notes widget specification
/// </summary>
public class NotesSpecValidator : AbstractValidator<NotesSpec>
{
    public NotesSpecValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Notes content is required");

        RuleFor(x => x.Style)
            .Must(style => style == "info" || style == "warning" || style == "note" || style == "instruction")
            .WithMessage("Notes style must be 'info', 'warning', 'note', or 'instruction'");

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Title));
    }
}

/// <summary>
/// Validator for HierarchicalChecklist widget specification
/// </summary>
public class HierarchicalChecklistSpecValidator : AbstractValidator<HierarchicalChecklistSpec>
{
    public HierarchicalChecklistSpecValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Hierarchical checklist must have at least one item");

        RuleFor(x => x.NumberingStyle)
            .Must(style => style == "decimal" || style == "alpha" || style == "roman" || style == "none")
            .WithMessage("Numbering style must be 'decimal', 'alpha', 'roman', or 'none'");

        RuleFor(x => x.IndentSize)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Indent size must be between 0 and 100 pixels");

        RuleForEach(x => x.Items)
            .SetValidator(new HierarchicalChecklistItemValidator());
    }
}

/// <summary>
/// Validator for HierarchicalChecklistItem
/// </summary>
public class HierarchicalChecklistItemValidator : AbstractValidator<HierarchicalChecklistItem>
{
    public HierarchicalChecklistItemValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .Matches(@"^[a-z0-9_-]+$")
            .WithMessage("Item key must contain only lowercase letters, numbers, hyphens, and underscores");

        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Item label is required");

        RuleFor(x => x.Type)
            .Must(type => type == "checkbox" || type == "text" || type == "select" || type == "radio")
            .WithMessage("Item type must be 'checkbox', 'text', 'select', or 'radio'");

        // Validate children recursively
        RuleForEach(x => x.Children)
            .SetValidator(new HierarchicalChecklistItemValidator())
            .When(x => x.Children != null && x.Children.Count > 0);
    }
}

/// <summary>
/// Validator for RadioGroup widget specification
/// </summary>
public class RadioGroupSpecValidator : AbstractValidator<RadioGroupSpec>
{
    public RadioGroupSpecValidator()
    {
        RuleFor(x => x.Options)
            .NotEmpty()
            .WithMessage("Radio group must have at least one option");

        RuleFor(x => x.Orientation)
            .Must(orientation => orientation == "horizontal" || orientation == "vertical")
            .WithMessage("Orientation must be 'horizontal' or 'vertical'");

        RuleForEach(x => x.Options)
            .SetValidator(new RadioOptionValidator());
    }
}

/// <summary>
/// Validator for RadioOption
/// </summary>
public class RadioOptionValidator : AbstractValidator<RadioOption>
{
    public RadioOptionValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty()
            .WithMessage("Option value is required");

        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Option label is required");
    }
}

/// <summary>
/// Validator for CheckboxGroup widget specification
/// </summary>
public class CheckboxGroupSpecValidator : AbstractValidator<CheckboxGroupSpec>
{
    public CheckboxGroupSpecValidator()
    {
        RuleFor(x => x.Options)
            .NotEmpty()
            .WithMessage("Checkbox group must have at least one option");

        RuleFor(x => x.Orientation)
            .Must(orientation => orientation == "horizontal" || orientation == "vertical" || orientation == "grid")
            .WithMessage("Orientation must be 'horizontal', 'vertical', or 'grid'");

        RuleFor(x => x.MinSelections)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinSelections.HasValue)
            .WithMessage("Minimum selections must be 0 or greater");

        RuleFor(x => x.MaxSelections)
            .GreaterThan(x => x.MinSelections ?? 0)
            .When(x => x.MaxSelections.HasValue && x.MinSelections.HasValue)
            .WithMessage("Maximum selections must be greater than minimum selections");

        RuleFor(x => x.GridColumns)
            .GreaterThan(0)
            .LessThanOrEqualTo(6)
            .WithMessage("Grid columns must be between 1 and 6");

        RuleForEach(x => x.Options)
            .SetValidator(new CheckboxOptionValidator());
    }
}

/// <summary>
/// Validator for CheckboxOption
/// </summary>
public class CheckboxOptionValidator : AbstractValidator<CheckboxOption>
{
    public CheckboxOptionValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty()
            .WithMessage("Option value is required");

        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Option label is required");
    }
}

/// <summary>
/// Validator for TimePicker widget specification
/// </summary>
public class TimePickerSpecValidator : AbstractValidator<TimePickerSpec>
{
    public TimePickerSpecValidator()
    {
        RuleFor(x => x.Format)
            .Must(format => format == "12h" || format == "24h")
            .WithMessage("Time format must be '12h' or '24h'");

        RuleFor(x => x.StepMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(60)
            .WithMessage("Step minutes must be between 1 and 60");

        RuleFor(x => x.MinTime)
            .Matches(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$")
            .When(x => !string.IsNullOrEmpty(x.MinTime))
            .WithMessage("Min time must be in HH:mm format");

        RuleFor(x => x.MaxTime)
            .Matches(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$")
            .When(x => !string.IsNullOrEmpty(x.MaxTime))
            .WithMessage("Max time must be in HH:mm format");

        RuleFor(x => x.DefaultValue)
            .Matches(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9](:[0-5][0-9])?$")
            .When(x => !string.IsNullOrEmpty(x.DefaultValue))
            .WithMessage("Default time must be in HH:mm or HH:mm:ss format");
    }
}
