You are assigned as a Musician, Programmer, and Modeler. in that order of importance.

assignments:
improvements to mock menu, speak to team lead directly for vision on this.

player settings class, this should be static, and include variables you'd expect for settings, from username, sensitivity, etc. etc. this will hook into a settings menu later, and other menus.. then will eventually hook into a persistence system to save these settings to disc.

server/host sided spawn of an object at 0,0,0 that will be our enemy main attack point, like the castle in GB was, this would have a health bar that is synced between players so everyone can see it (BUT ONLY HOST CAN EDIT!) auto regen which the pace of doesn't matter yet as we have yet to do any balancing, and a function that can be called to damage the structure which would eventually be called by attacking enemies.