//
// Ambient Cancellation Token Library
//
// Copyright (C) 2020-2026, Yegor Mialyk. All Rights Reserved.
//
// Licensed under the MIT License. See the LICENSE file for details.
//

using System.Runtime.CompilerServices;

namespace My.CancellationTokenScope;

public sealed class CancellationTokenScope : IDisposable
{
    private static readonly ConditionalWeakTable<object, CancellationTokenScope> scopeInstances = new();
    private static readonly AsyncLocal<object?> ambientCancellationTokenScopeIdHolder = new();

    private readonly object _instanceId = new();

    private readonly CancellationTokenScope? _parentScope;
    private bool _disposed;
    private readonly CancellationToken _cancellationToken;

    public CancellationTokenScope(CancellationToken cancellationToken)
    {
        _parentScope = GetAmbientScope();

        _cancellationToken = cancellationToken;

        SetAmbientScope(this);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        RemoveAmbientScope();

        SetAmbientScope(_parentScope);

        _disposed = true;
    }

    private static void RemoveAmbientScope()
    {
        var instanceIdentifier = ambientCancellationTokenScopeIdHolder.Value;

        ambientCancellationTokenScopeIdHolder.Value = null;

        if (instanceIdentifier is not null)
            scopeInstances.Remove(instanceIdentifier);
    }

    private static CancellationTokenScope? GetAmbientScope()
    {
        var instanceIdentifier = ambientCancellationTokenScopeIdHolder.Value;

        return instanceIdentifier is null || !scopeInstances.TryGetValue(instanceIdentifier, out var scope)
            ? null
            : scope;
    }

    public static CancellationToken CancellationToken =>
        GetAmbientScope()?._cancellationToken ?? CancellationToken.None;

    private static void SetAmbientScope(CancellationTokenScope? newAmbientScope)
    {
        if (newAmbientScope is null || ambientCancellationTokenScopeIdHolder.Value == newAmbientScope._instanceId)
            return;

        ambientCancellationTokenScopeIdHolder.Value = newAmbientScope._instanceId;

        scopeInstances.GetValue(newAmbientScope._instanceId, _ => newAmbientScope);
    }
}
