namespace OrtzIRC
{
    using System;
    using System.Windows.Forms;

    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            CompositionRoot.Wire(new Bindings());
            var form = CompositionRoot.Resolve<MainForm>();
            Application.Run(form);
        }
    }
}