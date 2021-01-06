using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PhotoShuffler.Model
{
	internal class FileData
	{
		public FileData(string filePath, DateTime fileDate, Configuration.Job job)
			: this(filePath)
		{
			FileDate = fileDate;
			Job = job;
			DestinationFilePath = GetDestinationFilePath();
		}

		public FileData(string filePath, string error)
			: this(filePath)
		{
			Error = error;
		}

		private FileData(string filePath)
		{
			SourceFilePath = filePath;
			FileName = Path.GetFileName(filePath);
		}

		public string SourceFilePath { get; }
		public string FileName { get; }
		public DateTime FileDate { get; }
		[JsonIgnore] public Configuration.Job Job { get; }
		public string Error { get; }
		public bool Valid => Error == null;
		public string DestinationFilePath { get; }

		private string GetDestinationFilePath()
		{
			string destinationFilePath = Job.DestinationPath;
			destinationFilePath = Regex.Replace(destinationFilePath, "%date:([^%]+)%", match => FileDate.ToString(match.Groups[1].Value));

			const string fileNamePattern = "%file:name%";
			if (Regex.IsMatch(destinationFilePath, fileNamePattern))
			{
				destinationFilePath = Regex.Replace(destinationFilePath, fileNamePattern, Path.GetFileNameWithoutExtension(FileName));
				destinationFilePath = Regex.Replace(destinationFilePath, "%file:ext%", Path.GetExtension(FileName));
			}
			else
			{
				destinationFilePath = Path.Combine(destinationFilePath, FileName);
			}

			return destinationFilePath;
		}
	}
}
