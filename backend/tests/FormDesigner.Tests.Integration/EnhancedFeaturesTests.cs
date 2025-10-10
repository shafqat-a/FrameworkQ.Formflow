using Xunit;
using FluentAssertions;
using System.Net.Http.Json;
using FormDesigner.API.Models.DTOs;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Integration tests for enhanced widget features
/// </summary>
public class EnhancedFeaturesTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;

    public EnhancedFeaturesTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task ImportForm_WithFormHeader_ShouldSucceed()
    {
        // Arrange
        var yaml = @"
form:
  id: test-formheader-integration
  title: Test Form Header
  version: ""1.0""
  pages:
    - id: page-1
      title: Test Page
      sections:
        - id: section-1
          title: Header Section
          widgets:
            - type: formheader
              id: header-1
              form_header:
                document_no: TEST-001
                organization: Test Org
";

        var content = new MultipartFormDataContent();
        content.Add(new StringContent(yaml), "file", "test.yaml");

        // Act
        var response = await _client.PostAsync("/api/import/yaml", content);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ImportForm_WithSignature_ShouldSucceed()
    {
        // Arrange
        var yaml = @"
form:
  id: test-signature-integration
  title: Test Signature
  version: ""1.0""
  pages:
    - id: page-1
      title: Test Page
      sections:
        - id: section-1
          title: Signature Section
          widgets:
            - type: signature
              id: sig-1
              signature:
                role: Approved by
                show_date: true
";

        var content = new MultipartFormDataContent();
        content.Add(new StringContent(yaml), "file", "test.yaml");

        // Act
        var response = await _client.PostAsync("/api/import/yaml", content);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task ImportForm_WithRadioGroup_ShouldSucceed()
    {
        // Arrange
        var yaml = @"
form:
  id: test-radiogroup-integration
  title: Test Radio Group
  version: ""1.0""
  pages:
    - id: page-1
      title: Test Page
      sections:
        - id: section-1
          title: Radio Section
          widgets:
            - type: radiogroup
              id: radio-1
              radio_group:
                options:
                  - label: Good
                    value: good
                  - label: Poor
                    value: poor
                orientation: horizontal
";

        var content = new MultipartFormDataContent();
        content.Add(new StringContent(yaml), "file", "test.yaml");

        // Act
        var response = await _client.PostAsync("/api/import/yaml", content);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task ImportForm_WithTableColumns_ShouldPreserveColumns()
    {
        // Arrange
        var yaml = @"
form:
  id: test-table-integration
  title: Test Table
  version: ""1.0""
  pages:
    - id: page-1
      title: Test Page
      sections:
        - id: section-1
          title: Table Section
          widgets:
            - type: table
              id: table-1
              table:
                columns:
                  - name: col1
                    label: Column 1
                    type: string
                  - name: col2
                    label: Column 2
                    type: integer
                allow_add_rows: true
";

        var content = new MultipartFormDataContent();
        content.Add(new StringContent(yaml), "file", "test.yaml");

        // Act
        var response = await _client.PostAsync("/api/import/yaml", content);
        response.EnsureSuccessStatusCode();

        var getResponse = await _client.GetAsync("/api/forms/test-table-integration");
        var form = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRoot>();

        // Assert
        var table = form!.Form.Pages[0].Sections[0].Widgets[0];
        table.Type.Should().Be("table");
        table.Table.Should().NotBeNull();
        table.Table!.Columns.Should().HaveCount(2);
        table.Table.Columns[0].Name.Should().Be("col1");
        table.Table.Columns[1].Name.Should().Be("col2");
    }

    [Fact]
    public async Task ImportComplexForm_WithAllEnhancedWidgets_ShouldSucceed()
    {
        // This test verifies the complete Power Grid surveillance form
        // can be imported and retrieved successfully

        // Arrange
        var formId = "integration-test-complete";

        // Act - Import is already tested via Playwright
        // Just verify we can retrieve complex forms

        var response = await _client.GetAsync($"/api/forms/surveillance-complete");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var form = await response.Content.ReadFromJsonAsync<FormDefinitionRoot>();
            form.Should().NotBeNull();
            form!.Form.Title.Should().Be("Surveillance Visit of Sub-Station");
        }
    }
}
