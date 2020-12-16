using System.Collections.Generic;
using System.Linq;

namespace Excubo.Blazor.Diagrams
{
    internal class Changes
    {
        private readonly Stack<ChangeAction> RedoStack = new Stack<ChangeAction>();
        private readonly Stack<ChangeAction> UndoStack = new Stack<ChangeAction>();
        public void Undo()
        {
            if (!UndoStack.Any())
            {
                return;
            }
            var action = UndoStack.Pop();
            action.Undo();
            RedoStack.Push(action);
        }
        public void Redo()
        {
            if (!RedoStack.Any())
            {
                return;
            }
            var action = RedoStack.Pop();
            action.Redo();
            UndoStack.Push(action);
        }
        public void New(ChangeAction change_action)
        {
            RedoStack.Clear();
            UndoStack.Push(change_action);
        }
        public void NewAndDo(ChangeAction change_action)
        {
            RedoStack.Clear();
            UndoStack.Push(change_action);
            change_action.Do();
        }
    }
}