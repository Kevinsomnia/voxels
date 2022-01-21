# Voxels

A simple and super-quick voxel renderer prototype developed in the Unity engine. Many optimizations can definitely be made, but I only finished the low-hanging fruit / obvious optimizations like face culling.

This was a prototype that was developed in a few days, so don't expect high quality code or implementation.

## Features

- Chunks system.
- Texture atlas, single material renderer
- Automatic culling for interior/invisible faces
- Max size of chunk "world" is 4.91 km^3. Can certainly be bigger, but not designed to be a nearly-infinite procedural world (Minecraft).

## Controls
- WASD/Control/Spacebar: Camera movement.
- Move mouse around for camera look.
- Left Mouse: Place/delete/paint block. Hold and drag to target multiple blocks.
- Left Shift: Planar mode (2.5D, or wall/floor mode)
- 1/2/3: Change brush mode to Place/Delete/Paint respectively.
