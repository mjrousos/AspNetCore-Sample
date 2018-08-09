ASPNET Core Sample Of Customers
===

This is an application using a customer data model written with ASP.NET Core.
There are several projects including REST apis, data models, middleware and an
MVC front end. Many ASP.NET Core features are showcased and documented in the
CustomersAPI project. The CustomersMVC project shows a few additional
MVC-specific features.

Most code which relates to a particular feature area (configuration,
dependency injection, localization, logging, middleware, routing) has a
comment near it with the feature area as a keyword (like `Middleware: `).
You can search for these keywords (or even view them in Visual Studio's
task list, by adding them to the token list at Tools > Options > Task List) to
find parts of the sample related to feature areas you'd like to see more
about.

Pre-Requisites
---
This sample uses [.NET Core 2.1](https://www.microsoft.com/net/download/core).
To build and run it, you will need the [.NET Core SDK](https://www.microsoft.com/net/download/core).
You will also need an editor like [Visual Studio 2017](https://www.visualstudio.com/vs/)
or [Visual Studio Code](https://code.visualstudio.com).

The docker-compose project requires [Docker-for-Windows](https://www.docker.com/docker-windows).
If you will be using the docker-compose project, make sure [Docker CE](https://store.docker.com/editions/community/docker-ce-desktop-windows)
is installed on your development machine.

Resources
---

For more information on ASP.NET Core, see:

* [official documentation](https://docs.microsoft.com/aspnet/core/), the
* [ASP.NET team blog](https://blogs.msdn.microsoft.com/webdev/), or the
* [GitHub repo](https://github.com/aspnet/Home)

Code Style Requirements
---

The code style requirements are enforced with
[EditorConfig](https://docs.microsoft.com/en-us/visualstudio/ide/create-portable-custom-editor-options)
via `.editorconfig` and
[StyleCop Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) via
`CodeStyling.rulset`. As a general rule we "use Visual Studio defaults".

1. We use [Allman style](http://en.wikipedia.org/wiki/Indent_style#Allman_style)
   braces, where each brace begins on a new line.
2. We use four spaces of indentation (no tabs).
3. We use `_camelCase` for internal and private fields and use `readonly` where
   possible. Prefix instance fields with `_`, static fields with `s_` and thread
   static fields with `t_`. When used on static fields, `readonly` should come
   after `static` (i.e. `static readonly` not `readonly static`).
4. We avoid `this.` unless absolutely necessary.
5. We always specify the visibility, even if it's the default (i.e. `private
   string _foo` not `string _foo`). Visibility should be the first modifier
   (i.e. `public abstract` not `abstract public`).
6. Namespace imports should be specified at the top of the file, *outside* of
   `namespace` declarations and should be sorted alphabetically.
7. Avoid more than one empty line at any time. For example, do not have two
   blank lines between members of a type.
8. Avoid spurious free spaces.
   For example avoid `if (someVar == 0)...`, where the dots mark the spurious free spaces.
   Consider enabling "View White Space (Ctrl+E, S)" if using Visual Studio, to aid detection.
9. We use language keywords instead of BCL types (i.e. `int, string, float`
   instead of `Int32, String, Single`, etc) for both type references as well as
   method calls (i.e. `int.Parse` instead of `Int32.Parse`). See issue
   [391](https://github.com/dotnet/corefx/issues/391) for examples.
10. We use PascalCasing to name all our constant local variables and fields. The
    only exception is for interop code where the constant value should exactly
    match the name and value of the code you are calling via interop.
11. We use ```nameof(...)``` instead of ```"..."``` whenever possible and
    relevant.
12. Fields should be specified at the top within type declarations.
13. When including non-ASCII characters in the source code use Unicode escape
    sequences (\uXXXX) instead of literal characters. Literal non-ASCII
    characters occasionally get garbled by a tool or editor.

In order to enforce these styles, We use `Directory.Build.props` and
`Directory.Build.targets` files, which are imported implicitly by MSBuild
version 15.0 or later. More information can be found at MSBuild doc site
[Customize your build](https://docs.microsoft.com/en-us/visualstudio/msbuild/customize-your-build)
