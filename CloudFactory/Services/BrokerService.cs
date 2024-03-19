using System.IO;
using System.Text;

using Microsoft.AspNetCore.Mvc;

namespace CloudFactory.Services
{
	public class BrokerService
	{
		private readonly string _folderRequest = "request/";

		private const string _requestExtension = ".req";

		private const string _responseExtension = ".res";

		private object _lock = new object();

		public BrokerService()
		{

		}

		public ActionResult<string> GetResponse(string key)
		{
			string pathResponse = GetResponsePath(key);
			int resultCode;
			(int statusCode, string answer) result;
			lock (_lock)
			{
				if (File.Exists(pathResponse) == false)
				{
					return new ContentResult ()
					{
						StatusCode =  StatusCodes.Status204NoContent,
						Content = string.Empty
					};
				}
				else
				{
					string[] contents = File.ReadAllLines(pathResponse);

					if (int.TryParse(contents[0], out resultCode) == false)
					{
						result = (StatusCodes.Status500InternalServerError, string.Empty);
					}
					else
					{
						result = (resultCode, contents[1]);
					}
					result = Decriment(key, result);
				}
			}
			return new ContentResult()
			{
				StatusCode = result.statusCode,
				Content = result.answer,
			};
		}

		public string CreateRequest(HttpRequest request)
		{
			string source = request.Method + request.Path + "\r\n 1";
			string key = MD5(source);
			string path = GetRequestPath(key);
			string pathResponse = GetResponsePath(key);
			lock (_lock)
			{
				if (File.Exists(path))
				{
					Increment(path);
				}
				else
				{
					File.WriteAllText(path, source);
				}
				if (File.Exists(pathResponse) == false)
				{
					List<string> responseContent = new List<string>();
					responseContent.Add(200.ToString());
					responseContent.Add(key);
					responseContent.Add(1.ToString());
					File.WriteAllLines(pathResponse, responseContent);
				}
			}
			return key;
		}

		private static void Increment(string path)
		{
			string[] content = File.ReadAllLines(path);
			int number;
			if (int.TryParse(content[1], out number))
			{
				number++;
				content[1] = number.ToString();
				File.WriteAllLines(path, content);
			}
			else
			{
				throw new ArgumentException();
			}
		}

		private (int statusCode, string answer) Decriment(string key, (int statusCode, string answer) result)
		{
			string pathRequest = GetRequestPath(key);
			string pathResponse = GetResponsePath(key);
			if (File.Exists(pathRequest))
			{
				List<string> content = File.ReadLines(pathRequest).ToList();
				int number = -1;
				if (int.TryParse(content[1], out number))
				{
					number--;
					if (number == 0)
					{
						File.Delete(pathRequest);
						File.Delete(pathResponse);
					}
					else
					{
						content[1] = number.ToString();
						File.WriteAllLines(pathRequest, content);
					}
				}
				else
				{
					result = (StatusCodes.Status500InternalServerError, string.Empty);
				}
			}
			else
			{
				if (File.Exists (pathResponse))
				{
					File.Delete(pathResponse);
				}
			}
			return result;
		}

		private string GetRequestPath(string requestKey) => Path.Combine(_folderRequest, requestKey + _requestExtension);
		private string GetResponsePath(string requestKey) => Path.Combine(_folderRequest, requestKey + _responseExtension);
		public string MD5(string s)
		{
			using var provider = System.Security.Cryptography.MD5.Create();
			StringBuilder builder = new StringBuilder();

			foreach (byte b in provider.ComputeHash(Encoding.UTF8.GetBytes(s)))
				builder.Append(b.ToString("x2").ToLower());

			return builder.ToString();
		}
	}
}
