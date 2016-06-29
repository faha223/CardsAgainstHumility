using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CardsAgainstHumility.Android.Controls;
using CardsAgainstHumility.Android.UIHelpers;
using Android.Graphics;

namespace CardsAgainstHumility.Android.ArrayAdapters
{
    public class SelectionListArrayAdapter : ArrayAdapter<SelectableItem>
    {
        List<SelectableItem> _list { get; set; }

        public override int Count
        {
            get
            {
                if (_list == null)
                    return 0;
                return _list.Count();
            }
        }

        public SelectionListArrayAdapter(Context context, List<SelectableItem> list) : base(context, Resource.Layout.WhiteCard)
        {
            if (list == null)
                list = new List<SelectableItem>();
            _list = list;
        }

        public void NewData(List<SelectableItem> _newList)
        {
            // Properties may have changed. Reload the whole list.
            lock (_list)
            {
                _list.Clear();
                if (_newList != null)
                    _list.AddRange(_newList);
            }

            // Notify that the data set has changed
            NotifyDataSetChanged();
            NotifyDataSetInvalidated();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View v = convertView;
            if (v == null)
            {
                v = ((LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService)).Inflate(Resource.Layout.SelectableItem, null);
            }
            if (_list.Count() > position)
            {
                var item = _list.ElementAt(position);
                var txtText = v.FindViewById<TextView>(Resource.Id.is_Text);
                var cb = v.FindViewById<CheckBox>(Resource.Id.is_Selected);

                txtText.Text = item.Text;
                cb.Checked = item.IsSelected;
                cb.Tag = position;

                if (UIAssets.AppFont != null)
                    txtText.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);

                cb.Click += (sender, args) =>
                {
                    var cbox = (sender as CheckBox);
                    var selectableItem = _list.ElementAt((int)cb.Tag);
                    selectableItem.IsSelected = cbox.Checked;
                };
            }

            return v;
        }
    }
}