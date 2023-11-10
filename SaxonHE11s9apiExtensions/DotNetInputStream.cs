////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2022 Martin Honnen 
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// adapted from https://saxonica.plan.io/projects/saxonmirrorhe/repository/he/revisions/he-saxon10_6/entry/src/main/java/net/sf/saxon/dotnet/DotNetInputStream.java
// which is 
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using JInputStream = java.io.InputStream;

namespace net.liberty_development.SaxonHE11s9apiExtensions
{
    /// <summary>
    /// A Java InputStream implemented as a wrapper around a .NET stream
    /// </summary>
    public class DotNetInputStream : JInputStream
    {

        Stream stream;
        long currentOffset;
        long markedOffset = 0;

        /// <summary>
        /// Create DotNetInputStream wrapper from .NET Stream
        /// </summary>
        /// <param name="stream">System.IO.Stream</param>
        public DotNetInputStream(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Closes this output stream and releases any system resources
        /// associated with this stream. The general contract of <code>close</code>
        /// associated with this stream. The general contract of <code>close</code>
        /// 
        /// </summary>
        public override void close()
        {
            stream.Close();
        }

        /// <summary>
        /// Get the underlying .NET Stream object
        /// </summary>
        /// <returns>System.IO.Stream</returns>
        public Stream getUnderlyingStream()
        {
            return stream;
        }

        public override void mark(int readlimit)
        {
            markedOffset = currentOffset;
        }

        public override bool markSupported()
        {
            return stream.CanSeek;
        }

        /// <summary>
        /// Reads the next byte of data from the input stream. The value byte is
        /// returned as an<code> int</code> in the range<code>0</code> to
        /// <code>255</code>. If no byte is available because the end of the stream
        /// has been reached, the value <code>-1</code> is returned.This method
        /// blocks until input data is available, the end of the stream is detected,
        /// or an exception is thrown.
        /// 
        /// A subclass must provide an implementation of this method.
        /// </summary>
        /// <returns>the next byte of data, or <code>-1</code> if the end of the stream is reached.</returns>
        public override int read()
        {
            int i = stream.ReadByte();
            if (i != -1)
            {
                currentOffset++;
                return i;
            }
            else
            {
                return -1;
            }
        }

        public override int read(byte[] b, int off, int len)
        {
            int i = stream.Read(b, off, len);
            if (i > 0)
            {
                currentOffset += i;
                return i;
            }
            else
            {
                // EOF returns 0 in .NET, -1 in Java
                return -1;
            }
        }

        public override void reset()
        {
            currentOffset = markedOffset;
            stream.Seek(markedOffset, SeekOrigin.Begin);
        }
    }
}
