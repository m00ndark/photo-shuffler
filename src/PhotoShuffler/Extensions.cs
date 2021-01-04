using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace PhotoShuffler
{
	internal static class Extensions
	{
		public static bool TryGetFileNameDateTime(this string imageFilePath, out DateTime dateTime)
		{
			dateTime = DateTime.MinValue;

			string imageFileName = Path.GetFileNameWithoutExtension(imageFilePath);
			Match imageFileNameMatch = Regex.Match(imageFileName, @"^(\d{8}_\d{6})(\b|_).*$");

			if (!imageFileNameMatch.Success)
				return false;

			string washedImageFileName = imageFileNameMatch.Groups[1].Value;

			return DateTime.TryParseExact(washedImageFileName, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
		}

		public static bool TryGetExifTagDateTime(this string imageFilePath, out DateTime dateTime)
		{
			dateTime = DateTime.MinValue;

			ExifSubIfdDirectory subIfdDirectory = ImageMetadataReader.ReadMetadata(imageFilePath)
				.OfType<ExifSubIfdDirectory>()
				.FirstOrDefault();

			return subIfdDirectory != null
				&& subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out dateTime);
		}

		public static bool Matches(this DateTime fileNameDateTime, DateTime tagDateTime)
		{
			return fileNameDateTime >= tagDateTime || fileNameDateTime < tagDateTime.AddSeconds(2);
		}

		public static void PrintAllMetadata(this string imageFilePath)
		{
			foreach (var directory in ImageMetadataReader.ReadMetadata(imageFilePath))
			{
				foreach (var tag in directory.Tags)
				{
					Console.WriteLine($"{directory.Name} - {tag.Name} = {tag.Description}");
				}
			}
		}
	}
}
