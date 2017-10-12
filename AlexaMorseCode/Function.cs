using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;
using Alexa.NET.Request;
using Alexa.NET.Response.Directive;
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

    //Class equal to database table
    public class alphabet
    {
        public char letter { get; set; }
        public string morse { get; set; }
    }

    public class Function
    {
        //Database access keys
        private const string accessKey = "xxxxxx";
        private const string secretKey = "xxxxxx";

        //Contains the morse code for the card
        string lastRequest = "";

        public List<Messages> GetResources()
        // List that defines english responses 
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

        //FunctionHandler is called whenever the User sends a request to the skill
        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            List<char> morse = new List<char>();
            var card = new StringBuilder();

            //Connect to database
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
            var dbContext = new DynamoDBContext(client);

            //Create empty response
            SkillResponse response = new SkillResponse();
            response.Response = new ResponseBody();
            response.Response.ShouldEndSession = false;
            IOutputSpeech innerResponse = null;
            var log = context.Logger;

            //Check if request is a launch request 
            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine($"Default LaunchRequest made: 'Alexa, start Morse Translator");
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = GetResources()[0].LaunchMessage;
                response.Response.OutputSpeech = innerResponse;

            }
            //If request is not a launch request, check what kind of intent has been requested
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                var intentRequest = (IntentRequest)input.Request;
                switch (intentRequest.Intent.Name)
                {
                    case "AMAZON.CancelIntent":
                        log.LogLine($"AMAZON.CancelIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = GetResources()[0].StopMessage;
                        response.Response.OutputSpeech = innerResponse;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.StopIntent":
                        log.LogLine($"AMAZON.StopIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = GetResources()[0].StopMessage;
                        response.Response.OutputSpeech = innerResponse;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.HelpIntent":
                        log.LogLine($"AMAZON.HelpIntent: send HelpMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = GetResources()[0].HelpMessage;
                        response.Response.OutputSpeech = innerResponse;
                        break;
                    case "TranslateIntent": 
                        log.LogLine($"TranslateIntent sent: send morse code");
                        var morseRequested = intentRequest.Intent.Slots["Literal"].Value;
                        lastRequest = morseRequested;
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
                                //Convert char to string uppercase the string and convert to char[0]
                                char comp = item.ToString().ToUpper().ToCharArray()[0];
                                //Create new Condition
                                List<ScanCondition> conditions = new List<ScanCondition>();
                                //Fill the condition. "letter" is the name of the Column, gets compared with comp
                                conditions.Add(new ScanCondition("letter", ScanOperator.Equal, comp));

                                //Start database query with conditions (alphabet contains the same poperties as the tables columns)
                                var allDocs = await dbContext.ScanAsync<alphabet>(conditions).GetRemainingAsync();
                                //Get the first result
                                var tempDoc = allDocs.FirstOrDefault();
                                //save the content of the morse column in tempString
                                string tempString = tempDoc.morse;
                                //Convert tempString to char array
                                char[] temp = tempString.ToCharArray();
                                //Add each content from the temp array to the final morse list
                                foreach (var sign in temp)
                                {
                                    morse.Add(sign);
                                }
                                //add a + to the morse list to indicate that the letter is finished
                                morse.Add('+');
                            }
                            i++;
                        }
                        var output = new StringBuilder();
                        output.Append(@"<speak>Here is your morse code for <break time=""30ms""/>");
                        output.Append(morseRequested);
                        output.Append(@"<break time=""100ms""/>");
                        foreach (var item in morse)
                        {
                            //output.Append adds to the output that you can hear from Alexa
                            //card.Append adds to the ouput that you can see in the Alexa app
                            if (item == '.')
                            {
                                output.Append(@"<prosody rate=""fast"">beep</prosody>");
                                card.Append(".");
                            }
                            else if (item == ',')
                            {
                                output.Append(@"<break time=""100ms""/>");
                            }
                            else if (item == '-')
                            {
                                output.Append(@"<prosody rate=""x-slow"">beep</prosody>");
                                card.Append("-");
                            }
                            else if (item == '|')
                            {
                                output.Append(@"<break time=""800ms""/>");
                                card.Append(" | ");
                            }
                            else if (item == '+')
                            {
                                output.Append(@"<break time=""400ms""/>");
                                card.Append(" ");
                            }
                        }
                        output.Append("</speak>");
                        //define that the output is a SsmlOutputSpeech
                        innerResponse = new SsmlOutputSpeech();
                        //fill the SsmlOuputSpeech
                        (innerResponse as SsmlOutputSpeech).Ssml = output.ToString();
                        //end the session after the response gets played
                        response.Response.ShouldEndSession = true;
                        //fill the response with the ssml output and the content for the card(Alexa App)
                        response = ResponseBuilder.TellWithCard(innerResponse, "Morse code for: " + lastRequest, "Here is your morse code: " + card.ToString());
                        break;

                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = GetResources()[0].HelpReprompt;
                        response.Response.OutputSpeech = innerResponse;
                        break;
                }
            }   
            response.Version = "1.0";
            //return the response to the function handler, the function handler returns the response to alexa
            return response;
        }
    }
}