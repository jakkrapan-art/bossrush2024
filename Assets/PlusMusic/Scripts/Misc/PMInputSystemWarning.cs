/* ---------------------------------------------------------------------------
Application:    PlusMusic Unity Plugin - Misc
Copyright:      PlusMusic, (c) 2023
Author:         Andy Schmidt
Description:    Missing Input System warning

TODO:
    Important todo items are marked with a $$$ comment

--------------------------------------------------------------------------- */

using UnityEngine;


namespace PlusMusic
{
    public class PMInputSystemWarning: MonoBehaviour
    {
        [TextArea(5, 10)]
        public string developerComments =
            "If you see a missing script warning above, your game is missing an input system.\n" +
            "In order for the mouse to work in our sample scenes, we require a working input system.\n" +
            "\n" +
            "Please go to your 'Package Manager', find the Unity 'Input System' and click install. " +
            "If prompted to restart Unity, click yes.\n" + 
            "The mouse should now work with our sample scenes.\n" +
            "\n";
    }
}
