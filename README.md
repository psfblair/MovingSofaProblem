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

      `git submodule add https://github.com/Microsoft/MRDesignLabs_Unity_Tools.git ./MRDesignLab`

      This will add HUX and related tools under Assets/MRDesignLabsUnityTools/ folder.

      To update the submodule you'll still need to pull from master by either going into the individual 
      submodule directory and doing a `git pull` or by doing the following command to do pulls on 
      all submodules:

     `git submodule foreach git pull`
     
      If you need to make tweaks for something that is broken in the MR Design
      Tools, I suggest doing the following (and submitting a pull request):

```bash
      cd Assets/MRDesignLab
      git checkout -b minor_tweaks
```

  3. The HoloToolkit is already in this repository, so you shouldn't need to import it.

  4. Create a folder named Assets/SampleRoom and a sample room mesh named
      SampleSpatialMapping.obj to it, if you want a sample room.

  5. If you're working on macOS, the build settings should be PC, Mac and Linux standalone
     with a Windows build.