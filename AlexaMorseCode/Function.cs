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
using Amazon.Runtime;
using Amazon.DynamoDBv2;
using Amazon;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System.Threading;

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

    public class alphabet
    {
        public char letter { get; set; }
        public string morse { get; set; }
    }

       
    public class Function
    {
        int a = 0;
        List<char> Test = new List<char>();
        private const string accessKey = "";
        private const string secretKey = "";
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

        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
            var dbContext = new DynamoDBContext(client);

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
                        //List<char> morse = Translate(ToArray(morseRequested));

                        //response.Response.Directives.Add(new AudioPlayerPlayDirective()
                        //{
                        //    PlayBehavior = PlayBehavior.ReplaceAll,
                        //    AudioItem = new Alexa.NET.Response.Directive.AudioItem()
                        //    {

                        //        Stream = new AudioItemStream()
                        //        {
                        //            Url = "https://s3.eu-central-1.amazonaws.com/morseitech/120ms.mp3",
                        //            Token = "120ms"
                        //        }
                        //    }
                        //});

                        //response.Response.Directives.Add(new AudioPlayerPlayDirective()
                        //{

                        //    PlayBehavior = PlayBehavior.Enqueue,
                        //    AudioItem = new Alexa.NET.Response.Directive.AudioItem()
                        //    {

                        //        Stream = new AudioItemStream()
                        //        {
                        //            Url = "https://s3.eu-central-1.amazonaws.com/morseitech/360ms.mp3",
                        //            Token = "360ms",
                        //            ExpectedPreviousToken = "120ms"
                        //        }
                        //    }
                        //});



                        //response.Response.ShouldEndSession = true;
                        char[] letters = morseRequested.ToCharArray();
                        List<char> morse = new List<char>();
                        int i = 0;
                        string token = "";
                        foreach (char item in letters)
                        {
                            if (item == ' ')
                            {
                                morse.Add('|');
                            }
                            else
                            {
                                char comp = item.ToString().ToUpper().ToCharArray()[0];
                                List<ScanCondition> conditions = new List<ScanCondition>();
                                conditions.Add(new ScanCondition("letter", ScanOperator.Equal, comp));

                                var allDocs = await dbContext.ScanAsync<alphabet>(conditions).GetRemainingAsync();
                                var tempDoc = allDocs.FirstOrDefault();
                                String tempString = tempDoc.morse;
                                char[] temp = tempString.ToCharArray();
                                foreach (var itemm in temp)
                                {
                                    morse.Add(itemm);
                                }
                                morse.Add('+');
                            }
                            i++;
                        }
                        foreach (var item in morse)
                        {
                            token += item;
                        }
                        response = ResponseBuilder.AudioPlayerPlay(PlayBehavior.ReplaceAll, "https://s3.eu-central-1.amazonaws.com/morseitech/360ms.mp3", token);
                        break;

                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = GetResources()[0].HelpReprompt;
                        break;
                }
            }
            else if (input.GetRequestType() == typeof(AudioPlayerRequest))
            {
                var audioPlayerRequest = (AudioPlayerRequest)input.Request;
                if (audioPlayerRequest.AudioRequestType == AudioRequestType.PlaybackNearlyFinished)
                {
                    //response.Response.Directives.Add(new AudioPlayerPlayDirective()
                    //{

                    //    PlayBehavior = PlayBehavior.Enqueue,
                    //    AudioItem = new Alexa.NET.Response.Directive.AudioItem()
                    //    {

                    //        Stream = new AudioItemStream()
                    //        {
                    //            Url = "https://s3.eu-central-1.amazonaws.com/morseitech/360ms.mp3",
                    //            Token = "360ms",
                    //            ExpectedPreviousToken = "120ms"
                    //        }
                    //    }
                    //});
                    response = ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, "https://s3.eu-central-1.amazonaws.com/morseitech/360ms.mp3", "test2", "test", 0);

                }







            }
            else if (input.GetRequestType() == typeof(AudioPlayerRequest))
            {
                var audioPlayerRequest = (AudioPlayerRequest)input.Request;
                if (audioPlayerRequest.AudioRequestType == AudioRequestType.PlaybackNearlyFinished)
                {
                    if (a <= Test.Count())
                    {
                       
                        
                        //response = ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, "https://s3.eu-central-1.amazonaws.com/morseitech/360ms.mp3", "test2", "test", 0);

                        char item = Test[a];
                        
                        if (item == '.')
                        {
                            response = ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, "https://s3.eu-central-1.amazonaws.com/morseitech/360ms.mp3", a.ToString(), "asd", 0);
                        }
                        else if (item == ',')
                        {
                            response = ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, "https://s3.eu-central-1.amazonaws.com/morseitech/120ms.mp3", a.ToString(), "asd", 0);
                        }
                        else if (item == '-')
                        {
                            response = ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, "https://s3.eu-central-1.amazonaws.com/morseitech/360ms.mp3", a.ToString(), "asd", 0);
                        }
                        else if (item == '|')
                        {
                            response = ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, "https://s3.eu-central-1.amazonaws.com/morseitech/360ms.mp3", a.ToString(), "asd", 0);
                        }


                        a++;
                    }
                    else
                    {
                        a = 0;
                        Test.Clear();
                    }
                }
            }
            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";
            return response;
        }
    }
}