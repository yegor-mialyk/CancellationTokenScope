# Cancellation Token Scope

[![License](https://img.shields.io/badge/License-MIT-brightgreen.svg?style=plastic)](LICENSE)
[![Issues](https://img.shields.io/github/issues-raw/yegor-mialyk/CancellationTokenScope.svg?style=plastic)](https://github.com/yegor-mialyk/CancellationTokenScope/issues)

A simple and flexible way to manage ambient `CancellationToken` instances in .NET applications.

`CancellationTokenScope` was created out of the need for a better way to propagate cancellation tokens through deep call stacks without explicitly passing them as method parameters.

The commonly advocated method of passing `CancellationToken` as a parameter to every async method works fine for simple scenarios. But it becomes cumbersome in large applications with deep call hierarchies, especially when retrofitting cancellation support into existing codebases or when working with third-party libraries that do not accept cancellation tokens.

`CancellationTokenScope` implements the ambient context pattern for `CancellationToken` instances. It's similar in concept to `TransactionScope`, it allows you to establish a cancellation token at a high level and have it automatically available to all code executing within that scope.

**It works seamlessly with async/await execution flows and is fully thread-safe.**

## Requirements

- .NET 10.0 or later
- C# with nullable reference types support

## Using CancellationTokenScope

### Overview

The purpose of a `CancellationTokenScope` is to make a `CancellationToken` available to all code executing within its scope without explicitly passing it through method parameters.

### Typical Usage

With `CancellationTokenScope`, your typical service method would look like this:

```csharp
using My.CancellationTokenScope;

public class OrderService
{
    private readonly IAmbientCancellationTokenLocator _cancellationTokenLocator;

    public OrderService(IAmbientCancellationTokenLocator cancellationTokenLocator)
    {
        _cancellationTokenLocator = cancellationTokenLocator;
    }

    public async Task ProcessOrdersAsync(CancellationToken cancellationToken)
    {
        // Establish the ambient cancellation token for this scope
        using (_cancellationTokenLocator.Set(cancellationToken))
        {
            await ValidateOrdersAsync();
            await SaveOrdersAsync();
            await SendNotificationsAsync();
        }
    }

    private async Task ValidateOrdersAsync()
    {
        // Access the ambient cancellation token - no parameter needed
        var token = _cancellationTokenLocator.Get();

        await SomeOperationAsync(token);
    }
}
```

### Nesting Scopes

`CancellationTokenScope` supports nesting. When a scope is disposed, the parent scope's cancellation token becomes the ambient token again:

```csharp
public async Task OuterMethodAsync(CancellationToken outerToken)
{
    using (new CancellationTokenScope(outerToken))
    {
        // Ambient token is outerToken
        await MiddleMethodAsync();
        // Ambient token is outerToken again
    }
}

private async Task MiddleMethodAsync()
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

    using (new CancellationTokenScope(cts.Token))
    {
        // Ambient token is now cts.Token (with 30-second timeout)
        await InnerMethodAsync();
    }
    // Ambient token is restored to outerToken
}
```

### Async Support

`CancellationTokenScope` works with async execution flows as you would expect:

```csharp
public async Task RandomServiceMethodAsync(CancellationToken cancellationToken)
{
    using (new CancellationTokenScope(cancellationToken))
    {
        var users = await _userRepository.GetAllAsync();
        var orders = await _orderRepository.GetOrdersAsync();

        // Both repository methods can access the ambient cancellation token
        // even though they're awaited on potentially different threads
    }
}
```

### Dependency Injection Setup

You can use the `IAmbientCancellationTokenLocator` interface for dependency injection scenarios. Register the `AmbientCancellationTokenLocator` in your DI container as a singleton:

```csharp
// In Program.cs or Startup.cs
services.AddSingleton<IAmbientCancellationTokenLocator, AmbientCancellationTokenLocator>();
```

Then inject it into your services:

```csharp
public class MyService
{
    private readonly IAmbientCancellationTokenLocator _tokenLocator;

    public MyService(IAmbientCancellationTokenLocator tokenLocator)
    {
        _tokenLocator = tokenLocator;
    }

    public async Task DoWorkAsync()
    {
        var token = _tokenLocator.Get();
        // Use token for operations
    }
}
```

### Default Behavior

If no ambient scope exists, `IAmbientCancellationTokenLocator.Get()` returns `CancellationToken.None`, allowing code to work safely even outside of an established scope:

```csharp
public async Task SafeMethodAsync()
{
    // Returns CancellationToken.None if no scope is active
    var token = _cancellationTokenLocator.Get();
    await SomeOperationAsync(token);
}
```

## Feedback

File an issue or request a new feature in [GitHub Issues](https://github.com/yegor-mialyk/CancellationTokenScope/issues).

## License

Copyright (C) 2020-2026 Yegor Mialyk. All Rights Reserved.

Licensed under the [MIT](LICENSE) License.
