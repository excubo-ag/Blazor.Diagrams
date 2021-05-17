namespace Excubo.Blazor.Diagrams
{
    internal enum ActionType
    {
        None,
        Pan,
        PanOrResetSelection,
        SelectRegion,
        Move,
        UpdateLinkTarget,
        ModifyLink,
        MoveControlPoint,
        MoveAnchor
    }
}