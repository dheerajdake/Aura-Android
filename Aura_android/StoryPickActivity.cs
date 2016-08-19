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
    [Activity(Label = "StoryPickActivity")]
    public class StoryPickActivity : Activity
    {
        private List<string> genre;
        private ListView genre_pick;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set the view from "StoryPick" layout resource
            SetContentView(Resource.Layout.StoryPick);

            //Assign resources
            genre_pick = FindViewById<ListView>(Resource.Id.listview_stories);

            //Instantiate genre
            genre = new List<string>();
            genre.Add("Demo");
            genre.Add("Superhero");
            genre.Add("Morals");
            genre.Add("Fables");
            genre.Add("Custom");

            Console.WriteLine("Items added to the list");

            //ArrayAdapter for display
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, genre); //add the list to the adapter for display!!
            genre_pick.Adapter = adapter;

            Console.WriteLine("Display adapter");

            //Action on listview click
            genre_pick.ItemClick += Genre_pick_ItemClick;
        }

        private void Genre_pick_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Console.WriteLine("List view clicked!");
            Console.WriteLine(genre[e.Position]);    //this gives the item in that particular list

            if(genre[e.Position] == genre[0])
            {
                Console.WriteLine(genre[0]);
                var narrate_intent = new Intent(this, typeof(NarrateActivity));
                StartActivity(narrate_intent);
            }
        }
    }
}