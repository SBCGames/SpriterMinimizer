# Spriter Minimizer
 Minimize XML output from Spriter animation program.

 [Spriter](http://www.brashmonkey.com) output is either in .xml (.scml) or .json (.scon) format. Both formats are rather verbose. For example setting timeline to zero means, that timeline="0" has to be in .xml file.

 SpriterMinimizer translates default Spriter elements and attributes names to ones you define in another .xml file. Of course, it means you have to adjust your Spriter loader in your game.

 Currently only .xml is supported.

Usage:
```
SpriteMinimizer inputFile [outputFile] [-defs definitions] [-prettyPrint] [-binary] [-smallOffset]

inputFile - file with Spriter .xml (.scml) animation to minimize
outputFile - file for minimized output (default = inputFile_out)
-defs definitions - .xml file with old and new names (default = definitions.xml)
-prettyPrint - make output nice and readable - good for checking if everything is OK or debug (default = false)
-binary - convert to binary output
-smallOffset - use short instead of int in binary format - can save around 10% of space
```

 Default file with definitions is definitions.xml - see its structure. When .xml/.scml output is minimized, definition.json is also output. Use it in your Spriter player to translate minimized tag names to full Spriter .xml tag names.


### Results

 Results for Hero.scml test file:
 - 346 kb standard, pretty printed, Spriter output,
 - 240 kb Spriter output with pretty printing unchecked,
 - 165 kb after minimization with SpriteMinimizer (with default definitions.xml),
 - 74 kb when converted into binary format with -smallOffset switch on.
 
 Savings between binary and Spriter output without pretty printing is 70%.

 Of course you need to write your own loader to load binary output.
 
