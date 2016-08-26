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
using System.ComponentModel;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aura_android
{
    [Activity(Label = "HueConnect")]
    public class HueConnect : Activity
    {
        private ListView hueListView;
        private TextView instext;
        private List<Hue_Id> hueItems;
        private BaseAdapter<Hue_Id> mAdapter;
        private Button scan_hue;  //search hue button
               
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Set resource layout
            SetContentView(Resource.Layout.HueConnect);

            //map resources
            scan_hue = FindViewById<Button>(Resource.Id.conn_hue);
            hueListView = FindViewById<ListView>(Resource.Id.displayHue);
            instext = FindViewById<TextView>(Resource.Id.instructions);

            //hueItems = new List<Hue_Id>();

            //Add code for button click -- Make a REST API call. This should be an async task
            scan_hue.Click += (sender, e) =>
            {
                WebClient client = new WebClient();
                Uri uri = new Uri("https://www.meethue.com/api/nupnp");

                client.DownloadDataAsync(uri);                         //handle REST services on a seperate thread
                client.DownloadDataCompleted += display_Hue;          //what to do after download
            };
        }

        /*Hue Scan on a seperate thread*/
        void display_Hue(object sender, DownloadDataCompletedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                string json = Encoding.UTF8.GetString(e.Result);
                //hueItems = JsonConvert.DeserializeObject<List<Hue_Id>>(json);
                //Extracting fields  //[{"id":"001788fffe1a68f4","internalipaddress":"10.0.0.15"}]

                Console.Write("The Json payload is "); Console.WriteLine(json);

                //Console.Write("The count is ");
                //Console.WriteLine(hueItems);

                mAdapter = new HueAdapter(this, Resource.Layout.HueConnect, hueItems);

                //HueAdapter adapter = new HueAdapter(this, hueItems);
                //ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, hueItems);
                //hueListView.Adapter = mAdapter;
            });
        }
    }
}