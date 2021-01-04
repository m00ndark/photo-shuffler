using System;
using System.IO;
using Newtonsoft.Json;

namespace PhotoShuffler.Model
{
	internal class FileData
	{
		public FileData(string filePath, DateTime fileDate, string destinationPath)
			: this(filePath)
		{
			FileDate = fileDate;
			DestinationPath = destinationPath;
		}

		public FileData(string filePath, string error)
			: this(filePath)
		{
			FileDate = DateTime.MinValue;
			Error = error;
		}

		private FileData(string filePath)
		{
			FileName = Path.GetFileName(filePath);
		}

		public string FileName { get; set; }
		public DateTime FileDate { get; set; }
		[JsonIgnore] public string DestinationPath { get; set; }
		public string Error { get; set; }

		public bool Valid =>
			Error == null;

		public string DestinationFilePath =>
			Valid ? Path.Combine(DestinationPath, FileDate.Year.ToString(), FileDate.ToString("yyyy-MM-dd"), FileName) : null;
	}
}
