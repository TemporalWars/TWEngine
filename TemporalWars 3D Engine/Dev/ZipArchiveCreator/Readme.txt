Hello and welcome to the ZipArchiveCreator for XNA.

Here is what you need to do to make this work:

First, create a post build event for your content project. In this solution, we use the post build event in the actual game, but for large projects you will probably want to create a solution that just builds the content, and then the game solution can just load the output from the content solution.

Here is the post build event for this test game:
$(ProjectDir)..\bin\debug\ZipArchiveCreator.exe "$(TargetDir)*.xnb|$(ProjectDir)Text\*.txt" $(OutputDir)Content.zip $(ProjectDir)ResourceId.cs TestContentGame ResourceId $(ProjectDir)Audio\Sound.xap

This looks complicated at first, but it isn't too bad.

The first argument is of course the .exe that is the zip archive program. $(ProjectDir) is the root directory for the test content game.

The second argument "$(TargetDir)*.xnb|$(ProjectDir)Text\*.txt" contains two directories (pipe delimited). These directories are where the program will search to get content to add to the .zip file (including sub-directories). $(TargetDir) is where the game .exe and all .xnb files get built to.

The third argument $(TargetDir)Content.zip specifies where the tool should output the .zip file.

The fourth argument is where the tool puts the gernated C# code for the resource constants. This is the file to use when you pass in the assetName parameter to the Load<T> method of the ZippedContent ContentManager class. 

The fifth argument is the namespace that the resource constants class will be in.

The sixth argument is the class name for your resource constants.

The seventh and final argument is optional and specifies your Xact project, and all Cue instances will have constants generated as well, and if the cue has any notes / comments saved in the Xact project, these will display in the intellisense. No two cues can have the same name.

Once you execute a build, the tool will run and output information into the debug window. Then in your game you can create a ZippedContent object to read from the .zip file that is output from the tool.

You might have noticed the .description.txt files that match the resource name of their counterpart files (i.e. for earth.jpg there is an earth.jpg.description.txt that is set to copy to the output directory). These files will have their text added to the intellisense generated for that resource constant, so when you hover over the resource in code, you will see the text from these files.

Here's how to use the tool in your game class:

ZippedContent content;
...
Somewhere in your constructor: 
content = new ZippedContent("content.zip", Services);

After that you can load your objects like this:
Texture2D earth = content.Load<Texture2d>(ResourceId.Earth);

You can see here that the constant for the texture of earth is being passed in from the auto-generated resource constants class. This prevents run time errors where your resource string is wrong. Isn't having compile time checking of your resource strings nice?

You can then play your Xact cues like this (if you specified an Xact project):
soundBank.PlayCue(ResourceId.XactCue_ButtonClick);


That's all for now, please visit www.codeplex.com/XnaZip if you want the latest updates and information.

Thanks!

-Jeff Johnson