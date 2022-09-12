﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2022 Martin Honnen 
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// adapted from https://saxonica.plan.io/projects/saxonmirrorhe/repository/he/revisions/he-saxon10_6/entry/src/main/java/net/sf/saxon/dotnet/DotNetOutputStream.java
// which is 
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using JOutputStream = java.io.OutputStream;

namespace SaxonHE11s9apiExtensions
{

    /**
    * A Java OutputStream implemented as a wrapper around a .NET stream
    */
    public class DotNetOutputStream : JOutputStream
    {
        Stream stream;

        public DotNetOutputStream(Stream stream)
        {
            this.stream = stream;
        }

        /**
         * Writes the specified byte to this output stream. The general
         * contract for <code>write</code> is that one byte is written
         * to the output stream. The byte to be written is the eight
         * low-order bits of the argument <code>b</code>. The 24
         * high-order bits of <code>b</code> are ignored.
         * <p>Subclasses of <code>OutputStream</code> must provide an
         * implementation for this method.</p>
         *
         * @param b the <code>byte</code>.
         * @throws java.io.IOException if an I/O error occurs. In particular,
         *                             an <code>IOException</code> may be thrown if the
         *                             output stream has been closed.
         */

        public override void write(int b) //throws IOException
        {
            stream.WriteByte((byte)b);
        }

        /**
         * Writes <code>len</code> bytes from the specified byte array
         * starting at offset <code>off</code> to this output stream.
         * The general contract for <code>write(b, off, len)</code> is that
         * some of the bytes in the array <code>b</code> are written to the
         * output stream in order; element <code>b[off]</code> is the first
         * byte written and <code>b[off+len-1]</code> is the last byte written
         * by this operation.
         * <p>The <code>write</code> method of <code>OutputStream</code> calls
         * the write method of one argument on each of the bytes to be
         * written out. Subclasses are encouraged to override this method and
         * provide a more efficient implementation.</p>
         * <p>If <code>b</code> is <code>null</code>, a
         * <code>NullPointerException</code> is thrown.</p>
         * <p>If <code>off</code> is negative, or <code>len</code> is negative, or
         * <code>off+len</code> is greater than the length of the array
         * <code>b</code>, then an <tt>IndexOutOfBoundsException</tt> is thrown.</p>
         *
         * @param b   the data.
         * @param off the start offset in the data.
         * @param len the number of bytes to write.
         * @throws java.io.IOException if an I/O error occurs. In particular,
         *                             an <code>IOException</code> is thrown if the output
         *                             stream is closed.
         */

        public override void write(byte[] b, int off, int len) //throws IOException
        {
            stream.Write(b, off, len);
        }

        /**
         * Writes <code>b.length</code> bytes from the specified byte array
         * to this output stream. The general contract for <code>write(b)</code>
         * is that it should have exactly the same effect as the call
         * <code>write(b, 0, b.length)</code>.
         *
         * @param b the data.
         * @throws java.io.IOException if an I/O error occurs.
         * @see java.io.OutputStream#write(byte[], int, int)
         */

        public override void write(byte[] b) //throws IOException
        {
            stream.Write(b, 0, b.Length);
        }

        /**
         * Flushes this output stream and forces any buffered output bytes
         * to be written out. The general contract of <code>flush</code> is
         * that calling it is an indication that, if any bytes previously
         * written have been buffered by the implementation of the output
         * stream, such bytes should immediately be written to their
         * intended destination.
         * <p>If the intended destination of this stream is an abstraction provided by
         * the underlying operating system, for example a file, then flushing the
         * stream guarantees only that bytes previously written to the stream are
         * passed to the operating system for writing; it does not guarantee that
         * they are actually written to a physical device such as a disk drive.</p>
         * <p>The <code>flush</code> method of <code>OutputStream</code> does nothing.</p>
         *
         * @throws java.io.IOException if an I/O error occurs.
         */

        public override void flush() //throws IOException
        {
            stream.Flush();
        }

        /**
         * Closes this output stream and releases any system resources
         * associated with this stream. The general contract of <code>close</code>
         * is that it closes the output stream. A closed stream cannot perform
         * output operations and cannot be reopened.
         * <p>The <code>close</code> method of <code>OutputStream</code> does nothing.</p>
         *
         * @throws java.io.IOException if an I/O error occurs.
         */

        public override void close() //throws IOException
        {
            stream.Close();
        }
    }
}
