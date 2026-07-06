// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Protobuf;

using System.Runtime.CompilerServices;

public sealed class ReferenceTracker : IReferenceTracker
{
    private readonly ConditionalWeakTable<object, object> _references = new();

    public bool TryGet<TTarget>(object source, out TTarget? target)
        where TTarget : class
    {
        if (_references.TryGetValue(source, out var boxed))
        {
            target = (TTarget)boxed;
            return true;
        }

        target = null;
        return false;
    }

    public void Register<TTarget>(object source, TTarget target)
        where TTarget : class
        => _references.Add(source, target);
}

public interface IReferenceTracker;
