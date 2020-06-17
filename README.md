## Excubo.Blazor.Diagrams

![Nuget](https://img.shields.io/nuget/v/Excubo.Blazor.Diagrams)
![Nuget](https://img.shields.io/nuget/dt/Excubo.Blazor.Diagrams)
![GitHub](https://img.shields.io/github/license/excubo-ag/Blazor.Diagrams)

Excubo.Blazor.Diagrams is a native-Blazor diagram component library. The project status is in alpha. API might still change considerably.

![Ready to install?](screenshot.png)

[Demo on github.io using Blazor Webassembly](https://excubo-ag.github.io/Blazor.Diagrams/)

## How to use

### 1. Install the nuget package Excubo.Blazor.Diagrams

Excubo.Blazor.Diagrams is distributed [via nuget.org](https://www.nuget.org/packages/Excubo.Blazor.Diagrams/).
![Nuget](https://img.shields.io/nuget/v/Excubo.Blazor.Diagrams)

#### Package Manager:
```ps
Install-Package Excubo.Blazor.Diagrams -Version 0.2.0
```

#### .NET Cli:
```cmd
dotnet add package Excubo.Blazor.Diagrams --version 0.2.0
```

#### Package Reference
```xml
<PackageReference Include="Excubo.Blazor.Diagrams" Version="0.2.0" />
```

### 2. Add the css and js to your `index.html` / `_Hosts.cshtml`

```html
<head>
    <!--your other code-->
    <script src="_content/Excubo.Blazor.Diagrams/script.js" type="text/javascript"></script>
</head>
```

### 3. Add the diagram service to your service collection

```cs
//using Excubo.Blazor.Diagrams;
services.AddDiagramServices();
```

### 4. Add the `Diagram` component to your component

```html
@using Excubo.Blazor.Diagrams

<Diagram @ref="Diagram">
    <!--The node library is where you can drag new nodes from. The style is fully customizable-->
    <NodeLibrary style="background-color: aliceblue; border: 1px solid blue;" Orientation="Orientation.Vertical">
        <!--Put any node you want in the library-->
        <RectangleNode>
            <!--Careful with node contents! There must be an area of the node where the node is draggable, so only a bare minimum of node content should receive pointer events-->
            <input style="margin:1em; pointer-events:visiblePainted" type="text" />
        </RectangleNode>
    </NodeLibrary>
    <Nodes DefaultType="NodeType.Ellipse" OnRemove="NodeRemoved">
        <!-- Adding a single node. As the node type is not specified, the type is taken from the default node type as defined in diagram's node collection. If that's missing, it defaults to Rectangle. In this case, we'll get an ellipse -->
        <Node Id="def" X="1000" Y="500">
            Hello node @context.Id
        </Node>
        <!-- Builtin node, node-type (i.e. shape) specified by Type property -->
        <Node @key="state" Id="@state.Id" X="state.X" Y="state.Y" Type="NodeType.Rectangle">
            State @context.Id
        </Node>
        <!-- Builtin node, node-type specified by strongly typed component -->
        <DiamondNode @key="decision.Id" Id="@decision.Id" X="decision.X" Y="decision.Y">
            <div style="color:green; width: 100px; height: 100px">Decision @decision.Id</div>
        </DiamondNode>
        <!-- Custom node, inherits from NodeBase. -->
        <UserNodeCode Id="abc" X="10" Y="20">
            Hello custom node
        </UserNodeCode>
    </Nodes>
    <!-- side note: maybe rename link to connector, as link seems to be a special tag, so auto-correct corrects Link to link all the time. -->
    <Links OnLinkModified="LinkModified" OnRemoveLink="LinkRemoved" BeforeRemoveLink="BeforeLinkRemoved" OnAddLink="LinkAdded" DefaultType="LinkType.Curved">
        <!-- Adding a single link. As the link type is not specified, the type is taken from the default link type as defined in diagram. If that's missing, it defaults to Straight. In this case, we'll get a curved link -->
        <Link Source="@(Diagram.GetAnchorTo("abc"))" Target="@(Diagram.GetAnchorTo("def"))" />
    </Links>
    <NavigationSettings Zoom="1" MinZoom=".1" MaxZoom="20" />
</Diagram>
```

## Design principles

- Extensibility

Users get a set of built-in node shapes and link types, but can expand this freely and with minimal amount of effort by adding their own shapes.

- Blazor API

The API should feel like you're using Blazor, not a javascript library.

- Minimal js, minimal css

The non-C# part of the code of the library should be as tiny as possible. We set ourselves a maximum amount of 10kB for combined js+css. The current payload is 833 bytes (unminified).

## How to design a custom node

Your node type has to inherit `NodeBase` to be compatible with the Diagram component.

A sample custom node is:

```html
@using Excubo.Blazor.Diagrams
@inherits NodeBase
@using Excubo.Blazor.Diagrams.Extensions
<!--This using statement helps with locales that do not have the period as decimal separator: The DOM expects the period as decimal separator.-->
@using (var temporary_culture = new CultureSwapper())
{
    <!--The outer g is mandatory (takes care of scaling and correct placement for you)-->
    <g transform="@NodePositionAndScale">
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
                style="@(Hidden? "display:none;" : "") @(Selected ? "stroke-dasharray: 8 2; animation: diagram-node-selected 0.4s ease infinite;" : "")" />
        <!--End of the customizable part-->
    </g>
}
@code {
    @*This part is essentially the same as the node above, except it's just the border. This is where links can be connected to. This does not need to be equivalent to the border, but can be any shape.*@
    public override RenderFragment node_border =>@<NodeBorder @ref="node_border_reference">
        @using (var temporary_culture = new CultureSwapper())
        {
            <!--The outer g is mandatory (takes care of scaling and correct placement for you)-->
            <g transform="@NodePositionAndScale">
                <!--Beginning of the customizable part-->
                <!--This defines the area which can be interacted with to create links To debug this, set the stroke to a visible color. fill is set to none so that only the border is interactive -->
                <!--Mandatory: onmouseover="OnBorderOver" and onmouseout="OnBorderOut" -->
                <rect width="@Width"
                        height="@Height"
                        style="@(Hidden? "display:none" : "")"
                        stroke="@(Hovered ? "#DDDDDD7F" : "transparent")"
                        stroke-width="@(.5 / Zoom)rem"
                        fill="none"
                        @onmouseover="OnBorderOver"
                        @onmouseout="OnBorderOut" />
                <!--End of the customizable part-->
            </g>
        }
    </NodeBorder>;
    @*This is optional, but if you want to define some default port, this is how you do it. Defaults to (0, 0).*@
    public override (double RelativeX, double RelativeY) GetDefaultPort()
    {
        return (0, 0);
    }
}
```

The same shape is defined twice: The second definition is for the shape itself, the first definition is the invisible border where links can be connected to. To make your shape work with the Diagram component, you need to at least have the four onmouseover/out callbacks registered, as well as the outer `g` with the transform as displayed above.

## Roadmap

This is an early alpha release of Excubo.Blazor.Diagrams.

- Editability of links
- Links with arrows
- More node types
    - image node
    - common shapes (e.g. DB icon)

Longer term goals include

- auto-layout
- undo/redo for modifications
- customizable background with gridlines
- virtualization
- overview screen

## Known issues

- Not all events are implemented

