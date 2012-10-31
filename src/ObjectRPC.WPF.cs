using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using D = System.Collections.Generic.Dictionary<string, object>;

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

    class ButtonFacet : Facet<Button>
    {
        public ButtonFacet(Entity entity, Button obj)
            : base(entity, obj)
        {
            obj.Click += OnClick;
        }

        public string Label
        {
            set
            {
                obj.Content = value;
            }
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            entity.SendUpdate(new Dictionary<string, object> { {"click", true } });
        }
    }

    class TreeViewFacet : Facet<TreeView>
    {
        private IList<object> data;

        public TreeViewFacet(Entity entity, TreeView obj)
            : base(entity, obj)
        {
        }

        public IList<object> Data
        {
            set
            {
                data = value;

                var items = obj.Items;
                items.Clear();
                foreach (var itemDataRaw in data)
                {
                    var itemData = (Dictionary<string, object>)itemDataRaw;
                    string id = (string)itemData["id"];
                    string text = (string)itemData["text"];

                    var tvi = new TreeViewItem();
                    tvi.Header = text;
                    tvi.Tag = id;
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
            rpc.Register(typeof(Button), typeof(ButtonFacet));
            rpc.Register(typeof(TreeView), typeof(TreeViewFacet));
        }
    }
}
