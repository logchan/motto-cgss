# motto-cgss

An IDOL M@STER CINDERELLA GIRLS STARLIGHT STAGE (CGSS) implementation with more features and fun.

[中文Readme](https://logu.co/motto-cgss/)

> Unfortunately, this Git repository has been re-created due to some messed up solutions

## Project Overview

motto-cgss is a rhythm game project inspired by [CGSS](http://cinderella.idolmaster.jp/sl-stage/) and [osu!](http://osu.ppy.sh) to provide

1. Game experience similar to CGSS, with
2. Modifiers(mods) and customizations to have more fun!

It consists of several smaller projects:

#### motto-cgss Unity

The unity project (`unity/motto-cgss`) is the core of motto-cgss, the rhythm game. It's more than a CGSS simulator (see *Project Highlights*).

At the moment, it can be used to preview a game play. Some mods are already available: 

1. **Hidden** Notes fade out before they reach the ends.
2. **DoubleTime** Everything goes faster! *FYI: DT is 1.5x speed, :P*
3. **HalfTime** You know what it means.
4. **Skip** some time. Check the hard part directly!

`HDDT` in CGSS!

For development progress, check out [Project Progress](https://github.com/logchan/motto-cgss/wiki/project-progress) page.

#### cgss2motto

It is infeasible to handcraft a beatmap file. This tool comes in helping! It converts an original CGSS beatmap (CSV) to a motto-cgss beatmap.

#### osu2motto

Want more maps? This tool converts osu! beatmaps to motto-cgss beatmaps! Excited!

*Calm down.* The tool has not been implemented yet. Will do after the game is playable.

## Project Highlights

This project is designed to provide more than CGSS.

#### Beatmap

The beatmap format is easier to parse and extend. Also, for each note it stores *beats* instead of *time*. This makes editing more accurate.

#### Skin

Every element in the game play is customizable. Like osu!, the game will support skin selection.

#### More

The game can support different numbers of buttons instead of just 5. Theoretically. Not tested yet but it should work.

## Information

[Project Progress](https://github.com/logchan/motto-cgss/wiki/project-progress)

[Beatmap Structure](https://github.com/logchan/motto-cgss/wiki/beatmap)

[Beatmap Format](https://github.com/logchan/motto-cgss/wiki/beatmap-file)

[Skin](https://github.com/logchan/motto-cgss/wiki/skin)

Development Documentation (preparing, will come with first release)

#### Important things

1. The repository contains certain beatmap files. However, the audio file is excluded for copyright reasons. Find them on your own if you wish to try. Remember to adjust the offset.

## Credits

Most of the code is my own work, including data structure design, flow control and others, except:

1. The path function of notes (`x(t)` and `y(t)`) is from the original CGSS game

Also, the major game experience comes from CGSS, while the modifiers come from osu!.

## Recommendations

If you just wish to play different songs in CGSS, you can also try:

[DereTore](https://github.com/hozuki/DereTore) - Music and score authoring toolkit for Idolmaster Cinderella Girls Starlight Stage (CGSS/DereSute). / 偶像大师灰姑娘女孩星光舞台音乐&谱面制作工具箱

## License

*Before the first release of the project*: you can view the source code. You can distribute the original source code but you are required to credit the author at the same time. You shall not distribute modified source code. You can compile the source code and run the binaries, but you shall not distribute the binaries. You shall not use any source files of this project in other software.

The license will change after the first release of the project.

*THE SOURCE CODE AND THE SOFTWARE COMES WITH ABSOLUTELY NO WARRANTY. UNDER NO CIRCUMSTANCE WILL THE AUTHOR BE RESPONSIBLE FOR ANY CONSEQUENCES CAUSED BY USING, DISTRIBUTING, COMPILING THE SOFTWARE OR BY RUNNING THE COMPILED BINARIES.*
