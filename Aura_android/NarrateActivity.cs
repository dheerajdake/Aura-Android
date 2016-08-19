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
using Android.Speech;
using Android.Media;

namespace Aura_android
{
    [Activity(Label = "NarrateActivity")]
    public class NarrateActivity : Activity
    {
        private bool isRecording;
        private readonly int VOICE = 10;
        private TextView SpeechBox;
        private Button narrateButton, prevButton, nextButton;
        private TextView storybox;
        private string[] lines;
        private int line_count = 0;
        private int currentLine = 0;

        MediaPlayer _player;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //set the IsRecording flag to false
            isRecording = false;

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Narrate);

            //get resources from the layout
            narrateButton = FindViewById<Button>(Resource.Id.record_start);
            SpeechBox = FindViewById<TextView>(Resource.Id.speech_box);
            storybox = FindViewById<TextView>(Resource.Id.story_box);
            prevButton = FindViewById<Button>(Resource.Id.Narrate_previous);
            nextButton = FindViewById<Button>(Resource.Id.Narrate_next);

            /*Demo story*/
            string tellTale = "It was a dark and stormy night. The rain fell in torrents. Except when it was checked by violent gust of wind."
                             + "And the scanty flame of the lamps, struggled against the darkness. But the next morning the sun came out."
                             + "The future seemed bright. And everyone lived happily ever after";
            splitsentences(tellTale);

            /*The page displays the first line*/
            storybox.Text = lines[0];

            /*Button Clicks*/
            prevButton.Click += (sender, e) =>
            {
                if (currentLine >= 1)
                {
                    currentLine--;
                    storybox.Text = lines[currentLine];
                }
                else
                {
                    Console.WriteLine("This is the beginning");
                    /*Alert popup*/
                    var alertBegin = new AlertDialog.Builder(prevButton.Context);
                    alertBegin.SetTitle("You reached the beginning");
                    alertBegin.SetPositiveButton("OK", (sender1, f) =>
                    {
                        return;
                    });
                    alertBegin.Show();
                }
                Console.WriteLine("Previous clicked");
            };

            nextButton.Click += (sender, e) =>
            {
                if (currentLine < line_count - 1)      //linecount is 8, but the array val is till 7
                {
                    currentLine++;
                    storybox.Text = lines[currentLine];
                }
                else
                {
                    Console.WriteLine("The end");
                    var alertEnd = new AlertDialog.Builder(nextButton.Context);
                    alertEnd.SetTitle("You reached the end");
                    alertEnd.SetPositiveButton("OK", (sender1, f) =>
                    {
                        return;
                    });
                    alertEnd.Show();
                }
                Console.WriteLine("Next clicked");
            };

            // check to see if we can actually record - if we can, assign the event to the button
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                // no microphone, no recording. Disable the button and output an alert
                var alert = new AlertDialog.Builder(narrateButton.Context);
                alert.SetTitle("You don't seem to have a microphone to record with");
                alert.SetPositiveButton("OK", (sender, e) =>
                {
                    SpeechBox.Text = "No microphone present";
                    narrateButton.Enabled = false;
                    return;
                });

                alert.Show();
            }
            else
                narrateButton.Click += delegate
                {
                    // change the text on the button
                    Console.WriteLine("Breakpoint ...");
                    narrateButton.Text = "End Recording";
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

        private void PrevButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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

                        SpeechBox.Text = textInput;
                        Console.WriteLine("Spoken -- ");
                        Console.WriteLine(textInput);
                        splitwords(textInput.ToLower());
                    }
                    else
                        SpeechBox.Text = "No speech was recognised";
                    // change the text back on the button
                    narrateButton.Text = "Start Recording";
                }
            }

            base.OnActivityResult(requestCode, resultVal, data);
        }

        /*Add function here*/
        void splitwords(String sentence)
        {
            char[] delimiters = { ' ' };
            string[] words = sentence.Split(delimiters);

            foreach (string s in words)
            {
                playsounds(s);
            }
        }

        /*Display sentences*/
        void splitsentences(String story)
        {
            char[] delimiters = { '.', '!', ',' };
            lines = story.Split(delimiters);

            foreach (string s in lines)
            {
                Console.WriteLine(lines[line_count]);
                line_count++;
            }

            Console.Write("The number of lines are "); Console.WriteLine(line_count);
        }

        /*Play sounds*/
        void playsounds(String boldwords)
        {
            if (boldwords == "bright")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.bird_cut);
                _player.Start();
            }
            else if (boldwords == "darkness")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.flute_cut);
                _player.Start();
            }
            else if (boldwords == "stormy")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.thunder_cut);
                _player.Start();
            }
            else if (boldwords == "rain")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.rain_cut);
                _player.Start();
            }
            else if (boldwords == "wind")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.wind_cut);
                _player.Start();
            }
            else if (boldwords == "flame")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.flame_cut);
                _player.Start();
            }
            else if (boldwords == "sun")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.rooster_cut);
                _player.Start();
            }
        }


    }
}