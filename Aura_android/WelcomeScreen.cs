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
    //THIS SHOWS UP FIRST, MainLauncher is set to true. 
    [Activity(Label = "WelcomeScreen", MainLauncher = true, Icon = "@drawable/icon")]
    public class WelcomeScreen : Activity
    {
        private TextView welcome_details;
        private Button welcome_next;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "welcome" layout resource
            SetContentView(Resource.Layout.Welcome);

            //Get the resources we created
            welcome_details = FindViewById<TextView>(Resource.Id.welcome_content);
            welcome_next = FindViewById<Button>(Resource.Id.welcome_next);

            //Code for button click
            welcome_next.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(HueConnect));
                StartActivity(intent);
            };

            //end
        }
    }
}