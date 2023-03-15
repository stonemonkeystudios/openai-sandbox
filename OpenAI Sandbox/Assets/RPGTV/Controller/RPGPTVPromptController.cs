using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HQDotNet;
using SMS.OpenAI;
using OpenAI.Chat;
using System.Threading.Tasks;

namespace RPGPTV {
    public class RPGPTVPromptController : HQController, ICommandCompletedListener {

        public enum COMMANDS { SCENE_CHANGE, ADD_CHARACTER_TO_SCENE, NARRATION, CHARACTER_SPEAK, PLAY_MUSIC };
        [HQInject]
        private SMSOpenAIController openAIController;

        private List<RPGPTVCommandModel> commandsList = new List<RPGPTVCommandModel>();
        private List<ChatPrompt> chatPrompts = new List<ChatPrompt>();
        private int tokenCount = 0;
        private Task chatTask;


        private void TestParse() {
            string s = "{\"command\":\"SCENE_CHANGE\", \"data\":\"{\\\"sceneName\\\":\\\"FOREST.unity\\\",\\\"isDay\\\":true}\"}";
            var p = JsonUtility.FromJson<RPGPTVCommandModel>(s);
        }

        public override bool Startup() {

            TestParse();

            //Set up initial prompts
            string systemPrompt = GetGeneralScenarioPrompt();
            systemPrompt += " " + GetCommandPromptDescription();
            systemPrompt += " " + GetScenePromptDescription();
            systemPrompt += " " + GetCharacterPromptDescription();
            systemPrompt += " " + GetCharacterSpeakDialogDescription();
            systemPrompt += " " + GetNarrationDescription();
            systemPrompt += " " + GetMusicCommandDescription();
            //systemPrompt += " " + GetGoodExamplesDescription();
            systemPrompt += " " + GetNegativeFilters();
            systemPrompt += " " + GetFinalSystemMessages();

            Debug.Log("System prompt length: " + systemPrompt.Length);

            string userPrompt = GetRecurringUserMessage();

            tokenCount = systemPrompt.Length;
            tokenCount += userPrompt.Length;

            chatPrompts.Add(new ChatPrompt("system", systemPrompt));
            chatPrompts.Add(new ChatPrompt("user", userPrompt));

            chatTask = new Task(GetResponses);
            chatTask.Start();

            return base.Startup();
        }

        public override bool Shutdown() {
            if (chatTask != null)
                chatTask.Dispose();
            return base.Shutdown();
        }

        private async void GetResponses() {
            Debug.Log("GetResponses()");
            ChatRequest chatRequest = new ChatRequest(chatPrompts, OpenAI.Models.Model.GPT3_5_Turbo);
            try {
                var response = await openAIController.openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);
                ParseResponseForCommands(response);
            }
            catch (System.Exception e) {
                Debug.LogException(e);
            }
        }

        private void ParseResponseForCommands(ChatResponse response) {
            Debug.Log("Response: \n" + response.FirstChoice.Message);
            string[] commandArr = response.FirstChoice.Message.Content.Split("\n");
            for (int i = 0; i < commandArr.Length; i++) {
                if (string.IsNullOrEmpty(commandArr[i]))
                    continue;
                try {
                    var obj = JsonUtility.FromJson<RPGPTVCommandModel>(commandArr[i]);
                    commandsList.Add(obj);
                    chatPrompts.Add(new ChatPrompt("assistant", commandArr[i]));
                    tokenCount += commandArr[i].Length;
                }
                catch (System.Exception e) {
                    Debug.LogException(e);
                }
            }
            Debug.Log("THERE ARE " + commandsList.Count + " COMMANDS PENDING");
            if (commandsList.Count > 0) {
                ExecuteNextCommand();
            }
            else {
                GetResponses();
            }
        }

        private void ExecuteNextCommand() {
            var command = commandsList[0];
            Debug.Log("Execute next command: " + command.command);
            commandsList.RemoveAt(0);
            if(command.command != COMMANDS.ADD_CHARACTER_TO_SCENE.ToString())
               Session.Dispatcher.Dispatch<ICommandReceivedListener>(listener => listener.CommandReceived(command));
            else {
                Session.Dispatcher.Dispatch<ICommandCompletedListener>(listener => listener.CommandCompleted(command));
            }

            //TODO: If we don't receive a command completed in x seconds, move on to the next command?
        }



        private void TrimChatPrompts() {
            Debug.Log("Trim Chat Prompts: " + tokenCount);
            //The token limit for prompts is 4096. We need to make sure we don't go over that limit.
            while (tokenCount >= 3500 && chatPrompts.Count > 2) {
                tokenCount -= chatPrompts[2].Content.Length;
                chatPrompts.RemoveAt(2);
            }
        }

        #region Initial Conditions (Prompt) setup

        private string GetMusicCommandDescription() {
            return "Available command: PLAY_MUSIC. The data for this command should be a simple string from this list: " +
                "AllIsLost, BackToNormal, BossBattle, Epilogue, RecoveringHope, Triumphant, Casual, Danger, Dramatic, MainTitle, Melancholy, ShadyDealing, Slow, ThingsWillBeOk, TraditionalJapanese, Upbeat";
        }

        private string GetFinalSystemMessages() {
            return "When you Begin issuing commands for the very first time, make sure to set up scene, music, and characters." +
                "There should be no other text but json objects representing commands." +
                "Any time you switch scenes, you must populate the scene with all characters involved." +
                "Every command should be on a single line, with new line characters ONLY appearing at the end of a command line.";
        }

        private string GetGoodExamplesDescription(){
            return "Good exaxmples of commands look like the following:\n" +
                " {command:\"SCENE_CHANGE\",data:\"{\\\"sceneName\\\":\\\"FOREST.unity\\\",\\\"isDay\\\":true}\"}\n" +
                " {command:\"ADD_CHARACTER_TO_SCENE\",data:\"{\\\"characterName\\\":\\\"Hero\\\",\\\"hairColor\\\":\\\"#000000\\\",\\\"shirtColor\\\":\\\"#00ff00\\\"}\"}\n";
        }

        private string GetRecurringUserMessage() {
            return "ABSOLUTELY IMPORTANT!! Give me a command to continue the scene. Only include the command objects and no other text. This should NOT be formatted as a list with numbers. Only one command object per line and nothing else.";
        }

        private string GetNegativeFilters() {
            return "IMPORTANT!!! This chat should by no means use any copyrighted material. Be creative, use names, mix genres, but don't directly rip off a franchise.";
        }

        private string GetCharacterSpeakDialogDescription() {
            return "Available command to use: CHARACTER_SPEAK - You can have a character speak dialogue by issuing a command of this format:" +
                "{" +
                "\"characterName\":\"The name of the character speaking (who is in scene).\"," +
                "\"characterDialog\": \"dialog text\"" +
                "}";
        }

        private string GetNarrationDescription() {
            return "Available command: NARRATION -  add narration to the scene by issuing a command of this format:" +
                "{" +
                "\"narrationText\": \"narration text.\"" +
                "}";
        }

        private string GetCharacterPromptDescription() {
            return "Available command to use: ADD_CHARACTER_TO_SCENE - You can add a character to the scene by issuing a command of this format:" +
                "{" +
                "\"characterName\": \"The name of the character\"," +
                "\"hairColor\": \"A hexcode string representing the character's hair color. This should stay consistent with their character throughout the cutscene.\"," +
                "\"shirtColor\": \"A hexcode string representing the character's shirt color. This should stay consistent with their character throughout the cutscene.\"" +
                "}";
        }

        private string GetScenePromptDescription() {
            return "Available command: SCENE_CHANGE - You can issue a change scenes by issuing a command of this format: " +
                "{" +
                " \"sceneName\":\"String representing the name of the scene\"," +
                " \"isDay\":true or false," +
                " " +
                "}." +
                " The data after CHANGE_SCENE is a json object describing a scene, and be one of the following strings:" +
                " FOREST, ROOM, CITY, TEMPLE, MOUNTAIN, SCHOOL ";
        }

        private string GetCommandPromptDescription() {
            return "You only respond with one of the available command options, which will be listed below" +
                "Every message should be formatted as a json file that follows these outlines:" +
                "{" +
                "\"command\":\"A valid command name\"," +
                "\"data\":\"{}\"" +
                "}" +
                "data is a string containing a json object that represents the data for one of the commands. Each command requires a separate json object format to be represented in the data object here." +
                "Every command should be a valid json object." +
                "The data object must be encapsulated as a string. The most important thing is that the data type of the 'data' parameter is string. it is always string, with substrings being encapsulated with a backslash.";

        }

        private string GetGeneralScenarioPrompt() {
            return "You are writing an infinite JRPG Cutscene, which interfaces with an app which will parse commands from your responses. " +
                "The scenario should be full of JRPG and Anime tropes.";
        }

        #endregion

        #region ICommandCompletedListener

        void ICommandCompletedListener.CommandCompleted(RPGPTVCommandModel command) {
            if(commandsList.Count == 0) {
                TrimChatPrompts();

                chatTask = new Task(GetResponses);
                chatTask.Start();
            }
            else {
                ExecuteNextCommand();
            }
        }

        #endregion
    }
}

