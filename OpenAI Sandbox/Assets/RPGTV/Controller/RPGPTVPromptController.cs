using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HQDotNet;

namespace RPGPTV {
    public class RPGPTVPromptController : HQController {

        public override bool Startup() {

            //Set up initial prompts
            string initialPrompt = GetGeneralScenarioPrompt();
            initialPrompt += " " + GetCommandPromptDescription();
            initialPrompt += " " + GetScenePromptDescription();
            initialPrompt += " " + GetCharacterPromptDescription();
            initialPrompt += " " + GetCharacterSpeakDialogDescription();
            initialPrompt += " " + GetNarrationDescription();

            initialPrompt += " " + GetInitialPrompt();
            Debug.Log(initialPrompt);


            return base.Startup();
        }

        private string GetInitialPrompt() {
            return "Begin issuing commands. The first two commands should be to change scene and populate it with one or more characters. " +
                "After that begin the scene using all available commands at your disposal. " +
                "Start with 5 commands after the scene setup and character setup." +
                "There should be no other text but json objects representing commands.";
        }

        private string GetCharacterSpeakDialogDescription() {
            return "Available command to use: CHARACTER_SPEAK - You can have a character speak dialogue by issuing a command of this format:" +
                "{" +
                "characterName:\"The name of the character speaking. The character must be in the scene to speak.\"," +
                "characterDialog: \"The dialog for the character to speak\"" +
                "}";
        }

        private string GetNarrationDescription() {
            return "Available command to use: NARRATION - You can add narration to the scene to highten emotion by issuing a command of this format:" +
                "{" +
                "narrationText: \"Text to use for narration.\"" +
                "}";
        }

        private string GetCharacterPromptDescription() {
            return "Available command to use: ADD_CHARACTER_TO_SCENE - You can add a character to the scene by issuing a command of this format:" +
                "{" +
                "characterName: \"The name of the character\"," +
                "hairColor: \"A hexcode string representing the character's hair color. This should stay consistent with their character throughout the cutscene.\"," +
                "shirtColor: \"A hexcode string representing the character's shirt color. This should stay consistent with their character throughout the cutscene.\"" +
                "}";
        }

        private string GetScenePromptDescription() {
            return "Available command to use: SCENE_CHANGE - You can issue a change of scenes by issuing a command of this format: " +
                "{" +
                " sceneName:\"String representing the name of the scene to change to\"," +
                " isDay:true or false depending on whether the scene is daytime or nighttime," +
                " " +
                "}." +
                " The data after CHANGE_SCENE is a json object describing a scene, and should only ever equal one of the following strings:" +
                " FOREST, ROOM1. " +
                " FOREST is a clearing in a dark forest." +
                " ROOM1 is a simple indoor room.";
        }

        private string GetCommandPromptDescription() {
            return "You only respond with one of the available command options, which will be listed below. Any other response is invalid if not formatted properly." +
                "Every message should be formatted as a json file that follows these outlines:" +
                "{" +
                "command:\"A valid command name from the available choices\"," +
                "data:{}" +
                "}" +
                "data is a json object that represents the data for one of the commands. Each command requires a separate json object format to be represented in the data object here." +
                "Every message should be a valid json object.";

        }

        private string GetGeneralScenarioPrompt() {
            return "You are writing an infinite JRPG Cutscene, which interfaces with an app which will parse commands from your responses. " +
                "The general scenario is up to you, but the plot should evolve using persistent characters, themes, locations, and more. " +
                "The scenario should be full of JRPG and Anime tropes.";
        }
    }
}

