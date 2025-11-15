// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests;

public class TestException : Exception
{
    public TestException(string message)
        : base(message)
    {
    }
}