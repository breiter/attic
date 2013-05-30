using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

class Program
{
	public static void Main(string[] args)
	{
		string handler = args[0];
		
		if(args.Length == 3) Process.Start(handler, string.Format("\"{0}\"", args[2]));
		else if(args.Length > 3)
		{
			string dirplus = args[2];
			int index = dirplus.LastIndexOf("\\");
			var dir = new DirectoryInfo(dirplus.Substring(0,index));
			var filepart = dirplus.Substring(index + 1);
			var expression = filepart + "\\s+" + string.Join("\\s+", args.Skip(3));

			var names = dir.GetFiles(filepart + "*").Select(x => x.Name);
			var files = names.Where(x => Regex.IsMatch(x, expression));
			if(files.Count() > 1)
			{
				MessageBox.Show(
					string.Format("{0} possible matches found with file names that differ only by spaces between words.\r\nChoosing the first match.", files.Count()), 
					"Warning", 
					MessageBoxButtons.OK, 
					MessageBoxIcon.Warning);
			}	
				 
			Process.Start(handler, string.Format("\"{0}\"", files.First()));
		}
	}
}
