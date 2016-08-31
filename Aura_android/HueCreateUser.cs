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
using System.Net;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Aura_android
{
    [Activity(Label = "HueCreateUser")]
    public class HueCreateUser : Activity
    {
        private TextView disp_createUser;
        private Button btn_createUser, btn_next;
        
        //strings
        public string Hue_User = null;   //initialization is important, else compiler error
        public string lights_JSON = null;
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


            //Set resource layout
            SetContentView(Resource.Layout.HueCreateUser);

            //map resources
            disp_createUser = FindViewById<TextView>(Resource.Id.text_pressBtn);
            btn_createUser = FindViewById<Button>(Resource.Id.btn_createuser);
            btn_next = FindViewById<Button>(Resource.Id.btn_next);

            //extract the IP address from the previous activity
            string IP_ADDR = Intent.GetStringExtra("IP address") ?? "IP not available";
            Console.Write("IP address is "); Console.WriteLine(IP_ADDR);


            //build the url with the IP
            string CREATE_USER_URL = "http://" + IP_ADDR + "/api";
            Console.Write("The url is "); Console.WriteLine(CREATE_USER_URL);

            //body for the request
            string PostBody = "{\"devicetype\":\"Aura#MotoX Dheeraj\"}";  //{"devicetype":"my_hue_app#iphone peter"}
            
            //Button 1
            //code for creating user
            //Add code for button click -- Make a REST API call. This should be an async task
            btn_createUser.Click += (sender, e) =>
            {
                WebClient client = new WebClient();
         
                //Headers
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
                client.Headers.Add(HttpRequestHeader.Accept, "application/json, text/javascript, */*; q=0.01");
                
                //POST request
                byte[] dataBytes = Encoding.UTF8.GetBytes(PostBody);
                client.UploadDataAsync(new Uri(CREATE_USER_URL), "POST" ,dataBytes);
                client.UploadDataCompleted += Hue_Create_User;
            };

            //Button 2
            //next button
            btn_next.Click += (sender, e) =>
            {
                if (Hue_User == null)
                {
                    var alertBegin = new AlertDialog.Builder(btn_createUser.Context);
                    alertBegin.SetTitle("Please press the connect and try again");
                    alertBegin.SetPositiveButton("OK", (sender1, f) =>
                    {
                        return;
                    });
                    alertBegin.Show();
                }
                else
                {
                    var intent = new Intent(this, typeof(HueLamps));
                    intent.PutExtra("HUE username", Hue_User);  //passing the lights JSON string to the next activity
                    intent.PutExtra("IP address", IP_ADDR);
                    StartActivity(intent);
                }
            };
        }

        void Hue_Create_User(object sender, UploadDataCompletedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                string json = Encoding.UTF8.GetString(e.Result);
                Console.WriteLine(json);
                
                int check_response = json.IndexOf("success");
                if(check_response<0) //[{"error":{"type":101,"address":"","description":"link button not pressed"}}]
                {
                    Console.WriteLine("Please press the button and try again");
                    var alertBegin = new AlertDialog.Builder(btn_createUser.Context);
                    alertBegin.SetTitle("Please press the button and try again");
                    alertBegin.SetPositiveButton("OK", (sender1, f) =>
                    {
                        return;
                    });
                    alertBegin.Show();
                }
                else//[{ "success":{ "username":"3IeqgOkkHJofdJvi8lhJB7VRP9uPf1ba3IoFx3Dh"} }]
                {
                    int index_username = json.IndexOf("username");
                    const int username_chars = 11;

                    int a = index_username + username_chars;
                    while(json[a] != '\"')
                    {
                        Hue_User += json[a];
                        a++;
                    }

                    Console.Write("USERNAME IS "); Console.WriteLine(Hue_User);
                    var alertBegin = new AlertDialog.Builder(btn_createUser.Context);
                    alertBegin.SetTitle("You are connected!");
                    alertBegin.SetPositiveButton("OK", (sender1, f) =>
                    {
                        return;
                    });
                    alertBegin.Show();
                }
            });
        }//end button download event   
    }
}