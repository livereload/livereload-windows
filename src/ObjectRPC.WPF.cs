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

    class TreeViewFacet : Facet<TreeView>
    {
        public TreeViewFacet(Entity entity, TreeView obj)
            : base(entity, obj)
        {
        }

        public IList<object> Data
        {
            set
            {
                var items = obj.Items;
                items.Clear();
                foreach (var itemDataRaw in value)
                {
                    var itemData = (Dictionary<string, object>)itemDataRaw;
                    string id = (string)itemData["id"];
                    string text = (string)itemData["text"];

                    var tvi = new TreeViewItem();
                    tvi.Header = text;
                    items.Add(tvi);
                }
            }
        }
   }

    public static class UIFacets
    {
        public static void Register(RootEntity rpc)
        {
            rpc.Register(typeof(TextBlock), typeof(TextBlockFacet));
            rpc.Register(typeof(TreeView), typeof(TreeViewFacet));
        }
    }
}
