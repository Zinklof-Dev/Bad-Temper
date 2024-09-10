You are assigned as a programmer, and if really needed modeling.

assignments:
Wave system basics, for now just a console command that forces the wave to start, should be ENTIRELY independent of the update function, this will become the brains of the game and manage everything for waves later.
ideally this basic form should remember what wave its on, increase when told, have an event that other scripts can subscribe to if needed, be entirely static, and have the ability to reset on demand to avoid static variables
staying at their value between scenes.

once thats done, change it so that its capable of ending a wave, but not immedietly starting one, when a wave is ended this should contact a basic day-night cycle, causing the sun to rise, take 2 minutes to set.
once the sun sets a new wave should automatically begin. ideally for now just use a console command to end the wave by force, and use a wave ended event that the day-night script is attached to in order to know when the wave
ends. the wave manager and day-night cycle could keep track of how long its been seperatley, but ideally the wave manager or day-night script would keep track, and the other would just listen to it. (ideally wave manager
keeps track as this is basically the "server")
