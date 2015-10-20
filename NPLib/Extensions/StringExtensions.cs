using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib
{
	public static class StringExtensions
	{
		public static string Substring(this string @this, string from = null, string until = null, StringComparison comparison = StringComparison.InvariantCulture)
		{
			var fromLength = (from ?? string.Empty).Length;
			var startIndex = !string.IsNullOrEmpty(from)
				? @this.IndexOf(from, comparison) + fromLength
				: 0;

			if (startIndex < fromLength) { throw new ArgumentException("from: Failed to find an instance of the first anchor"); }

			var endIndex = !string.IsNullOrEmpty(until)
			? @this.IndexOf(until, startIndex, comparison)
			: @this.Length;

			if (endIndex < 0) { throw new ArgumentException("until: Failed to find an instance of the last anchor"); }

			var subString = @this.Substring(startIndex, endIndex - startIndex);
			return subString;
		}

		public static string ToPostData(this Dictionary<string,string> @this)
		{
			List<string> _post_data = new List<string>();

			foreach (var kvp in @this)
			{
				_post_data.Add(string.Format("{0}={1}", kvp.Key, Uri.EscapeDataString(kvp.Value)));
			}

			return string.Join("&", _post_data);
		}

		public static HtmlDocument ToHtmlDocument(this string @this)
		{
			var _document = new HtmlDocument();
			_document.LoadHtml(@this);

			return _document;
		}
	}
}
