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

        public override int Count
        {
            get
            {
                if (_list == null)
                    return 0;
                return _list.Count();
            }
        }

        public PlayerArrayAdapter(Context context, List<Player> list) : base(context, Resource.Layout.WhiteCard)
        {
            if (list == null)
                list = new List<Player>();
            _list = list;
            try
            {
                tf = Typeface.CreateFromAsset(context.Assets, "Helvetica-Bold.ttf");
            }
            catch { }
        }

        public void NewData(List<Player> _newList)
        {
            // Properties may have changed. Reload the whole list.
            lock (_list)
            {
                _list.Clear();
                if(_newList != null)
                    _list.AddRange(_newList);
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
                var isCzar = v.FindViewById(Resource.Id.p_isCzar);
                var ready = v.FindViewById<TextView>(Resource.Id.p_ready);
                var aPoints = v.FindViewById<TextView>(Resource.Id.p_aPoints);

                txtName.Text = item.Name;
                if (tf != null)
                {
                    txtName.SetTypeface(tf, TypefaceStyle.Normal);
                    ready.SetTypeface(tf, TypefaceStyle.Normal);
                    aPoints.SetTypeface(tf, TypefaceStyle.Normal);
                }

                ready.Visibility = ((item.IsReady && CardsAgainstHumility.GameStarted && CardsAgainstHumility.ReadyForReview) ? ViewStates.Visible : ViewStates.Invisible);
                isCzar.Visibility = (item.IsCardCzar ? ViewStates.Visible : ViewStates.Invisible);
                aPoints.Text = item.AwesomePoints.ToString();
            }

            return v;
        }
    }
}