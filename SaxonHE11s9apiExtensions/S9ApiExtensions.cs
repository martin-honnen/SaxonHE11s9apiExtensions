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
    /// <summary>
    /// Static class holding a variety of extension methods to allow feeding .NET types like <c>string</c>, <c>System.IO.FileInfo</c> or
    /// <c>System.Uri</c> as the input to various Saxon s9api methods as well as using various .NET types like <c>System.IO.TextWriter</c>
    /// or <c>System.IO.FileInfo</c> or <c>System.IO.Stream</c> as the output.
    /// </summary>
    static public class S9ApiExtensions
    {
        /// <summary>
        /// This extension method converts a <paramref name="uri"/> into a <c>java.io.URI</c>
        /// as that type is needed for some s9api calls.
        /// </summary>
        /// <param name="uri">The .NET System.Uri to call the extension method on.</param>
        /// <returns>A Java java.io.URI needed for some s9api Java methods</returns>
        public static URI ToURI(this Uri uri)
        {
            return new URI(uri.AbsoluteUri);
        }

        /// <summary>
        /// <c>ToUri</c> converts the <paramref name="uri"/> it is called on into a System.Uri.
        /// </summary>
        /// <param name="uri">The java.io.URI to be converted.</param>
        /// <returns>System.Uri</returns>
        public static Uri ToUri(this URI uri)
        {
            return new Uri(uri.toASCIIString());
        }

        /// <summary>
        /// <c>AsSource</c> is an utility method needed to convert a .NET <c>string</c> containing XML markup
        /// into a <c>java.xml.transform.Source</c>, as that is one of the central types the s9api uses as its input.
        /// </summary>
        /// <param name="sourceString">A string containing XML markup.</param>
        /// <returns>A <c>java.xml.transform.Source</c></returns>
        public static Source AsSource(this string sourceString)
        {
            return new StreamSource(new java.io.StringReader(sourceString));
        }

        /// <summary>
        /// <c>AsSource</c> is an utility method needed to convert a .NET <c>string</c> containing XML markup
        /// into a <c>java.xml.transform.Source</c>, as that is one of the central types the s9api uses as its input.
        /// </summary>
        /// <param name="sourceString">A string containing XML markup.</param>
        /// <param name="systemId">A string with an absolute URL to serve as the system identifier.</param>
        /// <returns>A <c>java.xml.transform.Source</c></returns>
        public static Source AsSource(this string sourceString, string systemId)
        {
            return new StreamSource(new java.io.StringReader(sourceString), systemId);
        }

        /// <summary>
        /// Utility extension method to convert the <paramref name="uri"/> it is called on into a <c>java.xml.transform.Source</c>,
        /// the primary form of input most s9api methods take as an input.
        /// </summary>
        /// <param name="uri">This <c>System.Uri</c> to be converted.</param>
        /// <returns>A <c>java.xml.transform.Source</c></returns>
        public static Source AsSource(this Uri uri)
        {
            return new StreamSource(new URI(uri.AbsoluteUri).toASCIIString());
        }

        /// <summary>
        /// utility extension method that allows you to call <c>Build</c> on <c>DocumentBuilder</c> directly with a <paramref name="uri"/>
        /// to create a <c>net.sf.saxon.s9api.XdmNode</c>
        /// </summary>
        /// <param name="docBuilder">A <c>net.sf.saxon.s9api.DocumentBuilder</c> to call extension method on</param>
        /// <param name="uri">The <c>System.Uri</c> to build an Xdm tree from.</param>
        /// <returns>a <c>net.sf.saxon.s9api.XdmNode</c></returns>
        public static XdmNode Build(this DocumentBuilder docBuilder, Uri uri)
        {
            return docBuilder.build(uri.AsSource());
        }

        /// <summary>
        /// utility extension method that allows you to call <c>Build</c> on <c>DocumentBuilder</c> directly with a <paramref name="file"/>
        /// to create a <c>net.sf.saxon.s9api.XdmNode</c>.
        /// </summary>
        /// <param name="docBuilder">The <c>net.sf.saxon.s9api.DocumentBuilder</c> to call extension method on.</param>
        /// <param name="file">The <c>System.IO.FileInfo</c> to build an Xdm tree from.</param>
        /// <returns>A <c>net.sf.saxon.s9api.XdmNode</c></returns>
        public static XdmNode Build(this DocumentBuilder docBuilder, FileInfo file)
        {
            return docBuilder.build(new java.io.File(file.FullName));
        }


        /// <summary>
        /// Utility extension method to compile an XSLT stylesheet directly from a <paramref name="uri"/> <c>System.Uri</c>.
        /// </summary>
        /// <param name="compiler">The <c>net.sf.saxon.s9api.XsltCompiler</c> to call the extension method on.</param>
        /// <param name="uri">A <c>System.Uri</c> pointing to the XSLT stylesheet to be compiled.</param>
        /// <returns>A <c>net.sf.saxon.s9api.XsltExecutable</c></returns>
        public static XsltExecutable Compile(this XsltCompiler compiler, Uri uri)
        {
            return compiler.compile(uri.AsSource());
        }

        /// <summary>
        /// Utility extension method to compile an XSLT stylesheet directly from a <paramref name="file"/> <c>System.IO.FileInfo</c>.
        /// </summary>
        /// <param name="compiler">The <c>net.sf.saxon.s9api.XsltCompiler</c> to call the extension method on.</param>
        /// <param name="file">A <c>System.IO.File</c> holding the XSLT stylesheet to be compiled.</param>
        /// <returns>A <c>net.sf.saxon.s9api.XsltExecutable</c></returns>
        public static XsltExecutable Compile(this XsltCompiler compiler, FileInfo file)
        {
            return compiler.compile(new Uri(file.FullName).AsSource());
        }

        /// <summary>
        /// Utility extension method to compile XQuery code directly from a <paramref name="uri"/> <c>System.Uri</c>.
        /// </summary>
        /// <param name="compiler">The <c>net.sf.saxon.s9api.XsltCompiler</c> to call the extension method on.</param>
        /// <param name="uri">A <c>System.Uri</c> pointing to the XQuery code to be compiled.</param>
        /// <returns>A <c>net.sf.saxon.s9api.XQueryExecutable</c></returns>
        public static XQueryExecutable Compile(this XQueryCompiler compiler, Uri uri)
        {
            return compiler.compile(new URL(uri.AbsoluteUri).openStream());
        }

        /// <summary>
        /// Utility extension method to compile XQuery code directly from a <paramref name="file"/> <c>System.IO.FileInfo</c>.
        /// </summary>
        /// <param name="compiler">The <c>net.sf.saxon.s9api.XsltCompiler</c> to call the extension method on.</param>
        /// <param name="file">A <c>System.IO.FileInfo</c> holding the XQuery code to be compiled.</param>
        /// <returns>A <c>net.sf.saxon.s9api.XQueryExecutable</c></returns>
        public static XQueryExecutable Compile(this XQueryCompiler compiler, FileInfo file)
        {
            return compiler.compile(new java.io.File(file.FullName));
        }

        /// <summary>
        /// Creates a <c>net.sf.saxon.s9api.Serializer</c> directly from the <paramref name="file"/> <c>System.IO.FileInfo</c>.
        /// </summary>
        /// <param name="processor">The <c>net.sf.saxon.s9api.Processor</c> to call the extension method on.</param>
        /// <param name="file">A .NET <c>System.IO.FileInfo</c> to serve as the output of the newly created Serializer.</param>
        /// <returns>A <c>net.sf.saxon.s9api.Serializer</c></returns>
        public static Serializer NewSerializer(this Processor processor, FileInfo file)
        {
            return processor.newSerializer(new java.io.File(file.FullName));
        }

        /// <summary>
        /// Creates a <c>net.sf.saxon.s9api.Serializer</c> directly from the <paramref name="textWriter"/> <c>System.IO.TextWriter</c>.
        /// </summary>
        /// <param name="processor">The <c>net.sf.saxon.s9api.Processor</c> to call the extension method on.</param>
        /// <param name="textWriter">A .NET <c>System.IO.TextWriter</c> to serve as the output of the newly created Serializer.</param>
        /// <returns>A <c>net.sf.saxon.s9api.Serializer</c></returns>
        public static Serializer NewSerializer(this Processor processor, TextWriter textWriter)
        {
            return processor.newSerializer(new DotNetWriter(textWriter));
        }


        /// <summary>
        /// Creates a <c>net.sf.saxon.s9api.Serializer</c> directly from the <paramref name="stream"/> <c>System.IO.Stream</c>.
        /// </summary>
        /// <param name="processor">The <c>net.sf.saxon.s9api.Processor</c> to call the extension method on.</param>
        /// <param name="stream">A .NET <c>System.IO.Stream</c> to serve as the output of the newly created Serializer.</param>
        /// <returns>a <c>net.sf.saxon.s9api.Serializer</c></returns>
        public static Serializer NewSerializer(this Processor processor, Stream stream)
        {
            return processor.newSerializer(new DotNetOutputStream(stream));
        }

    }
}