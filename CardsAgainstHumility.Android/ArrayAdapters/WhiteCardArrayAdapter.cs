using Android.Content;
using Android.Views;
using Android.Widget;

using CardsAgainstHumility.GameClasses;
using CardsAgainstHumility.Android.UIHelpers;

using System;
using System.Collections.Generic;
using System.Linq;

namespace CardsAgainstHumility.Android.ArrayAdapters
{
    public class WhiteCardArrayAdapter : ArrayAdapter<WhiteCard>
    {
        List<WhiteCard> _list { get; set; }
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
            if (list == null)
                list = new List<WhiteCard>();
            _list = list;
            _onClickAction = OnClickAction;
        }

        public bool NewData(List<WhiteCard> _newList)
        {
            if (_newList == null)
                return false;
            lock (_list)
            {
                int changes = 0;
                bool completelyReplaced = false;

                if (_newList.Count == 0)
                {
                    changes = _list.Count;
                    _list.Clear();
                    completelyReplaced = true;
                }
                else
                {
                    // Remove all cards that aren't in the new list
                    changes += _list.Count(c => !_newList.Select(d => d.Id).Contains(c.Id));
                    if (changes > 1)
                        completelyReplaced = true;
                    _list.RemoveAll(d => !_newList.Select(c => c.Id).Contains(d.Id));

                    // Add all the cards that aren't in the old list
                    var toAdd = _newList.Where(d => !_list.Select(c => c.Id).Contains(d.Id));
                    changes += toAdd.Count();
                    _list.AddRange(toAdd);
                }

                // Notify that the data set has changed
                if (changes > 0)
                    NotifyDataSetChanged();

                return completelyReplaced;
            }
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
                UIAssets.PrepareWhiteCard(v, item);
            }

            return v;
        }
    }
}