## Excubo.Blazor.Diagrams

![Nuget](https://img.shields.io/nuget/v/Excubo.Blazor.Diagrams)
![Nuget](https://img.shields.io/nuget/dt/Excubo.Blazor.Diagrams)
![GitHub](https://img.shields.io/github/license/excubo-ag/Blazor.Diagrams)

Excubo.Blazor.Diagrams is a native-Blazor diagram component library.

![Ready to install?](screenshot.png)

[Demo on github.io using Blazor Webassembly](https://excubo-ag.github.io/Blazor.Diagrams/)

## Key features

- Adding/Moving/Removing of nodes
- Moving/Removing groups of nodes (select multiple nodes by drawing a region (-> press `[shift]`), or by adding/removing individual nodes by clicking them while holding `[ctrl]`)
- Adding/Modifying/Removing links (including shape of curve for CurvedLink!)
- Undo/Redo with `[Ctrl]+[z]` (undo) and `[Ctrl]+[Shift]+[z]` / `[Ctrl]+[y]`(redo)
- Panning/Zooming
- Default link connection ports by position (North, NorthEast, East,...)
- Custom nodes/links
- Node library (fully customizable) for adding new nodes
- Change shape and arrows for links
- Overview screen for easy navigation on large diagrams
- Customizable background (grid lines, color, any style you want)

## How to use

Using Excubo.Blazor.Diagrams doesn't require any difficult installation. You need to install it and use it, that's it:

### 1. Install the nuget package Excubo.Blazor.Diagrams

Excubo.Blazor.Diagrams is distributed [via nuget.org](https://www.nuget.org/packages/Excubo.Blazor.Diagrams/).
![Nuget](https://img.shields.io/nuget/v/Excubo.Blazor.Diagrams)

#### Package Manager:
```ps
Install-Package Excubo.Blazor.Diagrams -Version 1.0.1
```

#### .NET Cli:
```cmd
dotnet add package Excubo.Blazor.Diagrams --version 1.0.1
```

#### Package Reference
```xml
<PackageReference Include="Excubo.Blazor.Diagrams" Version="1.0.1" />
```

### 2. Add the `Diagram` component to your component

```html
@using Excubo.Blazor.Diagrams

<Diagram @ref="Diagram">
    <Nodes>
        <Node Id="abc" X="500" Y="500">
            Hello node @context.Id
        </Node>
        <Node Id="def" X="1000" Y="500">
            Hello node @context.Id
        </Node>
    </Nodes>
    <Links>
    </Links>
</Diagram>
```

This is of course only a minimalistic example.
For more examples, have a look at [the sample project](https://github.com/excubo-ag/Blazor.Diagrams/tree/master/TestProject_Components), which is the basis for the [demo application](https://excubo-ag.github.io/Blazor.Diagrams/).

## Design principles

- Extensibility

Users get a set of built-in node shapes and link types, but can expand this freely and with minimal amount of effort by adding their own shapes.

- Blazor API

The API should feel like you're using Blazor, not a javascript library.

- Minimal js, minimal css, lazy-loaded only when you use the component

The non-C# part of the code of the library should be as tiny as possible. We set ourselves a maximum amount of 10kB for combined js+css.
The current payload is less than 100 bytes, and only gets loaded dynamically when the component is actually used.

## How to design a custom node

A complete example of how to design a custom node is available [here](https://github.com/excubo-ag/Blazor.Diagrams/blob/master/TestProject_Components/Pages/UserDefinedNode.razor).

## Roadmap

There are more features to come! Goals include:

- More node types
- auto-layout

If you want to contribute, simply get in touch, open an issue, or open a pull request!
