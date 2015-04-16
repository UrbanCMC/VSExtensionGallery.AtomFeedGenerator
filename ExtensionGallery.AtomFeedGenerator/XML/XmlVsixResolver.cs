// from http://news.oreilly.com/XmlZipResolver.cs - no copyright notices or use restrictions of any kind with the original file.

using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;

namespace ExtensionGallery.AtomFeedGenerator.XML
{
	/// <summary>
	/// A subclass of the XmlUrlResolver class which allows you to resolve references inside vsix files.
	/// </summary>
	internal class XmlVsixResolver : XmlUrlResolver
	{
		private const string VsixRegex = "^(vsix):";
		private const string UriRegex = "^([^!]+)!(/[^!]+)$";

		///<summary>
		///Maps a URI to an object containing the actual resource.
		///      1) Tests whether the URL starts with "vsix:"
		///      2) If not, pass the URL to  XmlUrlResolver  (to the superclass)
		///      3) If it does, then interprets it like the apache version:
		///          a) Strip out leading "vsix:"
		///          b) Split into two strings, before and after the "!"
		///          c) use the first string as a VSIX file, use the second as an VSIX path to a file
		///          d) return a stream from that file
		///</summary>
		///<returns>A System.IO.Stream object or null if a type other than stream is specified.</returns>
		///<param name="role">The current implementation does not use this parameter when resolving URIs. This is provided for future extensibility purposes. For example, this can be mapped to the xlink:role and used as an implementation specific argument in other scenarios. </param>
		///<param name="ofObjectToReturn">The type of object to return. The current implementation only returns System.IO.Stream objects. </param>
		///<param name="absoluteUri">The URI returned from <see cref="M:System.Xml.XmlResolver.ResolveUri(System.Uri,System.String)"></see></param>
		///<exception cref="T:System.UriFormatException">The specified URI is not an absolute URI. </exception>
		///<exception cref="T:System.Exception">There is a runtime error (for example, an interrupted server connection). </exception>
		///<exception cref="T:System.Xml.XmlException">ofObjectToReturn is neither null nor a Stream type. </exception>
		//     There is a runtime error (for example, an interrupted server connection).
		public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			// Test whether the URL starts with "vsix:"
			var uriString = absoluteUri.ToString();
			var vsixMatch = Regex.Match(uriString, VsixRegex);

			// If not a vsix uri, pass the URL to XmlUrlResolver
			if (!vsixMatch.Success)
			{
				return base.GetEntity(absoluteUri, role, ofObjectToReturn);
			}

			// Strip out leading "vsix:"
			var stripedUriString = Regex.Replace(uriString, VsixRegex, "");

			// Split into two strings, before and after the "!"
			var uriMatch = Regex.Match(stripedUriString, UriRegex);
			if (!uriMatch.Success)
			{
				throw new UriFormatException("Vsix URI does not have a '!' between the vsix file path and " +
					"the path to the xml within the vsix file, or path to xml does not " +
					"start with a '/'. Vsix URI found was '" + uriString + "'");
			}

			var vsixFilePath = uriMatch.Groups[1].ToString();
			var xmlInVsixPath = uriMatch.Groups[2].ToString();

			if (xmlInVsixPath.StartsWith("/") || xmlInVsixPath.StartsWith("\\"))
			{
				xmlInVsixPath = xmlInVsixPath.Substring(1);
			}

			// Use the first string as a VSIX file, use the second as a VSIX path to a file
			using (var reader = new WebClient())
			{
				// need to do this for the self: case in particular
				Stream vsixFileStream = null;
				ZipFile vsixFile = null;

				try
				{
					vsixFileStream = vsixFilePath.ToLower().StartsWith("file:") ? new FileStream(vsixFilePath.Substring(5), FileMode.Open, FileAccess.Read, FileShare.ReadWrite) : reader.OpenRead(vsixFilePath);
					vsixFile = new ZipFile(vsixFileStream);
					var entry = vsixFile.GetEntry(xmlInVsixPath);
					if (entry == null)
					{
						throw new FileNotFoundException(
							String.Format("could not find the xml file {0} in the vsix file {1}", xmlInVsixPath, vsixFilePath), xmlInVsixPath);
					}

					// return the stream to that entry.
					return new StreamPair(vsixFile, vsixFileStream, vsixFile.GetInputStream(entry));
				}
				catch
				{
					if (vsixFileStream != null)
					{
						vsixFileStream.Close();
					}
					if (vsixFile != null)
					{
						vsixFile.Close();
					}
					throw;
				}
			}
		}
	}

	/// <summary>
	/// Holds both the vsix file and entry stream so both can be closed/disposed when done.
	/// </summary>
	internal class StreamPair : Stream
	{
		private readonly ZipFile _vsixFile;
		private readonly Stream _vsixFileStream;
		private readonly Stream _vsixEntryStream;

		internal StreamPair(ZipFile vsixFile, Stream vsixFileStream, Stream vsixEntryStream)
		{
			this._vsixFile = vsixFile;
			this._vsixFileStream = vsixFileStream;
			this._vsixEntryStream = vsixEntryStream;
		}

		///<summary>
		///Closes the current stream and releases any resources (such as sockets and file handles) associated with the current stream.
		///</summary>
		///<filterpriority>1</filterpriority>
		public override void Close()
		{
			this._vsixFile.Close();
			this._vsixEntryStream.Close();
			this._vsixFileStream.Close();
		}

		///<summary>
		///Releases the unmanaged resources used by the <see cref="T:System.IO.Stream"></see> and optionally releases the managed resources.
		///</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Close();
			}

			base.Dispose(disposing);
		}

		///<summary>
		///When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
		///</summary>
		///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception><filterpriority>2</filterpriority>
		public override void Flush()
		{
			this._vsixEntryStream.Flush();
		}

		///<summary>
		///Begins an asynchronous read operation.
		///</summary>
		///<returns>
		///An <see cref="T:System.IAsyncResult"></see> that represents the asynchronous read, which could still be pending.
		///</returns>
		///<param name="offset">The byte offset in buffer at which to begin writing data read from the stream. </param>
		///<param name="count">The maximum number of bytes to read. </param>
		///<param name="buffer">The buffer to read the data into. </param>
		///<param name="callback">An optional asynchronous callback, to be called when the read is complete. </param>
		///<param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests. </param>
		///<exception cref="T:System.IO.IOException">Attempted an asynchronous read past the end of the stream, or a disk error occurs. </exception>
		///<exception cref="T:System.NotSupportedException">The current Stream implementation does not support the read operation. </exception>
		///<exception cref="T:System.ArgumentException">One or more of the arguments is invalid. </exception>
		///<exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>2</filterpriority>
		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return this._vsixEntryStream.BeginRead(buffer, offset, count, callback, state);
		}

		/// <summary>
		/// Waits for the pending asynchronous read to complete.
		/// </summary>
		/// <returns>
		/// The number of bytes read from the stream, between zero (0) and the number of bytes you requested. Streams return zero (0) only at the end of the stream, otherwise, they should block until at least one byte is available.
		/// </returns>
		/// <param name="asyncResult">The reference to the pending asynchronous request to finish. </param>
		/// <exception cref="T:System.ArgumentException">asyncResult did not originate from a <see cref="M:System.IO.Stream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)"></see> method on the current stream. </exception>
		/// <exception cref="T:System.ArgumentNullException">asyncResult is null. </exception><filterpriority>2</filterpriority>
		/// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
		public override int EndRead(IAsyncResult asyncResult)
		{
			return this._vsixEntryStream.EndRead(asyncResult);
		}

		///<summary>
		///Begins an asynchronous write operation.
		///</summary>
		///<returns>
		///An IAsyncResult that represents the asynchronous write, which could still be pending.
		///</returns>
		///<param name="offset">The byte offset in buffer from which to begin writing. </param>
		///<param name="count">The maximum number of bytes to write. </param>
		///<param name="buffer">The buffer to write data from. </param>
		///<param name="callback">An optional asynchronous callback, to be called when the write is complete. </param>
		///<param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests. </param>
		///<exception cref="T:System.NotSupportedException">The current Stream implementation does not support the write operation. </exception>
		///<exception cref="T:System.IO.IOException">Attempted an asynchronous write past the end of the stream, or a disk error occurs. </exception>
		///<exception cref="T:System.ArgumentException">One or more of the arguments is invalid. </exception>
		///<exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>2</filterpriority>
		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return this._vsixEntryStream.BeginWrite(buffer, offset, count, callback, state);
		}

		/// <summary>
		/// Ends an asynchronous write operation.
		/// </summary>
		/// <param name="asyncResult">A reference to the outstanding asynchronous I/O request. </param>
		/// <exception cref="T:System.ArgumentNullException">asyncResult is null. </exception>
		/// <exception cref="T:System.ArgumentException">asyncResult did not originate from a <see cref="M:System.IO.Stream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)"></see> method on the current stream. </exception><filterpriority>2</filterpriority>
		/// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
		public override void EndWrite(IAsyncResult asyncResult)
		{
			this._vsixEntryStream.EndWrite(asyncResult);
		}

		///<summary>
		///When overridden in a derived class, sets the position within the current stream.
		///</summary>
		///<returns>
		///The new position within the current stream.
		///</returns>
		///<param name="offset">A byte offset relative to the origin parameter. </param>
		///<param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"></see> indicating the reference point used to obtain the new position. </param>
		///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		///<exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
		///<exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
		public override long Seek(long offset, SeekOrigin origin)
		{
			return this._vsixEntryStream.Seek(offset, origin);
		}

		///<summary>
		///When overridden in a derived class, sets the length of the current stream.
		///</summary>
		///<param name="value">The desired length of the current stream in bytes. </param>
		///<exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
		///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		///<exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>2</filterpriority>
		public override void SetLength(long value)
		{
			this._vsixEntryStream.SetLength(value);
		}

		///<summary>
		///When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		///</summary>
		///<returns>
		///The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
		///</returns>
		///<param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream. </param>
		///<param name="count">The maximum number of bytes to be read from the current stream. </param>
		///<param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source. </param>
		///<exception cref="T:System.ArgumentException">The sum of offset and count is larger than the buffer length. </exception>
		///<exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		///<exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
		///<exception cref="T:System.ArgumentNullException">buffer is null. </exception>
		///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		///<exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception><filterpriority>1</filterpriority>
		public override int Read(byte[] buffer, int offset, int count)
		{
			return this._vsixEntryStream.Read(buffer, offset, count);
		}

		///<summary>
		///Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
		///</summary>
		///<returns>
		///The unsigned byte cast to an Int32, or -1 if at the end of the stream.
		///</returns>
		///<exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
		///<exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>2</filterpriority>
		public override int ReadByte()
		{
			return this._vsixEntryStream.ReadByte();
		}

		///<summary>
		///When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
		///</summary>
		///<param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream. </param>
		///<param name="count">The number of bytes to be written to the current stream. </param>
		///<param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream. </param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			this._vsixEntryStream.Write(buffer, offset, count);
		}

		///<summary>
		///Writes a byte to the current position in the stream and advances the position within the stream by one byte.
		///</summary>
		///<param name="value">The byte to write to the stream. </param>
		///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		///<exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		///<exception cref="T:System.NotSupportedException">The stream does not support writing, or the stream is already closed. </exception><filterpriority>2</filterpriority>
		public override void WriteByte(byte value)
		{
			this._vsixEntryStream.WriteByte(value);
		}

		///<summary>
		///When overridden in a derived class, gets a value indicating whether the current stream supports reading.
		///</summary>
		///<returns>
		///true if the stream supports reading; otherwise, false.
		///</returns>
		///<filterpriority>1</filterpriority>
		public override bool CanRead
		{
			get { return this._vsixEntryStream.CanRead; }
		}

		///<summary>
		///When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
		///</summary>
		///<returns>
		///true if the stream supports seeking; otherwise, false.
		///</returns>
		///<filterpriority>1</filterpriority>
		public override bool CanSeek
		{
			get { return this._vsixEntryStream.CanSeek; }
		}

		///<summary>
		///Gets a value that determines whether the current stream can time out.
		///</summary>
		///<returns>
		///A value that determines whether the current stream can time out.
		///</returns>
		///<filterpriority>2</filterpriority>
		public override bool CanTimeout
		{
			get { return this._vsixEntryStream.CanTimeout; }
		}

		///<summary>
		///When overridden in a derived class, gets a value indicating whether the current stream supports writing.
		///</summary>
		///<returns>
		///true if the stream supports writing; otherwise, false.
		///</returns>
		///<filterpriority>1</filterpriority>
		public override bool CanWrite
		{
			get { return this._vsixEntryStream.CanWrite; }
		}

		///<summary>
		///When overridden in a derived class, gets the length in bytes of the stream.
		///</summary>
		///<returns>
		///A long value representing the length of the stream in bytes.
		///</returns>
		///<exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
		///<exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
		public override long Length
		{
			get { return this._vsixEntryStream.Length; }
		}

		///<summary>
		///When overridden in a derived class, gets or sets the position within the current stream.
		///</summary>
		///<returns>
		///The current position within the stream.
		///</returns>
		///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		///<exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
		///<exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
		public override long Position
		{
			get { return this._vsixEntryStream.Position; }
			set { this._vsixEntryStream.Position = value; }
		}

		///<summary>
		///Gets or sets a value that determines how long the stream will attempt to read before timing out.
		///</summary>
		///<returns>
		///A value that determines how long the stream will attempt to read before timing out.
		///</returns>
		///<exception cref="T:System.InvalidOperationException">The <see cref="P:System.IO.Stream.ReadTimeout"></see> method always throws an <see cref="T:System.InvalidOperationException"></see>. </exception><filterpriority>2</filterpriority>
		public override int ReadTimeout
		{
			get { return this._vsixEntryStream.ReadTimeout; }
			set { this._vsixEntryStream.ReadTimeout = value; }
		}

		///<summary>
		///Gets or sets a value that determines how long the stream will attempt to write before timing out.
		///</summary>
		///<returns>
		///A value that determines how long the stream will attempt to write before timing out.
		///</returns>
		///<exception cref="T:System.InvalidOperationException">The <see cref="P:System.IO.Stream.WriteTimeout"></see> method always throws an <see cref="T:System.InvalidOperationException"></see>. </exception><filterpriority>2</filterpriority>
		public override int WriteTimeout
		{
			get { return this._vsixEntryStream.WriteTimeout; }
			set { this._vsixEntryStream.WriteTimeout = value; }
		}
	}
}