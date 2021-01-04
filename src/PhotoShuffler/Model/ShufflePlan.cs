using System;
using System.Collections.Generic;

namespace PhotoShuffler.Model
{
	internal class ShufflePlan
	{
		public ShufflePlan(Configuration config)
		{
			Config = config;
		}

		public Configuration Config { get; }
		public IDictionary<string, FileData> Files { get; } = new Dictionary<string, FileData>();

		public void Add(string filePath, DateTime fileDate)
		{
			Files.Add(filePath, new FileData(filePath, fileDate, Config.DestinationPath));
		}

		public void AddInvalid(string filePath, string error)
		{
			Files.Add(filePath, new FileData(filePath, error: error));
		}
	}
}
