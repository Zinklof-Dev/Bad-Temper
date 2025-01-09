Experimental Build E-2501A

Will kill non dev builds, will kill itself often, be cautious.

The following known issues also print to the console on start:

Wrong Variables cause exceptions that sometimes kill the Shell.
Missformating of () causes exceptions that sometimes kill the shell.
GPU Driver may crash on particularly severe exceptions due to an issue on unity's end with texture math.
Help command doesn't exist.
Suggested commands don't tab auto fill, and freak out once you start entering variables.
Console may spazz to the corner, zero clue why.
Log container doesn't scroll, code is in place, variables are not set yet.
Console causes slowdown on start, increases exponentially with the ammount of assemblies, classes, and methods you have, this is an issue with using reflection, and can only be worked around in the future.


Please seek me if you need help, i will give a micro lesson at the start of 7th period today (1/9/2025) assuming luigi and cole are there.

as usual to add to scene drag the ConsoleBase prefab into your canvas (ensure scale with screen size, x1920 y1080, match width (ensure's compliance with 19:10 aspect ratios) as the console was built for this)
ensure it is at the top of the console hiarchy to ensure it renders above everything else (Console is not yet forcing itself to being drawn last in render queue)


ADDITIONAL FINAL NOTE:
Though originally stated (iirc) that legacy commands would auto work with V2, They do not, at least as of now, this will not be a primary concern, though will be on the back burner for additions to the console once 1.0 releases