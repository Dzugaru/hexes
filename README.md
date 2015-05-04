# Hexes
Hexes is a (maybe) world first [Unity3D](http://unity3d.com/) game with core logic written in [D](http://dlang.org/). It still uses C# code for graphics front-end though.
For now it's just an experiment, but it aims to be a finished roguelike on Windows.

##### Features (for a programmer)
* Great separation of logic and UI, D part could be used on a multiplayer game server eventually
* PInvoke is used for interop, including callbacks from native to managed code
* Aim to little or no memory garbage generated in D part, this means using freelists and non-allocating data structures like single-linked lists
* D coroutines - fibers - are used to write complex logic like spells

![Screeshot](https://monosnap.com/file/gS7c3bMYUSveAm3KLAtLO0cZRebf1E.png)
