using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Speech;
using Android.Media;
using System.Collections.Generic;

namespace Aura_android
{
    //[Activity(Label = "Aura_android", MainLauncher = true, Icon = "@drawable/icon")]
    [Activity(Label = "Aura_android")]
    public class MainActivity : Activity
    {
        private bool isRecording;
        private readonly int VOICE = 10;
        private TextView textBox;
        private Button recButton;

        MediaPlayer _player;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            //set the IsRecording flag to false
            isRecording = false;

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            //get resources from the layout
            recButton = FindViewById<Button>(Resource.Id.btnRecord);
            textBox = FindViewById<TextView>(Resource.Id.textYourText);

            // check to see if we can actually record - if we can, assign the event to the button
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                // no microphone, no recording. Disable the button and output an alert
                var alert = new AlertDialog.Builder(recButton.Context);
                alert.SetTitle("You don't seem to have a microphone to record with");
                alert.SetPositiveButton("OK", (sender, e) =>
                {
                    textBox.Text = "No microphone present";
                    recButton.Enabled = false;
                    return;
                });

                alert.Show();
            }
            else
                recButton.Click += delegate
                {
                    // change the text on the button
                    Console.WriteLine("Breakpoint ...");
                    recButton.Text = "End Recording";
                    isRecording = !isRecording;
                    if (isRecording)
                    {
                        // create the intent and start the activity
                        var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

                        // put a message on the modal dialog
                        voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, Application.Context.GetString(Resource.String.messageSpeakNow));

                        // if there is more then 1.5s of silence, consider the speech over
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 10000);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 50000);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 50000);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

                        // you can specify other languages recognised here, for example
                        // voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.German);
                        // if you wish it to recognise the default Locale language and German
                        // if you do use another locale, regional dialects may not be recognised very well

                        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
                        StartActivityForResult(voiceIntent, VOICE);
                    }
                };
        }

        protected override void OnActivityResult(int requestCode, Result resultVal, Intent data)
        {
            if (requestCode == VOICE)
            {
                if (resultVal == Result.Ok)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        string textInput;
                        //textInput = textBox.Text + matches[0]; -- this keeps the previous words on the screen
                        textInput = matches[0];  //matches is the output from the speech to text engine
                        Console.WriteLine("Getting data from matches...");

                        // limit the output to 500 characters
                        if (textInput.Length > 500)
                        {
                            textInput = textInput.Substring(0, 500).ToLower();
                        }//this only executes if the character limit is >500

                        textBox.Text = textInput;
                        Console.WriteLine("Spoken -- ");
                        Console.WriteLine(textInput);
                        splitwords(textInput.ToLower());
                    }
                    else
                        textBox.Text = "No speech was recognised";
                    // change the text back on the button
                    recButton.Text = "Start Recording";
                }
            }

            base.OnActivityResult(requestCode, resultVal, data);
        }

        /*Add function here*/
        void splitwords(String sentence)
        {
            char[] delimiters = {' '};
            string[] words = sentence.Split(delimiters); 

            foreach (string s in words)
            {
                playsounds(s);
            }
        }

        /*Play sounds*/
        void playsounds(String boldwords)
        {
            if(boldwords == "bright")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.bird_cut);
                _player.Start();
            }
            else if(boldwords == "darkness")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.flute_cut);
                _player.Start();
            }
            else if(boldwords == "stormy")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.thunder_cut);
                _player.Start();
            }
            else if(boldwords == "rain")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.rain_cut);
                _player.Start();
            }
            else if(boldwords == "wind")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.wind_cut);
                _player.Start();
            }
            else if(boldwords == "flame")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.flame_cut);
                _player.Start();
            }
            else if(boldwords == "sun")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.rooster_cut);
                _player.Start();
            }
        }
    }
}

