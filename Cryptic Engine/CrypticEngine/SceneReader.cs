using System;
using System.IO;
using System.Reflection;
using CrypticEngine.Graphics;
using CrypticEngine.Scripts;
using Microsoft.Xna.Framework;

namespace CrypticEngine
{
    public static class SceneReader
    {
        //A Token is an object that contains a label and field (e.g. name: "Player")
        private class Token
        {
            public string label;
            public string field;
            public int line, character, fileIndex;
            public bool EOF;

            public Token(string label, string field, int line, int character, int fileIndex)
            {
                this.label = label;
                this.field = field;
                this.fileIndex = fileIndex;
                this.line = line;
                this.character = character;
                EOF = false;
            }
        }

        private static CrypticObject currentObj; //The current object being loading
        private static CrypticScene currentScene; //The current scene being loading
        private static string currentFile; //The current scene file text
        private static Token currentToken; //The current label and field pair (token)
        private static bool addingScript; //If a script block is being loaded

        /// <summary> Reads and loads the CrypticObjects from the scene file </summary>
        public static bool loadScene(CrypticGame game, CrypticScene scene)
        {
            //Do not load if game is not initialized
            if (game == null) 
            { 
                sceneError("Game instance has not been initialized!", 0, 0); 
                return false; 
            }

            //Do not load if scene has already been loaded
            if(scene.getCrypticObjectStorage().Count > 0) 
            { 
                sceneError("Scene already loaded!", 0, 0); 
                return false; 
            }

            //Unable to find scene file or file permissions are invalid
            if (!File.Exists(scene.getFullPath())) 
            {
                sceneError($"Unable to find or open scene file at path \"{currentScene.getFullPath()}\"", 0, 0);
                return false;
            }

            //Unload the objects in the current scene
            if (game.getCurrentScene() != null)
                game.getCurrentScene().unloadCrypticObjects();

            //Read the scene file for the new scene
            currentFile = File.ReadAllText(scene.getFullPath());
            currentScene = scene;
            currentToken = new Token("", "", 0, 0, 0);
            currentObj = null;
            addingScript = false;

            //Insert newline to ensure last field is checked
            currentFile += '\n';

            //Get the first token
            currentToken = findNextToken();

            //Repeatedly search the file for each line token until EOF
            while (currentToken != null)
            {
                //End of file reached; scene file parsed successfully
                if (currentToken.EOF)
                {
                    if (currentScene.getCrypticObjectStorage().Count == 0)
                        sceneWarning("No objects in scene file", 0, 0);

                    CrypticGame.printMsg($"Successfully loaded scene \"{currentScene.getName()}\"");
                    return true;
                }

                //Run the current token
                switch (currentToken.label)
                {
                    case "name":
                        string parsedField = unformatStringField(currentToken.field);
                        if (parsedField != null)
                            currentObj.setName(parsedField);
                        break;
                    case "add_script":
                        if (initializeScript(game, currentToken.field) == null)
                            return false;
                        break;
                    default:
                        sceneWarning($"Unknown CrypticObject label \"{currentToken.label}\" encountered; skipping", currentToken.line, currentToken.character);
                        break;
                }

                currentToken = findNextToken();
            }

            //Unable to find the next token; scene failed to load  
            return false;
        }

        /// <summary> Finds the next label and field (token) in the scene file </summary>
        private static Token findNextToken()
        {
            bool isComment = false;
            bool labelFound = false;
            bool fieldFound = false;
            int line, character, fileIndex;
            bool inSpaceField = false; //A space field is a field that allowes spaces (quotes, vectors, etc.)
            char blockChar = '"';
            string field = "";
            string label = "";
            Token EOFToken;

            //Line, character, and fileIndex numbers persist through each token pass
            line = currentToken.line;
            character = currentToken.character;
            fileIndex = currentToken.fileIndex;

            //Read the file
            for (int x = fileIndex; x < currentFile.Length; x++)
            {
                //Start of comment line
                if (currentFile[x] == '#' && !inSpaceField)
                    isComment = true;

                if (!isComment && currentObj != null)
                {
                    //Check character in space block
                    if (inSpaceField)
                    {
                        if (currentFile[x] == blockChar && blockChar == '"')
                        {
                            //Backslash escapes quote
                            if (x > 0 && currentFile[x - 1] == '\\')
                                field = field.Substring(0, field.Length - 1);
                            else
                                inSpaceField = false;
                        }

                        else if(currentFile[x] == ')' && blockChar == '(')
                            inSpaceField = false;

                        else if(currentFile[x] == '\n')
                        {
                            sceneError("Unexpected newline character in field; failed to load scene", line, character);
                            return null;
                        }

                        field += currentFile[x];
                    }

                    //Check character normally
                    else
                    {
                        switch (currentFile[x])
                        {
                            case ':': //Colon marks end of label
                                label = label.Trim();
                                labelFound = true;
                                break;                            
                            case ' ': //Whitespace and newline mark end of field or are ignored (unless in label)
                            case '\n':
                                if (!labelFound && label.Trim() != "")
                                {
                                    string charType = (currentFile[x] == '\n' ? "newline" : "whitespace");
                                    sceneError($"Unexpected {charType} character in label; failed to load scene", line, character);
                                    return null;
                                }
                                else if (labelFound && field.Trim() != "")
                                {
                                    fieldFound = true;
                                    field = field.Trim();
                                }
                                break;
                            case '{': //Open bracket marks start of CrypticObject or CrypticScript block
                                if (!labelFound)
                                {
                                    string blockType = (addingScript ? "CrytpicScript" : "CrypticObject");
                                    sceneError($"Unexpected character '{{' in {blockType} block; failed to load scene", line, character);
                                    return null;
                                }
                                break;
                            case '}': //Close bracket marks end of CrypticObject or CrypticScript block
                                if (!labelFound)
                                {
                                    if (label.Trim() != "")
                                    {
                                        sceneError("Unexpected character '}' in CrypticObject block; failed to load scene", line, character);
                                        return null;
                                    }

                                    //End of CrypticScript block
                                    else if (addingScript)
                                    {
                                        addingScript = false;
                                        return new Token("", "", line, character + 1, x + 1);
                                    }

                                    //End of CrypticObject block
                                    else
                                    {
                                        if (currentObj.getName() == null)
                                            currentObj.setName("Default " + (currentScene.getCrypticObjectStorage().Count + 1));

                                        currentScene.addCrypticObject(currentObj);
                                        currentObj = null;
                                    }
                                }
                                break;
                            case '(': //Parentheses mark the start or end of a vector field
                            case '"': //Quotes mark the start or end of a string field
                                if (!labelFound)
                                {
                                    string blockType = (addingScript ? "CrytpicScript" : "CrypticObject");
                                    sceneError($"Unexpected character '{currentFile[x]}' in {blockType} block; failed to load scene", line, character);
                                    return null;
                                }
                                //Start of space block
                                else if (!inSpaceField)
                                {
                                    inSpaceField = true;
                                    field += currentFile[x];
                                    blockChar = currentFile[x];
                                }
                                break;
                            default:
                                if (labelFound)
                                    field += currentFile[x];
                                else
                                    label += currentFile[x];
                                break;
                        }
                    }

                    //Return the current label and field as a new Token
                    if (fieldFound)
                        return new Token(label, field, line, character, x);
                }

                //Keep track of line and char numbers
                if (currentFile[x] == '\n')
                {
                    line++;
                    character = -1;

                    //End of comment line
                    if (isComment)
                        isComment = false;
                }

                //Beginning of new CrypticObject block
                else if(currentFile[x] == '{' && currentObj == null)
                    currentObj = new CrypticObject();

                character++;
            }

            //End of file reached; return EOF token
            EOFToken = new Token("", "", 0, 0, 0);
            EOFToken.EOF = true;

            return EOFToken;
        }

        /// <summary> Parses and loads all tokens in a CrypticScript block. Returns an empty token with the current line and character position after parsing </summary>
        private static Token initializeScript(CrypticGame game, string script)
        {
            Type scriptType;
            CrypticScript scriptInstance;
            FieldInfo memberField;

            if (!script.StartsWith("\"") || !script.EndsWith("\""))
            {
                sceneError("Script name must be wrapped in double quotes; failed to load scene", currentToken.line, currentToken.character);
                return null;
            }

            addingScript = true;
            script = script.Substring(1, script.Length - 2);

            //Check plain user-passed script name first
            scriptType = Type.GetType(script);

            //Check CrypticEngine scripts second
            if (scriptType == null)
            {
                script = "CrypticEngine.Scripts." + script + ", CrypticEngine";
                scriptType = Type.GetType(script);
            }

            //Check user scripts (user-defined namespace and assembly) third
            if (scriptType == null)
            {
                script = game.getScriptNamespace() + script + game.getScriptAssembly();
                scriptType = Type.GetType(script);
            }

            //Specified script does not exist or does not have the full qualified name (namespace and assembly)
            if (scriptType == null)
            {
                sceneError($"Unable to find CrypticScript \"{script}\" (fully qualified name required); failed to load scene", currentToken.line, currentToken.character);
                return null;
            }

            //Script exists; check if the type is valid and attach to currentObject
            scriptInstance = Activator.CreateInstance(scriptType) as CrypticScript;

            if(scriptInstance == null)
            {
                sceneError($"Type \"{script}\" is not of type CrypticScript; failed to load scene", currentToken.line, currentToken.character);
                return null;
            }

            //Add the script if duplicates are allowed
            if(scriptInstance.getDuplicatesAllowed())
                currentObj.addScript(scriptInstance);
            else
            {
                var existingScript = typeof(CrypticObject).GetMethod("getScript").MakeGenericMethod(scriptType).Invoke(currentObj, null);

                //If the non-duplicatable script already exists, reassign scriptInstance to the exisitng script 
                if (existingScript != null)
                    scriptInstance = (CrypticScript)existingScript;
                else
                    currentObj.addScript(scriptInstance);
            }

            //Find start of script block
            currentToken = findNextScriptBlock();
            if (currentToken.fileIndex == -1)
            {
                sceneError("Unable to find start of CrypticScript block ('{' expected); failed to load scene", currentToken.line, currentToken.character);
                return null;
            }

            //Find the first token in the CrypticScript block
            currentToken = findNextToken();

            //Repeatedly search the CrypticScript block for each line token
            while (currentToken != null)
            {
                //End of file reached in the middle of script block; error
                if (currentToken.EOF)
                {
                    sceneError("Unexpeced EOF in CrypticScript block; failed to load scene", currentToken.line, currentToken.character);
                    return null;
                }

                //End of script block reached; return to normal token parsing
                if (!addingScript)
                    return currentToken;

                //Get the public field of the current script instance whose name matches the current label
                memberField = scriptType.GetField(currentToken.label);

                if(memberField == null)
                    sceneWarning($"Unable to find public field \"{currentToken.label}\" in CrypticScript \"{scriptInstance.GetType().ToString()}\"; skipping", currentToken.line, currentToken.character);
                else
                    parseMember(memberField, scriptInstance);
                
                currentToken = findNextToken();
            }

            return null;
        }

        /// <summary> Finds the next CrypticObject block in the scene file and returns the new Token </summary>
        private static Token findNextScriptBlock()
        {
            int line, character, fileIndex;

            line = currentToken.line;
            character = currentToken.character;
            fileIndex = currentToken.fileIndex;

            for (int x = fileIndex; x < currentFile.Length; x++)
            {
                if (currentFile[x] == '{')
                    return new Token(currentToken.label, currentToken.field, line, character + 1, x + 1);
                else if (currentFile[x] == '\n')
                {
                    line++;
                    character = -1;
                }

                character++;
            }

            return new Token("", "", 0, 0, -1);
        }

        ///<summary> Parses the user-passed token from the scene file and assigns it to the corresponding member of the script instance </summary>
        private static void parseMember(FieldInfo memberField, CrypticScript scriptInstance)
        {
            object fieldObj = memberField.GetValue(scriptInstance);
            switch (fieldObj)
            {
                case int _:
                    int parseInt;
                    if (!int.TryParse(currentToken.field, out parseInt))
                        sceneWarning($"Unable to parse field \"{currentToken.field}\" as type int; skipping", currentToken.line, currentToken.character);
                    else { memberField.SetValue(scriptInstance, parseInt); }
                    break;
                case long _:
                    long parseLong;
                    if (!long.TryParse(currentToken.field, out parseLong))
                        sceneWarning($"Unable to parse field \"{currentToken.field}\" as type long; skipping", currentToken.line, currentToken.character);
                    else { memberField.SetValue(scriptInstance, parseLong); }
                    break;
                case short _:
                    short parseShort;
                    if (!short.TryParse(currentToken.field, out parseShort))
                        sceneWarning($"Unable to parse field \"{currentToken.field}\" as type short; skipping", currentToken.line, currentToken.character);
                    else { memberField.SetValue(scriptInstance, parseShort); }
                    break;
                case float _:
                    float parseFloat;
                    if (!float.TryParse(currentToken.field, out parseFloat))
                        sceneWarning($"Unable to parse field \"{currentToken.field}\" as type float; skipping", currentToken.line, currentToken.character);
                    else { memberField.SetValue(scriptInstance, parseFloat); }
                    break;
                case double _:
                    double parseDouble;
                    if (!double.TryParse(currentToken.field, out parseDouble))
                        sceneWarning($"Unable to parse field \"{currentToken.field}\" as type double; skipping", currentToken.line, currentToken.character);
                    else { memberField.SetValue(scriptInstance, parseDouble); }
                    break;
                case string _:
                    string parseString = unformatStringField(currentToken.field);
                    if (parseString != null)
                        memberField.SetValue(scriptInstance, parseString);
                    break;
                case char _:
                    char parseChar;
                    if (!char.TryParse(currentToken.field, out parseChar))
                        sceneWarning($"Unable to parse field \"{currentToken.field}\" as type char; skipping", currentToken.line, currentToken.character);
                    else { memberField.SetValue(scriptInstance, parseChar); }
                    break;
                case bool _:
                    bool parseBool;
                    if (!bool.TryParse(currentToken.field, out parseBool))
                        sceneWarning($"Unable to parse field \"{currentToken.field}\" as type bool; skipping", currentToken.line, currentToken.character);
                    else { memberField.SetValue(scriptInstance, parseBool); }
                    break;
                case Vector3 _:
                    Vector3 parseVector3;
                    if (!TextHandler.tryParseVector3(currentToken.field, out parseVector3))
                        sceneWarning($"Unable to parse field \"{currentToken.field}\" as type Vector3; skipping", currentToken.line, currentToken.character);
                    else { memberField.SetValue(scriptInstance, parseVector3); }
                    break;
                case Vector2 _:
                    Vector2 parseVector2;
                    if (!TextHandler.tryParseVector2(currentToken.field, out parseVector2))
                        sceneWarning($"Unable to parse field \"{currentToken.field}\" as type Vector2; skipping", currentToken.line, currentToken.character);
                    else { memberField.SetValue(scriptInstance, parseVector2); }
                    break;
                case CrypticDraw.SpriteAnchor _:
                    CrypticDraw.SpriteAnchor parseAnchor;
                    if (!Enum.TryParse(currentToken.field, out parseAnchor))
                        sceneWarning($"Unable to parse field \"{currentToken.field}\" as type SpriteAnchor (enum); skipping", currentToken.line, currentToken.character);
                    else { memberField.SetValue(scriptInstance, parseAnchor); }
                    break;
                case Color _:
                    Color parseColor;
                    if (!TextHandler.tryParseColor(currentToken.field, out parseColor))
                        sceneWarning($"Unable to parse field \"{currentToken.field}\" as type Color; skipping", currentToken.line, currentToken.character);
                    else { memberField.SetValue(scriptInstance, parseColor); }
                    break;
                default:
                    if(fieldObj is null)
                        sceneWarning($"Unable to parse field \"{currentToken.field}\" (the type is null and must be initialized to access); skipping", currentToken.line, currentToken.character);
                   else
                        sceneWarning($"Unable to parse field \"{currentToken.field}\" (the type \"{fieldObj}\" is not supported); skipping", currentToken.line, currentToken.character);
                    break;
            }
        }

        /// <summary> Checks if a string field is wrapped in double quotes. Returns the unformatted string or null on error </summary>
        private static string unformatStringField(string s)
        {
            if (!s.StartsWith("\"") || !s.EndsWith("\""))
            {
                sceneWarning("String fields must be wrapped in double quotes; skipping", currentToken.line, currentToken.character);
                return null;
            }

            return s.Substring(1, s.Length - 2);
        }

        private static void sceneError(string error, int line, int character)
        {
            Console.WriteLine($"[Cryptic Engine] Error while reading scene file \"{currentScene.getName()}.txt\": {error} [line {line}, char {character}]");
        }

        private static void sceneWarning(string warning, int line, int character)
        {
            Console.WriteLine($"[Cryptic Engine] Warning while reading scene file \"{currentScene.getName()}.txt\": {warning} [line {line}, char {character}]");
        }
    }
}
