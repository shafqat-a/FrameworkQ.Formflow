# Implementation Plan: Visual Form Designer

**Branch**: `001-designer-implement-please` | **Date**: 2025-10-01 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-designer-implement-please/spec.md`

## Execution Flow (/plan command scope)
```
✅ 1. Load feature spec from Input path
✅ 2. Fill Technical Context (scan for NEEDS CLARIFICATION)
✅ 3. Fill the Constitution Check section
✅ 4. Evaluate Constitution Check section
✅ 5. Execute Phase 0 → research.md
✅ 6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent file
✅ 7. Re-evaluate Constitution Check section
✅ 8. Plan Phase 2 → Describe task generation approach
✅ 9. STOP - Ready for /tasks command
```

**STATUS**: ✅ COMPLETE - All phases finished, ready for /tasks command

## Summary

Implement a web-based visual form designer that enables users to create complex data collection forms through drag-and-drop without writing YAML manually. The designer outputs YAML compliant with DSL v0.1 and generates PostgreSQL DDL for data storage.

**Technical Approach**:
- Frontend: HTML5 + Bootstrap 5 + jQuery 3.7 + jQuery UI 1.13 for drag-drop
- Backend: .NET 8.0 ASP.NET Core Web API
- Database: PostgreSQL 15+ with JSONB storage
- Architecture: Clean Architecture with service layer, RESTful API
- Testing: xUnit + Moq + FluentAssertions

## Technical Context

**Language/Version**: C# 12 / .NET 8.0 (LTS), JavaScript ES6+
**Primary Dependencies**:
  - Backend: ASP.NET Core 8.0, Entity Framework Core 8.0, Npgsql.EntityFrameworkCore.PostgreSQL 8.0, YamlDotNet 13.7.1, Swashbuckle 6.5.0, FluentValidation 11.3.0
  - Frontend: jQuery 3.7.0, jQuery UI 1.13.2, Bootstrap 5.3.0
**Storage**: PostgreSQL 15+ (JSONB for form definitions)
**Testing**: xUnit 2.6.2, Moq 4.20.70, FluentAssertions 6.12.0, Microsoft.AspNetCore.Mvc.Testing 8.0.0
**Target Platform**: Cross-platform web application (Linux/Windows/macOS server, modern browsers)
**Project Type**: web (frontend + backend)
**Performance Goals**:
  - UI: <200ms drag-drop response time
  - API: <100ms CRUD operations, <500ms YAML export, <1s SQL generation
  - Support: 50+ pages, 500+ widgets, 100+ table columns per form
**Constraints**:
  - DSL v0.1 compliance required
  - ID pattern: `^[a-z0-9_-]+$`
  - Browser support: Chrome 90+, Firefox 88+, Edge 90+
  - No authentication in v1 (single-user mode)
**Scale/Scope**:
  - Initial deployment: Single-user desktop-style application
  - Target: 100+ form definitions, 10k+ form instances (future)
  - Complexity limits: 100 pages, 1000 widgets max per form

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Principle I: GitHub Issue-Driven Development
- ✅ **PASS**: All tasks will be tracked via GitHub issues using `gh issue` CLI
- ✅ **PASS**: Issues marked "in-progress" before work starts, closed with commit references
- **Action**: Create initial issues for major components (frontend, backend, database schema)

### Principle II: Test-Driven Development (NON-NEGOTIABLE)
- ✅ **PASS**: Contract tests written first for all API endpoints
- ✅ **PASS**: Integration tests for user scenarios before implementation
- ✅ **PASS**: Unit tests for service layer methods
- **Verification**: research.md specifies xUnit + TDD workflow

### Principle III: Git Workflow Best Practices
- ✅ **PASS**: Feature branch `001-designer-implement-please` created from main
- ✅ **PASS**: Conventional commits planned (feat, test, refactor, docs)
- ✅ **PASS**: Pull request workflow for merge to main
- **Verification**: Branch naming follows `feature/###-description` pattern

### Principle IV: Comprehensive Testing Strategy
- ✅ **PASS**: Contract tests for API boundaries (OpenAPI spec defined)
- ✅ **PASS**: Integration tests for drag-drop, save/load, export workflows
- ✅ **PASS**: Unit tests for YAML serialization, SQL generation, validation
- **Coverage Target**: 80%+ excluding generated code

### Principle V: Code Review and Quality Gates
- ✅ **PASS**: PR review required before merge
- ✅ **PASS**: Review checklist: tests pass, conventions followed, docs updated
- **Reviewer**: At least one approval required
- **Quality Gates**: All tests pass, no compiler warnings, Swagger docs updated

**Initial Constitution Check**: ✅ PASS (all principles satisfied)

## Project Structure

### Documentation (this feature)
```
specs/001-designer-implement-please/
├── plan.md              # This file (/plan command output)
├── research.md          # ✅ Phase 0 output - Technology decisions complete
├── data-model.md        # ✅ Phase 1 output - Entity models complete
├── contracts/           # ✅ Phase 1 output - OpenAPI spec complete
│   └── forms-api.yaml
├── quickstart.md        # 🔄 Phase 1 output - In progress
└── tasks.md             # ⏳ Phase 2 output (/tasks command - NOT created yet)
```

### Source Code (repository root)

```
backend/
├── src/
│   ├── FormDesigner.API/
│   │   ├── Controllers/
│   │   │   ├── FormDefinitionController.cs
│   │   │   └── ExportController.cs
│   │   ├── Services/
│   │   │   ├── IFormBuilderService.cs
│   │   │   ├── FormBuilderService.cs
│   │   │   ├── IYamlExportService.cs
│   │   │   ├── YamlExportService.cs
│   │   │   ├── ISqlGeneratorService.cs
│   │   │   └── SqlGeneratorService.cs
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── Repositories/
│   │   │       ├── IFormRepository.cs
│   │   │       └── FormRepository.cs
│   │   ├── Models/
│   │   │   ├── Entities/
│   │   │   │   └── FormDefinitionEntity.cs
│   │   │   └── DTOs/
│   │   │       ├── FormDefinitionRoot.cs
│   │   │       ├── FormDefinition.cs
│   │   │       ├── Page.cs
│   │   │       ├── Section.cs
│   │   │       ├── Widget.cs
│   │   │       └── Specs/
│   │   ├── Validators/
│   │   │   └── FormDefinitionValidator.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── FormDesigner.API.csproj
│   └── wwwroot/
│       ├── index.html
│       ├── css/
│       │   └── designer.css
│       └── js/
│           └── designer.js
└── tests/
    ├── FormDesigner.Tests.Contract/
    │   ├── FormsApiContractTests.cs
    │   └── ExportApiContractTests.cs
    ├── FormDesigner.Tests.Integration/
    │   ├── FormCrudIntegrationTests.cs
    │   ├── YamlExportIntegrationTests.cs
    │   └── SqlGenerationIntegrationTests.cs
    └── FormDesigner.Tests.Unit/
        ├── Services/
        │   ├── YamlExportServiceTests.cs
        │   ├── SqlGeneratorServiceTests.cs
        │   └── FormBuilderServiceTests.cs
        └── Validators/
            └── FormDefinitionValidatorTests.cs
```

**Structure Decision**: Web application pattern (Option 2) selected. Backend contains both API and static frontend files served from `wwwroot/`. This simplifies deployment and avoids CORS complexity in single-user mode. Frontend uses CDN references for jQuery/Bootstrap to minimize build requirements.

## Phase 0: Outline & Research

**Status**: ✅ COMPLETE

**Output**: [research.md](./research.md)

### Key Decisions Made

1. **Frontend Stack**: HTML5 + Bootstrap 5 + jQuery 3.7 + jQuery UI 1.13
   - Rationale: Simple, no build tooling, mature drag-drop library
   - Alternative: React/Vue rejected due to complexity for this use case

2. **Backend Stack**: .NET 8.0 ASP.NET Core Web API
   - Rationale: High performance, strong typing, excellent PostgreSQL support
   - Alternative: .NET 6 rejected (8.0 is LTS with better performance)

3. **Database**: PostgreSQL 15+ with JSONB storage
   - Rationale: Native JSON support, GIN indexes, SQL generation target
   - Alternative: MongoDB rejected (need relational integrity for generated tables)

4. **YAML Library**: YamlDotNet 13.7.1
   - Rationale: Most popular .NET YAML library, snake_case support
   - Alternative: Custom writer rejected (maintenance burden)

5. **Testing Framework**: xUnit + Moq + FluentAssertions
   - Rationale: Modern, parallel execution, readable assertions
   - Alternative: NUnit rejected (xUnit more idiomatic for .NET Core)

6. **Architecture**: Clean Architecture with Service Layer
   - Layers: Controllers → Services → Repositories → Database
   - Rationale: Testability, separation of concerns, maintainability

### Assumptions Documented

- Single-user mode (no auth required in v1)
- Formula validation at export-time (not real-time)
- Form complexity limits: 100 pages, 1000 widgets max
- Undo/redo deferred to future version
- Preview mode: static HTML (not interactive)
- No multi-tenancy (single database)
- Version control: simple version string (no git-style history)

## Phase 1: Design & Contracts

**Status**: ✅ COMPLETE (all 5 artifacts generated)

### Completed Artifacts

1. ✅ **data-model.md**: Complete entity relationship diagram, C# models, database schema
   - FormDefinitionEntity (PostgreSQL table)
   - FormDefinitionRoot → Form → Pages → Sections → Widgets
   - All widget types: Field, Group, Table, Grid, Checklist
   - Validation rules, state transitions, repository interfaces
   - EF Core DbContext configuration

2. ✅ **contracts/forms-api.yaml**: OpenAPI 3.0 specification
   - 6 endpoints: List, Create, Get, Update, Delete, Export YAML, Export SQL
   - Complete request/response schemas
   - Validation rules (pattern, required fields, enums)
   - Error response schemas (400, 404, 409, 500)

3. ✅ **research.md**: Technology stack decisions and rationale
   - Frontend: HTML5 + Bootstrap 5 + jQuery 3.7 + jQuery UI 1.13
   - Backend: .NET 8.0 ASP.NET Core Web API
   - Database: PostgreSQL 15+ with JSONB
   - Testing: xUnit + Moq + FluentAssertions
   - Architecture: Clean Architecture with service layer

4. ✅ **quickstart.md**: User acceptance test scenarios
   - 10 comprehensive manual test scenarios
   - Maps all acceptance criteria from spec
   - Step-by-step validation procedures
   - Performance validation tests
   - Troubleshooting guide

5. ✅ **CLAUDE.md**: Agent context file
   - Project overview and tech stack
   - Recent changes tracked
   - Development guidance for AI agents
   - Generated via update-agent-context.sh

### Post-Design Constitution Re-Check

✅ **All principles still satisfied**:
- Principle I: GitHub issue tracking planned
- Principle II: TDD workflow embedded in quickstart.md and task ordering
- Principle III: Git workflow documented
- Principle IV: Contract tests, integration tests, unit tests all specified
- Principle V: PR review process defined

## Phase 2: Task Planning Approach

**Status**: ⏳ NOT STARTED (will be executed by /tasks command)

**Task Generation Strategy**:
- Load data-model.md → generate entity creation tasks
- Load contracts/forms-api.yaml → generate contract test tasks per endpoint
- Load quickstart.md → generate integration test tasks per scenario
- Generate implementation tasks to make tests pass

**Ordering Strategy**:
1. Setup: Project structure, NuGet packages, database schema
2. Tests First (TDD - CRITICAL):
   - Contract tests for each API endpoint [P]
   - Integration tests for user scenarios [P]
   - Unit tests for services [P]
3. Core Implementation:
   - Database models and EF Core context
   - Repository layer
   - Service layer (YAML export, SQL generation)
   - API controllers
4. Frontend Implementation:
   - HTML structure
   - CSS styling
   - JavaScript modules (drag-drop, inspector, API client)
5. Integration & Polish:
   - End-to-end testing
   - Performance validation
   - Documentation

**Estimated Output**: 40-50 numbered, ordered tasks in tasks.md

**IMPORTANT**: Phase 2 is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation

*These phases are beyond the scope of the /plan command*

**Phase 3**: Task generation (/tasks command creates tasks.md)
**Phase 4**: Implementation (execute tasks.md following TDD principles)
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking

*No constitutional violations detected*

This implementation follows all constitutional principles:
- GitHub issues for all tasks
- TDD workflow (tests before implementation)
- Standard Git workflow with feature branch
- Comprehensive test coverage (contract, integration, unit)
- PR review before merge

## Progress Tracking

*This checklist is updated during execution flow*

**Phase Status**:
- [x] Phase 0: Research complete (/plan command) - ✅ research.md
- [x] Phase 1: Design complete (/plan command) - ✅ All 5 artifacts generated
  - [x] data-model.md
  - [x] contracts/forms-api.yaml
  - [x] research.md
  - [x] quickstart.md
  - [x] CLAUDE.md
  - [x] Re-evaluate Constitution Check
- [x] Phase 2: Task planning approach described (/plan command)
- [ ] Phase 3: Tasks generated (/tasks command) - NEXT STEP
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS (all principles satisfied)
- [x] All NEEDS CLARIFICATION resolved (via research decisions + assumptions)
- [x] Complexity deviations documented: NONE

**Artifacts Created**:
- ✅ specs/001-designer-implement-please/research.md
- ✅ specs/001-designer-implement-please/data-model.md
- ✅ specs/001-designer-implement-please/contracts/forms-api.yaml
- ✅ specs/001-designer-implement-please/quickstart.md
- ✅ CLAUDE.md
- ⏳ specs/001-designer-implement-please/tasks.md (ready for /tasks command)

---

**Next Steps**:
1. ✅ Phase 0-2 complete
2. Run `/tasks` command to generate tasks.md
3. Execute tasks following TDD principles
4. Create GitHub issues for each major task group
5. Implement following constitutional principles

*Based on Constitution v1.0.0 - See `.specify/memory/constitution.md`*
