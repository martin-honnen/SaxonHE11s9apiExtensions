////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2022 Martin Honnen 
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// adapted from https://saxonica.plan.io/projects/saxonmirrorhe/repository/he/revisions/he-saxon10_6/entry/src/main/java/net/sf/saxon/dotnet/DotNetWriter.java
// which is
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using JWriter = java.io.Writer;
using JException = java.lang.Exception;

namespace SaxonHE11s9apiExtensions
{

    /**
    * Implement a Java Writer that wraps a supplied .NET TextWriter
    */
    public class DotNetWriter : JWriter
    {
        private TextWriter textWriter;

        /**
         * Create a Java Writer that wraps a supplied .NET TextWriter
         *
         * @param writer the .NET TextWriter to be wrapped
         */

        public DotNetWriter(TextWriter writer)
        {
            this.textWriter = writer;
        }

        /**
         * Close the stream, flushing it first.  Once a stream has been closed,
         * further write() or flush() invocations will cause an IOException to be
         * thrown.  Closing a previously-closed stream, however, has no effect.
         *
         * @throws java.io.IOException If an I/O error occurs
         */
        public override void close() //throws IOException
        {
            try
            {
                textWriter.Close();
            }
            catch (JException e)
            {
                throw new IOException(e.getMessage());
            }
        }

        /**
         * Flush the stream.  If the stream has saved any characters from the
         * various write() methods in a buffer, write them immediately to their
         * intended destination.  Then, if that destination is another character or
         * byte stream, flush it.  Thus one flush() invocation will flush all the
         * buffers in a chain of Writers and OutputStreams.
         * <p>If the intended destination of this stream is an abstraction provided by
         * the underlying operating system, for example a file, then flushing the
         * stream guarantees only that bytes previously written to the stream are
         * passed to the operating system for writing; it does not guarantee that
         * they are actually written to a physical device such as a disk drive.</p>
         *
         * @throws java.io.IOException If an I/O error occurs
         */

        public override void flush() //throws IOException
        {
            try
            {
                textWriter.Flush();
            }
            catch (JException e)
            {
                throw new IOException(e.getMessage());
            }
        }

        /**
         * Write a portion of an array of characters.
         *
         * @param cbuf Array of characters
         * @param off  Offset from which to start writing characters
         * @param len  Number of characters to write
         * @throws java.io.IOException If an I/O error occurs
         */

        public override void write(char[] cbuf, int off, int len) //throws IOException
        {
            try
            {
                textWriter.Write(cbuf, off, len);
            }
            catch (JException e)
            {
                throw new IOException(e.getMessage());
            }
        }

        /**
         * Write a single character.  The character to be written is contained in
         * the 16 low-order bits of the given integer value; the 16 high-order bits
         * are ignored.
         * <p> Subclasses that intend to support efficient single-character output
         * should override this method.</p>
         *
         * @param c int specifying a character to be written.
         * @throws java.io.IOException If an I/O error occurs
         */
        public override void write(int c) //throws IOException
        {
            try
            {
                textWriter.Write((char)c);
            }
            catch (JException e)
            {
                throw new IOException(e.getMessage());
            }
        }

        /**
         * Write an array of characters.
         *
         * @param cbuf Array of characters to be written
         * @throws java.io.IOException If an I/O error occurs
         */

        public override void write(char[] cbuf) //throws IOException
        {
            try
            {
                textWriter.Write(cbuf);
            }
            catch (JException e)
            {
                throw new IOException(e.getMessage());
            }
        }

        /**
         * Write a string.
         *
         * @param str String to be written
         * @throws java.io.IOException If an I/O error occurs
         */

        public override void write(String str) //throws IOException
        {
            try
            {
                textWriter.Write(str);
            }
            catch (JException e)
            {
                throw new IOException(e.getMessage());
            }
        }
    }
}
