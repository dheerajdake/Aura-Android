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

//hueItems = JsonConvert.DeserializeObject<List<Hue_Id>>(json);
//Extracting fields  //[{"id":"001788fffe1a68f4","internalipaddress":"10.0.0.15"}]
//XW's Extracting fields is [{"id":"001788fffe27e619","internalipaddress":"192.168.1.232"},{"id":"001788fffe1a58eb","internalipaddress":"192.168.1.29"}]

                Console.Write("The Json payload is "); Console.WriteLine(json);
// see how many hubs are there.
// for XW there are two, for the hackathon there were 6.
                int countHueHub = 0;
                for (int i = 0; i < json.Length; i++)
                {
                    if (json[i] == '}')
                    countHueHub++;
                }
// the hub count is 2 in XW's home
                Console.Write("The hub count is "); Console.WriteLine(countHueHub);


//print out all hue hubs one by one.
                string[] hueHubs = new string[countHueHub];
                int[] start_hue = new int[countHueHub];
                int[] len_hue = new int[countHueHub];
                int whichHueHub = 0;
                for (int i = 0; i < json.Length; i++)
                {
                    if (json[i] == '{')
                    {
                        start_hue[whichHueHub] = i;
                        Console.Write("start place "); Console.WriteLine(i);
                    }
                    if (json[i] == '}')
                    {
                        len_hue[whichHueHub] = i - start_hue[whichHueHub] + 1;
                        //Console.Write("len "); Console.WriteLine(i);
                        whichHueHub++;
                    }
                }
                for (int i = 0; i < countHueHub; i++)
                {
                    hueHubs[i] = json.Substring(start_hue[i], len_hue[i]);
                    Console.Write("hub is  "); Console.WriteLine(json.Substring(start_hue[i], len_hue[i]));
                }
//end of print all Hue hubs.

// HERE YOU NEED TO CHOOSE WHICH HUB YOU WANT TO USE
// FOR EXAMPLE YOU WANT TO USE THE FIRST HUB
                string hub = hueHubs[0];

                Console.Write("hub is  ");Console.WriteLine(hub);
//for example I use the first hue hub

//	var newhue = new Hue_Id { id = "001788fffe27e619", internalipaddress = "192.168.1.232" };
//	var newjson = JsonConvert.SerializeObject(newhue);

//    hueItems = JsonConvert.DeserializeObject< Hue_Id >(newjson);
//	Console.WriteLine("hueItems",newhue.id);
// using substring get id and ip
                int[] start_id_ip= new int[2];
                int[] len_id_ip = new int[2];

                int position = 0;

                for (int i = 0; i <hub.Length ; i++)
                {
                    if (hub[i] == '"')
                    {
                        position++;
        //start_hue[whichHueHub] = i;
        //Console.Write("start place "); Console.WriteLine(i);
                    }
                    if (position == 3)
                    start_id_ip[0] = i + 1;
                    else if (position == 4)
                    len_id_ip[0] = i - start_id_ip[0];
                    else if (position == 7)
                    start_id_ip[1] = i + 1;
                    else if (position == 8)
                    len_id_ip[1] = i - start_id_ip[1];
    
                }

                hueItems.id = hub.Substring(start_id_ip[0], len_id_ip[0]);
                hueItems.internalipaddress = hub.Substring(start_id_ip[1], len_id_ip[1]);
                Console.WriteLine(len_id_ip[0]);
                Console.WriteLine(len_id_ip[1]);


                mAdapter = new HueAdapter(this, Resource.Layout.HueConnect, hueItems);

                //HueAdapter adapter = new HueAdapter(this, hueItems);
                //ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, hueItems);
                //hueListView.Adapter = mAdapter;
            });
        }
    }
}