// Based on code found at:
// http://skysanders.net/subtext/archive/2010/04/12/c-file-upload-with-form-fields-cookies-and-headers.aspx
//
// Modified to remove Windows dependenies and make Rack-compatible (it wasn't parsing per the RFC spec Rack uses)

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace MooGet {

	/// <summary>
	/// This class contains methods excepted from Salient.Web.HttpLib.HttpRequestUtility
	/// for demonstration purposes. Please see http://salient.codeplex.com for full 
	/// implementation
	/// </summary>
	public static class Upload
	{
		/// <summary>
		/// Uploads a stream using a multipart/form-data POST.
		/// </summary>
		/// <param name="requestUri"></param>
		/// <param name="postData">A NameValueCollection containing form fields to post with file data</param>
		/// <param name="fileData">An open, positioned stream containing the file data</param>
		/// <param name="fileName">Optional, a name to assign to the file data.</param>
		/// <param name="fileContentType">Optional. If omitted, registry is queried using <paramref name="fileName"/>. 
		/// If content type is not available from registry, application/octet-stream will be submitted.</param>
		/// <param name="fileFieldName">Optional, a form field name to assign to the uploaded file data. 
		/// If ommited the value 'file' will be submitted.</param>
		/// <param name="cookies">Optional, can pass null. Used to send and retrieve cookies. 
		/// Pass the same instance to subsequent calls to maintain state if required.</param>
		/// <param name="headers">Optional, headers to be added to request.</param>
		/// <returns></returns>
		/// Reference: 
		/// http://tools.ietf.org/html/rfc1867
		/// http://tools.ietf.org/html/rfc2388
		/// http://www.w3.org/TR/html401/interact/forms.html#h-17.13.4.2
		/// 
		public static WebResponse PostFile(Uri requestUri, NameValueCollection postData, Stream fileData, string fileName,
										   string fileContentType, string fileFieldName, CookieContainer cookies, NameValueCollection headers)
		{
			HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(requestUri);

			string ctype;

			fileContentType = string.IsNullOrEmpty(fileContentType)
								  ? Util.MimeTypeFor(fileName)
								  : fileContentType;

			fileFieldName = string.IsNullOrEmpty(fileFieldName) ? "file" : fileFieldName;

			if (headers != null)
			{
				// set the headers
				foreach (string key in headers.AllKeys)
				{
					string[] values = headers.GetValues(key);
					if (values != null)
						foreach (string value in values)
						{
							webrequest.Headers.Add(key, value);
						}
				}
			}
			webrequest.Method = "POST";

			if (cookies != null)
			{
				webrequest.CookieContainer = cookies;
			}

			string boundary = "----------" + DateTime.Now.Ticks.ToString("x", CultureInfo.InvariantCulture);

			webrequest.ContentType = "multipart/form-data; boundary=" + boundary;

			StringBuilder sbHeader = new StringBuilder();

			// add form fields, if any
			if (postData != null)
			{
				foreach (string key in postData.AllKeys)
				{
					string[] values = postData.GetValues(key);
					if (values != null)
						foreach (string value in values)
						{
							sbHeader.AppendFormat("--{0}\r\n", boundary);
							sbHeader.AppendFormat("Content-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}\r\n", key,
												  value);
						}
				}
			}


			if (fileData != null)
			{
				sbHeader.AppendFormat("--{0}\r\n", boundary);
				sbHeader.AppendFormat("Content-Disposition: form-data; name=\"{0}\"; {1}\r\n", fileFieldName,
									  string.IsNullOrEmpty(fileName)
										  ?
											  ""
										  : string.Format(CultureInfo.InvariantCulture, "filename=\"{0}\"",
														  Path.GetFileName(fileName)));

				sbHeader.AppendFormat("Content-Type: {0}\r\n\r\n", fileContentType);
			}

			byte[] header = Encoding.UTF8.GetBytes(sbHeader.ToString());
			byte[] footer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
			long contentLength = header.Length + (fileData != null ? fileData.Length : 0) + footer.Length;

			webrequest.ContentLength = contentLength;

			using (Stream requestStream = webrequest.GetRequestStream())
			{
				requestStream.Write(header, 0, header.Length);


				if (fileData != null)
				{
					// write the file data, if any
					byte[] buffer = new Byte[checked((uint)Math.Min(4096, (int)fileData.Length))];
					int bytesRead;
					while ((bytesRead = fileData.Read(buffer, 0, buffer.Length)) != 0)
					{
						requestStream.Write(buffer, 0, bytesRead);
					}
				}

				// write footer
				requestStream.Write(footer, 0, footer.Length);

				return webrequest.GetResponse();
			}
		}

		/// <summary>
		/// Uploads a file using a multipart/form-data POST.
		/// </summary>
		/// <param name="requestUri"></param>
		/// <param name="postData">A NameValueCollection containing form fields to post with file data</param>
		/// <param name="fileName">The physical path of the file to upload</param>
		/// <param name="fileContentType">Optional. If omitted, registry is queried using <paramref name="fileName"/>. 
		/// If content type is not available from registry, application/octet-stream will be submitted.</param>
		/// <param name="fileFieldName">Optional, a form field name to assign to the uploaded file data. 
		/// If ommited the value 'file' will be submitted.</param>
		/// <param name="cookies">Optional, can pass null. Used to send and retrieve cookies. 
		/// Pass the same instance to subsequent calls to maintain state if required.</param>
		/// <param name="headers">Optional, headers to be added to request.</param>
		/// <returns></returns>
		public static WebResponse PostFile(Uri requestUri, NameValueCollection postData, string fileName,
										   string fileContentType, string fileFieldName, CookieContainer cookies,
										   NameValueCollection headers)
		{
			using (FileStream fileData = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return PostFile(requestUri, postData, fileData, fileName, fileContentType, fileFieldName, cookies,
								headers);
			}
		}
	}
}
