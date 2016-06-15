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
    public class WhiteCardArrayAdapter : ArrayAdapter<WhiteCard>
    {
        List<WhiteCard> _list { get; set; }
        private Typeface tf;
        private Action<WhiteCard, View> _onClickAction;

        public override int Count
        {
            get
            {
                if (_list == null)
                    return 0;
                return _list.Count();
            }
        }

        public WhiteCardArrayAdapter(Context context, Action<WhiteCard, View> OnClickAction, List<WhiteCard> list) : base(context, Resource.Layout.WhiteCard)
        {
            _list = list;
            _onClickAction = OnClickAction;
            try
            {
                tf = Typeface.CreateFromAsset(context.Assets, "Helvetica-Bold.ttf");
            }
            catch { }
        }

        public void NewData(List<WhiteCard> _newList)
        {
            // Remove all cards that aren't in the new list
            _list.RemoveAll(d => !_list.Select(c => c.Id).Contains(d.Id));

            // Add all the cards that aren't in the old list
            var toAdd = _newList.Where(d => !_list.Select(c => c.Id).Contains(d.Id));
            _list.AddRange(toAdd);

            // Notify that the data set has changed
            NotifyDataSetChanged();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View v = convertView;
            if (v == null)
            {
                v = ((LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService)).Inflate(Resource.Layout.WhiteCard, null);
            }
            if (_list.Count() > position)
            {
                var item = _list.ElementAt(position);
                var txtText = v.FindViewById<TextView>(Resource.Id.wc_CardText);
                if (tf != null)
                {
                    var txtlogo = v.FindViewById<TextView>(Resource.Id.wc_logo_text);
                    txtText.SetTypeface(tf, TypefaceStyle.Normal);
                    txtlogo.SetTypeface(tf, TypefaceStyle.Normal);
                }
                txtText.Text = WebUtility.HtmlDecode(item.Text);
                txtText.SetTextSize(Android.Util.ComplexUnitType.Dip, item.FontSize);

                v.Click += (sender, args) =>
                {
                    _onClickAction?.Invoke(item, v);
                };
            }

            return v;
        }
    }
}