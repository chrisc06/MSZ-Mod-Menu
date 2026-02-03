# MiSide Zero – Mod Menu (Mobile & PC)

This is a mod menu I built for the Unity-based fan game **MiSide Zero**.  
It runs directly inside the game and gives you a bunch of tools for controlling gameplay, debugging things, and messing around with the world on both **Android and PC**.

The whole thing runs as a persistent Unity script that hooks into the game at runtime, no access to the original source code needed.

Note: This project was developed for a local single-player Unity game with explicit developer permission, for educational and tooling purposes.

---

## What it can do

### In-game menu
- Simple, touch-friendly UI for mobile  
- Collapsible categories so it’s not cluttered  

### Player & movement
- Change movement speed  
- Fly mode and noclip  
- Slow down or speed up time  
- Free movement using Rigidbody controls  

### Mita control
- Take control of Mita (camera + movement)  
- Switch between Idle / Follow / Chase  
- Control animation values  
- Teleport the player to Mita  

### Spawning & scenes
- Clone the player or Mita  
- Clone any object in the scene  
- Load any scene from the build  
- Browse and search all scene objects and spawn them  

### Debug & performance
- Live FPS and memory display  
- Frame-time graph  
- Automatically disable far-away renderers to improve performance  
- Refresh renderer cache while playing  

### World interaction
- Tap objects to delete them  
- Force-trigger interactions using raycasts  

---

## How it works

The mod runs as a single `ModMenuManager` MonoBehaviour that stays alive when scenes change.  
It finds important game objects at runtime (player, camera, NPCs, etc.) and then applies changes like:

- Moving characters and cameras  
- Changing animation parameters  
- Spawning and destroying objects  
- Enabling and disabling renderers  
- Loading scenes  
- Reading frame time and memory usage  

Everything is done live while the game is running.

---

## Structure

The menu is split into feature groups (movement, visuals, spawner, Mita control, etc.), so it’s easy to add new things or turn features on and off without breaking others.

---

## Platforms

- **Android** (touch controls)  
- **PC** (mouse + keyboard)  

The UI scales automatically so it works on different screen sizes.

---

## Why I made this

It’s basically a playground for exploring and extending a Unity game from the inside.
