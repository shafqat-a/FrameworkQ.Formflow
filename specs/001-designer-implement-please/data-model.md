# Data Model: Visual Form Designer

## Entity Relationship Overview

```
FormDefinitionEntity (PostgreSQL)
  ├── form_id (PK)
  ├── version
  ├── dsl_json (JSONB) ────→ Contains full FormDefinitionRoot structure
  ├── created_at
  ├── updated_at
  └── is_active

FormDefinitionRoot (C# Model / JSON Structure)
  └── Form
      ├── id
      ├── title
      ├── version
      ├── locale[]
      ├── labels{}
      ├── meta
      ├── options
      ├── storage
      └── pages[]
          ├── id
          ├── title
          ├── labels{}
          └── sections[]
              ├── id
              ├── title
              ├── labels{}
              └── widgets[]
                  ├── type
                  ├── id
                  ├── title
                  ├── labels{}
                  ├── when
                  ├── help
                  ├── field (if type=field)
                  ├── fields[] (if type=group)
                  ├── layout (if type=group)
                  ├── table (if type=table)
                  ├── grid (if type=grid)
                  └── checklist (if type=checklist)
```

## Database Schema

### Table: form_definitions

Stores persisted form definitions with metadata.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| form_id | TEXT | PRIMARY KEY | Form identifier (slug) |
| version | TEXT | NOT NULL | Semantic version |
| dsl_json | JSONB | NOT NULL | Complete form definition |
| created_at | TIMESTAMPTZ | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updated_at | TIMESTAMPTZ | NULL | Last modification timestamp |
| is_active | BOOLEAN | NOT NULL, DEFAULT TRUE | Soft delete flag |

**Indexes**:
```sql
CREATE INDEX idx_form_definitions_active ON form_definitions(is_active) WHERE is_active = TRUE;
CREATE INDEX idx_form_definitions_created ON form_definitions(created_at DESC);
CREATE INDEX idx_form_definitions_json ON form_definitions USING GIN(dsl_json);
```

**Rationale**:
- JSONB storage: Flexible schema for complex nested structures, supports indexing
- GIN index: Fast queries on JSON fields (e.g., search by form title)
- Soft delete: Preserve history, allow undelete operations

## C# Domain Models

### Core Entities

#### FormDefinitionEntity (EF Core Entity)

```csharp
public class FormDefinitionEntity
{
    [Key]
    [Required]
    [RegularExpression(@"^[a-z0-9_-]+$")]
    public string FormId { get; set; }

    [Required]
    public string Version { get; set; }

    [Required]
    [Column(TypeName = "jsonb")]
    public string DslJson { get; set; }  // Serialized FormDefinitionRoot

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;
}
```

#### FormDefinitionRoot (DSL Model)

```csharp
public class FormDefinitionRoot
{
    [Required]
    [JsonPropertyName("form")]
    public FormDefinition Form { get; set; }
}

public class FormDefinition
{
    [Required]
    [RegularExpression(@"^[a-z0-9_-]+$")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [Required]
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [Required]
    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("locale")]
    public List<string> Locale { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; }

    [JsonPropertyName("meta")]
    public FormMetadata Meta { get; set; }

    [JsonPropertyName("options")]
    public FormOptions Options { get; set; }

    [JsonPropertyName("storage")]
    public StorageOptions Storage { get; set; }

    [Required]
    [JsonPropertyName("pages")]
    public List<Page> Pages { get; set; } = new List<Page>();
}
```

### Supporting Entities

#### Page

```csharp
public class Page
{
    [Required]
    [RegularExpression(@"^[a-z0-9_-]+$")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [Required]
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; }

    [Required]
    [JsonPropertyName("sections")]
    public List<Section> Sections { get; set; } = new List<Section>();
}
```

#### Section

```csharp
public class Section
{
    [Required]
    [RegularExpression(@"^[a-z0-9_-]+$")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [Required]
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; }

    [Required]
    [JsonPropertyName("widgets")]
    public List<Widget> Widgets { get; set; } = new List<Widget>();
}
```

#### Widget (Polymorphic)

```csharp
public class Widget
{
    [Required]
    [JsonPropertyName("type")]
    public string Type { get; set; }  // field, group, table, grid, checklist

    [Required]
    [RegularExpression(@"^[a-z0-9_-]+$")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; }

    [JsonPropertyName("when")]
    public string When { get; set; }  // Conditional visibility expression

    [JsonPropertyName("help")]
    public string Help { get; set; }

    // Type-specific properties (populated based on Type)
    [JsonPropertyName("field")]
    public FieldSpec Field { get; set; }

    [JsonPropertyName("fields")]
    public List<FieldSpec> Fields { get; set; }

    [JsonPropertyName("layout")]
    public LayoutSpec Layout { get; set; }

    [JsonPropertyName("table")]
    public TableSpec Table { get; set; }

    [JsonPropertyName("grid")]
    public GridSpec Grid { get; set; }

    [JsonPropertyName("checklist")]
    public ChecklistSpec Checklist { get; set; }
}
```

#### FieldSpec

```csharp
public class FieldSpec
{
    [Required]
    [RegularExpression(@"^[a-z0-9_-]+$")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [Required]
    [JsonPropertyName("label")]
    public string Label { get; set; }

    [Required]
    [JsonPropertyName("type")]
    public string Type { get; set; }  // string, integer, date, etc.

    [JsonPropertyName("required")]
    public bool? Required { get; set; }

    [JsonPropertyName("readonly")]
    public bool? Readonly { get; set; }

    [JsonPropertyName("placeholder")]
    public string Placeholder { get; set; }

    [JsonPropertyName("default")]
    public object Default { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; }

    [JsonPropertyName("pattern")]
    public string Pattern { get; set; }

    [JsonPropertyName("min")]
    public object Min { get; set; }

    [JsonPropertyName("max")]
    public object Max { get; set; }

    [JsonPropertyName("enum")]
    public List<string> Enum { get; set; }

    [JsonPropertyName("format")]
    public string Format { get; set; }

    [JsonPropertyName("compute")]
    public string Compute { get; set; }  // Formula expression

    [JsonPropertyName("override")]
    public bool? Override { get; set; }
}
```

#### TableSpec

```csharp
public class TableSpec
{
    [JsonPropertyName("row_mode")]
    public string RowMode { get; set; }  // infinite, finite

    [JsonPropertyName("min")]
    public int? Min { get; set; }

    [JsonPropertyName("max")]
    public int? Max { get; set; }

    [JsonPropertyName("row_key")]
    public List<string> RowKey { get; set; }

    [Required]
    [JsonPropertyName("columns")]
    public List<ColumnSpec> Columns { get; set; } = new List<ColumnSpec>();

    [JsonPropertyName("row_generators")]
    public List<RowGenerator> RowGenerators { get; set; }

    [JsonPropertyName("aggregates")]
    public List<AggregateSpec> Aggregates { get; set; }
}
```

#### ColumnSpec

```csharp
public class ColumnSpec
{
    [Required]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [Required]
    [JsonPropertyName("label")]
    public string Label { get; set; }

    [Required]
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("required")]
    public bool? Required { get; set; }

    [JsonPropertyName("readonly")]
    public bool? Readonly { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; }

    [JsonPropertyName("pattern")]
    public string Pattern { get; set; }

    [JsonPropertyName("min")]
    public object Min { get; set; }

    [JsonPropertyName("max")]
    public object Max { get; set; }

    [JsonPropertyName("enum")]
    public List<string> Enum { get; set; }

    [JsonPropertyName("default")]
    public object Default { get; set; }

    [JsonPropertyName("formula")]
    public string Formula { get; set; }  // SQL expression

    [JsonPropertyName("format")]
    public string Format { get; set; }
}
```

#### GridSpec

```csharp
public class GridSpec
{
    [Required]
    [JsonPropertyName("rows")]
    public GridRowSpec Rows { get; set; }

    [Required]
    [JsonPropertyName("columns")]
    public GridColumnSpec Columns { get; set; }

    [Required]
    [JsonPropertyName("cell")]
    public GridCellSpec Cell { get; set; }
}

public class GridRowSpec
{
    [Required]
    [JsonPropertyName("mode")]
    public string Mode { get; set; }  // finite, infinite

    [JsonPropertyName("generator")]
    public GridRowGenerator Generator { get; set; }

    [JsonPropertyName("min")]
    public int? Min { get; set; }

    [JsonPropertyName("max")]
    public int? Max { get; set; }
}

public class GridColumnSpec
{
    [Required]
    [JsonPropertyName("generator")]
    public GridColumnGenerator Generator { get; set; }
}

public class GridCellSpec
{
    [Required]
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("enum")]
    public List<string> Enum { get; set; }

    [JsonPropertyName("default")]
    public object Default { get; set; }

    [JsonPropertyName("required")]
    public bool? Required { get; set; }

    [JsonPropertyName("help")]
    public string Help { get; set; }
}
```

#### ChecklistSpec

```csharp
public class ChecklistSpec
{
    [Required]
    [JsonPropertyName("items")]
    public List<ChecklistItem> Items { get; set; } = new List<ChecklistItem>();
}

public class ChecklistItem
{
    [Required]
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [Required]
    [JsonPropertyName("label")]
    public string Label { get; set; }

    [Required]
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("enum")]
    public List<string> Enum { get; set; }

    [JsonPropertyName("default")]
    public object Default { get; set; }

    [JsonPropertyName("required")]
    public bool? Required { get; set; }
}
```

### Metadata & Configuration

#### FormMetadata

```csharp
public class FormMetadata
{
    [JsonPropertyName("organization")]
    public string Organization { get; set; }

    [JsonPropertyName("document_no")]
    public string DocumentNo { get; set; }

    [JsonPropertyName("effective_date")]
    public DateTime? EffectiveDate { get; set; }

    [JsonPropertyName("revision_no")]
    public string RevisionNo { get; set; }

    [JsonPropertyName("reference")]
    public string Reference { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }
}
```

#### FormOptions

```csharp
public class FormOptions
{
    [JsonPropertyName("print")]
    public PrintOptions Print { get; set; }

    [JsonPropertyName("permissions")]
    public PermissionOptions Permissions { get; set; }
}

public class PrintOptions
{
    [JsonPropertyName("page_size")]
    public string PageSize { get; set; }  // A4, Letter

    [JsonPropertyName("margins_mm")]
    public Margins MarginsMm { get; set; }
}

public class Margins
{
    [JsonPropertyName("top")]
    public int Top { get; set; }

    [JsonPropertyName("left")]
    public int Left { get; set; }

    [JsonPropertyName("right")]
    public int Right { get; set; }

    [JsonPropertyName("bottom")]
    public int Bottom { get; set; }
}

public class PermissionOptions
{
    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; }

    [JsonPropertyName("visibility")]
    public string Visibility { get; set; }  // public, private, role-based
}
```

#### StorageOptions

```csharp
public class StorageOptions
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; }  // normalized, denormalized

    [JsonPropertyName("copy_header")]
    public List<string> CopyHeader { get; set; }

    [JsonPropertyName("indexes")]
    public List<List<string>> Indexes { get; set; }
}
```

## Validation Rules

### Form-Level Validation

1. **Unique IDs**: All `id` fields (form, page, section, widget, field names) must be unique within their scope
2. **Pattern Compliance**: IDs must match `^[a-z0-9_-]+$`
3. **Required Fields**: form.id, form.title, form.version, page.id, page.title, section.id, section.title, widget.id, widget.type
4. **Non-Empty Arrays**: pages[] must have at least 1 page, sections[] at least 1 section per page

### Widget-Specific Validation

**Field Widget**:
- Must have `field` property populated
- field.name, field.label, field.type are required
- If type=enum, field.enum must be populated

**Group Widget**:
- Must have `fields[]` array populated
- Each field follows FieldSpec validation

**Table Widget**:
- Must have `table.columns[]` populated
- columns[].name, columns[].label, columns[].type required
- If formula present, cannot have default/readonly

**Grid Widget**:
- Must have `grid.rows`, `grid.columns`, `grid.cell` populated
- rows.mode and columns.generator required
- cell.type required

**Checklist Widget**:
- Must have `checklist.items[]` populated
- Each item.key, item.label, item.type required

### Custom Validators (FluentValidation)

```csharp
public class FormDefinitionValidator : AbstractValidator<FormDefinition>
{
    public FormDefinitionValidator()
    {
        RuleFor(x => x.Id).Matches(@"^[a-z0-9_-]+$");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Version).NotEmpty();
        RuleFor(x => x.Pages).NotEmpty();

        RuleForEach(x => x.Pages).SetValidator(new PageValidator());
    }
}
```

## State Transitions

### Form Lifecycle States

```
[New] ──(Save)──> [Draft] ──(Save)──> [Draft]
                     │
                     └──(Export)──> [Published] (YAML exported)
                     │
                     └──(Delete)──> [Inactive] (soft delete)
```

**State Management**:
- `is_active = true`: Form is available for editing/export
- `is_active = false`: Form is soft-deleted (can be undeleted)
- `updated_at`: Tracks last modification for conflict detection

## Data Access Patterns

### Repository Interface

```csharp
public interface IFormRepository
{
    Task<List<FormDefinitionEntity>> GetAllAsync(bool includeInactive = false);
    Task<FormDefinitionEntity> GetByIdAsync(string formId);
    Task<FormDefinitionEntity> CreateAsync(FormDefinitionRoot formRoot);
    Task<FormDefinitionEntity> UpdateAsync(string formId, FormDefinitionRoot formRoot);
    Task<bool> DeleteAsync(string formId, bool softDelete = true);
    Task<bool> ExistsAsync(string formId);
}
```

### EF Core DbContext

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<FormDefinitionEntity> FormDefinitions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormDefinitionEntity>(entity =>
        {
            entity.HasKey(e => e.FormId);
            entity.Property(e => e.DslJson).HasColumnType("jsonb");
            entity.HasIndex(e => e.IsActive).HasFilter("is_active = true");
            entity.HasIndex(e => e.CreatedAt).IsDescending();
        });
    }
}
```

## JSON Serialization Configuration

### System.Text.Json Settings

```csharp
services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
```

### YamlDotNet Settings (for export)

```csharp
var serializer = new SerializerBuilder()
    .WithNamingConvention(UnderscoredNamingConvention.Instance)
    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
    .Build();
```

## Mapping Strategy

### DTO → Entity

```csharp
// FormDefinitionRoot → FormDefinitionEntity
var entity = new FormDefinitionEntity
{
    FormId = formRoot.Form.Id,
    Version = formRoot.Form.Version,
    DslJson = JsonSerializer.Serialize(formRoot, _jsonOptions),
    CreatedAt = DateTime.UtcNow,
    IsActive = true
};
```

### Entity → DTO

```csharp
// FormDefinitionEntity → FormDefinitionRoot
var formRoot = JsonSerializer.Deserialize<FormDefinitionRoot>(
    entity.DslJson,
    _jsonOptions
);
```

## Performance Considerations

1. **JSONB Indexing**: GIN index on dsl_json enables fast JSON path queries
2. **Lazy Loading**: Frontend loads pages on-demand (not implemented in backend)
3. **Connection Pooling**: EF Core default (max 128 connections)
4. **Caching**: Consider adding Redis for frequently accessed forms (future)

## Migration Strategy

### Initial Schema

```sql
CREATE TABLE form_definitions (
    form_id TEXT PRIMARY KEY,
    version TEXT NOT NULL,
    dsl_json JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX idx_form_definitions_active ON form_definitions(is_active) WHERE is_active = TRUE;
CREATE INDEX idx_form_definitions_created ON form_definitions(created_at DESC);
CREATE INDEX idx_form_definitions_json ON form_definitions USING GIN(dsl_json);
```

### Future Schema Evolution

- Add `created_by`, `updated_by` columns when auth implemented
- Add `tenant_id` column when multi-tenancy implemented
- Add `version_history` JSONB array for version control
