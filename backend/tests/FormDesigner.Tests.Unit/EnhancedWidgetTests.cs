using Xunit;
using FluentAssertions;
using FormDesigner.API.Models.DTOs.Widgets;
using FormDesigner.API.Validators;

namespace FormDesigner.Tests.Unit;

/// <summary>
/// Unit tests for enhanced widget DTOs and validators
/// </summary>
public class EnhancedWidgetTests
{
    [Fact]
    public void FormHeaderSpec_ShouldSetDefaultShowOnAllPages()
    {
        // Arrange & Act
        var formHeader = new FormHeaderSpec();

        // Assert
        formHeader.ShowOnAllPages.Should().BeTrue();
    }

    [Fact]
    public void FormHeaderSpec_ShouldAcceptAllProperties()
    {
        // Arrange & Act
        var formHeader = new FormHeaderSpec
        {
            DocumentNo = "QF-GMD-17",
            RevisionNo = "01",
            EffectiveDate = "03/03/15",
            PageNumber = "1 of 1",
            Organization = "POWER GRID COMPANY",
            FormTitle = "SURVEILLANCE VISIT",
            Category = "QUALITY FORMS"
        };

        // Assert
        formHeader.DocumentNo.Should().Be("QF-GMD-17");
        formHeader.Organization.Should().Be("POWER GRID COMPANY");
    }

    [Fact]
    public void SignatureSpec_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var signature = new SignatureSpec();

        // Assert
        signature.NameLabel.Should().Be("Name");
        signature.DesignationLabel.Should().Be("Designation");
        signature.DateLabel.Should().Be("Date");
        signature.ShowDesignation.Should().BeTrue();
        signature.ShowDate.Should().BeTrue();
        signature.SignatureType.Should().Be("draw");
        signature.SignatureWidth.Should().Be(400);
        signature.SignatureHeight.Should().Be(100);
    }

    [Fact]
    public void NotesSpec_ShouldRequireContent()
    {
        // Arrange
        var notes = new NotesSpec { Content = "Test note" };
        var validator = new NotesSpecValidator();

        // Act
        var result = validator.Validate(notes);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void NotesSpec_ShouldFailWithoutContent()
    {
        // Arrange
        var notes = new NotesSpec { Content = "" };
        var validator = new NotesSpecValidator();

        // Act
        var result = validator.Validate(notes);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("content is required"));
    }

    [Fact]
    public void RadioGroupSpec_ShouldRequireOptions()
    {
        // Arrange
        var radioGroup = new RadioGroupSpec();
        var validator = new RadioGroupSpecValidator();

        // Act
        var result = validator.Validate(radioGroup);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("at least one option"));
    }

    [Fact]
    public void RadioGroupSpec_WithOptions_ShouldBeValid()
    {
        // Arrange
        var radioGroup = new RadioGroupSpec
        {
            Options = new List<RadioOption>
            {
                new() { Label = "Good", Value = "good" },
                new() { Label = "Poor", Value = "poor" }
            },
            Orientation = "horizontal"
        };
        var validator = new RadioGroupSpecValidator();

        // Act
        var result = validator.Validate(radioGroup);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CheckboxGroupSpec_ShouldValidateMinMaxSelections()
    {
        // Arrange
        var checkboxGroup = new CheckboxGroupSpec
        {
            Options = new List<CheckboxOption>
            {
                new() { Label = "Option 1", Value = "opt1" }
            },
            MinSelections = 2,
            MaxSelections = 1 // Invalid: max < min
        };
        var validator = new CheckboxGroupSpecValidator();

        // Act
        var result = validator.Validate(checkboxGroup);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("greater than minimum"));
    }

    [Fact]
    public void TimePickerSpec_ShouldValidateTimeFormat()
    {
        // Arrange
        var timePicker = new TimePickerSpec
        {
            MinTime = "25:00" // Invalid time
        };
        var validator = new TimePickerSpecValidator();

        // Act
        var result = validator.Validate(timePicker);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void TimePickerSpec_WithValidTime_ShouldPass()
    {
        // Arrange
        var timePicker = new TimePickerSpec
        {
            Format = "24h",
            MinTime = "09:00",
            MaxTime = "17:00",
            StepMinutes = 15
        };
        var validator = new TimePickerSpecValidator();

        // Act
        var result = validator.Validate(timePicker);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void HierarchicalChecklistSpec_ShouldRequireItems()
    {
        // Arrange
        var checklist = new HierarchicalChecklistSpec();
        var validator = new HierarchicalChecklistSpecValidator();

        // Act
        var result = validator.Validate(checklist);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("at least one item"));
    }

    [Fact]
    public void HierarchicalChecklistItem_ShouldValidateKey()
    {
        // Arrange
        var item = new HierarchicalChecklistItem
        {
            Key = "Invalid Key!", // Spaces and special chars not allowed
            Label = "Test Item",
            Type = "checkbox"
        };
        var validator = new HierarchicalChecklistItemValidator();

        // Act
        var result = validator.Validate(item);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("lowercase letters, numbers, hyphens"));
    }

    [Fact]
    public void HierarchicalChecklistItem_WithValidKey_ShouldPass()
    {
        // Arrange
        var item = new HierarchicalChecklistItem
        {
            Key = "valid-key_123",
            Label = "Test Item",
            Type = "checkbox"
        };
        var validator = new HierarchicalChecklistItemValidator();

        // Act
        var result = validator.Validate(item);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
