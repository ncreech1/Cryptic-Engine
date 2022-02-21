using System;
using System.IO;

namespace CrypticEngine
{
    public static class ConfigReader
    {
        /// <summary> Loads the configuration file </summary>
        public static bool loadConfig(CrypticGame game)
        {
            string config;
            bool isComment = false;
            bool labelFound = false;
            bool fieldFound = false;
            string field = "";
            string label = "";
            int currentLine = 1;
            int currentChar = 1;

            if (game == null)
                return false;

            //Create config file if none exists
            if (!File.Exists(CrypticGame.scenePath + "/../ce_config.txt"))
                File.WriteAllText(CrypticGame.scenePath + "/../ce_config.txt", $"# The Cryptic Engine Configuration File{Environment.NewLine}{Environment.NewLine}last_scene: \"Default.txt\"");

            //Read config file
            config = File.ReadAllText(CrypticGame.scenePath + "/../ce_config.txt");

            //Insert newline to ensure last field is checked
            config += '\n';

            for (int x = 0; x < config.Length; x++)
            {
                //Start of comment line
                if (config[x] == '#')
                    isComment = true;

                if (!isComment)
                {
                    switch (config[x])
                    {
                        case ':':
                            label = label.Trim();
                            labelFound = true;
                            break;
                        case ' ':
                            if (!labelFound && label.Trim() != "")
                                return configError("Unexpected whitespace character in label; loading default config", currentLine, currentChar);
                            break;
                        case '\n':

                            //Newline marks end of field
                            if (labelFound)
                            {
                                fieldFound = true;
                                field = field.Trim();
                                break;
                            }

                            else if (label.Trim() != "")
                                return configError("Unexpected newline character in label; loading default config", currentLine, currentChar);

                            break;
                        default:
                            if (labelFound)
                                field += config[x];
                            else
                                label += config[x];
                            break;
                    }

                    //Run the current label
                    if (fieldFound)
                    {
                        //All fields are strings and must be wrapped in double or single quotes
                        if(!field.StartsWith("\"") || !field.EndsWith("\""))
                            return configError("Fields are strings and must be wrapped in double quotes; loading default scene", currentLine, currentChar);

                        field = field.Substring(1, field.Length - 2);

                        //Run the current label and field
                        switch (label)
                        {
                            case "last_scene":
                                if (game.getScene(field) != null)
                                    game.openScene(field);
                                else
                                    configWarning($"Could not find scene \"{field}\"; loading default scene", currentLine, currentChar);
                                break;
                            default:
                                configWarning($"Unknown label \"{label}\" encountered; skipping", currentLine, currentChar);
                                break;
                        }

                        labelFound = false;
                        fieldFound = false;
                        label = "";
                        field = "";
                    }
                }

                //End of comment line
                if (config[x] == '\n' && isComment)
                    isComment = false;

                currentChar++;

                //Keep track of line and char numbers
                if (config[x] == '\n')
                {
                    currentLine++;
                    currentChar = 1;
                }
            }

            return true;
        }

        /// <summary> Loads the default configuration file </summary>
        public static void loadDefaultConfig(CrypticGame game)
        {
            CrypticScene defaultScene = new CrypticScene("Default");
            game.addScene(defaultScene);
            game.openScene(defaultScene);
        }

        private static bool configError(string error, int line, int character)
        {
            Console.WriteLine($"[Cryptic Engine] Error while reading config file \"ce_config.txt\": {error} [line {line}, char {character}]");
            return false;
        }

        private static void configWarning(string warning, int line, int character)
        {
            Console.WriteLine($"[Cryptic Engine] Warning while reading config file \"ce_config.txt\": {warning} [line {line}, char {character}]");
        }
    }
}
