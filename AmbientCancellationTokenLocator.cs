//
// Ambient Cancellation Token Library
//
// Copyright (C) 2020-2026, Yegor Mialyk. All Rights Reserved.
//
// Licensed under the MIT License. See the LICENSE file for details.
//

namespace My.CancellationTokenScope;

public class AmbientCancellationTokenLocator : IAmbientCancellationTokenLocator
{
    public CancellationToken Get() => CancellationTokenScope.CancellationToken;

    public IDisposable Set(CancellationToken cancellationToken) => new CancellationTokenScope(cancellationToken);
}
