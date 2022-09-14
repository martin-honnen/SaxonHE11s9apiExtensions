////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2022 Martin Honnen 
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Uses the executable form of Saxon HE 11 Java which is an open source product from Saxonica https://saxonica.com/
// 

using javax.xml.transform.stream;
using net.sf.saxon.s9api;

using URL = java.net.URL;
using URI = java.net.URI;
using Source = javax.xml.transform.Source;

namespace net.liberty_development.SaxonHE11s9apiExtensions
{
    static public class S9ApiExtensions
    {
        public static URI ToURI(this Uri uri)
        {
            return new URI(uri.AbsoluteUri);
        }

        public static Uri ToUri(this URI uri)
        {
            return new Uri(uri.toASCIIString());
        }
        public static Source AsSource(this string sourceString)
        {
            return new StreamSource(new java.io.StringReader(sourceString));
        }
        public static Source AsSource(this string sourceString, string systemId)
        {
            return new StreamSource(new java.io.StringReader(sourceString), systemId);
        }
        public static Source AsSource(this Uri uri)
        {
            return new StreamSource(new URI(uri.AbsoluteUri).toASCIIString());
        }

        public static XdmNode Build(this DocumentBuilder docBuilder, Uri uri)
        {
            return docBuilder.build(uri.AsSource());
        }

        public static XdmNode Build(this DocumentBuilder docBuilder, FileInfo file)
        {
            return docBuilder.build(new java.io.File(file.FullName));
        }
        public static XsltExecutable Compile(this XsltCompiler compiler, Uri uri)
        {
            return compiler.compile(uri.AsSource());
        }

        public static XsltExecutable Compile(this XsltCompiler compiler, FileInfo file)
        {
            return compiler.compile(new Uri(file.FullName).AsSource());
        }

        public static XQueryExecutable Compile(this XQueryCompiler compiler, Uri uri)
        {
            return compiler.compile(new URL(uri.AbsoluteUri).openStream());
        }

        public static XQueryExecutable Compile(this XQueryCompiler compiler, FileInfo file)
        {
            return compiler.compile(new java.io.File(file.FullName));
        }

        public static Serializer NewSerializer(this Processor processor, FileInfo file)
        {
            return processor.newSerializer(new java.io.File(file.FullName));
        }

        public static Serializer NewSerializer(this Processor processor, TextWriter textWriter)
        {
            return processor.newSerializer(new DotNetWriter(textWriter));
        }

        public static Serializer NewSerializer(this Processor processor, Stream stream)
        {
            return processor.newSerializer(new DotNetOutputStream(stream));
        }

    }
}