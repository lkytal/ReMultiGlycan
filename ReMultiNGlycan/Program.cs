using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace COL.MultiGlycan
{
	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			//string XcaliburKeyX64 = @"CLSID\{1d23188d-53fe-4c25-b032-dc70acdbdc02}\InprocServer32";  //X64
			////string XcaliburKeyX32 = @"Wow6432Node\CLSID\{1d23188d-53fe-4c25-b032-dc70acdbdc02}\InprocServer32"; //X32
			//RegistryKey X64 = Registry.ClassesRoot.OpenSubKey(XcaliburKeyX64);
			////RegistryKey X32 = Registry.ClassesRoot.OpenSubKey(XcaliburKeyX32);

			//if (X64 == null)
			//{
			//    MessageBox.Show("Xcalibur Library is not installed. Please install 32 bits MSFileReader or Xcalibur", "Library is not detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			//    Environment.Exit(1);
			//}

			if (!File.Exists(Environment.CurrentDirectory + "\\Default_Combination.csv"))
			{
				Assembly assembly = Assembly.GetExecutingAssembly();
				Stream Out = assembly.GetManifestResourceStream("COL.MultiGlycan.Resources.Default_Combination.csv");
				Stream inStream = new FileStream(Environment.CurrentDirectory + "\\Default_Combination.csv",
					FileMode.Create);
				if (Out != null)
				{
					Out.CopyTo(inStream);
					inStream.Close();
					Out.Close();
				}
			}
			Application.Run(new frmMainESI());
		}
	}
}