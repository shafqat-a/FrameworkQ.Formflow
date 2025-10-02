# Research: Visual Form Designer

## Technology Stack Decisions

### Frontend Stack

**Decision**: HTML5 + Bootstrap 5 + jQuery 3.7 + jQuery UI 1.13

**Rationale**:
- **HTML5**: Native drag-and-drop API support, modern semantic elements
- **Bootstrap 5**: Rapid UI development, responsive grid system, consistent styling
- **jQuery 3.7**: Simplified DOM manipulation, AJAX handling, broad browser compatibility
- **jQuery UI 1.13**: Mature drag-and-drop components (draggable/droppable), rich interactions

**Alternatives Considered**:
- React/Vue/Angular: Rejected due to added complexity and build tooling requirements
- Plain JavaScript: Rejected due to development time and cross-browser compatibility concerns
- Vanilla CSS: Rejected in favor of Bootstrap's pre-built components and grid system

### Backend Stack

**Decision**: .NET 8.0 (LTS) with ASP.NET Core Web API

**Rationale**:
- **Performance**: High-throughput HTTP APIs, async/await support
- **Type Safety**: Strong typing reduces runtime errors
- **Entity Framework Core**: Robust ORM with PostgreSQL provider (Npgsql)
- **Built-in DI**: Native dependency injection container
- **Cross-platform**: Runs on Linux, Windows, macOS

**Alternatives Considered**:
- .NET Framework 4.8: Rejected (Windows-only, not cross-platform)
- .NET 6: Rejected in favor of .NET 8 (latest LTS with better performance)
- Node.js/Express: Rejected to match user requirement for .NET Core

### Database

**Decision**: PostgreSQL 15+

**Rationale**:
- **JSON Support**: Native JSONB type for storing form definitions efficiently
- **Schema Validation**: JSON schema validation functions
- **Indexing**: GIN indexes on JSONB for fast queries
- **Reliability**: ACID compliance, mature replication
- **SQL Generation Target**: Forms will generate PostgreSQL-compatible DDL

**Alternatives Considered**:
- SQL Server: Rejected (licensing costs, less JSON performance)
- MongoDB: Rejected (forms need relational integrity for generated tables)
- MySQL: Rejected (weaker JSON support compared to PostgreSQL)

### YAML Serialization

**Decision**: YamlDotNet 13.x

**Rationale**:
- Most popular .NET YAML library (4M+ downloads)
- Supports custom serialization behaviors
- Snake_case naming convention support (required for DSL v0.1)
- Null value omission (cleaner YAML output)

**Alternatives Considered**:
- Custom YAML writer: Rejected (reinventing the wheel, maintenance burden)
- JSON-only: Rejected (spec requires YAML export)

### Testing Framework

**Decision**: xUnit + Moq + FluentAssertions

**Rationale**:
- **xUnit**: Modern, extensible, parallel test execution
- **Moq**: Flexible mocking for unit tests
- **FluentAssertions**: Readable assertions, better error messages

**Alternatives Considered**:
- NUnit: Rejected (xUnit more idiomatic for .NET Core)
- MSTest: Rejected (less feature-rich than xUnit)

## Architecture Patterns

### Backend Architecture

**Decision**: Clean Architecture with Service Layer

**Layers**:
1. **API Controllers**: HTTP endpoints, request/response handling
2. **Services**: Business logic, orchestration
3. **Data Access**: EF Core repositories, database operations
4. **Models**: Domain entities, DTOs

**Rationale**:
- Separation of concerns
- Testability (services can be unit tested independently)
- Maintainability (changes to one layer don't cascade)

### Frontend Architecture

**Decision**: Module Pattern with State Management

**Components**:
1. **DesignerState**: Centralized form definition state
2. **WidgetTemplates**: Reusable widget configuration
3. **API Module**: AJAX communication wrapper
4. **UI Modules**: Drag-drop, inspector, canvas management

**Rationale**:
- jQuery-compatible pattern
- Avoids global namespace pollution
- State centralization simplifies debugging

### API Design

**Decision**: RESTful API with conventional HTTP methods

**Endpoints**:
- `GET /api/forms` - List all forms
- `GET /api/forms/{id}` - Get specific form
- `POST /api/forms` - Create new form
- `PUT /api/forms/{id}` - Update existing form
- `DELETE /api/forms/{id}` - Delete form
- `GET /api/export/{id}/yaml` - Export YAML
- `GET /api/export/{id}/sql` - Generate SQL DDL

**Rationale**:
- Standard REST conventions
- Clear resource semantics
- HTTP verbs match CRUD operations

## DSL v0.1 Compliance

### ID Validation Pattern

**Decision**: Regex `^[a-z0-9_-]+$`

**Implementation**:
- Frontend: HTML5 pattern attribute + JavaScript validation
- Backend: Model validation attributes + FluentValidation

### Data Type Mapping

| DSL Type | C# Type | PostgreSQL Type | HTML Input |
|----------|---------|----------------|------------|
| string | string | TEXT | text |
| text | string | TEXT | textarea |
| integer | int | INTEGER | number |
| decimal | decimal | NUMERIC(18,6) | number (step=0.01) |
| date | DateTime | DATE | date |
| time | TimeSpan | TIME | time |
| datetime | DateTimeOffset | TIMESTAMPTZ | datetime-local |
| bool | bool | BOOLEAN | checkbox |
| enum | string | TEXT | select |
| attachment | string | TEXT | file (URI stored) |
| signature | string | TEXT | canvas (data URL) |

### Formula Translation

**Decision**: Simple expression parser for basic formulas

**Supported Operations**:
- Arithmetic: `+`, `-`, `*`, `/`
- Column references: `column_name`
- SQL translation: `forced + scheduled` → `COALESCE(forced, 0) + COALESCE(scheduled, 0)`

**Limitations** (Document for users):
- No function calls initially
- No nested expressions in v1
- Numeric types only

## Performance Targets

### UI Responsiveness

**Target**: <200ms for drag-drop operations

**Approach**:
- Minimize DOM manipulations
- Use event delegation
- Debounce property updates (300ms)
- Virtual scrolling for large widget lists (future)

### API Response Times

**Target**: <100ms for form CRUD, <500ms for YAML export, <1s for SQL generation

**Approach**:
- Database indexing on form_id
- JSONB storage for fast retrieval
- Async/await throughout
- Response caching (ETag headers)

### Scalability

**Target**: Support forms with 50+ pages, 500+ widgets, 100+ table columns

**Approach**:
- Pagination for form lists
- Lazy loading of pages in designer
- Efficient JSON serialization
- Connection pooling (EF Core default)

## Security Considerations

### Input Validation

**Implementation**:
- Frontend: HTML5 validation + pattern attributes
- Backend: Model validation attributes + custom validators
- SQL Injection: Parameterized queries only (EF Core default)
- XSS Prevention: Encode all user input in HTML output

### Authentication/Authorization (Deferred)

**Decision**: Initially no auth (single-user desktop-style app)

**Future**:
- JWT-based authentication
- Role-based access (admin, designer, viewer)
- Form-level permissions

**Rationale**: Spec unclear on auth requirements; implement basic CRUD first

### CORS Configuration

**Decision**: Configurable allowed origins via appsettings.json

**Default**: `http://localhost:5173` (frontend dev server)

## Deployment Architecture

### Development Environment

**Frontend**: Served as static files from `wwwroot/`
**Backend**: Kestrel (port 5000)
**Database**: PostgreSQL Docker container (port 5432)

### Production Considerations (Future)

**Frontend**: Nginx reverse proxy
**Backend**: Docker container, behind nginx
**Database**: Managed PostgreSQL (AWS RDS, Azure Database)
**Monitoring**: Serilog → Seq/Elasticsearch

## Open Questions & Assumptions

### Assumptions Made

1. **Single-user mode**: No authentication required initially
2. **Formula validation**: Export-time validation (not real-time)
3. **Form complexity**: Reasonable limits (100 pages, 1000 widgets max)
4. **Undo/Redo**: Not in v1 (future enhancement)
5. **Preview mode**: Static HTML preview (not interactive)
6. **Multi-tenancy**: Not required (single database, no tenant isolation)
7. **Version control**: Simple version string (no git-style history)

### Deferred Decisions

1. **Real-time collaboration**: SignalR integration (future)
2. **Advanced formulas**: Expression parser with functions (future)
3. **Template library**: Pre-built form templates (future)
4. **Import YAML**: Reverse parsing (future)
5. **Localization**: Multi-language UI (future)

## Dependencies & Package Versions

### Backend (.NET)

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="YamlDotNet" Version="13.7.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
```

### Frontend (CDN)

```html
<script src="https://code.jquery.com/jquery-3.7.0.min.js"></script>
<script src="https://code.jquery.com/ui/1.13.2/jquery-ui.min.js"></script>
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
```

### Testing

```xml
<PackageReference Include="xUnit" Version="2.6.2" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
```

## Development Workflow

### Git Branching

**Strategy**: Feature branches from `main`
**Branch Naming**: `feature/001-designer-implement-please`
**Commits**: Conventional commits with issue references

### GitHub Issues

**Workflow**:
1. Create issue via `gh issue create`
2. Assign to self
3. Label "in-progress"
4. Implement with TDD
5. Create PR with "Closes #N"
6. Code review
7. Merge after approval

### Testing Strategy

**Order** (per TDD):
1. Write contract tests (API endpoints)
2. Write integration tests (user scenarios)
3. Write unit tests (service methods)
4. Implement until all tests pass

**Coverage Target**: 80%+ (excluding generated code)

## Risk Mitigation

### Technical Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| jQuery UI drag-drop bugs | Medium | Thorough testing, fallback to HTML5 API |
| Large form performance | High | Pagination, lazy loading, performance profiling |
| YAML serialization edge cases | Medium | Comprehensive test suite, YamlDotNet maturity |
| PostgreSQL JSONB limits | Low | Document limits, test with large forms |

### Project Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| Unclear auth requirements | Medium | Implement without auth, design for retrofit |
| Formula complexity growth | Medium | Start simple, plan parser architecture |
| Browser compatibility | Low | Target modern browsers (Chrome 90+, Firefox 88+) |

## Success Metrics

1. **Functionality**: All 45 functional requirements implemented
2. **Performance**: <200ms drag-drop, <100ms API responses
3. **Quality**: 80%+ test coverage, zero critical bugs
4. **Usability**: Can create 10-page, 50-widget form in <30 minutes
5. **Compliance**: Exported YAML validates against DSL v0.1 schema 100%
