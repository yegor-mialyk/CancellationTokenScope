//
// Ambient Cancellation Token Library
//
// Copyright (C) 2020-2026, Yegor Mialyk. All Rights Reserved.
//
// Licensed under the MIT License. See the LICENSE file for details.
//

namespace My.CancellationTokenScope;

public interface IAmbientCancellationTokenLocator
{
    CancellationToken Get();

    IDisposable Set(CancellationToken cancellationToken);
}
