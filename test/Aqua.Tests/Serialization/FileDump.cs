﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization
{
    using System.IO;

    public static class FileDump
    {
        public static void Dump(this Stream stream, string path)
        {
            //using (var fileStream = File.OpenWrite(path))
            //{
            //    stream.Seek(0, SeekOrigin.Begin);
            //    stream.CopyTo(fileStream);
            //    fileStream.Flush();
            //}
        }
    }
}
