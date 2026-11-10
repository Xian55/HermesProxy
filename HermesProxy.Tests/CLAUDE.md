# HermesProxy.Tests

xUnit test suite for the HermesProxy solution. See root [CLAUDE.md](../CLAUDE.md) for solution-wide conventions.

## Run Tests

```bash
dotnet test                         # Run all tests from repo root
dotnet test HermesProxy.Tests       # Run this project only
dotnet test --filter "ClassName"    # Filter by class or method name
```

## Test Framework

- **xunit.v3 3.2.2** — test runner (supports `Assert.Skip` / `Assert.SkipUnless` / `Assert.SkipWhen` natively)
- **Moq 4.21.0** — mocking
- **coverlet.collector** — code coverage

## Organization

Tests mirror the source project structure:

| Directory | Covers |
|---|---|
| `BnetServer/` | Battle.net server logic |
| `Framework/` | Framework library (IO, crypto, networking) |
| `World/` | Packet handling and translation |

## Conventions

- **Naming**: `MethodName_Scenario_ExpectedResult`
- **Pattern**: Arrange-Act-Assert (AAA)
- **Parameterized tests**: `[Theory]` with `[InlineData]`
- **Access**: Can test `internal` members via `InternalsVisibleTo` (set in Framework and HermesProxy `.csproj` files)

## Project References

- `Framework` — direct reference for testing framework internals
- `HermesProxy` — direct reference for testing application logic
