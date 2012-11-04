using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using D = System.Collections.Generic.Dictionary<string, object>;

namespace ObjectRPC.WPF
{
    class UIElementFacet : Facet<UIElement>
    {
        public UIElementFacet(Entity entity, UIElement obj)
            : base(entity, obj)
        {
        }

        public bool Visible
        {
            set { obj.Visibility = value ? Visibility.Visible : Visibility.Hidden; }
        }
        public bool Enable
        {
            set { obj.IsEnabled = value; }
        }
    }

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

    class CheckBoxFacet : Facet<CheckBox>
    {
        public CheckBoxFacet(Entity entity, CheckBox obj)
            : base(entity, obj)
        {
            obj.Click += OnClick;
        }

        public string Label
        {
            set { obj.Content = value; }
        }

        public bool Value
        {
            set { obj.IsChecked = value; }
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            entity.SendUpdate(new Dictionary<string, object> { { "value", obj.IsChecked } });
        }
    }

    class TextBoxFacet : Facet<TextBox>
    {
        private bool isBeingChangedByUser = false;
        private bool isBeingChangedProgramatically = false;

        public TextBoxFacet(Entity entity, TextBox obj)
            : base(entity, obj)
        {
            obj.TextChanged += OnTextChanged;
            obj.LostFocus   += OnLostFocus;
        }

        public string Text
        {
            set
            {
                if (!isBeingChangedByUser)
                {
                    isBeingChangedProgramatically = true;
                    obj.Text = value;
                    isBeingChangedProgramatically = false;
                }
            }
        }

        private void OnTextChanged(object sender, RoutedEventArgs e)
        {
            if (!isBeingChangedProgramatically)
            {
                isBeingChangedByUser = true;
                entity.SendUpdate(new Dictionary<string, object> { { "text", obj.Text } });
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            isBeingChangedByUser = false;
        }
    }


    class TreeViewFacet : Facet<TreeView>
    {
        private IList<object> data;
        private bool isTreeViewUpdateInProgress = false;
        
        public TreeViewFacet(Entity entity, TreeView obj)
            : base(entity, obj)
        {
            obj.SelectedItemChanged += OnSelectedItemChanged;
        }

        public string SelectedId
        {
            get
            {
                var selectedTVI = (TreeViewItem)obj.SelectedItem;
                return (string)((selectedTVI != null) ? selectedTVI.Tag : null);
            }
        }

        public IList<object> Data
        {
            set
            {
                data = value;

                var items = obj.Items;

                TreeViewItem newSelectedTVI = null;

                isTreeViewUpdateInProgress = true;
                string oldSelectedId = SelectedId;
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

                    if (id == oldSelectedId)
                    {
                        newSelectedTVI = tvi;
                    }
                }

                if (oldSelectedId != null)
                    if (newSelectedTVI == null )
                    {
                        isTreeViewUpdateInProgress = false;
                        OnSelectedItemChanged(null, null); // need to reset view
                    }
                    else
                    {
                        SelectItem(newSelectedTVI);
                        isTreeViewUpdateInProgress = false;
                    }
                else
                    isTreeViewUpdateInProgress = false;
            }
        }

        private void SelectItemHelper(TreeViewItem item) // unneeded ATM, retest when we will have tree depth > 1
        {
            if (item == null)
                return;
            SelectItemHelper((TreeViewItem)item.Parent);
            if (!item.IsExpanded)
            {
                item.IsExpanded = true;
                item.UpdateLayout();
            }
        }
        private void SelectItem(TreeViewItem item) // QND solution
        {
            SelectItemHelper(item.Parent as TreeViewItem);
            item.IsSelected = true;
        }

        private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!isTreeViewUpdateInProgress)
                entity.SendUpdate(new Dictionary<string, object> { { "selectedId", SelectedId } });
        }
    }

    public static class UIFacets
    {
        public static void Register(RootEntity rpc)
        {
            rpc.Register(typeof(UIElement), typeof(UIElementFacet));
            rpc.Register(typeof(TextBlock), typeof(TextBlockFacet));
            rpc.Register(typeof(Button), typeof(ButtonFacet));
            rpc.Register(typeof(TreeView), typeof(TreeViewFacet));
            rpc.Register(typeof(CheckBox), typeof(CheckBoxFacet));
            rpc.Register(typeof(TextBox), typeof(TextBoxFacet));
        }
    }
}
