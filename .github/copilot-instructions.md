# Project Guidelines

## Code Style

- **Target**: .NET 10, C# `latest`, nullable reference types enabled
- **Namespaces**: Use file-scoped (`namespace My.CancellationTokenScope;`)
- **Field naming**: Private instance fields use `_prefix`, static fields use `camelCase`
- **Copyright header**: Required in all `.cs` files (see [CancellationTokenScope.cs](../CancellationTokenScope.cs#L1-L8))

## Architecture

This library implements **ambient cancellation token propagation** (similar to `TransactionScope`) to avoid passing `CancellationToken` through deep call hierarchies. It establishes a token at high level and makes it automatically available to all code within that scope.

**Implementation uses**:
- `AsyncLocal<object?>` - Stores scope identity (flows across async/await boundaries)
- `ConditionalWeakTable<object, CancellationTokenScope>` - Maps identity → scope (enables GC)

**Critical pattern**: Identity object indirection prevents memory leaks while maintaining async context flow. See [CancellationTokenScope.cs](../CancellationTokenScope.cs) for the core implementation using:
- Nested scope management via `_parentScope` tracking
- Auto-restore parent scope on `Dispose()`
- Static `GetAmbientScope()` for retrieving current ambient scope

**Public API**:
- [CancellationTokenScope](../CancellationTokenScope.cs) - Core ambient scope (IDisposable)
- [IAmbientCancellationTokenLocator](../IAmbientCancellationTokenLocator.cs) - DI abstraction
- [AmbientCancellationTokenLocator](../AmbientCancellationTokenLocator.cs) - Concrete DI implementation

**Key behaviors**:
- Thread-safe and async-aware (scopes flow correctly across async/await)
- Returns `CancellationToken.None` when no ambient scope exists
- Supports nesting - parent scope restores automatically on dispose

## Build and Test

```powershell
dotnet build
```

No test project exists currently.

## Project Conventions

**Do NOT**:
- Remove AsyncLocal + ConditionalWeakTable pattern (causes memory leaks)
- Change `GetAmbientScope()` signature (breaking change)
- Modify `IAmbientCancellationTokenLocator` interface (public API)
- Add external NuGet dependencies without justification

**Always**:
- Maintain parent scope restoration in `Dispose()`
- Preserve null safety checks before ConditionalWeakTable operations
- Use `is not null` pattern matching and `??` operators
- Keep copyright year range: 2020-2026, Yegor Mialyk

## Integration Points

Pure BCL library, no external dependencies. Consumers use either:
1. Direct: `new CancellationTokenScope(token)` with `using` statement
2. DI: Inject `IAmbientCancellationTokenLocator` for `Get()`/`Set()` methods
