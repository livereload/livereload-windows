using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace ObjectRPC.WPF
{
    class TextBlockFacet : Facet<TextBlock>
    {
        public TextBlockFacet(Entity entity, TextBlock obj)
            : base(entity, obj)
        {
        }

        public string Text
        {
            set
            {
                obj.Text = value;
            }
        }
    }

    public static class UIFacets
    {
        public static void Register(RootEntity rpc)
        {
            rpc.Register(typeof(TextBlock), typeof(TextBlockFacet));
        }
    }
}
