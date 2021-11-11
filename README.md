# Kaboooom
A simple 'mine sweeper' game with computer aided evaluation

Game is based on code written by A. Lane in Dr Dobbs Journal (DDJ) #163,
April 1990.  Code originally converted to Turbo Pascal on 1990-03-18, then
converted again to OS/2 Sibyl (Delphi-like Object Pascal) 1997-07-26.
Completed rewrite into C# on 2021-11-01.

Game includes a built in evaluation process that, is ok, if started from a
position that is pretty open (0 mines around current position).  Also can
be played via keyboard or mouse.

Game is dependent on GameStatistics library and is needed if re-compiling
game.  Last updated/compiled using VS 2019.
