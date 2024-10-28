You are assigned as a modeler, and programmer.

I'd offer the day(s) off for a bit, but you take them off anyways so heres something else to do

using the zinklofdev.utils library place a bunch of cubes randomly in a scene, and ensure they do not clip.

i know this will be daunting at first read, but its pretty simple. 

poisson disc sampling is a neat little method of placing random dots that don't intersect, and stay x distance away from each other... it's also a super easy way to crash your game so be careful.

the utils library has a poisson disc sampling function that will do all the hard work for you and return a list of vector 2's that meet the qualifications you give it.

all you need to do is make logic to place cubes at these vectors, and ensure that they follow terrain height changes.

once you have some cubes getting placed, we can plug in our trees and boom! procedurally generated trees! now adding rocks will be tricky, because we need to ensure that they don't clip with trees right? eh not really, just slap those fools down too. 

since we don't have any new game button yet, you will have to rely on the command system to begin generation, and i'd suggest making the trees and rocks two seperate commands.

if you manage to complete that, good job, inform me, and i'll think of something else for you