You are assigned as a Musician, Programmer, and Modeler. in that order of importance.

assignments:
take the perlin noise tree gen, and separate the start function into different functions.

then begin to generate a secondary perlin map, and use it alongside another poisson sample to place rocks around the map.

once this is done, we will add RNG to place different tree models, and different rock models.

once that is all done. we will establish a static string (I will likely have this set up by that point) that will have a function to change its value. you will update this string every step of the way, this string will end up displaying on the loading screen to inform the player what's happening.

we will then consider our best option to network these objects. likely sending information only once to have every client place them, then send information to destroy them when they are harvested.