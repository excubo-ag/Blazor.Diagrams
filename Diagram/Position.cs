using System;

namespace Excubo.Blazor.Diagrams
{
    [Flags]
    public enum Position
    {
        Any = 0,
        North = 1,
        East = 2,
        South = 4,
        West = 8,
        NorthEast = North | East,
        SouthEast = South | East,
        NorthWest = North | West,
        SouthWest = South | West,
        Top = North,
        Left = West,
        Right = East,
        Bottom = South,
        TopLeft = NorthWest,
        TopRight = NorthEast,
        BottomLeft = SouthWest,
        BottomRight = SouthEast,
        Center = 16
    }
}