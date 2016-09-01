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
using System.Net;
using System.Threading;

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
        private int hueLightCount = 0;
        private IList<string> HueLightState;
        private string IP_ADDR;
        private string Hue_User;
        private string LIGHT_URL;

        //Define situations
        private string THUNDER_HIGH = "{\"on\":true, \"sat\":254, \"bri\":254, \"hue\":34069}";
        private string THUNDER_LOW  = "{\"on\":true, \"sat\":254, \"bri\":76, \"hue\":34069}";
        private string RAIN_HIGH    = "{\"on\":true, \"sat\":254, \"bri\":144, \"hue\":47125}";
        private string RAIN_LOW     = "{\"on\":true, \"sat\":252, \"bri\":60, \"hue\":42608}";
        private string WIND_HIGH    = "{\"on\":true, \"sat\":252, \"bri\":60, \"hue\":65527}";
        private string WIND_MED     = "{\"on\":true, \"sat\":252, \"bri\":60, \"hue\":7321}";
        private string WIND_LOW     = "{\"on\":true, \"sat\":252, \"bri\":60, \"hue\":14832}";
        private string DARKNESS     = "{\"on\":true, \"sat\":141, \"bri\":10, \"hue\":14989}";
        private string FLAME_HIGH   = "{\"on\":true, \"sat\":228, \"bri\":144, \"hue\":168}";
        private string FLAME_LOW    = "{\"on\":true, \"sat\":252, \"bri\":240, \"hue\":7452}";
        private string BIRD_HIGH    = "{\"on\":true, \"sat\":230, \"bri\":144, \"hue\":52637}";
        private string BIRD_LOW     = "{\"on\":true, \"sat\":230, \"bri\":64, \"hue\":52637}";
        private string SUN          = "{\"on\":true, \"sat\":141, \"bri\":254, \"hue\":14988}";
        private string SLEEP        = "{\"on\":true, \"sat\":254, \"bri\":10, \"hue\":10806}";

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

            //Get info from intents
            hueLightCount = Intent.GetIntExtra("HueLightCount", 0);
            HueLightState = new string[hueLightCount];
            HueLightState = Intent.GetStringArrayListExtra("Selected lamps");
            
            IP_ADDR = Intent.GetStringExtra("IP_ADDR");
            Hue_User = Intent.GetStringExtra("HUE_USER");

            /*Base Light URL*/          //http://<bridge ip address>/api/1028d66426293e821ecfd9ef1a0731df/lights/1/state
            LIGHT_URL = "http://" + IP_ADDR + "/api/" + Hue_User + "/lights/";    //equivalent sprintf() could be better
         
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
                playMedia(s);
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

        /*Light Effects -- Does once for all the lights*/
        void playLights(string URL, string METHOD, string BODY)
        {
            for(int a=0; a<hueLightCount; a++)
            {
                WebClient client = new WebClient();
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
                client.Headers.Add(HttpRequestHeader.Accept, "application/json, text/javascript, */*; q=0.01");

                //Generate custom URL's   //1/state
                string custom_URL = null;
                if (HueLightState[a] == "ON")
                {
                    custom_URL = URL + (a + 1) + "/" + "state";
                    Console.WriteLine(custom_URL);
                    //PUT request
                    byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(BODY);  //conflicting with android media encoding
                    client.UploadDataAsync(new Uri(custom_URL), METHOD, dataBytes);
                }
            } 
        }

        /*Thread functions*/
        public void doWork1()
        {
            for (int a = 0; a < 4; a++)
            {
                 playLights(LIGHT_URL, "PUT", BIRD_HIGH);
                 playLights(LIGHT_URL, "PUT", BIRD_LOW);
            }
        }

        public void doWork2()
        {
            playLights(LIGHT_URL, "PUT", DARKNESS);
        }

        public void doWork3()
        {
            for (int a = 0; a < 4; a++)
            {
                playLights(LIGHT_URL, "PUT", THUNDER_HIGH);
                playLights(LIGHT_URL, "PUT", THUNDER_LOW);
            }
        }

        public void doWork4()
        {
            for (int a = 0; a < 4; a++)
            {
                playLights(LIGHT_URL, "PUT", RAIN_HIGH);
                playLights(LIGHT_URL, "PUT", RAIN_LOW);
            }
        }

        public void doWork5()
        {
            for (int a = 0; a < 5; a++)
            {
                playLights(LIGHT_URL, "PUT", WIND_HIGH);
                playLights(LIGHT_URL, "PUT", WIND_MED);
                playLights(LIGHT_URL, "PUT", WIND_LOW);
            }
        }

        public void doWork6()
        {
            for (int a = 0; a < 4; a++)
            {
                playLights(LIGHT_URL, "PUT", FLAME_HIGH);
                playLights(LIGHT_URL, "PUT", FLAME_LOW);
            }
        }

        public void doWork7()
        {
            playLights(LIGHT_URL, "PUT", SUN);
        }

        public void doWork8()
        {
            playLights(LIGHT_URL, "PUT", SLEEP);
        }

        /*Play sounds*/
        //Lights should be on seperate threads
        void playMedia(String boldwords)
        {
            if (boldwords == "bright")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.bird_cut);
                _player.Start();

                ThreadStart myThreadDelegate = new ThreadStart(doWork1);
                Thread myThread = new Thread(myThreadDelegate);
                myThread.Start();
            }
            else if (boldwords == "darkness")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.flute_cut);
                _player.Start();

                ThreadStart myThreadDelegate = new ThreadStart(doWork2);
                Thread myThread = new Thread(myThreadDelegate);
                myThread.Start();
            }
            else if (boldwords == "stormy")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.thunder_cut);
                _player.Start();

                ThreadStart myThreadDelegate = new ThreadStart(doWork3);
                Thread myThread = new Thread(myThreadDelegate);
                myThread.Start();
            }
            else if (boldwords == "rain")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.rain_cut);
                _player.Start();

                ThreadStart myThreadDelegate = new ThreadStart(doWork4);
                Thread myThread = new Thread(myThreadDelegate);
                myThread.Start();
            }
            else if (boldwords == "wind")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.wind_cut);
                _player.Start();

                ThreadStart myThreadDelegate = new ThreadStart(doWork5);
                Thread myThread = new Thread(myThreadDelegate);
                myThread.Start();
            }
            else if (boldwords == "flame")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.flame_cut);
                _player.Start();

                ThreadStart myThreadDelegate = new ThreadStart(doWork6);
                Thread myThread = new Thread(myThreadDelegate);
                myThread.Start();
            }
            else if (boldwords == "sun")
            {
                _player = MediaPlayer.Create(this, Resource.Raw.rooster_cut);
                _player.Start();

                ThreadStart myThreadDelegate = new ThreadStart(doWork7);
                Thread myThread = new Thread(myThreadDelegate);
                myThread.Start();
            }
            else if(boldwords == "happily")
            {
                ThreadStart myThreadDelegate = new ThreadStart(doWork8);
                Thread myThread = new Thread(myThreadDelegate);
                myThread.Start();
            }
        }


    }
}