namespace TESVSnip.TranslateUI
{
    using System.Globalization;
    using System.Resources;
    using System.Threading;
    using System.Windows.Forms;

    public static class TranslateUiGlobalization
    {
        public static string CultureCode = string.Empty;

        private static string resourcesPath = string.Empty;

        public static ResourceManager ResManager { get; private set; }

        /// <summary>
        /// Globalize Application
        /// </summary>
        public static void GlobalizeApp()
        {
            SetCulture();
            SetResource();
        }

        /// <summary>
        /// Set Culture
        /// </summary>
        private static void SetCulture()
        {
            CultureInfo objCi = new CultureInfo(CultureCode);
            Thread.CurrentThread.CurrentCulture = objCi;
            Thread.CurrentThread.CurrentUICulture = objCi;
        }

        /// <summary>
        /// Set Resource from file
        /// </summary>
        private static void SetResource()
        {
            resourcesPath = System.IO.Path.Combine(Application.StartupPath, "Lang");
            ResManager = null;
            ResManager = ResourceManager.CreateFileBasedResourceManager("resource", resourcesPath, null);
        }
    }
}