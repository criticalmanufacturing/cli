# AI Agents Guidelines

This document defines how AI agents should interact with the `criticalmanufacturing/cli` repository.

The goal of these guidelines is to ensure that automated agents produce safe, maintainable, and consistent contributions.

---

# Repository Overview

This repository contains the **Critical Manufacturing CLI**, a command line tool used to support development workflows and project automation.

Primary characteristics:

- Main language: **C#**
- Secondary languages: **PowerShell, TypeScript**
- Architecture organized around:
  - `core` – shared CLI infrastructure
  - `cmf-cli` – main executable
  - `features` – CLI commands/features
  - `tests` – automated tests
  - `docs` – documentation
- Supports **CLI plugin extensions**

Agents should respect the architectural boundaries above.

---

# Agent Responsibilities

AI agents operating on this repository may:

- Suggest improvements to CLI commands
- Implement new CLI features
- Improve documentation
- Fix bugs
- Improve test coverage
- Refactor code when explicitly requested

Agents should always prioritize:

1. Minimal changes
2. Backward compatibility
3. Code readability

---

# Allowed Modifications

Agents are allowed to modify:

- `core/`
- `cmf-cli/`
- `features/`
- `tests/`
- `docs/`

Agents may:

- add new CLI commands
- improve command help text
- update tests
- update documentation

---

# Restricted Modifications

Agents MUST NOT:

- change licensing
- modify publishing workflows without explicit instruction
- introduce breaking CLI changes
- remove existing commands without migration
- introduce new external dependencies without justification

---

# Code Guidelines

When generating code:

- Follow existing repository coding patterns
- Prefer **existing abstractions in `core`**
- Maintain **consistent CLI command structure**
- Avoid unnecessary complexity
- Keep methods small and testable

C# guidelines:

- Use async where appropriate
- Follow existing naming conventions
- Avoid large classes
- Prefer dependency injection

---

# CLI Command Design

When implementing new commands:

Commands should include:

- clear description
- argument validation
- helpful error messages
- usage examples

Example structure:

cmf [options]

---

# Testing Rules

All functional changes should include tests.

Agents should:

- add tests under `tests/`
- avoid breaking existing tests
- update tests if behaviour intentionally changes

---

# Documentation

Agents should update documentation when:

- adding commands
- modifying command behaviour
- introducing configuration options

Documentation should be updated in: docs/

---

# Pull Request Rules

Agent-generated changes should follow:

- small PRs
- clear commit messages
- explanation of reasoning

Commit messages should follow conventional commit format: https://www.conventionalcommits.org/