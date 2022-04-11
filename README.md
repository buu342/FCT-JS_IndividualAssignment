# Shell Time

Being a full-time super hero can be really boring if you have nothing to do. I just want some action! 

Shell Time is a run and gun side scrolling platformer, made in 2 weeks for the Jogos e Simulação subject at FCT, in the year 2021/2022. A build of the game is available in the [releases](../../releases) page.


## Controls

The game has a built-in tutorial covering the controls, but they will be written here just in case:
* D to move forward, A to move backwards
* W to jump (you can double jump)
* Aim with the mouse
* Pressing left click performs a melee attack which reflects projectiles. Holding it shoots your guns
* Right click slows down time, which can be used to aid in combat and give you more time to think

Besides the given controls, the following debug controls are available to aid the professors' evaluation:
* Pressing 1 moves you to Level1_1
* Pressing 2 moves you to Level1_2
* Pressing 3 moves you to Level1_3
* Pressing 4 moves you to Level1_Boss

Sometimes pressing the button will not jump to the correct level, so you'll need to press it again.
Jumping between levels can potentially break scripting... I'm going to be honest that I haven't had a lot of time to properly test it because it wasn't originally in the requirements PDF, I slapped it on last minute when I was told it was needed in the final practical lesson before the handout.


## How does the meet the requirements?

Hopefully it's clear, but I'm writing it here just in case:

#### Title
Shell Time

#### Minimal Story
Aliens are invading! Lets put a stop to that.

#### Winning Conditions
Defeat the boss at the end, and get the "Level Complete" screen.

#### Static and Mobile Platforms
There's 3 sets of moving platforms. In the construction site, in the hotel (the elevator), and in the alien ship.

#### Collectables
There are 3 hidden tokens, which are displayed in the level end screen

#### Power-ups
If you build up your streak (by killing and reflecting projectiles without taking damage in between), your character powers up. In the powered up state, your bullets penetrate enemies and do more damage. 
You also obtain a jetpack at one point, which completely changes your movement style.

#### State Variables
Besides your health, Shell Time (slow motion) takes up stamina which regenerates over time.

#### Mobile enemies
There's a bunch of enemy types, some that stand still, some that move between patrol points, some that fly, some that fly between patrol points. These enemies can either always aim at you, or aim in a single direction.
The boss also has a staggered movement pattern that occurs at every powerchord on the music's electric guitar.

#### Obstacles and hazards
Fire, pitfalls, and bullets.

#### Character animation
You'll get this in spades, don't worry.


## Asset Credits
Almost everything in this game was made by me from the ground up, except the following:
* Music was made by Hidenori Shoji, taken from the Yakuza franchise of games. I knew I wasn't going to have time to make music from scratch, so I borrowed...
* Some of the particle textures were not made by me, they were taken from a myriad of sources. Notably, Speed Brawl and Guilty Gear. I wanted to do these myself but ran out of time.
* Some sound effects were taken from some audio effect CDs I have (Most came from "1001 Sound Effects"), but I mixed and combined them myself.
* I use two shaders written by Ryan Ross. The [Toon](https://github.com/IronWarrior/UnityToonShader) shader and [Outline](https://github.com/IronWarrior/UnityOutlineShader) shader. I would have written them myself, but I didn't feel like wasting an entire day learning how to take my existing HLSL code and wrangle it to work with Unity's stuff (doesn't help that the shader documentation is a mess). I did have to make several tweaks to both shaders to get them to play nicely with each other, as well as adding in a cool video glitch effect which I learnt about by reading the source code in [this repository](https://github.com/keijiro/KinoGlitch).

Everything else, I made it. I have included the Raw Assets in the project's [Google Drive](https://drive.google.com/drive/u/1/folders/1LjgYdFYKpYOtT2UIXvhdY1lmiuzb3UDS). Feel free to use them for whatever you want, no credit needed! I license all my work as WTFPL :)


## Foreword
I treated this like a 2 week game jam, and I had a blast, despite torturing myself with the scope of the project (why did I think it was a good idea to make all the assets myself????). This was my first time making something "proper" in Unity, but not my first Game Development rodeo (I've been doing this since I was a kid, both professionally and not!). 

I do wish I had made some aspects of the story a bit clearer, but all I was able to do was tell it via environmental storytelling and via the title, which hopefully someone ought to pick up on? I'll add it here, just in case:

The game's title is both a play on the term ["Bullet Time"](https://en.wikipedia.org/wiki/Bullet_time), but it also refers to the character's ego (after all, main character is named Shell. As in, "it's Shell's Time to shine!"). Shell is an egomaniac. Her room is covered in articles about herself, and when the city gets attacked, she doesn't care that people are dying. She wants action! She wants to run around, shoot, and say a quip every chance she gets. She's grinning the entire game, and even the coins she picks up have her face on it. It's all about her.