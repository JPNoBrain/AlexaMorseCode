using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;
using Alexa.NET.Request;
using Alexa.NET.Response.Directive;
using AudioSkillSample.Assets;
using Alexa.NET;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AudioSkillSample.Assets
{
    public class AudioAssets
    {
        public static AudioItem[] GetSampleAudioFiles()
        {
            AudioItem[] returnAudio = new AudioItem[5];

            returnAudio[0] = (new AudioItem()
            {
                Title = "50ms",
                Url = "https://s3.eu-central-1.amazonaws.com/morseitech/50ms.mp3"
            }
            );
            returnAudio[1] = (new AudioItem()
            {
                Title = "50ms_silence",
                Url = "https://s3.eu-central-1.amazonaws.com/morseitech/50ms_silence.mp3"
            });
            returnAudio[2] = (new AudioItem()
            {
                Title = "150ms",
                Url = "https://s3.eu-central-1.amazonaws.com/morseitech/150ms.mp3"
            }
            );
            returnAudio[3] = (new AudioItem()
            {
                Title = "150ms_silence",
                Url = "https://s3.eu-central-1.amazonaws.com/morseitech/150ms_silence.mp3"
            });
            returnAudio[4] = (new AudioItem()
            {
                Title = "350ms",
                Url = "https://s3.eu-central-1.amazonaws.com/morseitech/350ms.mp3"
            }
            );

            return returnAudio;
        }
    }

    public class AudioItem
    {
        public string Title { get; set; }
        public string Url { get; set; }
    }
}

namespace AlexaMorseCode
{
    public class Messages
    {
        public Messages(string language)
        {
            this.Language = language;
        }

        public string Language { get; set; }
        public string SkillName { get; set; }
        public string HelpMessage { get; set; }
        public string HelpReprompt { get; set; }
        public string StopMessage { get; set; }
        public string GetMorseCode { get; set; }
        public string LaunchMessage { get; set; }
    }

    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public List<Messages> GetResources()
        // Actually needed? Maybe without List, but raw object.
        {
            List<Messages> resources = new List<Messages>();
            Messages enUSResource = new Messages("en-US");
            enUSResource.SkillName = "Morse Code Translator";
            enUSResource.GetMorseCode = "Here's your morse code: ";
            enUSResource.HelpMessage = "You can say Alexa, morse hello, or, you can say exit... What can I help you with?";
            enUSResource.HelpReprompt = "What can I help you with?";
            enUSResource.StopMessage = "Goodbye!";
            enUSResource.LaunchMessage = "Welcome to Morse Translator. Please provide the desired text to be morsed.";
            resources.Add(enUSResource);
            return resources;
        }

        public char[] ToArray(string input)
        {
            char[] characters = input.ToCharArray();
            return characters;
        }

        public List<char> Translate(char[] letters)
        {
            List<char> morse = new List<char>();
            //for (int i = 0; i < letters.Length; i++) //loop through each letter
            //{
            //    List<char> temp = new List<char>(); //list for morse code for one single letter
            //    //TODO: Get translation from letters[i] and add to temp<char>
            //    //DB tutorial: http://matthiasshapiro.com/2017/03/21/tutorial-dynamodb-in-net/
            //    for (int j = 0; j < temp.Capacity; j++)
            //    {
            //        morse.Add(temp.ElementAt(0)); //add morse sign to total morse code
            //        if (j != temp.Capacity - 1) //add short pause if not final morse sign for current letter
            //        {
            //            morse.Add(',');
            //        }
            //    }
                  //Calc last sign for current letter
            //    if (i != letters.Length - 1) //if not final letter
            //    {
            //        if (letters[i + 1] == ' ') //add very long pause if next letter is space
            //        {
            //            morse.Add('|');
            //        }
            //        else //add long pause if next letter is normal letter
            //        {
            //            morse.Add('+');
            //        }
            //    }
            //}
            morse.Add('.');
            morse.Add(',');
            morse.Add('.');
            morse.Add(',');
            morse.Add('-');
            morse.Add('+');
            morse.Add('-');
            morse.Add(',');
            morse.Add('|');
            morse.Add('.');
            return morse;
        }

        public void Output(List<char> input)
        {
            var audioItems = AudioAssets.GetSampleAudioFiles();
            for (int i = 0; i < input.Capacity; i++)
            {
                switch (input.ElementAt(i))
                {
                    case '.':
                        ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, audioItems[0].Url, audioItems[0].Title); //50ms
                        break;
                    case ',':
                        ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, audioItems[1].Url, audioItems[1].Title); //50ms_silence
                        break;
                    case '-':
                        ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, audioItems[2].Url, audioItems[2].Title); //150ms
                        break;
                    case '+':
                        ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, audioItems[3].Url, audioItems[3].Title); //150ms_silence
                        break;
                    case '|':
                        ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, audioItems[4].Url, audioItems[4].Title); //350ms_silence
                        
                        break;
                }
            }
            //TODO
        }

        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            SkillResponse response = new SkillResponse();
            response.Response = new ResponseBody();
            response.Response.ShouldEndSession = false;
            IOutputSpeech innerResponse = null;
            var log = context.Logger;

            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine($"Default LaunchRequest made: 'Alexa, open Science Facts");
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = GetResources()[0].LaunchMessage;

            }
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                var intentRequest = (IntentRequest)input.Request;
                switch (intentRequest.Intent.Name)
                {
                    case "AMAZON.CancelIntent":
                        log.LogLine($"AMAZON.CancelIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = GetResources()[0].StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.StopIntent":
                        log.LogLine($"AMAZON.StopIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = GetResources()[0].StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.HelpIntent":
                        log.LogLine($"AMAZON.HelpIntent: send HelpMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = GetResources()[0].HelpMessage;
                        break;
                    case "TranslateIntent": //TODO: Check if working
                        log.LogLine($"GetFactIntent sent: send new fact");
                        //innerResponse = new PlainTextOutputSpeech();
                        var morseRequested = intentRequest.Intent.Slots["Literal"].Value;
                        response = ResponseBuilder.AudioPlayerPlay(PlayBehavior.ReplaceAll, "https://s3.eu-central-1.amazonaws.com/morseitech/150ms.mp3", "penis");
                        //(innerResponse as PlainTextOutputSpeech).Text = GetResources()[0].GetMorseCode;
                        //Output(Translate(ToArray(morseRequested)));
                        break;
                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = GetResources()[0].HelpReprompt;
                        break;
                }
            }
            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";
            return response;
        }
    }
}