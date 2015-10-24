# Spriter Minimizer
 Minimize XML output from Spriter animation program.

 [Spriter](http://www.brashmonkey.com) output is either in .xml (.scml) or .json (.scon) format. Both formats are rather verbose. For example setting timeline to zero means, that timeline="0" has to be in .xml file.

 SpriterMinimizer translates default Spriter elements and attributes names to ones you define in another .xml file. Of course, it means you have to adjust your Spriter loader in your game.

 Currently only .xml is supported.

Usage:
```
SpriteMinimizer inputFile [outputFile] [-defs definitions] [-prettyPrint]

inputFile - file with Spriter .xml (.scml) animation to minimize
outputFile - file for minimized output (default = inputFile_out)
-defs definitions - .xml file with old and new names (default = definitions.xml)
-prettyPrint - make output nice and readable - good for checking if everything is OK or debug (default = false)
```

