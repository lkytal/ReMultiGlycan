﻿using System;
using System.IO;

namespace COL.ReMultiGlycan
{
	public static class Logger
	{
		public static void WriteLog(string argMsg)
		{
			var LogWriter = new StreamWriter(System.Windows.Forms.Application.StartupPath + "\\log.txt", true);
			LogWriter.WriteLine(DateTime.Now.ToString("MMdd HH:mm") + "\t\t" + argMsg);
			LogWriter.Close();
		}
	}
}