# Saxon HE 11 s9api .NET extension method helpers
Extension methods that help/ease the task of using Saxon HE 11 Java s9api from .NET code

This is a sample project outlining my successful attempt to apply https://github.com/ikvm-revived/ikvm and
https://github.com/ikvm-revived/ikvm-maven to use the open-source Saxon HE 11 Java XSLT 3.0, XQuery 3.1 and XPath 3.1 library in .NET 6 code.

Please understand that this is my own experiment, it uses the official Saxon HE 11 release from Maven, but the integration with IKVM and IKVM Maven is an experimental work of my own, not in any way an officially tested and supported product by Saxonica, the company that has produced Saxon.

So feel free to try and use it under the Mozilla Public License 2.0. 

Understand that this is work in progress and kind of experimental, I don't have access to a complete test suite of unit tests to rigorously test this, I nevertheless feel it can be useful for folks to at least know about this option to run [XSLT 3.0](https://www.w3.org/TR/xslt-30/), [XPath 3.1](https://www.w3.org/TR/xpath-31/) and [XQuery 3.1](https://www.w3.org/TR/xquery-31/) in .NET 6 code.

To use Saxon under .NET, the coding is mainly done against the Java s9api API of Saxon HE 11 although I have provided some extension methods as helpers to ease the task of using .NET FileInfo or Stream instead of needing to know about and use Java specific java.io.File or Stream classes/APIs.

Known issues: I have created the project with VS 2022 Community Edition on Windows, apps built that way could be deployed and run successfully under Linux or Mac where the dotnet .NET 6 runtime is installed; however, the https://github.com/ikvm-revived/ikvm-maven does seem to work on a Mac, so in experiments of your own you will probably be restricted to develop and build on Windows.

The basic usage is to to install the NuGet package IKVM.Maven.Sdk to be able to pull in the Saxon HE 11 (e.g. 11.4) and the XmlResolver code it uses directly from Maven:
```
  <ItemGroup>
    <PackageReference Include="IKVM.Maven.Sdk" Version="1.2.0" />
    <MavenReference Include="net.sf.saxon:Saxon-HE" version="11.4" />
    <MavenReference Include="org.xmlresolver:xmlresolver" Version="4.5.1" />
    <MavenReference Include="org.xmlresolver:xmlresolver" Category="data" Version="4.5.1" />
  </ItemGroup>
```

This extension project is also on NuGet so you can add it in your project e.g.

```
  <ItemGroup>
    <PackageReference Include="IKVM.Maven.Sdk" Version="1.2.0" />
    <PackageReference Include="SaxonHE11s9apiExtensions" Version="11.4.0.3" />
    <MavenReference Include="net.sf.saxon:Saxon-HE" version="11.4" />
    <MavenReference Include="org.xmlresolver:xmlresolver" Version="4.5.1" />
    <MavenReference Include="org.xmlresolver:xmlresolver" Category="data" Version="4.5.1" />
  </ItemGroup>
```

Then you are ready to write .NET 6 code against the Saxon 11 s9api API, helped by this extension library to not have to use most Java classes for input/output/URIs/URLs but to be able to use relevant .NET classes:

```
using net.sf.saxon.s9api;
using net.liberty_development.SaxonHE11s9apiExtensions;
using System.Reflection;

// force loading of updated xmlresolver
ikvm.runtime.Startup.addBootClassPathAssembly(Assembly.Load("org.xmlresolver.xmlresolver"));
ikvm.runtime.Startup.addBootClassPathAssembly(Assembly.Load("org.xmlresolver.xmlresolver_data"));

var processor = new Processor(false);

Console.WriteLine($"{processor.getSaxonEdition()} {processor.getSaxonProductVersion()}");

var xslt30Transformer = processor.newXsltCompiler().Compile(new Uri("https://github.com/martin-honnen/martin-honnen.github.io/raw/master/xslt/processorTestHTML5Xslt3InitialTempl.xsl")).load30();

xslt30Transformer.callTemplate(null, processor.NewSerializer(Console.Out));
```



