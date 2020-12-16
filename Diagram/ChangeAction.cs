using System;

namespace Excubo.Blazor.Diagrams
{
    internal class ChangeAction
    {
        public Action Do { get; private set; }
        public Action Redo => Do;
        public Action Undo { get; private set; }
        public ChangeAction(Action @do, Action undo)
        {
            Do = @do;
            Undo = undo;
        }
    }
}