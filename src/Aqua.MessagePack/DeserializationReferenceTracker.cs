// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack;

using Aqua.TypeExtensions;

internal sealed class DeserializationReferenceTracker : IDeserializationReferenceTracker
{
    private readonly Dictionary<int, object> _registry = [];

    public void Register<T>(T value, int id) where T : class
    {
        if (id == default)
        {
            return;
        }

        lock (_registry)
        {
            _registry[id] = value;
        }
    }

    public T Resolve<T>(int id) where T : class
    {
        if (id == default)
        {
            throw new MessagePackSerializationException($"Reference {id} is not valid");
        }

        lock (_registry)
        {
            if (!_registry.TryGetValue(id, out var value))
            {
                throw new MessagePackSerializationException($"Reference {id} is not registered");
            }

            if (value is null)
            {
                return null!;
            }

            if (value is T t)
            {
                return t;
            }

            throw new InvalidCastException($"Reference #{id} is expected to by of tyope {typeof(T).GetFriendlyName()} but was {value.GetType().GetFriendlyName()}");
        }
    }
}
