# MovingSofaProblem

HoloLens project to assist in figuring out how to move furniture out of a room.

## Setup

Before opening this project in Unity:

  1. Download the HoloSymMDL2.ttf font (used by the MR Design tools), read the
      license, and add it under Assets/Fonts. You can get it here:
      "http://download.microsoft.com/download/3/8/D/38D659E2-4B9C-413A-B2E7-1956181DC427/Hololens font.zip"

  2. The MR Design Tools have already been added to .gitmodules as a submodule
      but it still needs to be reinitialized in your project on your first
      checkout by running the following command in the Assets folder:

      `git submodule add https://github.com/Microsoft/MRDesignLabs_Unity_Tools.git ./MRDesignLabsUnityTools`

      You may need to make the following tweak for something that is broken in
      the MR Design Tools at the time of this writing:

```bash
      cd Assets/MRDesignLabsUnityTools
      git checkout -b minor_tweaks

      # Now in  HUX/Editor/StartupChecksWindow.cs
      # comment out the two lines that read
      # HUXEditorUtils.Header("Fonts Missing");
```

  3. HoloToolkit 1.5.7.0 is already in this repository, so you shouldn't need
      to import it.

  4. Create a folder named Assets/SampleRoom and a sample room mesh named
      SampleSpatialMapping.obj to it, if you want a sample room.
