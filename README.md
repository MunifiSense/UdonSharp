# UdonSharp (Muni's version)

This is **my maintained fork** of [Merlin's UdonSharp](https://github.com/MerlinVR/UdonSharp), based on the VRChat Worlds SDK integration. I'm trying to keep this repo updated because:

- **VRChat** doesn't seem to be adding support for **Collections** in their bundled UdonSharp.
- **Merlin** doesn't seem to be actively updating his upstream repo either.

My goal is **feature parity with VRChat's shipped UdonSharp** while also having:

- **Collections** support (e.g. `List<T>`, dictionaries, and related APIs where the compiler allows them).
- Access to **non-UdonSharp** Unity / VRChat types where appropriate for world development.

There are no formal releases here — I use this repo directly in my own projects. I'll **try** to fix bugs and keep things working when I can; if something breaks, open an issue on this repo. That said, this is a personal fork — **use at your own risk**, and please don't blame me if it breaks your project.

For problems with VRChat's bundled UdonSharp or the Worlds SDK itself, take it up with **VRChat** (support, feedback, Canny, etc.) — their public community repo for UdonSharp isn't kept up to date either.

---

## Installation

**Make a backup of your project first.** If Unity mishandles importing packages like this, things can go badly wrong.

1. **Close** your Unity project.
2. Confirm your project is **backed up**.
3. Go to `Packages/com.vrchat.worlds/Integrations` and **delete** the `UdonSharp` folder and its `UdonSharp.meta` file (VRChat's bundled copy).
4. **Download this repository** (clone or "Download ZIP" from GitHub — use the full source, not random assets GitHub might list elsewhere).
5. Copy the **`Packages/com.merlin.UdonSharp`** folder from this repo into the **`Packages`** directory of your Unity project (merge/replace if you already had an older copy).
6. **Open** your project again. If everything imported cleanly, it should behave like before, with the updated compiler and features from this fork.

---

## Credits

- Original UdonSharp by [Merlin](https://github.com/MerlinVR/UdonSharp)
- VRChat Worlds SDK integration maintained separately by VRChat
