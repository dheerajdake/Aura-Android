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

namespace Aura_android
{
    class HueAdapter : BaseAdapter<Hue_Id>  //BaseAdapter is an abstract member and have to 
                                               //implement its members
    {
        private List<Hue_Id> mItems;
        private int mLayout;
        private Context mContext;
        
        public HueAdapter(Context context, int layout, List<Hue_Id> items)
        {
            mItems = items;
            mLayout = layout;
            mContext = context;
        }

        public override int Count
        {
            get { return mItems.Count; }        //returns the items in the list
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override Hue_Id this[int position]
        {
            get {return mItems[position]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;
            if(row == null)
            {
                row = LayoutInflater.From(mContext).Inflate(Resource.Layout.HueConnect, null, false);
            }
            return row;
        }
    }
}