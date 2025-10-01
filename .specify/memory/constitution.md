<!--
Sync Impact Report - Constitution Update
========================================
Version Change: INITIAL → 1.0.0
Rationale: Initial constitution ratification for FrameworkQ.Formflow project

Modified Principles: N/A (initial creation)

Added Sections:
  - Core Principles (5 principles)
  - Development Workflow
  - Governance

Removed Sections: N/A (initial creation)

Templates Requiring Updates:
  ✅ .specify/templates/plan-template.md - Constitution Check section ready
  ✅ .specify/templates/spec-template.md - No changes required
  ✅ .specify/templates/tasks-template.md - No changes required
  ⚠ .specify/templates/agent-file-template.md - Review recommended

Follow-up TODOs:
  - Consider adding agent-specific file updates to workflow once agents are configured
  - Monitor GitHub issue workflow effectiveness over first 3 sprints
-->

# FrameworkQ.Formflow Constitution

## Core Principles

### I. GitHub Issue-Driven Development
Every task MUST be tracked as a GitHub issue using `gh issue` command-line tool. Before
starting work on any task, the issue MUST be marked as "in progress" (via labels,
assignment, or project board). Once completed, the issue MUST be marked as "done" and
closed with a reference to the implementing commit or PR.

**Rationale**: GitHub issues provide transparent work tracking for distributed teams,
maintain audit trails, enable asynchronous collaboration, and integrate seamlessly with
pull requests and project boards.

### II. Test-Driven Development (NON-NEGOTIABLE)
Tests MUST be written and approved before implementation begins. The development cycle is
strictly: (1) Write tests, (2) Get user/team approval on test coverage, (3) Verify tests
fail, (4) Implement until tests pass. No implementation code may be written until
corresponding tests exist and have been reviewed.

**Rationale**: TDD ensures requirements are understood before coding, provides living
documentation, catches regressions early, and enforces design-for-testability. The
non-negotiable status reflects its critical role in code quality and maintainability.

### III. Git Workflow Best Practices
All development MUST follow standard Git workflow: (1) Create feature branch from main,
(2) Make atomic commits with descriptive messages, (3) Push commits regularly to remote,
(4) Create pull request for review, (5) Merge only after review approval. Force pushes to
main/master are PROHIBITED. Commits MUST be signed and follow conventional commit format
where applicable.

**Rationale**: Standardized Git workflow prevents code conflicts, enables code review,
maintains clean history, supports rollback capabilities, and facilitates team coordination
in remote/distributed environments.

### IV. Comprehensive Testing Strategy
Every feature MUST include appropriate test coverage: unit tests for individual functions,
integration tests for component interactions, and contract tests for API boundaries. Tests
MUST cover edge cases, error conditions, and happy paths. Integration tests are REQUIRED
for: new API contracts, contract changes, inter-service communication, and shared data
schemas.

**Rationale**: Comprehensive testing reduces production defects, documents system behavior,
enables confident refactoring, and serves as regression protection during future changes.

### V. Code Review and Quality Gates
All code changes MUST pass through pull request review before merging. Reviews MUST verify:
(1) Tests exist and pass, (2) Code follows project conventions, (3) Documentation is
updated, (4) No security vulnerabilities introduced, (5) Performance implications
considered. At least one approval is REQUIRED before merge.

**Rationale**: Peer review catches bugs early, ensures knowledge sharing, maintains code
quality standards, provides learning opportunities, and prevents single-point-of-failure
knowledge silos.

## Development Workflow

### Issue Lifecycle
1. **Creation**: Issues created via `gh issue create` with appropriate labels (bug,
   feature, documentation, etc.)
2. **Assignment**: Developer assigns issue to self via `gh issue edit` or project board
3. **Work Start**: Issue labeled "in-progress" or moved to "In Progress" column
4. **Implementation**: Follow TDD cycle, make commits referencing issue number (#123)
5. **Pull Request**: Create PR with "Closes #123" in description
6. **Review**: Address review comments, update tests/code as needed
7. **Merge**: PR merged after approval, issue auto-closed
8. **Verification**: Verify issue shows in "Done" with linked PR/commits

### Commit Standards
- Use conventional commit format: `type(scope): description`
- Types: `feat`, `fix`, `test`, `refactor`, `docs`, `chore`
- Include issue reference: `feat(auth): add login endpoint (#42)`
- Keep commits atomic and focused on single logical change

### Branch Naming
- Feature: `feature/###-short-description` (e.g., `feature/042-user-auth`)
- Bugfix: `fix/###-bug-description`
- Hotfix: `hotfix/###-critical-issue`
- Where `###` is the issue number

## Governance

### Amendment Procedure
Constitution amendments require: (1) Proposal documented in GitHub issue with "constitution"
label, (2) Team discussion and consensus (majority approval in distributed teams),
(3) Update to this document with version bump, (4) Migration plan for affected workflows.

### Versioning Policy
Constitution version follows semantic versioning:
- **MAJOR**: Backward-incompatible governance changes, principle removals, or redefinitions
- **MINOR**: New principles added, sections expanded, new requirements introduced
- **PATCH**: Clarifications, wording improvements, typo fixes, formatting updates

### Compliance Review
All pull requests MUST verify constitutional compliance as part of code review. Reviewers
MUST check: (1) Tests written before implementation, (2) Issue tracking followed,
(3) Commit standards met, (4) Code review conducted, (5) Quality gates passed.

Violations MUST be documented in the PR with justification for any exceptions. Repeated
violations trigger process review and potential constitution amendment.

### Development Guidance
Runtime development guidance for AI agents is maintained in agent-specific files in the
repository root (e.g., `CLAUDE.md`, `GEMINI.md`, `.github/copilot-instructions.md`). These
files provide tactical guidance that supplements but never contradicts this constitution.

**Version**: 1.0.0 | **Ratified**: 2025-10-01 | **Last Amended**: 2025-10-01
