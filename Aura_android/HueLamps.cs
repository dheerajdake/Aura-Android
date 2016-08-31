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
using Multiselect;
using Android.Content.Res;
using System.Net;

namespace Aura_android
{
    [Activity(Label = "HueLamps")]
    public class HueLamps : Activity
    {
        private Button btn_HueLamps;
        private ListView disp_Hues;
        string Hue_User, IP_ADDR;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.HueLamps);

            disp_Hues = FindViewById<ListView>(Resource.Id.disp_HueLights);
            btn_HueLamps = FindViewById<Button>(Resource.Id.btn_hueLamps);

            //get Hue_user
            Hue_User = Intent.GetStringExtra("HUE username") ?? "User not created";
            IP_ADDR = Intent.GetStringExtra("IP address") ?? "IP address not found";

            //Make the GET call
            //http://<bridge ip address>/api/1028d66426293e821ecfd9ef1a0731df/lights
            string GET_lights = "http://" + IP_ADDR + "/api/" + Hue_User + "/lights";
            Console.WriteLine(GET_lights);

            WebClient client = new WebClient();
            Uri uri = new Uri(GET_lights);

            string response = client.DownloadString(uri);
            Console.WriteLine(response);
            findLights(response);
            displayLightsOnUI();
        }

        int hueLightCount = 0;
        void findLights(string light_string)
        {
            //int stateIndex = light_string.IndexOf("state");
            //Console.WriteLine("State index is "); Console.WriteLine(stateIndex);
            //using state as a token for counting lights
            for (int a = 0; a < light_string.Length-5; a++)
            {
                if (light_string.Substring(a, 5) == "state")  //state is a 5 letter word!
                {
                    hueLightCount++;
                    Console.Write("The light count is "); Console.WriteLine(hueLightCount);
                }
            }

            Console.Write("Total lights detected = "); Console.WriteLine(hueLightCount);
        }

        void displayLightsOnUI()
        {
            List<string> HueLightsDisp = new List<string>();

            //add the lights to this list
            for (int a = 1; a <= hueLightCount; a++)
            {
                string lightName = "HueLamp" + a;
                HueLightsDisp.Add(lightName);
            }

            //pass this list into the list adapter
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, HueLightsDisp); //add the list to the adapter for display!!
            disp_Hues.Adapter = adapter;

            //listView Onclick
            disp_Hues.ItemClick += dispHues_Listview_ItemClick;
        }

        private void dispHues_Listview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //get id's of the respective lights??  
        }
    }
}