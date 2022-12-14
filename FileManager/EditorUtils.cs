using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FileManager
{
	static class EditorUtils
	{
		private static int SkipToContent(string s, int startIndex = 0)
		{
			string spaces = " \n\r\t";
			while (startIndex < s.Length && spaces.Contains(s[startIndex]))
				startIndex++;
			return startIndex;
		}

		public static string ToCamelCase(string s)
		{
			var builer = new StringBuilder(s);
			char[] separators = { ' ', '\n', '\r', '\t' };
			int index = SkipToContent(s);
			builer[index] = builer[index].ToString().ToUpper()[0];
			index = s.IndexOfAny(separators, index);
			while (index != -1 && index < s.Length)
			{
				index = SkipToContent(s, index + 1);
				if (index >= s.Length)
					break;
				builer[index] = builer[index].ToString().ToUpper()[0];
				index = s.IndexOfAny(separators, index + 1);
			}

			return builer.ToString();
		}

		public static string ToSentenceRegister(string s)
		{
			var builder = new StringBuilder();
			char[] separators = { '.', '!', '?' };
			int start = 0;
			int index = s.IndexOfAny(separators, start);

			while (index != -1)
			{
				index = SkipToContent(s, index + 1);
				string sentence = start + 1 < index ? s[++start..index] : "";
				builder.Append(s[start - 1].ToString().ToUpper() + sentence.ToLower());
				start = index;
				index = s.IndexOfAny(separators, start);
			}

			if (start < s.Length)
			{
				string sentence = ++start < s.Length ? s[start..] : "";
				builder.Append(s[start - 1].ToString().ToUpper() + sentence.ToLower());
			}

			return builder.ToString();
		}

		public static string TrimText(string s)
		{
			string text = s.Replace("\t", "");
			text = Regex.Replace(text, @" {2,}", " ");
			var result = new StringBuilder();
			foreach (var line in text.Split('\n'))
			{
				string trimmed = line.Trim();
				if (!string.IsNullOrEmpty(trimmed))
					result.AppendLine(trimmed);
			}
			return result.ToString();
		}

		public static List<string> GetImagesFromHtml(string s)
		{
			var html = new HtmlDocument();
			html.LoadHtml(s);
			var nodes = html.DocumentNode.SelectNodes("//img");
			if (nodes == null)
				return new();
			return nodes
				.Select(node => node.Attributes["src"]?.Value ?? "")
				.Where(src => !string.IsNullOrWhiteSpace(src))
				.ToList();
		}

		public static (string, bool) MakePathAbsoulute(string path, string parent)
		{
			if (Uri.TryCreate(path, UriKind.Absolute, out _))
				return (path, false);
			else if (Path.IsPathFullyQualified(path) && path[0] != '/')
				return (path, true);
			else
				return (Path.Join(parent, path).Replace('/', '\\'), true);
		}
	}
}
