// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.MessagePack.Tests;

public static class AquaMessagePackExtensions
{
    extension(DateTime)
    {
#if NETFRAMEWORK
        public static DateTime UnixEpoch => new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#endif // NETFRAMEWORK
    }
}
