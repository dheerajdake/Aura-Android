using System;
using System.Collections.Generic;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System.Net;
//using Xamarin.Forms;

namespace Aura_android
{
    [Activity(Label = "HueLamps")]
    public class HueLamps : Activity
    {
        private Button btn_HueLamps;
        private ListView disp_Hues;
        private List<string> HueLightsDisp;
        public string []HueToggleLights;    //size is equal to huelightcount
        private int hueLightCount = 0;
        string Hue_User, IP_ADDR, GET_LIGHTS;
        private ToggleButton disp_Hue_switchs;// add toggle button

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.HueLamps);

            disp_Hues = FindViewById<ListView>(Resource.Id.disp_HueLights);
            // add toggle button
            disp_Hue_switchs = FindViewById<ToggleButton>(Resouce.Id.disp_HueLights);
            btn_HueLamps = FindViewById<Button>(Resource.Id.btn_hueLamps);

            //get Hue_user
            Hue_User = Intent.GetStringExtra("HUE username") ?? "User not created";
            IP_ADDR = Intent.GetStringExtra("IP address") ?? "IP address not found";

            //Make the GET call
            //http://<bridge ip address>/api/1028d66426293e821ecfd9ef1a0731df/lights
            GET_LIGHTS = "http://" + IP_ADDR + "/api/" + Hue_User + "/lights";
            
            Console.WriteLine(GET_LIGHTS);

            WebClient client = new WebClient();
            Uri uri = new Uri(GET_LIGHTS);

            string response = client.DownloadString(uri);
            Console.WriteLine(response);
            HueToggleLights = new string[findLights(response)];      //finds the lights
            displayLightsOnUI();   //displays them as a list on the UI
            selectAllLightsON();   //sets the state of lights to ON

            // set toggle button on and off
            disp_Hue_switchs.Click += (o, e) =>
            {
                if (disp_Hue_switchs.Checked)
                    // Perform action on clicks
                    if (togglebutton.Checked)
                        Toast.MakeText(this, "Checked", ToastLength.Short).Show();
                    else
                        Toast.MakeText(this, "Not checked", ToastLength.Short).Show();

            };// end of toggle


            //button click
            btn_HueLamps.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(StoryPickActivity));
                intent.PutStringArrayListExtra("Selected lamps", HueToggleLights);
                intent.PutExtra("HueLightCount", hueLightCount);
                intent.PutExtra("IP_ADDR", IP_ADDR);
                intent.PutExtra("HUE_USER", Hue_User);
                StartActivity(intent);
            };
        }

      
        int findLights(string light_string)
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
            return hueLightCount;
        }

        
        void displayLightsOnUI()
        {
            HueLightsDisp = new List<string>();
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
            //disp_Hues.ItemClick += OnSelection;
        }

        void selectAllLightsON()
        {
            //this toggles all the lights states to ON and saves that state
            for(int a=0; a<hueLightCount; a++)
            {
                HueToggleLights[a] = "ON";
                //string PUT_LightsON = "http://" + IP_ADDR + "/api/" + Hue_User + "/lights/" + (a + 1) + "/state";
                string PUT_LIGHTS = GET_LIGHTS + "/" + (a + 1) + "/state";
                controlLights(PUT_LIGHTS, "PUT", "{\"on\":true}");
                Console.Write("Light "); Console.Write(a+1); Console.WriteLine(" is ON");
            }
        }

        void controlLights(string URL, string METHOD, string BODY)
        {
            WebClient client = new WebClient();
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
            client.Headers.Add(HttpRequestHeader.Accept, "application/json, text/javascript, */*; q=0.01");
         
            //PUT request
            byte[] dataBytes = Encoding.UTF8.GetBytes(BODY);
            client.UploadDataAsync(new Uri(URL), METHOD, dataBytes);
        }
        //This requires Xamarin.Forms which is conflicting with Android widget button. Have to do a work around
        /*void OnSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
            }
            Console.WriteLine("Item Selected", e.SelectedItem.ToString(), "Ok");
            //((ListView)sender).SelectedItem = null; //uncomment line if you want to disable the visual selection state.
        }*/

        private void DisplayAlert(string v1, string v2, string v3)
        {
            //Use alers for now

        }

        private void dispHues_Listview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //get id's of the respective lights??  // can use the last index of the lamps for id
            //e.position will give the number
            //Do a PUT request to flash the lights //http://<bridge ip address>/api/1028d66426293e821ecfd9ef1a0731df/lights/1/state
            string PUT_LIGHTS = GET_LIGHTS + "/" + (e.Position + 1) + "/state";

            //**Using toggle to change the lights - By default all lights will be selected and will be on
            if (HueToggleLights[e.Position] == "ON")
            {
                //turn off the lights
                controlLights(PUT_LIGHTS, "PUT", "{\"on\":false}");

                HueToggleLights[e.Position] = "OFF";
            }
            else if(HueToggleLights[e.Position] == "OFF")
            {
                //turn on the lights
                controlLights(PUT_LIGHTS, "PUT", "{\"on\":true}");

                HueToggleLights[e.Position] = "ON";
            }
            Console.Write("Lights "); Console.Write(e.Position); Console.Write(" toggled "); Console.WriteLine(HueToggleLights[e.Position]);

            //**Lights
           /* string PUT_FLASH_LIGHTS_URL = "http://" + IP_ADDR + "/api/" + Hue_User + "/lights/" + (e.Position+1) + "/state";
            Console.WriteLine(PUT_FLASH_LIGHTS_URL);

            string PUT_FLASH_LIGHTS_BODY_ON = "{\"on\":true}";
            string PUT_FLASH_LIGHTS_BODY_OFF = "{\"on\":false}";
            WebClient client1 = new WebClient();
            WebClient client2 = new WebClient();*/

            /*Turning ON*/
            //Headers
            //client1.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
            //client1.Headers.Add(HttpRequestHeader.Accept, "application/json, text/javascript, */*; q=0.01");

            //POST request
            //byte[] dataBytes1 = Encoding.UTF8.GetBytes(PUT_FLASH_LIGHTS_BODY_ON);
            //client1.UploadDataAsync(new Uri(PUT_FLASH_LIGHTS_URL), "PUT", dataBytes1);

            /*Turning OFF*/
            //Headers
            //client2.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
            //client2.Headers.Add(HttpRequestHeader.Accept, "application/json, text/javascript, */*; q=0.01");

            //POST request
            //byte[] dataBytes2 = Encoding.UTF8.GetBytes(PUT_FLASH_LIGHTS_BODY_OFF);
            //client2.UploadDataAsync(new Uri(PUT_FLASH_LIGHTS_URL), "PUT", dataBytes2);
            //client.UploadDataCompleted += Hue_Create_User;  //Can do a success alert
        }
    }
}