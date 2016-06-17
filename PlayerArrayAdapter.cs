using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using CardsAgainstHumility.GameClasses;
using System.Net;
using Android.Graphics;
using static Android.Views.View;

namespace CardsAgainstHumility
{
    public class PlayerArrayAdapter : ArrayAdapter<Player>
    {
        List<Player> _list { get; set; }
        private Typeface tf;
        private Action<Player, View> _onClickAction;

        public override int Count
        {
            get
            {
                if (_list == null)
                    return 0;
                return _list.Count();
            }
        }

        public PlayerArrayAdapter(Context context, Action<Player, View> OnClickAction, List<Player> list) : base(context, Resource.Layout.WhiteCard)
        {
            if (list == null)
                list = new List<Player>();
            _list = list;
            _onClickAction = OnClickAction;
            try
            {
                tf = Typeface.CreateFromAsset(context.Assets, "Helvetica-Bold.ttf");
            }
            catch { }
        }

        public void NewData(List<Player> _newList)
        {
            if (_newList == null)
                return;
            lock (_list)
            {
                if (_newList.Count == 0)
                {
                    _list.Clear();
                }
                else
                {
                    // Remove all cards that aren't in the new list
                    _list.RemoveAll(d => !_newList.Select(c => c.Id).Contains(d.Id));

                    // Add all the cards that aren't in the old list
                    var toAdd = _newList.Where(d => !_list.Select(c => c.Id).Contains(d.Id));
                    _list.AddRange(toAdd);
                }
            }

            // Notify that the data set has changed
            NotifyDataSetChanged();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View v = convertView;
            if (v == null)
            {
                v = ((LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService)).Inflate(Resource.Layout.Player, null);
            }
            if (_list.Count() > position)
            {
                var item = _list.ElementAt(position);
                var txtName = v.FindViewById<TextView>(Resource.Id.p_Name);
                txtName.Text = item.Name;
                if (tf != null)
                {
                    txtName.SetTypeface(tf, TypefaceStyle.Normal);
                }
            }

            return v;
        }
    }
}