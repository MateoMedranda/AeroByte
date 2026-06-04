namespace AeroByte.UI_System
{
    public static class CameraViewHudRules
    {
        public static bool ShouldShowCenterIndicator(int cameraIndex)
        {
            return cameraIndex != 0 && cameraIndex != 1 && cameraIndex != 4;
        }
    }
}
