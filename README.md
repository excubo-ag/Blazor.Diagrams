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

## How to use

### 1. Install the nuget package Excubo.Blazor.Diagrams

Excubo.Blazor.Diagrams is distributed [via nuget.org](https://www.nuget.org/packages/Excubo.Blazor.Diagrams/).
![Nuget](https://img.shields.io/nuget/v/Excubo.Blazor.Diagrams)

#### Package Manager:
```ps
Install-Package Excubo.Blazor.Diagrams -Version 0.9.0
```

#### .NET Cli:
```cmd
dotnet add package Excubo.Blazor.Diagrams --version 0.9.0
```

#### Package Reference
```xml
<PackageReference Include="Excubo.Blazor.Diagrams" Version="0.9.0" />
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
        <Link Source="link.Source" Target="link.Target" Arrow="Arrow.Target" />
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

Your node type has to inherit `NodeBase` to be compatible with the Diagram component.

A sample custom node is:

```html
@using Excubo.Blazor.Diagrams
@inherits NodeBase
<!--This is important to prohibit rendering of deleted, user-provided nodes.-->
@if (Deleted)
{
    return;
}
@using Excubo.Blazor.Diagrams.Extensions
<!--This using statement helps with locales that do not have the period as decimal separator: The DOM expects the period as decimal separator.-->
@using (var temporary_culture = new CultureSwapper())
{
    <!--The outer g is mandatory (takes care of scaling and correct placement for you)-->
    <g transform="@PositionAndScale">
        <!--Beginning of the customizable part-->
        <!--This defines the area which can be interacted with to select/move the node. -->
        <!--Mandatory: onmouseover="OnNodeOver" and onmouseout="OnNodeOut" -->
        <rect width="@Width"
              height="@Height"
              @onmouseover="OnNodeOver"
              @onmouseout="OnNodeOut"
              stroke="@Stroke"
              stroke-width="2px"
              fill="@Fill"
              cursor="move"
              style="@(Hidden? "display:none;" : "") @(Selected ? "stroke-dasharray: 8 2; animation: diagram-node-selected 0.4s ease infinite;" : "")" />
        <!--End of the customizable part-->
    </g>
}
@code {
    @*This part is essentially the same as the node above, except it's just the border. This is where links can be connected to. This does not need to be equivalent to the border, but can be any shape.*@
    public override RenderFragment border =>@<text>
        @using (var temporary_culture = new CultureSwapper())
        {
            <!--The outer g is mandatory (takes care of scaling and correct placement for you)-->
            <g transform="@PositionAndScale">
                <!--Beginning of the customizable part-->
                <!--This defines the area which can be interacted with to create links To debug this, set the stroke to a visible color. fill is set to none so that only the border is interactive -->
                <!--Mandatory: onmouseover="OnBorderOver" and onmouseout="OnBorderOut" -->
                <rect width="@Width"
                      height="@Height"
                      style="@(Hidden? "display:none" : "")"
                      stroke="@(Hovered ? "#DDDDDD7F" : "transparent")"
                      stroke-width="@(.5 / Zoom)rem"
                      fill="none"
                      cursor="pointer"
                      @onmouseover="OnBorderOver"
                      @onmouseout="OnBorderOut" />
                <!--End of the customizable part-->
            </g>
        }
    </text>;
    @*This is optional, but if you want to define some default port, this is how you do it. Defaults to (0, 0).*@
    public override (double RelativeX, double RelativeY) GetDefaultPort(Position position = Position.Any)
    {
        return position switch
        {
            Position.North => (Width / 2, 0),
            Position.NorthEast => (Width, 0),
            Position.East => (Width, Height / 2),
            Position.SouthEast => (Width, Height),
            Position.South => (Width / 2, Height),
            Position.SouthWest => (0, Height),
            Position.NorthWest => (0, Height / 2),
            _ => (0, 0)
        };
    }
    @*The shape drawn might be larger than the rectangle from the X,Y position as top left corner with its width and height.
      The margins here (positive if the shape is larger than the rectangle) help draw the node correctly in the node library.
      There's no need to override this, if it's (0, 0, 0, 0).*@
    protected override (double Left, double Top, double Right, double Bottom) GetDrawingMargins()
    {
        return (0, 0, 0, 0);
    }
}
```

The same shape is defined twice nearly identically: The first definition is for the shape itself (as razor markup), the second definition is the invisible border where links can be connected to. This is defined in the code section, because the diagram component will put it in a dedicated layer. To make your shape work with the Diagram component, you need to at least have the four onmouseover/out callbacks registered, as well as the outer `g` with the transform as displayed above.

## Roadmap

This is an early release of Excubo.Blazor.Diagrams.

Longer term goals include:

- More node types
- auto-layout
- customizable background with gridlines
