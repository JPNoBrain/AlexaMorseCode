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
using System.Text;

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
        List<char> morse = new List<char>();
        int a = 0;
        List<char> Test = new List<char>();
        private const string accessKey = "AKIAIXTAPN4OUT5GPKQQ";
        private const string secretKey = "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";
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
                        char[] letters = morseRequested.ToCharArray();
                        int i = 0;
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
                        var output = new StringBuilder();
                        output.Append("<speak>");
                        foreach (var item in morse)
                        {
                            if (item == '.')
                            {
                                //output.Append(@"<audio src = ""https://s3.eu-central-1.amazonaws.com/morseitech/120ms.mp3"" />");
                                //output.Append(@"<say-as interpret-as=""expletive"">hulume.</say-as><break time=""100ms""/>");
                                output.Append(@"beep");
                            }
                            else if (item == ',')
                            {
                                //output.Append(@"<audio src = ""https://s3.eu-central-1.amazonaws.com/morseitech/120ms_silence.mp3"" />");
                                //output.Append(@"<say-as interpret-as=""expletive"">hulume.</say-as><break time=""100ms""/>");
                                output.Append(@"<break time=""100ms""/>");
                            }
                            else if (item == '-')
                            {
                                //output.Append(@"<audio src = ""https://s3.eu-central-1.amazonaws.com/morseitech/360ms.mp3"" />");
                                //output.Append(@"<say-as interpret-as=""expletive"">hulume</say-as><break time=""100ms""/>");
                                output.Append(@"<prosody rate=""x-slow"">beep</prosody>");
                            }
                            else if (item == '|')
                            {
                                //output.Append(@"<audio src = ""https://s3.eu-central-1.amazonaws.com/morseitech/820ms_silence.mp3"" />");
                                output.Append(@"<break time=""700ms""/>");
                            }
                            else if (item == '+')
                            {
                                //output.Append(@"<audio src = ""https://s3.eu-central-1.amazonaws.com/morseitech/360ms_silence.mp3"" />");
                                output.Append(@"<break time=""400ms""/>");
                            }
                        }
                        output.Append("</speak>");
                        morse.Clear();
                        innerResponse = new SsmlOutputSpeech();
                        (innerResponse as SsmlOutputSpeech).Ssml = output.ToString();
                        response.Response.ShouldEndSession = true;
                        //response = ResponseBuilder.AudioPlayerPlay(PlayBehavior.ReplaceAll, "https://s3.eu-central-1.amazonaws.com/morseitech/120ms_silence.mp3", "Hello");
                        break;

                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = GetResources()[0].HelpReprompt;
                        break;
                }
            }
            //else if (input.GetRequestType() == typeof(AudioPlayerRequest))
            //{
            //    var audioPlayerRequest = (AudioPlayerRequest)input.Request;
            //    if (audioPlayerRequest.AudioRequestType == AudioRequestType.PlaybackNearlyFinished)
            //    {
            //        if (a <= morse.Count())
            //        {
            //            char item = morse[a];

            //            if (item == '.')
            //            {
            //                response = ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, "https://s3.eu-central-1.amazonaws.com/morseitech/120ms.mp3", a.ToString(), "asd", 0);
            //            }
            //            else if (item == ',')
            //            {
            //                response = ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, "https://s3.eu-central-1.amazonaws.com/morseitech/120ms_silence.mp3", a.ToString(), "asd", 0);
            //            }
            //            else if (item == '-')
            //            {
            //                response = ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, "https://s3.eu-central-1.amazonaws.com/morseitech/360ms.mp3", a.ToString(), "asd", 0);
            //            }
            //            else if (item == '|')
            //            {
            //                response = ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, "https://s3.eu-central-1.amazonaws.com/morseitech/820ms_silence.mp3", a.ToString(), "asd", 0);
            //            }
            //            else if (item == '+')
            //            {
            //                response = ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, "https://s3.eu-central-1.amazonaws.com/morseitech/360ms_silence.mp3", a.ToString(), "asd", 0);
            //            }
            //            a++;
            //        }
            //        else
            //        {
            //            a = 0;
            //            morse.Clear();
            //        }
            //    }
            //}
            //response.Response.OutputSpeech = innerResponse;
            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";
            return response;
        }
    }
}