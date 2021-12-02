using System.Collections;
using System.Collections.Generic;

namespace vpet
{
    public class MenuTree
    {
        private Stack<MenuItem> m_Stack = new Stack<MenuItem>();
        public List<MenuItem> Items { get; } = new List<MenuItem>();

        public MenuTree Begin(MenuItem.IType type)
        {
            return Begin(type, null);
        }

        public MenuTree Begin(AbstractParameter param)
        {
            return Begin(MenuItem.IType.PARAMETER, param);
        }

        private MenuTree Begin(MenuItem.IType type, AbstractParameter param = null)
        {
            if (m_Stack.Count == 0)
            {
                MenuItem item = new MenuItem(type, param, null);
                Items.Add(item);
                m_Stack.Push(item);
            }
            else
            {
                MenuItem item = m_Stack.Peek().Add(type, param);
                m_Stack.Push(item);
            }

            return this;
        }

        public MenuTree Add(MenuItem.IType type)
        {
            return Add(type, null);
        }

        public MenuTree Add(AbstractParameter param)
        {
            return Add(MenuItem.IType.PARAMETER, param);
        }

        private MenuTree Add(MenuItem.IType type, AbstractParameter param = null)
        {
            m_Stack.Peek().Add(type, param);
            return this;
        }

        public MenuTree End()
        {
            m_Stack.Pop();
            return this;
        }
    }

    public class MenuItem
    {
        public enum IType
        {
            HSPLIT, VSPLIT, PARAMETER
        }
        public IType Type { get; }
        public AbstractParameter Parameter { get; }
        public MenuItem Parent { get; }
        public List<MenuItem> Children { get; }

        public MenuItem(IType type, AbstractParameter param, MenuItem parent)
        {
            Type = type;
            Parameter = param;
            Parent = parent;
            Children = new List<MenuItem>();
        }

        public MenuItem Add(IType type, AbstractParameter param)
        {
            MenuItem item = new MenuItem(type, param, this);
            Children.Add(item);
            return item;
        }
    }

    public class Example
    {
        MenuTree menu = new MenuTree()
            .Begin(MenuItem.IType.HSPLIT)
                .Begin(MenuItem.IType.VSPLIT)
                    .Add(MenuItem.IType.PARAMETER, new Parameter<string>("This is a test string", "StringParameter"))
                    .Add(MenuItem.IType.PARAMETER, new Parameter<bool>(true, "BoolParameter"))
                .End()
                .Begin(MenuItem.IType.HSPLIT)
                    .Add(MenuItem.IType.PARAMETER, new Parameter<object>(null, "OK"))
                    .Add(MenuItem.IType.PARAMETER, new Parameter<object>(null, "Abort"))
                .End()
           .End();

        //tree.Nodes.ForEach(p => PrintNode(p, 0));

        void TraverseAndPrintTree(MenuItem item, int level)
        {
            Helpers.Log(new string(' ', level * 3) + " " + item.Parameter.ToString());
            level++;
            item.Children.ForEach(p => TraverseAndPrintTree(p, level));
        }
    }

}
