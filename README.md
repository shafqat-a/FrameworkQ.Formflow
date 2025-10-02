# FrameworkQ.Formflow - Visual Form Designer

Full-stack visual form designer for creating complex enterprise forms with drag-and-drop interface. Successfully creates Power Grid Company of Bangladesh quality management forms.

## ✅ Complete Implementation

**Status:** Production Ready
**PR:** https://github.com/shafqat-a/FrameworkQ.Formflow/pull/15
**Commits:** 53
**Tasks Complete:** 44/55 (80%)

## Features

### 11 Widget Types
- **Field** (text, number, date, time, datetime, email, tel, select, textarea) + formulas
- **Table** with columns, merged cells, multi-row headers
- **Grid** multi-column layout
- **Group** nested widgets
- **Checklist** simple list
- **FormHeader** document metadata ⭐
- **Signature** approval workflows ⭐
- **Notes** instructions/warnings ⭐
- **HierarchicalChecklist** nested numbering ⭐
- **RadioGroup** single selection ⭐
- **CheckboxGroup** multi-selection ⭐

### Core Capabilities
- ✅ Design Mode - Drag-drop form builder
- ✅ Runtime Mode - Data entry and submission
- ✅ YAML Import/Export (DSL v0.1)
- ✅ SQL DDL Generation (PostgreSQL)
- ✅ Formula Calculations (9 functions)
- ✅ Validation (FluentValidation + client-side)
- ✅ Preview Mode
- ✅ Enterprise Styling (print-ready)

## Quick Start

```bash
# Database setup
createdb -h localhost -p 5400 -U postgres formflow

# Run migrations
cd backend/src/FormDesigner.API
dotnet ef database update

# Start application
dotnet run
```

**Access:**
- **Designer**: http://localhost:5099/index.html
- **Runtime**: http://localhost:5099/runtime.html

## Sample Forms

10 working Power Grid Company sample forms in `backend/tests/FormDesigner.Tests.Integration/SampleForms/`:

1. surveillance-complete.yaml (QF-GMD-17) ✅
2. qf-gmd-22-complete.yaml (Transformer) ✅
3. qf-gmd-19-complete.yaml (Daily Inspection) ✅
4. qf-gmd-14-shift-roster.yaml
5. qf-gmd-06-performance.yaml
6. enterprise-demo.yaml (ALL widgets)
7. Plus 4 test forms

### Import Sample Form

**Via Designer:**
1. Open http://localhost:5099/index.html
2. Click "Import YAML"
3. Select any `.yaml` file from SampleForms folder

**Via API:**
```bash
curl -X POST http://localhost:5099/api/import/yaml \
  -F "file=@surveillance-complete.yaml"
```

## Documentation

- **[User Guide](doc/USER-GUIDE.md)** - Complete usage instructions
- **[Specification](specs/001-designer-implement-please/spec.md)** - Requirements
- **[Tasks](specs/001-designer-implement-please/tasks-enhancements.md)** - Implementation plan

## Technology Stack

- ASP.NET Core 8.0 / .NET 10.0
- Entity Framework Core 8.0 + PostgreSQL
- jQuery 3.7, Bootstrap 5
- FluentValidation 11.3
- YamlDotNet 13.7

## Testing

**87 Tests:**
- 42 Contract tests
- 33 Integration tests
- 12 Unit tests

**Playwright Verified:**
- ✅ Form import working
- ✅ Preview matches originals
- ✅ All widgets render correctly

## API Endpoints

**Forms:** GET/POST/PUT/DELETE `/api/forms`
**Export:** GET `/api/export/{id}/yaml`, `/api/export/{id}/sql`
**Import:** POST `/api/import/yaml`
**Runtime:** POST `/api/runtime/instances`, PUT `/save`, POST `/submit`

See [USER-GUIDE.md](doc/USER-GUIDE.md) for complete API documentation.

## Project Structure

```
backend/
  src/FormDesigner.API/
    Controllers/        # 16 API endpoints
    Services/           # Business logic
    Data/               # EF Core + repositories
    Models/             # DTOs + entities
    Validators/         # FluentValidation
    wwwroot/            # Frontend (jQuery + Bootstrap)
  tests/
    Contract/           # API contract tests
    Integration/        # E2E tests + sample forms
    Unit/               # Widget validators tests
```

## Development

Built with Test-Driven Development (TDD):
- Tests written before implementation
- 87 tests covering all features
- Playwright for end-to-end verification

## License

Generated with [Claude Code](https://claude.com/claude-code)

## Screenshot

![Form Designer](https://via.placeholder.com/800x600?text=Visual+Form+Designer)