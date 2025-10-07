<!--
SYNC IMPACT REPORT
==================
Version Change: [Not versioned previously] → 1.0.0
Rationale: Initial constitution creation with mandatory GitHub-driven workflow

Added Sections:
  - Core Principles (5 principles)
  - Development Workflow
  - Quality Standards
  - Governance

Modified Templates:
  ✅ .specify/templates/plan-template.md - Constitution Check gate aligned
  ✅ .specify/templates/tasks-template.md - Task tracking requirements added
  ⚠ .specify/templates/commands/*.md - Review for GitHub workflow references

Follow-up TODOs:
  - Update CI/CD pipeline to enforce constitution checks
  - Add GitHub issue templates for task tracking
  - Configure branch protection rules to enforce principles
-->

# FrameworkQ.Formflow Constitution

## Core Principles

### I. GitHub Issue-Driven Development (NON-NEGOTIABLE)

Every task MUST have a corresponding GitHub issue before work begins. This ensures:
- Transparent work tracking and prioritization
- Clear acceptance criteria and scope definition
- Audit trail for all changes
- Proper linkage between code and requirements

**Requirements**:
- Create GitHub issue with descriptive title and acceptance criteria
- Label issue appropriately (bug, feature, enhancement, test, docs)
- Reference issue number in all related commits and PRs
- Close issue only when all acceptance criteria met and tests pass

**Rationale**: GitHub issues provide single source of truth for what work is being done, why it's being done, and the current status. This prevents duplicate work, ensures alignment with requirements, and maintains project transparency.

### II. Work-In-Progress (WIP) Marking (NON-NEGOTIABLE)

Tasks MUST be marked as WIP before implementation begins and MUST remain WIP until completion criteria are met.

**Requirements**:
- Mark task as `[ ]` (pending) in tasks.md initially
- Update to `[WIP]` when starting implementation
- Update to `[X]` only after:
  * Implementation complete
  * Tests written and passing
  * Code reviewed (if applicable)
  * Committed to repository

**Rationale**: WIP marking prevents concurrent work on same tasks, provides clear status visibility, and ensures completion criteria are met before marking tasks done.

### III. Test-After-Implementation (NON-NEGOTIABLE)

After completing any implementation task, corresponding tests MUST be written and MUST pass before the task can be marked complete.

**Requirements**:
- Unit tests for all business logic and services
- Integration tests for API endpoints and database operations
- Contract tests for external interfaces
- All tests must pass before marking task complete
- Test coverage must be maintained or improved (target: ≥80%)

**Test Hierarchy**:
1. **Unit Tests**: Isolated component testing (FormDesigner.Tests.Unit)
2. **Integration Tests**: Full-stack API testing (FormDesigner.Tests.Integration)
3. **Contract Tests**: API contract validation (FormDesigner.Tests.Contract)

**Rationale**: Tests provide confidence that implementations meet requirements, catch regressions early, and serve as executable documentation of expected behavior.

### IV. Commit and Push After Task Completion (NON-NEGOTIABLE)

Code MUST be committed and pushed to GitHub after each completed task. Work-in-progress commits are discouraged unless checkpointing long-running tasks.

**Requirements**:
- Commit message format: `<type>(<scope>): <description> (#<issue-number>)`
  * Types: feat, fix, test, docs, refactor, chore, perf, style
  * Example: `feat(sql-server): add SQL Server test fixture (#123)`
- Include issue reference in commit message
- Push to feature branch after task completion
- Create PR when feature branch ready for review

**Commit Guidelines**:
- One logical change per commit
- Clear, descriptive commit messages
- Reference GitHub issue number
- Sign commits if project requires GPG signing

**Rationale**: Regular commits create checkpoint history, enable collaboration, prevent data loss, and provide clear mapping between work items and code changes.

### V. Quality and Documentation Standards

Code quality, testing, and documentation are non-negotiable aspects of software delivery.

**Requirements**:
- Code must follow project style guidelines (C# conventions, .NET best practices)
- All public APIs must have XML documentation comments
- Complex logic must include inline comments explaining "why" not "what"
- README and documentation must be updated when functionality changes
- No warnings or errors in build output (treat warnings as errors)

**Code Quality Standards**:
- Follow SOLID principles
- Prefer composition over inheritance
- Keep methods focused (single responsibility)
- Avoid magic numbers and strings (use constants/enums)
- Handle errors explicitly (no empty catch blocks)

**Rationale**: Quality standards ensure maintainability, reduce technical debt, enable onboarding of new developers, and prevent bugs in production.

## Development Workflow

### Task Lifecycle

1. **Task Planning** (`/specify`, `/plan`, `/tasks` commands)
   - Feature specification created
   - Implementation plan documented
   - Tasks broken down with dependencies

2. **Task Selection**
   - Developer selects next task from tasks.md
   - Creates GitHub issue for task
   - Marks task as `[WIP]` in tasks.md

3. **Implementation**
   - Write code following architecture in plan.md
   - Follow test-after-implementation approach
   - Commit regularly with descriptive messages

4. **Testing**
   - Write tests for implemented functionality
   - Run test suite: `dotnet test`
   - Verify all tests pass
   - Check code coverage meets threshold

5. **Completion**
   - Mark task as `[X]` in tasks.md
   - Commit and push changes
   - Update GitHub issue status
   - Move to next task

6. **Feature Completion**
   - All tasks marked `[X]`
   - Integration tests pass
   - Documentation updated
   - Create PR for review

### Branch Strategy

- **main**: Production-ready code, protected branch
- **feature/xxx-description**: Feature branches from main
- **bugfix/xxx-description**: Bug fix branches
- **hotfix/xxx-description**: Urgent production fixes

**Branch Rules**:
- Feature branches for all new work
- Merge to main only via pull request
- Require passing tests before merge
- Delete feature branches after merge

## Quality Standards

### Testing Requirements

**Minimum Coverage**: 80% code coverage for backend services and repositories

**Test Categories**:
- **Unit Tests**: Fast, isolated, no external dependencies
- **Integration Tests**: Database, API, full stack
- **Contract Tests**: API contract validation

**Test Execution**:
```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter "Category=Integration"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Performance Standards

- API response time: <200ms for GET requests (p95)
- Database queries: <100ms for simple queries
- Application startup: <10 seconds
- Memory usage: <500MB baseline

### Documentation Requirements

**Code Documentation**:
- XML comments for all public APIs
- Inline comments for complex logic
- README.md updated for feature changes

**Project Documentation**:
- Architecture decisions recorded in specs/
- Database schema documented
- API contracts defined in contracts/
- Quickstart guides for new features

## Governance

### Constitution Authority

This constitution supersedes all other development practices and guidelines. All team members and automated systems MUST comply with these principles.

**Compliance Verification**:
- All pull requests reviewed for constitution compliance
- Automated checks enforce testing and commit message standards
- Regular audits of task tracking and issue linkage

### Amendment Process

Constitution amendments require:
1. Proposal documented with rationale
2. Team consensus (or project owner approval)
3. Version bump following semantic versioning
4. Update of all dependent templates and documentation
5. Communication to all stakeholders

**Version Numbering**:
- **MAJOR**: Breaking changes to core principles or workflow
- **MINOR**: New principles or significant guidance additions
- **PATCH**: Clarifications, typo fixes, minor refinements

### Enforcement

**Pre-Commit Checks**:
- Commit message format validation
- No compiler warnings or errors
- Code formatting standards

**Pre-Merge Checks** (CI/CD):
- All tests pass (unit, integration, contract)
- Code coverage threshold met (≥80%)
- No security vulnerabilities
- GitHub issue referenced and validated

**Post-Merge**:
- Task marked complete in tasks.md
- GitHub issue closed
- Documentation updated

### Exceptions

Exceptions to constitutional principles require:
- Explicit justification documented in commit/PR
- Project owner approval
- Time-bound: must be resolved in follow-up work
- Tracked in GitHub issues with `tech-debt` label

**Valid Exception Scenarios**:
- Emergency hotfixes (document and remediate)
- Prototype/spike work (in separate branch, not merged)
- Temporary workarounds (with follow-up issue)

### Reporting

**Project Health Metrics**:
- Task completion rate (from tasks.md)
- Test pass rate and coverage
- Open vs closed GitHub issues
- Time from task start (WIP) to completion
- Commit frequency and PR cycle time

**Reviews**:
- Weekly: Task progress and blockers
- Monthly: Constitution compliance audit
- Quarterly: Process improvements and amendments

---

**Version**: 1.0.0
**Ratified**: 2025-10-07
**Last Amended**: 2025-10-07
