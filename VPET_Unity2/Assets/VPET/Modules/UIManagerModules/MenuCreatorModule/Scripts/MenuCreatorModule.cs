using System;

namespace vpet
{
    public class MenuCreatorModule : UIManagerModule
    {
        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public MenuCreatorModule(string name, Core core) : base(name, core)
        {
            
        }

        protected override void Init(object sender, EventArgs e)
        { 
            MenuTree menu = new MenuTree()
               .Begin(MenuItem.IType.HSPLIT)
                   .Begin(MenuItem.IType.VSPLIT)
                       .Add(new Parameter<string>("This is a test string", "StringParameter"))
                       .Add(new Parameter<bool>(true, "BoolParameter"))
                   .End()
                   .Begin(MenuItem.IType.HSPLIT)
                       .Add(new Parameter<object>(null, "OK"))
                       .Add(new Parameter<object>(null, "Abort"))
                   .End()
              .End();
        
            menu.Items.ForEach(p => TraverseAndPrintTree(p, 0));
        }

        static void TraverseAndPrintTree(MenuItem item, int level)
        {
            if (item.Parameter != null)
                Helpers.Log(new string(' ', level * 6) + "- " + item.Parameter.name.ToString() + " | " + item.Type.ToString());
            else
                Helpers.Log(new string(' ', level * 6) + "- " + item.Type.ToString());

            level++;
            item.Children.ForEach(p => TraverseAndPrintTree(p, level));
        }
    }
}
