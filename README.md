# <img src=Spark/Spark.png width=48 height=48/> Spark
#### A Launcher for Dark Ages Clients

#### Summary
Spark is a graphical application that provides additional options when launching <a href="http://www.darkages.com">Dark Ages</a> clients. These options include redirecting the client to a custom server hostname/port, skipping the intro video, allowing multiple instances, and hiding foreground walls.

#### Screenshot

<img src="http://imgur.com/u0U4ZcO.png" width=456 height=617/>

#### Download (Binaries)

The latest Spark binaries are available on Zolian's Discord. Please keep in mind that they may not reflect the latest code changes here.

You will need the <a href="https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.0-windows-x64-installer">.NET 8</a> to run Spark.

#### Language and Framework
Spark is a WPF application written in C# using Visual Studio 2022. It targets .NET 8 and implements a simple MVVM (Model-View-ViewModel) architecture.

#### Client Version Support
Spark currently supports <strong>version 7.41</strong> of the official Dark Ages client. It also support Zolian 9.13, and Legends 7.18. You may edit the generated <strong>Versions.xml</strong> file to support additional client versions.

#### Developers

Spark is designed to be both a useful application as well as an example codebase for numerous .NET topics:

<ul>
<li>Writing a WPF application.</li>
<li>Implementing the MVVM pattern in a WPF application.</li>
<li>Showing modal dialogs using the MVVM pattern.</li>
<li>Creating custom value converters for data bindings.</li>
<li>Theming an application via custom control templates and styles.</li>
<li>Using LINQ to simplify business logic within an application.</li>
<li>Using LINQ to serialize and deserialize XML files.</li>
<li>Calling native Win32 APIs from Managed (C#) code.</li>
</ul>
