![](fractured_wall.gif)

#### In order to have destructible geometry, what do we actually need?

##### A naive approach would be something like this:
1) Cut the mesh to chunks
2) Add rigidbodies
3) ?????
4) Profit

##### How do we cut the mesh?
I started reading up on Boolean operations and Voronoi. It got complicated pretty quickly, there are a bunch of assets on the store, but either they are slow, buggy or don't work at all. Luckily I stumbled upon this forum thread: https://forum.unity.com/threads/nvidia-blast.472623. Someone figured out how to use Nvidia blast library in the Unity. It gets pretty straightforward after this. You just feed the mesh to this library and receive chunks.

##### Add rigidbodies to chunks and let the physics play out.
Since there is no connection between the rigidbodies, the wall crumbles under its own weight.

##### Problem #1: The chunks are not connected, so the wall crumbles.
Let us connect the chunks with fixed joints, so they stay in place. Take each chunk and its neighbors (chunks that are in close proximity or in touch) and connect them with fixed joints. This is not ideal, the joints are not 100% fixed in place due to how PhysX handles collisions. There is a big deal of springiness within the joints. The wall ripples when force is applied.

##### Problem #2: The wall is wobbly, it does not act like a wall and it looks like its made of jello.
After reading a bit I found this forum post:https://answers.unity.com/questions/230995/fixed-joint-not-really-fixed.html where somebody advised OP to freeze the rigidbody when it has joints on it. Freezing the rigidbodies makes them stay in place (they kinda drift apart after some time, but let's not worry about that now) and the rigidbodies still register impact and the joints can be broken. The problem is that rigidbodies will stay in place even though there is no support under them. They will only resume movement when all joints are broken.

##### Problem #3: Chunks can float in the air without any support.
Let's create a graph of connected chunks. If a chunk does not have a connection to kinematic body, we will unfreeze it. Get all the chunks and create a two-way connection between the neighbours. Recursivelly travers all kinematic chunk neighbours. The chunks that were not traversed are not connected with any kinematic body. That means they are in free fall and should be unfrozen.
