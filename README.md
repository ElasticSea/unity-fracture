#### In order to have destructible geometry, what do we actually need ?

##### A naive approach would be something like this:
1) Cut the mesh to chunks
2) Add rigidbodies
3) ?????
4) Profit

##### How do we cut the mesh ?
I started reading up on Boolean operations and Voronoi. It got complicated pretty quickly, there are bunch of assets on the store, but either they don't work or are slow. Luckily I stumbled upon this thread: https://forum.unity.com/threads/nvidia-blast.472623/, someone figured out how to use Nvidia blast library in the unity. It gets pretty straightforwards after this. You just feed the mesh to this library and recieve chunks.

##### We have chunks let's add rigidbodies and let's the physics play out.
Add the rigidbodies and let the physics play out. Since there is no connection between the rigidbodies, the wall crumbles under its own weight.

##### Problem #1: The chunks are not connected so the wall crumbles.
Lets connect the chunks with fixed joints, so they stay in place. Tak each chunk and take its neightbours (chunks that are in close proximity or is touching it) and connect them with fixed joints. This is not ideal, the joints are not 100% fixed in place due to how Physx handles collision. There is a big deal of springiness in the joints. The wall ripples when force is applied.

##### Problem #2: The wall is wobbly, it does not act like a wall, it looks like its made of jello.
After reading a bit I found out this forum post:https://answers.unity.com/questions/230995/fixed-joint-not-really-fixed.html and used the second answer with freezing. Freezing the rigidbodies makes them stay in place (they kinda drift apart after some time, but let's not worry about that now) and the rigidbodies still register impact and the joints can be broken, the problem is that rigidbodies will stay in place even though there is no support under them. They will only resume movement when fixed joints are broken.

##### Problem #3: Chunks can float in the air without any support.
Let's create some sort of graph of connected chunks, if a chunk does not have a connection to kinematic body, we will unfreeze it. Get all chunks and create a two way connection between the neightbours. For each kinematic chunk go through all neighbours and do the same for each neightbour. The chunks that were not traversed, are not connected with any kinematic body, that means they are in free fall and should be unfreezed.
