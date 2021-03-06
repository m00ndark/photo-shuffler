﻿using System;
using System.Collections.Generic;

namespace PhotoShuffler.Model
{
	internal class ShufflePlan
	{
		public ShufflePlan(Configuration configuration)
		{
			Configuration = configuration;
		}

		public Configuration Configuration { get; }
		public List<FileData> Files { get; } = new List<FileData>();

		public void Add(string filePath, DateTime fileDate, Configuration.Job job)
		{
			Files.Add(new FileData(filePath, fileDate, job));
		}

		public void AddInvalid(string filePath, string error)
		{
			Files.Add(new FileData(filePath, error));
		}
	}
}
