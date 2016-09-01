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
        private Button scan_hue;  //search hue button
        private List<string> hueItems;
               
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Set resource layout
            SetContentView(Resource.Layout.HueConnect);

            //map resources
            scan_hue = FindViewById<Button>(Resource.Id.conn_hue);
            hueListView = FindViewById<ListView>(Resource.Id.displayHue);
            instext = FindViewById<TextView>(Resource.Id.instructions);


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
                Console.Write("The Json payload is "); Console.WriteLine(json);
                //[{ "id":"001788fffe27e619","internalipaddress":"192.168.1.232"},{ "id":"001788fffe1a58eb","internalipaddress":"192.168.1.29"}]
                //string TEST_JSON = "[{ "id":"001788fffe27e619","internalipaddress":"192.168.1.232"},{ "id":"001788fffe1a58eb","internalipaddress":"192.168.1.29"}]";
                /*Using substring approach for extracting Id's and IP addresses*/
                int countHueHub = 0;
                for (int i = 0; i < json.Length; i++)
                {
                    if (json[i] == '}')
                        countHueHub++;
                }
                Console.Write("The hub count is "); Console.WriteLine(countHueHub);

                /*No hues alert*/
                if(countHueHub == 0)
                {
                    //alert
                    var alertBegin = new AlertDialog.Builder(scan_hue.Context);
                    alertBegin.SetTitle("No hues found. Try again.");
                    alertBegin.SetPositiveButton("OK", (sender1, f) =>
                    {
                        return;
                    });
                    alertBegin.Show();
                }

                /*Take an int array to store the indices of { and }*/
                int []JSON_storeIndices = new int[countHueHub*2];
                int index = 0;
                for(int a=0; a<json.Length; a++)
                {
                    if((json[a] == '{') || (json[a] == '}'))
                    {
                        JSON_storeIndices[index] = a;
                        Console.Write(" Hit "); Console.WriteLine(JSON_storeIndices[index]);
                        index++;
                    }
                }
                

                /*Store strings*/
                string[] JSON_HueData = new string[countHueHub];
                int Hue_index = 0;
                for(int a=0; a<countHueHub; a++)
                {
                    JSON_HueData[a] = json.Substring(JSON_storeIndices[a+Hue_index], JSON_storeIndices[a+ Hue_index +1]);  //0 1 -- 1 2
                    Hue_index++;                                            //
                }
                
                /*Extract the fields from each JSONHueData*/
                string[] JSON_HUE_ID = new string[countHueHub];
                string[] JSON_HUE_IP = new string[countHueHub];
                int Hue_id_index, Hue_ip_index;
                
                for (int a=0; a<countHueHub; a++)
                {
                    Console.Write("Extracted payload is "); Console.WriteLine(JSON_HueData[a]);
                    Hue_id_index = JSON_HueData[a].IndexOf("id");  //this is not working??
                    Hue_ip_index = JSON_HueData[a].IndexOf("internalipaddress", 0);
                    //Console.WriteLine("Indices are: ");
                    Console.Write(Hue_id_index); Console.Write(Hue_ip_index);
                    Console.WriteLine("............................................................");
                    const int ID_chars = 5;
                    const int IP_chars = 20;
                    int b = Hue_id_index+ID_chars;
                    int c = Hue_ip_index + IP_chars;
                
                    //Extract ID
                    while(JSON_HueData[a][b] != '\"')
                    {
                        JSON_HUE_ID[a] += JSON_HueData[a][b];
                        b++;
                    }

                    //Extract IP
                    while(JSON_HueData[a][c] != '\"')
                    {
                        JSON_HUE_IP[a] += JSON_HueData[a][c];
                        c++;
                    }
                    
                    Console.WriteLine(JSON_HUE_IP[a]); Console.WriteLine(JSON_HUE_ID[a]);
                    Console.WriteLine("..................................END..........................");
                
                }

                //Make a list to store the IP's
                hueItems = new List<string>();
                for(int a=0; a<countHueHub; a++)
                {
                    hueItems.Add(JSON_HUE_IP[a]);
                }

                //Pass this list to the list adapter 
                ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, hueItems); //add the list to the adapter for display!!
                hueListView.Adapter = adapter;

                //Action on listview click
                hueListView.ItemClick += hueListView_ItemClick;

            });
        }

        //Listview click function
        private void hueListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Console.WriteLine(hueItems[e.Position]);  //this is the selected ip address
                                                      //pass this to the next activity -- tell user to press the button, creates user
            var intent = new Intent(this, typeof(HueCreateUser));
            intent.PutExtra("IP address", hueItems[e.Position]);
            StartActivity(intent);
        }
    }
}
