using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;

using CardsAgainstHumility.GameClasses;
using CardsAgainstHumility.Android.UIHelpers;

using System;
using System.Collections.Generic;
using System.Linq;

namespace CardsAgainstHumility.Android.ArrayAdapters
{
    public class GameInstanceArrayAdapter : ArrayAdapter<GameInstance>
    {
        IEnumerable<GameInstance> _list { get; set; }

        private Action<GameInstance> _onJoinButtonClick;

        public override int Count
        {
            get
            {
                if (_list == null)
                    return 0;
                return _list.Count();
            }
        }
        public GameInstanceArrayAdapter(Context context, Action<GameInstance> OnJoinButtonClick, IEnumerable<GameInstance> list) : base(context, Resource.Layout.GameInstance)
        {
            _list = list;
            _onJoinButtonClick = OnJoinButtonClick;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View v = convertView;
            if (v == null)
            {
                v = ((LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService)).Inflate(Resource.Layout.GameInstance, null);
            }
            if (_list.Count() > position)
            {
                var item = _list.ElementAt(position);
                var btnJoin = v.FindViewById<Button>(Resource.Id.gi_Join);
                if (item.Players >= item.MaxPlayers)
                    btnJoin.Text = "Full";
                else
                {
                    btnJoin.Click += delegate
                    {
                        if (_onJoinButtonClick != null)
                            _onJoinButtonClick.Invoke(item);
                    };
                }
                btnJoin.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                v.FindViewById<TextView>(Resource.Id.gi_Name).Text = item.Name;
                v.FindViewById<TextView>(Resource.Id.gi_Name).SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                v.FindViewById<TextView>(Resource.Id.gi_Players).Text = string.Format("{0}/{1}", item.Players, item.MaxPlayers);
                v.FindViewById<TextView>(Resource.Id.gi_Players).SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
            }

            return v;
        }
    }
}