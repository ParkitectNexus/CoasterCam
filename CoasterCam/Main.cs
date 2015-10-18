using UnityEngine;

namespace CoasterCam
{
    public class Main : IMod
    {
        private static GameObject go;

        public void onEnabled()
        {
            go = new GameObject();

            go.AddComponent<CoasterCam>();
        }

        public void onDisabled()
        {
            Object.Destroy(go);
        }

        public string Name { get { return "CoasterCam"; } }
        public string Description { get { return "Camera for riding coasters"; } }
    }
}
