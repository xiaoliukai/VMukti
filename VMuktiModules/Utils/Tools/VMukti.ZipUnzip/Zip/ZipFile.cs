// ZipFile.cs
//
// Copyright (C) 2001 Mike Krueger
// Copyright (C) 2004 John Reilly
//
// This file was translated from java, it was part of the GNU Classpath
// Copyright (C) 2001 Free Software Foundation, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
// 
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Globalization;

#if !COMPACT_FRAMEWORK
using System.Security.Cryptography;
#endif

using VMukti.ZipUnzip.Core;
using VMukti.ZipUnzip.Checksums;
using VMukti.ZipUnzip.Zip.Compression.Streams;
using VMukti.ZipUnzip.Zip.Compression;
using VMukti.ZipUnzip.Encryption;

namespace VMukti.ZipUnzip.Zip 
{
	#region Keys Required Event Args
	/// <summary>
	/// Arguments used with KeysRequiredEvent
	/// </summary>
	public class KeysRequiredEventArgs : EventArgs
	{
		#region Constructors
		/// <summary>
		/// Initialise a new instance of <see cref="KeysRequiredEventArgs"></see>
		/// </summary>
		/// <param name="name">The name of the file for which keys are required.</param>
		public KeysRequiredEventArgs(string name)
		{
			fileName = name;
		}
	
		/// <summary>
		/// Initialise a new instance of <see cref="KeysRequiredEventArgs"></see>
		/// </summary>
		/// <param name="name">The name of the file for which keys are required.</param>
		/// <param name="keyValue">The current key value.</param>
		public KeysRequiredEventArgs(string name, byte[] keyValue)
		{
			fileName = name;
			key = keyValue;
		}

		#endregion
		#region Properties
		/// <summary>
		/// Get the name of the file for which keys are required.
		/// </summary>
		public string FileName
		{
			get { return fileName; }
		}
	
		/// <summary>
		/// Get/set the key value
		/// </summary>
		public byte[] Key
		{
			get { return key; }
			set { key = value; }
		}
		#endregion
		#region Instance Fields
		string fileName;
		byte[] key;
		#endregion
	}
	#endregion
	#region Test Definitions
	/// <summary>
	/// The strategy to apply to testing.
	/// </summary>
	public enum TestStrategy
	{
		/// <summary>
		/// Find the first error only.
		/// </summary>
		FindFirstError,
		/// <summary>
		/// Find all possible errors.
		/// </summary>
		FindAllErrors,
	}

	/// <summary>
	/// The operation in progress reported by a <see cref="ZipTestResultHandler"/> during testing.
	/// </summary>
	public enum TestOperation
	{
		/// <summary>
		/// Setting up testing.
		/// </summary>
		Initialising,
		/// <summary>
		/// Testing an individual entries header
		/// </summary>
		EntryHeader,
		/// <summary>
		/// Testing an individual entries data
		/// </summary>
		EntryData,
		/// <summary>
		/// Testing an individual entry has completed.
		/// </summary>
		EntryComplete,
		/// <summary>
		/// Running miscellaneous tests
		/// </summary>
		MiscellaneousTests,
		/// <summary>
		/// Testing is complete
		/// </summary>
		Complete,
	}

	/// <summary>
	/// Status returned returned by <see cref="ZipTestResultHandler"/> during testing.
	/// </summary>
	public class TestStatus
	{
		#region Constructors
		/// <summary>
		/// Initialise a new instance of <see cref="TestStatus"/>
		/// </summary>
		/// <param name="file">The <see cref="ZipFile"/> this status applies to.</param>
		public TestStatus(ZipFile file)
		{
			file_ = file;
		}
		#endregion
		#region Properties

		/// <summary>
		/// Get the current <see cref="TestOperation"/> in progress.
		/// </summary>
		public TestOperation Operation
		{
			get { return operation_; }
		}

		/// <summary>
		/// Get the <see cref="ZipFile"/> this status is applicable to.
		/// </summary>
		public ZipFile File
		{
			get { return file_; }
		}

		/// <summary>
		/// Get the current/last entry tested.
		/// </summary>
		public ZipEntry Entry
		{
			get { return entry_; }
		}

		/// <summary>
		/// Get the number of errors detected so far.
		/// </summary>
		public int ErrorCount
		{
			get { return errorCount_; }
		}

		/// <summary>
		/// Get the number of bytes tested so far for the current entry.
		/// </summary>
		public long BytesTested
		{
			get { return bytesTested_; }
		}

		/// <summary>
		/// Get a value indicating wether the last entry test was valid.
		/// </summary>
		public bool EntryValid
		{
			get { return entryValid_; }
		}
		#endregion
		#region Internal API
		internal void AddError()
		{
			errorCount_++;
			entryValid_ = false;
		}

		internal void SetOperation(TestOperation operation)
		{
			operation_ = operation;
		}

		internal void SetEntry(ZipEntry entry)
		{
			entry_ = entry;
			entryValid_ = true;
			bytesTested_ = 0;
		}

		internal void SetBytesTested(long value)
		{
			bytesTested_ = value;
		}
		#endregion
		#region Instance Fields
		ZipFile file_;
		ZipEntry entry_;
		bool entryValid_;
		int errorCount_;
		long bytesTested_;
		TestOperation operation_;
		#endregion
	}

	/// <summary>
	/// Delegate invoked during testing if supplied indicating current progress and status.
	/// </summary>
	/// <remarks>If the message is non-null an error has occured.  If the message is null
	/// the operation as found in <see cref="TestStatus">status</see> has started.</remarks>
	public delegate void ZipTestResultHandler(TestStatus status, string message);
	#endregion
	#region Update Definitions
	/// <summary>
	/// The possible ways of applying updates to an archive.
	/// </summary>
	public enum FileUpdateMode
	{
		/// <summary>
		/// Perform all updates on temporary files ensuring that the original file is saved.
		/// </summary>
		Safe,
		/// <summary>
		/// Update the archive directly, which is faster but less safe.
		/// </summary>
		Direct,
	}
	#endregion
	#region ZipFile Class
	/// <summary>
	/// This class represents a Zip archive.  You can ask for the contained
	/// entries, or get an input stream for a file entry.  The entry is
	/// automatically decompressed.
	/// 
	/// You can also update the archive adding or deleting entries.
	/// 
	/// This class is thread safe for input:  You can open input streams for arbitrary
	/// entries in different threads.
	/// <br/>
	/// <br/>Author of the original java version : Jochen Hoenicke
	/// </summary>
	/// <example>
	/// <code>
	/// using System;
	/// using System.Text;
	/// using System.Collections;
	/// using System.IO;
	/// 
	/// using _videoConference.ZipUnzip.Zip;
	/// 
	/// class MainClass
	/// {
	/// 	static public void Main(string[] args)
	/// 	{
	/// 		using (ZipFile zFile = new ZipFile(args[0])) {
	/// 			Console.WriteLine("Listing of : " + zFile.Name);
	/// 			Console.WriteLine("");
	/// 			Console.WriteLine("Raw Size    Size      Date     Time     Name");
	/// 			Console.WriteLine("--------  --------  --------  ------  ---------");
	/// 			foreach (ZipEntry e in zFile) {
	/// 				DateTime d = e.DateTime;
	/// 				Console.WriteLine("{0, -10}{1, -10}{2}  {3}   {4}", e.Size, e.CompressedSize,
	/// 			                                                    d.ToString("dd-MM-yy"), d.ToString("HH:mm"),
	/// 			                                                    e.Name);
	/// 			}
	/// 		}
	/// 	}
	/// }
	/// </code>
	/// </example>
	public class ZipFile : IEnumerable, IDisposable
	{
		#region KeyHandling
		
		/// <summary>
		/// Delegate for handling keys/password setting during compresion/decompression.
		/// </summary>
		public delegate void KeysRequiredEventHandler(
			object sender,
			KeysRequiredEventArgs e
		);

		/// <summary>
		/// Event handler for handling encryption keys.
		/// </summary>
		public KeysRequiredEventHandler KeysRequired;

		/// <summary>
		/// Handles getting of encryption keys when required.
		/// </summary>
		/// <param name="fileName">The file for which encryption keys are required.</param>
		void OnKeysRequired(string fileName)
		{
			if (KeysRequired != null) {
				KeysRequiredEventArgs krea = new KeysRequiredEventArgs(fileName, key);
				KeysRequired(this, krea);
				key = krea.Key;
			}
		}
		
		
		/// <summary>
		/// Get/set the encryption key value.
		/// </summary>
		byte[] Key
		{
			get { return key; }
			set { key = value; }
		}
		

		/// <summary>
		/// Password to be used for encrypting/decrypting files.
		/// </summary>
		/// <remarks>Set to null if no password is required.</remarks>
		public string Password
		{
			set 
			{
				if ( (value == null) || (value.Length == 0) ) {
					key = null;
				}
				else {
					key = PkzipClassic.GenerateKeys(Encoding.ASCII.GetBytes(value));
				}
			}
		}

		/// <summary>
		/// Get a value indicating wether encryption keys are currently available.
		/// </summary>
		bool HaveKeys
		{
			get { return key != null; }
		}
		#endregion
		#region Constructors
		/// <summary>
		/// Opens a Zip file with the given name for reading.
		/// </summary>
		/// <exception cref="IOException">
		/// An i/o error occurs
		/// </exception>
		/// <exception cref="ZipException">
		/// The file doesn't contain a valid zip archive.
		/// </exception>
		public ZipFile(string name)
		{
			name_ = name;
			baseStream_ = File.OpenRead(name);
			
			try {
				ReadEntries();
			}
			catch {
				DisposeInternal(true);
				throw;
			}
		}
		
		/// <summary>
		/// Opens a Zip file reading the given <see cref="FileStream"/>.
		/// </summary>
		/// <exception cref="IOException">
		/// An i/o error occurs.
		/// </exception>
		/// <exception cref="ZipException">
		/// The file doesn't contain a valid zip archive.
		/// </exception>
		public ZipFile(FileStream file)
		{
			if ( file == null ) {
				throw new ArgumentNullException("file");
			}

			if ( !file.CanSeek ) {
				throw new ArgumentException("Stream is not seekable", "file");
			}

			baseStream_  = file;
			name_ = file.Name;
			
			try {
				ReadEntries();
			}
			catch {
				DisposeInternal(true);
				throw;
			}
		}
		
		/// <summary>
		/// Opens a Zip file reading the given <see cref="Stream"/>.
		/// </summary>
		/// <exception cref="IOException">
		/// An i/o error occurs
		/// </exception>
		/// <exception cref="ZipException">
		/// The file doesn't contain a valid zip archive.<br/>
		/// The stream provided cannot seek
		/// </exception>
		public ZipFile(Stream stream)
		{
			if ( stream == null ) {
				throw new ArgumentNullException("stream");
			}

			if ( !stream.CanSeek ) {
				throw new ArgumentException("Stream is not seekable", "stream");
			}

			baseStream_  = stream;
		
			if ( baseStream_.Length > 0 ) {
				try {
					ReadEntries();
				}
				catch {
					DisposeInternal(true);
					throw;
				}
			} else {
				entries_ = new ZipEntry[0];
				isNewArchive_ = true;
			}
		}

		/// <summary>
		/// Initialises a default <see cref="ZipFile"/> instance with no entries and no file storage.
		/// </summary>
		internal ZipFile()
		{
			entries_ = new ZipEntry[0];
			isNewArchive_ = true;
		}
		
		#endregion
		#region Destructors and Closing
		/// <summary>
		/// Finalize this instance.
		/// </summary>
		~ZipFile()
		{
			Dispose(false);
		}
		
		/// <summary>
		/// Closes the ZipFile.  If the stream is <see cref="IsStreamOwner">owned</see> then this also closes the underlying input stream.
		/// Once closed, no further instance methods should be called.
		/// </summary>
		/// <exception cref="System.IO.IOException">
		/// An i/o error occurs.
		/// </exception>
		public void Close()
		{
			DisposeInternal(true);
			GC.SuppressFinalize(this);
		}
		
		#endregion
		#region Creators
		/// <summary>
		/// Create a new <see cref="ZipFile"/> whose data will be stored in a file.
		/// </summary>
		/// <param name="fileName">The name of the archive to create.</param>
		/// <returns>Returns the newly created <see cref="ZipFile"/></returns>
		public static ZipFile Create(string fileName)
		{
			if ( fileName == null ) {
				throw new ArgumentNullException("fileName");
			}

			ZipFile result = new ZipFile();
			result.name_ = fileName;
			result.baseStream_ = File.Create(fileName);
			return result;
		}
		
		/// <summary>
		/// Create a new <see cref="ZipFile"/> whose data will be stored on a stream.
		/// </summary>
		/// <param name="outStream">The stream providing data storage.</param>
		/// <returns>Returns the newly created <see cref="ZipFile"/></returns>
		public static ZipFile Create(Stream outStream)
		{
			if ( outStream == null ) {
				throw new ArgumentNullException("outStream");
			}

			if ( !outStream.CanWrite ) {
				throw new ArgumentException("Stream is not writeable", "outStream");
			}

			if ( !outStream.CanSeek ) {
				throw new ArgumentException("Stream is not seekable", "outStream");
			}

			ZipFile result = new ZipFile();
			result.baseStream_ = outStream;
			return result;
		}
		
		#endregion
		#region Properties
		/// <summary>
		/// Get/set a flag indicating if the underlying stream is owned by the ZipFile instance.
		/// If the flag is true then the stream will be closed when <see cref="Close">Close</see> is called.
		/// </summary>
		/// <remarks>
		/// The default value is true in all cases.
		/// </remarks>
		public bool IsStreamOwner
		{
			get { return isStreamOwner; }
			set { isStreamOwner = value; }
		}
		
		/// <summary>
		/// Get a value indicating wether
		/// this archive is embedded in another file or not.
		/// </summary>
		public bool IsEmbeddedArchive
		{
			// Not strictly correct in all circumstances currently
			get { return offsetOfFirstEntry > 0; }
		}

		/// <summary>
		/// Get a value indicating that this archive is a new one.
		/// </summary>
		public bool IsNewArchive
		{
			get { return isNewArchive_; }
		}

		/// <summary>
		/// Gets the comment for the zip file.
		/// </summary>
		public string ZipFileComment 
		{
			get { return comment_; }
		}
		
		/// <summary>
		/// Gets the name of this zip file.
		/// </summary>
		public string Name 
		{
			get { return name_; }
		}
		
		/// <summary>
		/// Gets the number of entries in this zip file.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// The Zip file has been closed.
		/// </exception>
		[Obsolete("Use the Count property instead")]
		public int Size 
		{
			get 
			{
				if (entries_ != null) {
					return entries_.Length;
				} else {
					throw new InvalidOperationException("ZipFile is closed");
				}
			}
		}
		
		/// <summary>
		/// Get the number of entries contained in this <see cref="ZipFile"/>.
		/// </summary>
		public long Count 
		{
			get 
			{
				if (entries_ != null) {
					return entries_.Length;
				} else {
					throw new InvalidOperationException("ZipFile is closed");
				}
			}
		}
		
		/// <summary>
		/// Indexer property for ZipEntries
		/// </summary>
		[System.Runtime.CompilerServices.IndexerNameAttribute("EntryByIndex")]
		public ZipEntry this[int index] 
		{
			get {
				return (ZipEntry) entries_[index].Clone();	
			}
		}
		
		#endregion
		#region Input Handling
		/// <summary>
		/// Returns an enumerator for the Zip entries in this Zip file.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// The Zip file has been closed.
		/// </exception>
		public IEnumerator GetEnumerator()
		{
			if (entries_ == null) {
				throw new InvalidOperationException("ZipFile has closed");
			}
			
			return new ZipEntryEnumerator(entries_);
		}
		
		/// <summary>
		/// Return the index of the entry with a matching name
		/// </summary>
		/// <param name="name">Entry name to find</param>
		/// <param name="ignoreCase">If true the comparison is case insensitive</param>
		/// <returns>The index position of the matching entry or -1 if not found</returns>
		/// <exception cref="InvalidOperationException">
		/// The Zip file has been closed.
		/// </exception>
		public int FindEntry(string name, bool ignoreCase)
		{
			if (entries_ == null) {
				throw new InvalidOperationException("ZipFile has been closed");
			}
			
			for (int i = 0; i < entries_.Length; i++) {
				if (string.Compare(name, entries_[i].Name, ignoreCase, CultureInfo.InvariantCulture) == 0) {
					return i;
				}
			}
			return -1;
		}
		
		/// <summary>
		/// Searches for a zip entry in this archive with the given name.
		/// String comparisons are case insensitive
		/// </summary>
		/// <param name="name">
		/// The name to find. May contain directory components separated by slashes ('/').
		/// </param>
		/// <returns>
		/// A clone of the zip entry, or null if no entry with that name exists.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// The Zip file has been closed.
		/// </exception>
		public ZipEntry GetEntry(string name)
		{
			if (entries_ == null) {
				throw new InvalidOperationException("ZipFile has been closed");
			}
			
			int index = FindEntry(name, true);
			return index >= 0 ? (ZipEntry) entries_[index].Clone() : null;
		}

		/// <summary>
		/// Creates an input stream reading the given zip entry as
		/// uncompressed data.  Normally zip entry should be an entry
		/// returned by GetEntry().
		/// </summary>
		/// <returns>
		/// the input stream.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// The ZipFile has already been closed
		/// </exception>
		/// <exception cref="_videoConference.ZipUnzip.Zip.ZipException">
		/// The compression method for the entry is unknown
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// The entry is not found in the ZipFile
		/// </exception>
		public Stream GetInputStream(ZipEntry entry)
		{
			if ( entry == null ) {
				throw new ArgumentNullException("entry");
			}

			if ( entries_ == null ) {
				throw new InvalidOperationException("ZipFile has closed");
			}
			
			long index = entry.ZipFileIndex;
			if ( (index < 0) || (index >= entries_.Length) || (entries_[index].Name != entry.Name) ) {
				index = FindEntry(entry.Name, true);
				if (index < 0) {
					throw new ZipException("Entry cannot be found");
				}
			}
			return GetInputStream(index);			
		}
		
		/// <summary>
		/// Creates an input stream reading a zip entry
		/// </summary>
		/// <param name="entryIndex">The index of the entry to obtain an input stream for.</param>
		/// <returns>
		/// An input stream.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// The ZipFile has already been closed
		/// </exception>
		/// <exception cref="_videoConference.ZipUnzip.Zip.ZipException">
		/// The compression method for the entry is unknown
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// The entry is not found in the ZipFile
		/// </exception>
		public Stream GetInputStream(long entryIndex)
		{
			if ( entries_ == null ) {
				throw new InvalidOperationException("ZipFile is not open");
			}
			
			long start = LocateEntry(entries_[entryIndex]);
			CompressionMethod method = entries_[entryIndex].CompressionMethod;
			Stream result = new PartialInputStream(baseStream_, start, entries_[entryIndex].CompressedSize);

			if (entries_[entryIndex].IsCrypted == true) {
				result = CreateAndInitDecryptionStream(result, entries_[entryIndex]);
				if (result == null) {
					throw new ZipException("Unable to decrypt this entry");
				}
			}

			switch (method) {
				case CompressionMethod.Stored:
					// read as is.
					break;

				case CompressionMethod.Deflated:
					// No need to worry about ownership and closing as underlying stream close does nothing.
					result = new InflaterInputStream(result, new Inflater(true));
					break;

				default:
					throw new ZipException("Unsupported compression method " + method);
			}

			return result;
		}

		#endregion
		#region Archive Testing
		/// <summary>
		/// Test an archive for integrity/validity
		/// </summary>
		/// <param name="testData">Perform low level data Crc check</param>
		/// <returns>true if all tests pass, false otherwise</returns>
		/// <remarks>Testing will terminate on the first error found.</remarks>
		public bool TestArchive(bool testData)
		{
			return TestArchive(testData, TestStrategy.FindFirstError, null);
		}
		
		/// <summary>
		/// Test an archive for integrity/validity
		/// </summary>
		/// <param name="testData">Perform low level data Crc check</param>
		/// <param name="strategy">The <see cref="TestStrategy"></see> to apply.</param>
		/// <param name="resultHandler">The <see cref="ZipTestResultHandler"></see> handler to call during testing.</param>
		/// <returns>true if all tests pass, false otherwise</returns>
		public bool TestArchive(bool testData, TestStrategy strategy, ZipTestResultHandler resultHandler)
		{
			TestStatus status = new TestStatus(this);

			if ( resultHandler != null ) {
				resultHandler(status, null);
			}

			HeaderTest test = testData ? (HeaderTest.Header | HeaderTest.Extract) : HeaderTest.Header;

			bool testing = true;

			try {
				int entryIndex = 0;

				while ( testing && (entryIndex < Count) ) {
					if ( resultHandler != null ) {
						status.SetEntry(this[entryIndex]);
						status.SetOperation(TestOperation.EntryHeader);
						resultHandler(status, null);
					}

					try	{
						TestLocalHeader(this[entryIndex], test);
					}
					catch(ZipException ex) {
						status.AddError();

						if ( resultHandler != null ) {
							resultHandler(status,
								string.Format("Exception during test - '{0}'", ex.Message));
						}

						if ( strategy == TestStrategy.FindFirstError ) {
							testing = false; 
						}
					}

					if ( testing && testData && this[entryIndex].IsFile ) {
						if ( resultHandler != null ) {
							status.SetOperation(TestOperation.EntryData);
							resultHandler(status, null);
						}

						Stream entryStream = this.GetInputStream(this[entryIndex]);
						
						Crc32 crc = new Crc32();
						byte[] buffer = new byte[4096];
						long totalBytes = 0;
						int bytesRead;
						while ((bytesRead = entryStream.Read(buffer, 0, buffer.Length)) > 0) {
							crc.Update(buffer, 0, bytesRead);

							if ( resultHandler != null ) {
								totalBytes += bytesRead;
								status.SetBytesTested(totalBytes);
								resultHandler(status, null);
							}
						}
	
						if (this[entryIndex].Crc != crc.Value) {
							status.AddError();
							
							if ( resultHandler != null ) {
								resultHandler(status, "CRC mismatch");
							}

							if ( strategy == TestStrategy.FindFirstError ) {
								testing = false;
							}
						}
					}

					if ( resultHandler != null ) {
						status.SetOperation(TestOperation.EntryComplete);
						resultHandler(status, null);
					}

					entryIndex += 1;
				}

				if ( resultHandler != null ) {
					status.SetOperation(TestOperation.MiscellaneousTests);
					resultHandler(status, null);
				}

				// TODO: the 'Corrina Johns' test where local headers are missing from
				// the central directory.  They are therefore invisible to many archivers.
			}
			catch (Exception ex) {
				status.AddError();

				if ( resultHandler != null ) {
					resultHandler(status, string.Format("Exception during test - '{0}'", ex.Message));
				}
			}

			if ( resultHandler != null ) {
				status.SetOperation(TestOperation.Complete);
				status.SetEntry(null);
				resultHandler(status, null);
			}

			return (status.ErrorCount == 0);
		}

		[Flags]
		enum HeaderTest
		{
			Extract = 0x01,     // Check that this header represents an entry that can be extracted
			Header  = 0x02,     // Check that this header is valid
		}
	
		/// <summary>
		/// Test the local header against that provided from the central directory
		/// </summary>
		/// <param name="entry">
		/// The entry to test against
		/// </param>
		/// <param name="tests">The type of test to carry out.</param>
		/// <returns>The offset of the entries data in the file</returns>
		long TestLocalHeader(ZipEntry entry, HeaderTest tests)
		{
			lock(baseStream_) 
			{
				bool testHeader = (tests & HeaderTest.Header) != 0;
				bool testData = (tests & HeaderTest.Extract) != 0;

				baseStream_.Seek(offsetOfFirstEntry + entry.Offset, SeekOrigin.Begin);
				if ((int)ReadLEUint() != ZipConstants.LocalHeaderSignature) {
					throw new ZipException(string.Format("Wrong local header signature @{0:X}", offsetOfFirstEntry + entry.Offset));
				}

				short extractVersion = ( short )ReadLEUshort();
				short localFlags = ( short )ReadLEUshort();
				short compressionMethod = ( short )ReadLEUshort();
				short fileTime = ( short )ReadLEUshort();
				short fileDate = ( short )ReadLEUshort();
				uint crcValue = ReadLEUint();
				long size = ReadLEUint();
				long compressedSize = ReadLEUint();
				int storedNameLength = ReadLEUshort();
				int extraDataLength = ReadLEUshort();

				if ( testData ) {
					if ( entry.IsFile ) {
						if ( !entry.IsCompressionMethodSupported() ) {
							throw new ZipException("Compression method not supported");
						}

						if ( (extractVersion > ZipConstants.VersionMadeBy)
							|| ((extractVersion > 20) && (extractVersion < ZipConstants.VersionZip64)) ) {
							throw new ZipException(string.Format("Version required to extract this entry not supported ({0})", extractVersion));
						}

						if ( (localFlags & ( int )(GeneralBitFlags.Patched | GeneralBitFlags.StrongEncryption | GeneralBitFlags.EnhancedCompress | GeneralBitFlags.HeaderMasked)) != 0 ) {
							throw new ZipException("The library does not support the zip version required to extract this entry");
						}
					}
				}

				if ( testHeader ) {
					if ((extractVersion < 63) &&	// Ignore later versions as we dont know about them..
						(extractVersion != 10) &&
						(extractVersion != 20) &&
						(extractVersion != 21) &&
						(extractVersion != 25) &&
						(extractVersion != 27) &&
						(extractVersion != 45) &&
						(extractVersion != 46) &&
						(extractVersion != 50) &&
						(extractVersion != 51) &&
						(extractVersion != 61) &&
						(extractVersion != 62)
						) {
						throw new ZipException(string.Format("Version required to extract this entry is invalid ({0})", extractVersion));
					}

					// Local entry flags dont have reserved bit set on.
					if ( (localFlags & ( int )GeneralBitFlags.Reserved) != 0 ) {
						throw new ZipException("Reserved bit flag cannot bet set.");
					}

					// Encryption requires extract version >= 20
					if ( ((localFlags & ( int )GeneralBitFlags.Encrypted) != 0) && (extractVersion < 20) ) {
						throw new ZipException(string.Format("Version required to extract this entry is too low for encryption ({0})", extractVersion));
					}

					// Strong encryption requires encryption flag to be set and extract version >= 50.
					if ( (localFlags & (int)GeneralBitFlags.StrongEncryption) != 0 ) {
						if ( (localFlags & (int)GeneralBitFlags.Encrypted) == 0 ) {
							throw new ZipException("Strong encryption flag set but encryption flag is not set");
						}

						if ( extractVersion < 50 ) {
							throw new ZipException(string.Format("Version required to extract this entry is too low for encryption ({0})", extractVersion));
						}
					}

					// Patched entries require extract version >= 27
					if ( ((localFlags & ( int )GeneralBitFlags.Patched) != 0) && (extractVersion < 27) ) {
						throw new ZipException(string.Format("Patched data requires higher version than ({0})", extractVersion));
					}

					// Central header flags match local entry flags.
					if ( localFlags != entry.Flags ) {
						throw new ZipException("Central header/local header flags mismatch");
					}

					// Central header compression method matches local entry
					if ( entry.CompressionMethod != ( CompressionMethod )compressionMethod ) {
						throw new ZipException("Central header/local header compression method mismatch");
					}

					// Strong encryption and extract version match
					if ( (localFlags & ( int )GeneralBitFlags.StrongEncryption) != 0 ) {
						if ( extractVersion < 62 ) {
							throw new ZipException("Strong encryption flag set but version not high enough");
						}
					}

					if ( (localFlags & ( int )GeneralBitFlags.HeaderMasked) != 0 ) {
						if ( (fileTime != 0) || (fileDate != 0) ) {
							throw new ZipException("Header masked set but date/time values non-zero");
						}
					}

					if ( (localFlags & ( int )GeneralBitFlags.Descriptor) == 0 ) {
						if ( crcValue != (uint)entry.Crc ) {
							throw new ZipException("Central header/local header crc mismatch");
						}
					}

					// Crc valid for empty entry.
					if ( (size == 0) && (compressedSize == 0) ) {
						if ( crcValue != 0 ) {
							throw new ZipException("Invalid CRC for empty entry");
						}
					}

					// TODO: make test more correct...  can't compare lengths as was done originally as this can fail for MBCS strings
					// Assuming a code page at this point is not valid?  Best is to store the name length in the ZipEntry probably
					if ( entry.Name.Length > storedNameLength ) {
						throw new ZipException("File name length mismatch");
					}

					byte[] nameData = new byte[storedNameLength];
					StreamUtils.ReadFully(baseStream_, nameData);

					string localName = ZipConstants.ConvertToString(nameData);

					// Central directory and local entry name match
					if ( localName != entry.Name ) {
						throw new ZipException("Central header and local header file name mismatch");
					}

					// Directories have zero size.
					if ( entry.IsDirectory ) {
						if ( (compressedSize != 0) || (size != 0) ) {
							throw new ZipException("Directory cannot have size");
						}
					}

					if ( !ZipNameTransform.IsValidName(localName, true) ) {
						throw new ZipException("Name is invalid");
					}

					byte[] data = new byte[extraDataLength];
					StreamUtils.ReadFully(baseStream_, data);
					ZipExtraData ed = new ZipExtraData(data);

					// Extra data / zip64 checks
					if ( ed.Find(1) ) {
						// Zip64 extra data but extract version too low
						if ( extractVersion < ZipConstants.VersionZip64 ) {
							throw new ZipException(
								string.Format("Extra data contains Zip64 information but version {0}.{1} is not high enough",
								extractVersion / 10, extractVersion % 10));
						}

						// Zip64 extra data but size fields dont indicate its required.
						if ( (( uint )size != uint.MaxValue) && (( uint )compressedSize != uint.MaxValue) ) {
							throw new ZipException("Entry sizes not correct for Zip64");
						}

						size = ed.ReadLong();
						compressedSize = ed.ReadLong();
					}
					else {
						// No zip64 extra data but entry requires it.
						if ( (extractVersion >= ZipConstants.VersionZip64) &&
							((( uint )size == uint.MaxValue) || (( uint )compressedSize == uint.MaxValue)) ) {
							throw new ZipException("Required Zip64 extended information missing");
						}
					}
				}
					
				int extraLength = storedNameLength + extraDataLength;
				return offsetOfFirstEntry + entry.Offset + ZipConstants.LocalHeaderBaseSize + extraLength;
			}
		}
		
		#endregion
		#region Updating

		const int DefaultBufferSize = 4096;

		/// <summary>
		/// The kind of update to apply.
		/// </summary>
		enum UpdateCommand
		{
			Copy,       // Copy original file contents.
			Modify,     // Change encryption, compression, attributes, name, time etc, of an existing file.
			Add,        // Add a new file to the archive.
		}

		#region Properties
		/// <summary>
		/// Get / set the <see cref="INameTransform"/> to apply to names when updating.
		/// </summary>
		public INameTransform NameTransform
		{
			get {
				return updateNameTransform_;
			}

			set {
				updateNameTransform_ = value;
			}
		}

		/// <summary>
		/// Get /set the buffer size to be used when updating this zip file.
		/// </summary>
		public int BufferSize
		{
			get { return bufferSize_; }
			set {
				if ( value < 1024 ) {
					throw new ArgumentOutOfRangeException("value", "cannot be below 1024");
				}

				if ( bufferSize_ != value ) {
					bufferSize_ = value;
					copyBuffer_ = null;
				}
			}
		}

		/// <summary>
		/// Get a value indicating an update has <see cref="BeginUpdate">been started</see>.
		/// </summary>
		public bool IsUpdating
		{
			get { return updates_ != null; }
		}

		#endregion
		#region Immediate updating
//		TBD: Direct form of updating
// 
//		public void Update(IEntryMatcher deleteMatcher)
//		{
//		}
//
//		public void Update(IScanner addScanner)
//		{
//		}
		#endregion
		#region Deferred Updating
		/// <summary>
		/// Begin updating this <see cref="ZipFile"/> archive.
		/// </summary>
		/// <param name="archiveStorage">The <see cref="IArchiveStorage">archive storage</see> for use during the update.</param>
		/// <param name="dataSource">The <see cref="IDynamicDataSource">data source</see> to utilise during updating.</param>
		public void BeginUpdate(IArchiveStorage archiveStorage, IDynamicDataSource dataSource)
		{
			if ( IsEmbeddedArchive ) {
				throw new ZipException ("Cannot update embedded/SFX archives");
			}

			if ( archiveStorage == null ) {
				throw new ArgumentNullException("archiveStorage");
			}

			if ( dataSource == null ) {
				throw new ArgumentNullException("dataSource");
			}

			archiveStorage_ = archiveStorage;
			updateDataSource_ = dataSource;

			// NOTE: the baseStream_ may not currently support writing or seeking.

			if ( entries_ != null ) {
				updates_ = new ArrayList(entries_.Length);
				foreach(ZipEntry entry in entries_) {
					updates_.Add(new ZipUpdate(entry));
				}
			}
			else {
				updates_ = new ArrayList();
			}

			contentsEdited_ = false;
			commentEdited_ = false;
			newComment_ = null;
		}

		/// <summary>
		/// Begin updating to this <see cref="ZipFile"/> archive.
		/// </summary>
		/// <param name="archiveStorage">The storage to use during the update.</param>
		public void BeginUpdate(IArchiveStorage archiveStorage)
		{
			BeginUpdate(archiveStorage, new DynamicDiskDataSource());
		}
		
		/// <summary>
		/// Begin updating this <see cref="ZipFile"/> archive.
		/// </summary>
		/// <seealso cref="CommitUpdate"></seealso>
		/// <seealso cref="AbortUpdate"></seealso>
		public void BeginUpdate()
		{
			if ( Name == null ) {
				BeginUpdate(new MemoryArchiveStorage(), new DynamicDiskDataSource());
			}
			else {
				BeginUpdate(new DiskArchiveStorage(this), new DynamicDiskDataSource());
			}
		}

		/// <summary>
		/// Commit current updates, updating this archive.
		/// </summary>
		/// <seealso cref="BeginUpdate"></seealso>
		/// <seealso cref="AbortUpdate"></seealso>
		public void CommitUpdate()
		{
			CheckUpdating();

			if ( contentsEdited_ ) {
				RunUpdates();
			}
			else if ( commentEdited_ ) {
				UpdateCommentOnly();
			}
			else {
				// Create an empty archive if none existed originally.
				if ( (entries_ != null) && (entries_.Length == 0) ) {
					byte[] theComment = (newComment_ != null) ? newComment_.RawComment : ZipConstants.ConvertToArray(comment_);
					using ( ZipHelperStream zhs = new ZipHelperStream(baseStream_) ) {
						zhs.WriteEndOfCentralDirectory(0, 0, 0, theComment);
					}
				}
			}

			PostUpdateCleanup();
		}

		/// <summary>
		/// Abort updating leaving the archive unchanged.
		/// </summary>
		/// <seealso cref="BeginUpdate"></seealso>
		/// <seealso cref="CommitUpdate"></seealso>
		public void AbortUpdate()
		{
			updates_ = null;
			PostUpdateCleanup();
		}

		/// <summary>
		/// Set the file comment to be recorded when the current update is <see cref="CommitUpdate">commited</see>.
		/// </summary>
		/// <param name="comment">The comment to record.</param>
		public void SetComment(string comment)
		{
			CheckUpdating();

			newComment_ = new ZipString(comment);

			if ( newComment_.RawLength  > 0xffff ) {
				newComment_ = null;
				throw new ZipException("Comment length exceeds maximum - 65535");
			}

			// We dont take account of the original and current comment appearing to be the same
			// as encoding may be different.
			commentEdited_ = true;
		}

		#endregion
		#region Adding Entries
		/// <summary>
		/// Add a new entry to the archive.
		/// </summary>
		/// <param name="fileName">The name of the file to add.</param>
		/// <param name="compressionMethod">The compression method to use.</param>
		public void Add(string fileName, CompressionMethod compressionMethod)
		{
			if ( fileName == null ) {
				throw new ArgumentNullException("fileName");
			}

			if ( !ZipEntry.IsCompressionMethodSupported(compressionMethod) ) {
				throw new ZipException("Compression method not supported");
			}

			CheckUpdating();
			contentsEdited_ = true;

			string zipEntryName = GetTransformedFileName(fileName);
			int index = FindExistingUpdate(zipEntryName);

			if ( index >= 0 ) {
				updates_.RemoveAt(index);
			}

			updates_.Add(
				new ZipUpdate(fileName, zipEntryName, compressionMethod));
		}

		/// <summary>
		/// Add a file to the archive.
		/// </summary>
		/// <param name="fileName">The name of the file to add.</param>
		public void Add(string fileName)
		{
			if ( fileName == null ) {
				throw new ArgumentNullException("fileName");
			}

			CheckUpdating();
			Add(fileName, CompressionMethod.Deflated);
		}

		/// <summary>
		/// Add a file entry with data.
		/// </summary>
		/// <param name="dataSource">The source of the data for this entry.</param>
		/// <param name="entryName">The name to give to the entry.</param>
		public void Add(IStaticDataSource dataSource, string entryName)
		{
			if ( dataSource == null ) {
				throw new ArgumentNullException("dataSource");
			}

			CheckUpdating();
			contentsEdited_ = true;
			updates_.Add(new ZipUpdate(dataSource, entryName, CompressionMethod.Deflated));
		}

		/// <summary>
		/// Add a <see cref="ZipEntry"/> that contains no data.
		/// </summary>
		/// <param name="entry">The entry to add.</param>
		public void Add(ZipEntry entry)
		{
			if ( entry == null ) {
				throw new ArgumentNullException("entry");
			}

			CheckUpdating();
			if ( (entry.Size != 0) || (entry.CompressedSize != 0) ) {
				throw new ZipException("Entry cannot have any data");
			}
			contentsEdited_ = true;
			updates_.Add(new ZipUpdate(UpdateCommand.Add, entry));
		}

		/// <summary>
		/// Add a directory entry to the archive.
		/// </summary>
		/// <param name="directoryName">The directory to add.</param>
		public void AddDirectory(string directoryName)
		{
			if ( directoryName == null ) {
				throw new ArgumentNullException("directoryName");
			}

			ZipEntry dirEntry = new ZipEntry(GetTransformedDirectoryName(directoryName));
			dirEntry.ExternalFileAttributes = 16;
			updates_.Add(new ZipUpdate(UpdateCommand.Add, dirEntry));
		}

		#endregion
		#region Modifying Entries
/* Modify not yet ready for public consumption.
   Direct modification of an entry should not overwrite original data before its read.
   Safe mode is trivial in this sense.
		public void Modify(ZipEntry original, ZipEntry updated)
		{
			if ( original == null ) {
				throw new ArgumentNullException("original");
			}

			if ( updated == null ) {
				throw new ArgumentNullException("updated");
			}

			CheckUpdating();
			contentsEdited_ = true;
			updates_.Add(new ZipUpdate(original, updated));
		}
*/
		#endregion
		#region Deleting Entries
		/// <summary>
		/// Delete an entry by name
		/// </summary>
		/// <param name="fileName">The filename to delete</param>
		/// <returns>True if the entry was found and deleted; false otherwise.</returns>
		public bool Delete(string fileName)
		{
			CheckUpdating();

			bool result = false;
			int index = FindExistingUpdate(fileName);
			if ( index >= 0 ) {
				result = true;
				contentsEdited_ = true;
				updates_.RemoveAt(index);
			}
			else {
				throw new ZipException("Cannot find entry to delete");
			}
			return result;
		}

		/// <summary>
		/// Delete a <see cref="ZipEntry"/> from the archive.
		/// </summary>
		/// <param name="entry">The entry to delete.</param>
		public void Delete(ZipEntry entry)
		{
			CheckUpdating();

			int index = FindExistingUpdate(entry);
			if ( index >= 0 ) {
				contentsEdited_ = true;
				updates_.RemoveAt(index);
			}
			else {
				throw new ZipException("Cannot find entry to delete");
			}
		}

		#endregion
		#region Update Support
		#region Writing Values/Headers
		void WriteLEShort(int value)
		{
			baseStream_.WriteByte(( byte )(value & 0xff));
			baseStream_.WriteByte(( byte )((value >> 8) & 0xff));
		}

		/// <summary>
		/// Write an unsigned short in little endian byte order.
		/// </summary>
		void WriteLEUshort(ushort value)
		{
			baseStream_.WriteByte(( byte )(value & 0xff));
			baseStream_.WriteByte(( byte )(value >> 8));
		}

		/// <summary>
		/// Write an int in little endian byte order.
		/// </summary>
		void WriteLEInt(int value)
		{
			WriteLEShort(value);
			WriteLEShort(value >> 16);
		}

		/// <summary>
		/// Write an unsigned int in little endian byte order.
		/// </summary>
		void WriteLEUint(uint value)
		{
			WriteLEUshort((ushort)(value & 0xffff));
			WriteLEUshort((ushort)(value >> 16));
		}

		/// <summary>
		/// Write a long in little endian byte order.
		/// </summary>
		void WriteLeLong(long value)
		{
			WriteLEInt(( int )(value & 0xffffffff));
			WriteLEInt(( int )(value >> 32));
		}

		void WriteLEUlong(ulong value)
		{
			WriteLEUint(( uint )(value & 0xffffffff));
			WriteLEUint(( uint )(value >> 32));
		}

		void WriteLocalEntryHeader(ZipUpdate update)
		{
			ZipEntry entry = update.OutEntry;

			// TODO: Local offset will require adjusting for multi-disk zip files?
			entry.Offset = baseStream_.Position;

			// Write the local file header
			WriteLEInt(ZipConstants.LocalHeaderSignature);

			WriteLEShort(entry.Version);
			WriteLEShort(entry.Flags);

			if (entry.CompressionMethod == CompressionMethod.Deflated) {
				if (entry.Size == 0) {
					// No need to compress - no data.
					entry.CompressedSize = entry.Size;
					entry.Crc = 0;
					entry.CompressionMethod = CompressionMethod.Stored;
				} 
			}
			else if ( entry.CompressionMethod == CompressionMethod.Stored ) {
				entry.Flags &= ~8;
			}

			WriteLEShort(( byte )entry.CompressionMethod);
			WriteLEInt(( int )entry.DosTime);

			if ( !entry.HasCrc ) {
				// Note patch address for updating later.
				update.CrcPatchOffset = baseStream_.Position;
				WriteLEInt(( int )0);
			}
			else
			{
				WriteLEInt(( int )entry.Crc);
			}

			// Force Zip64 if the size of an entry isnt known.
			// A more flexible strategy could be created here if required.
#if !FORCE_ZIP64
			if ( entry.Size < 0 )
#endif
			{
				entry.ForceZip64();
			}

			if ( entry.LocalHeaderRequiresZip64 ) {
				WriteLEInt(-1);
				WriteLEInt(-1);
			}
			else {
				if ( (entry.CompressedSize < 0) || (entry.Size < 0) ) {
					update.SizePatchOffset = baseStream_.Position;
				}

				WriteLEInt(( int )entry.CompressedSize);
				WriteLEInt(( int )entry.Size);
			}

			byte[] name = ZipConstants.ConvertToArray(entry.Name);

			if ( name.Length > 0xFFFF ) {
				throw new ZipException("Entry name too long.");
			}

			ZipExtraData ed = new ZipExtraData(entry.ExtraData);

			if ( entry.LocalHeaderRequiresZip64 ) {
				ed.StartNewEntry();

				// Local entry header always includes size and compressed size.
				// NOTE the order of these fields is reversed when compared to the headers!
				ed.AddLeLong(entry.Size);
				ed.AddLeLong(entry.CompressedSize);
				ed.AddNewEntry(1);
			}
			else {
				ed.Delete(1);
			}

			entry.ExtraData = ed.GetEntryData();

			WriteLEShort(name.Length);
			WriteLEShort(entry.ExtraData.Length);

			if ( name.Length > 0 ) {
				baseStream_.Write(name, 0, name.Length);
			}

			if ( entry.LocalHeaderRequiresZip64 ) {
				if ( !ed.Find(1) ) {
					throw new ZipException("Internal error cannot find extra data");
				}

				update.SizePatchOffset = baseStream_.Position + ed.CurrentReadIndex;
			}

			if ( entry.ExtraData.Length > 0 ) {
				baseStream_.Write(entry.ExtraData, 0, entry.ExtraData.Length);
			}
		}

		int WriteCentralDirectoryHeader(ZipEntry entry)
		{
			// Write the central file header
			WriteLEInt(ZipConstants.CentralHeaderSignature);

			// Version made by
			WriteLEShort(ZipConstants.VersionMadeBy);

			// Version required to extract
			WriteLEShort(entry.Version);

			WriteLEShort(entry.Flags);
			WriteLEShort((byte)entry.CompressionMethod);
			WriteLEInt((int)entry.DosTime);
			WriteLEInt((int)entry.Crc);

			if ( entry.CompressedSize >= 0xffffffff ) {
				WriteLEInt(-1);
			}
			else {
				WriteLEInt((int)(entry.CompressedSize & 0xffffffff));
			}

			if ( entry.Size >= 0xffffffff ) {
				WriteLEInt(-1);
			}
			else {
				WriteLEInt((int)entry.Size);
			}

			byte[] name = ZipConstants.ConvertToArray(entry.Name);

			if ( name.Length > 0xFFFF ) {
				throw new ZipException("Entry name is too long.");
			}

			WriteLEShort(name.Length);

			// Central header extra data is different to local header version so regenerate.
			ZipExtraData ed = new ZipExtraData(entry.ExtraData);

			if ( entry.CentralHeaderRequiresZip64 ) {
				ed.StartNewEntry();

#if !FORCE_ZIP64
				if ( entry.Size >= 0xffffffff )
#endif
				{
					ed.AddLeLong(entry.Size);
				}

#if !FORCE_ZIP64
				if ( entry.CompressedSize >= 0xffffffff )
#endif
				{
					ed.AddLeLong(entry.CompressedSize);
				}

				if ( entry.Offset >= 0xffffffff ) {
					ed.AddLeLong(entry.Offset);
				}

				// Number of disk on which this file starts isnt supported and is never written here.
				ed.AddNewEntry(1);
			}
			else {
				// Should have already be done when local header was added.
				ed.Delete(1);
			}

			byte[] centralExtraData = ed.GetEntryData();

			WriteLEShort(centralExtraData.Length);
			WriteLEShort(entry.Comment != null ? entry.Comment.Length : 0);

			WriteLEShort(0);	// disk number
			WriteLEShort(0);	// internal file attributes

			// External file attributes...
			if ( entry.ExternalFileAttributes != -1 ) {
				WriteLEInt(entry.ExternalFileAttributes);
			}
			else {
				if ( entry.IsDirectory ) {
					WriteLEUint(16);
				}
				else {
					WriteLEUint(0);
				}
			}

			if ( entry.Offset >= 0xffffffff ) {
				WriteLEUint(0xffffffff);
			}
			else {
				WriteLEUint((uint)(int)entry.Offset);
			}

			if ( name.Length > 0 ) {
				baseStream_.Write(name, 0, name.Length);
			}

			if ( centralExtraData.Length > 0 ) {
				baseStream_.Write(centralExtraData, 0, centralExtraData.Length);
			}

			byte[] rawComment = (entry.Comment != null) ? Encoding.ASCII.GetBytes(entry.Comment) : new byte[0];

			if ( rawComment.Length > 0 ) {
				baseStream_.Write(rawComment, 0, rawComment.Length);
			}

			return ZipConstants.CentralHeaderBaseSize + name.Length + centralExtraData.Length + rawComment.Length;
		}
		#endregion
		void PostUpdateCleanup()
		{
			if ( archiveStorage_ != null ) {
				archiveStorage_.Dispose();
				archiveStorage_ = null;
			}

			updateDataSource_ = null;
		}

		string GetTransformedFileName(string name)
		{
			return (updateNameTransform_ != null) ?
				updateNameTransform_.TransformFile(name) :
				name;
		}

		string GetTransformedDirectoryName(string name)
		{
			return (updateNameTransform_ != null) ?
				updateNameTransform_.TransformDirectory(name) :
				name;
		}

		/// <summary>
		/// Get a raw memory buffer.
		/// </summary>
		/// <returns>Returns a raw memory buffer.</returns>
		byte[] GetBuffer()
		{
			if ( copyBuffer_ == null ) {
				copyBuffer_ = new byte[bufferSize_];
			}
			return copyBuffer_;
		}

		void CopyDescriptorBytes(ZipUpdate update, Stream dest, Stream source)
		{
			int bytesToCopy = 0;

			if ( (update.Entry.Flags & (int)GeneralBitFlags.Descriptor) != 0) {
				bytesToCopy = 12;
				if ( update.Entry.LocalHeaderRequiresZip64 ) {
					bytesToCopy = 20;
				}
			}

			if ( bytesToCopy > 0 ) {
				byte[] buffer = GetBuffer();

				while ( bytesToCopy > 0 ) {
					int readSize = Math.Min(buffer.Length, bytesToCopy);

					int bytesRead = source.Read(buffer, 0, readSize);
					if ( bytesRead > 0 ) {
						dest.Write(buffer, 0, bytesRead);
						bytesToCopy -= bytesRead;
					}
					else {
						throw new ZipException("Unxpected end of stream");
					}
				}
			}
		}

		void CopyBytes(ZipUpdate update, Stream destination, Stream source,
			long bytesToCopy, bool updateCrc)
		{
			if ( destination == source ) {
				throw new InvalidOperationException("Destination and source are the same");
			}

			// NOTE: Compressed size is updated elsewhere.
			Crc32 crc = new Crc32();
			byte[] buffer = GetBuffer();

			long targetBytes = bytesToCopy;
			long totalBytesRead = 0;

			int bytesRead;
			do {
				int readSize = buffer.Length;

				if ( bytesToCopy < readSize ) {
					readSize = (int)bytesToCopy;
				}

				bytesRead = source.Read(buffer, 0, readSize);
				if ( bytesRead > 0 ) {
					if ( updateCrc ) {
						crc.Update(buffer, 0, bytesRead);
					}
					destination.Write(buffer, 0, bytesRead);
					bytesToCopy -= bytesRead;
					totalBytesRead += bytesRead;
				}
			}
			while ( (bytesRead > 0) && (bytesToCopy > 0) );

			if ( totalBytesRead != targetBytes ) {
				throw new ZipException(string.Format("Failed to copy bytes expected {0} read {1}", targetBytes, totalBytesRead));
			}

			if ( updateCrc ) {
				update.OutEntry.Crc = crc.Value;
			}
		}

		int GetDescriptorSize(ZipUpdate update)
		{
			int result = 0;
			if ( (update.Entry.Flags & (int)GeneralBitFlags.Descriptor) != 0) {
				result = 12;
				if ( update.Entry.LocalHeaderRequiresZip64 ) {
					result = 20;
				}
			}
			return result;
		}

		void CopyDescriptorBytesDirect(ZipUpdate update, Stream stream, ref long destinationPosition, long sourcePosition)
		{
			int bytesToCopy = GetDescriptorSize(update);

			while ( bytesToCopy > 0 ) {
				int readSize = (int)bytesToCopy;
				byte[] buffer = GetBuffer();

				stream.Position = sourcePosition;
				int bytesRead = stream.Read(buffer, 0, readSize);
				if ( bytesRead > 0 ) {
					stream.Position = destinationPosition;
					stream.Write(buffer, 0, bytesRead);
					bytesToCopy -= bytesRead;
					destinationPosition += bytesRead;
					sourcePosition += bytesRead;
				}
				else {
					throw new ZipException("Unxpected end of stream");
				}
			}
		}

		void CopyEntryDataDirect(ZipUpdate update, Stream stream, bool updateCrc, ref long destinationPosition, ref long sourcePosition)
		{
			long bytesToCopy = update.Entry.CompressedSize;
			
			// NOTE: Compressed size is updated elsewhere.
			Crc32 crc = new Crc32();
			byte[] buffer = GetBuffer();

			long targetBytes = bytesToCopy;
			long totalBytesRead = 0;

			int bytesRead;
			do
			{
				int readSize = buffer.Length;

				if ( bytesToCopy < readSize ) {
					readSize = (int)bytesToCopy;
				}

				stream.Position = sourcePosition;
				bytesRead = stream.Read(buffer, 0, readSize);
				if ( bytesRead > 0 ) {
					if ( updateCrc ) {
						crc.Update(buffer, 0, bytesRead);
					}
					stream.Position = destinationPosition;
					stream.Write(buffer, 0, bytesRead);

					destinationPosition += bytesRead;
					sourcePosition += bytesRead;
					bytesToCopy -= bytesRead;
					totalBytesRead += bytesRead;
				}
			}
			while ( (bytesRead > 0) && (bytesToCopy > 0) );

			if ( totalBytesRead != targetBytes ) {
				throw new ZipException(string.Format("Failed to copy bytes expected {0} read {1}", targetBytes, totalBytesRead));
			}

			if ( updateCrc ) {
				update.OutEntry.Crc = crc.Value;
			}
		}

		int FindExistingUpdate(ZipEntry entry)
		{
			// TODO: Handling of relative\absolute paths when finding entries?
			int result = -1;
			string convertedName = GetTransformedFileName(entry.Name);
			for ( int index = 0; index < updates_.Count; ++index ) {
				ZipUpdate zu = ( ZipUpdate )updates_[index];
				if ( (zu.Entry.ZipFileIndex == entry.ZipFileIndex) &&
					(string.Compare(convertedName, zu.Entry.Name, true, CultureInfo.InvariantCulture) == 0) ) {
					result = index;
					break;
				}
			}
			return result;
		}

		int FindExistingUpdate(string fileName)
		{
			// TODO: Handling of relative\absolute paths when finding updates by name.
			int result = -1;
			string convertedName = GetTransformedFileName(fileName);
			for ( int index = 0; index < updates_.Count; ++index ) {
				if ( string.Compare(convertedName, (( ZipUpdate )updates_[index]).Entry.Name,
					true, CultureInfo.InvariantCulture) == 0 ) {
					result = index;
					break;
				}
			}
			return result;
		}

		/// <summary>
		/// Get an output stream for the specified <see cref="ZipEntry"/>
		/// </summary>
		/// <param name="entry">The entry to get an output stream for.</param>
		/// <returns>The output stream obtained for the entry.</returns>
		Stream GetOutputStream(ZipEntry entry)
		{
			Stream result = baseStream_;

			if ( entry.IsCrypted == true ) {
				result = CreateAndInitEncryptionStream(result, entry);
			}

			switch ( entry.CompressionMethod ) {
				case CompressionMethod.Stored:
					result = new UncompressedStream(result);
					break;

				case CompressionMethod.Deflated:
					DeflaterOutputStream dos = new DeflaterOutputStream(result, new Deflater(9, true));
					dos.IsStreamOwner = false;
					result = dos;
					break;

				default:
					throw new ZipException("Unknown compression method " + entry.CompressionMethod);
			}
			return result;
		}

		void AddEntry(ZipFile workFile, ZipUpdate update)
		{
			long dataStart = 0;
			Stream source = null;

			if ( update.Entry.IsFile ) {
				source = update.GetSource();
				
				if ( source == null ) {
					source = updateDataSource_.GetSource(update.Entry, update.Filename);
				}
			}

			if ( source != null ) {
				using ( source ) {
					long sourceStreamLength = source.Length;
					if ( update.OutEntry.Size < 0 ) {
						update.OutEntry.Size = sourceStreamLength;
					}
					else {
						// Check for errant entries.
						if ( update.OutEntry.Size != sourceStreamLength ) {
							throw new ZipException("Entry size/stream size mismatch");
						}
					}

					workFile.WriteLocalEntryHeader(update);
					dataStart = workFile.baseStream_.Position;

					using ( Stream output = workFile.GetOutputStream(update.Entry) ) {
						CopyBytes(update, output, source, sourceStreamLength, true);
					}
				}
			}
			else {
				workFile.WriteLocalEntryHeader(update);
				dataStart = workFile.baseStream_.Position;
			}

			long dataEnd = workFile.baseStream_.Position;
			update.OutEntry.CompressedSize = dataEnd - dataStart;
		}

		void ModifyEntry(ZipFile workFile, ZipUpdate update)
		{
			workFile.WriteLocalEntryHeader(update);
			long dataStart = workFile.baseStream_.Position;

			// TODO: This is slow if the changes don't effect the data!!
			if ( update.Entry.IsFile && (update.Filename != null) ) {
				using ( Stream output = workFile.GetOutputStream(update.OutEntry) ) {
					using ( Stream source = this.GetInputStream(update.Entry) ) {
						CopyBytes(update, output, source, source.Length, true);
					}
				}
			}

			long dataEnd = workFile.baseStream_.Position;
			update.Entry.CompressedSize = dataEnd - dataStart;
		}

		void CopyEntryDirect(ZipFile workFile, ZipUpdate update, ref long destinationPosition)
		{
			bool skipOver = false;
			if ( update.Entry.Offset == destinationPosition ) {
				skipOver = true;
			}

			if ( !skipOver ) {
				baseStream_.Position = destinationPosition;
				workFile.WriteLocalEntryHeader(update);
				destinationPosition = baseStream_.Position;
			}

			long sourcePosition = 0;

			const int NameLengthOffset = 26;
			// TODO: Add base for SFX friendly handling
			long entryDataOffset = update.Entry.Offset + NameLengthOffset;

			baseStream_.Seek(entryDataOffset, SeekOrigin.Begin);

			// Clumsy way of handling retrieving the original name and extra data length for now.
			uint nameLength = ReadLEUshort();
			uint extraLength = ReadLEUshort();

			sourcePosition = baseStream_.Position + nameLength + extraLength;

			if ( skipOver ) {
				destinationPosition += 
					(sourcePosition - entryDataOffset) + NameLengthOffset +	// Header size
					update.Entry.CompressedSize + GetDescriptorSize(update);
			}
			else {
				if ( update.Entry.CompressedSize > 0 ) {
					CopyEntryDataDirect(update, baseStream_, false, ref destinationPosition, ref sourcePosition );
				}
				CopyDescriptorBytesDirect(update, baseStream_, ref destinationPosition, sourcePosition);
			}
		}

		void CopyEntry(ZipFile workFile, ZipUpdate update)
		{
			workFile.WriteLocalEntryHeader(update);

			if ( update.Entry.CompressedSize > 0 ) {
				const int NameLengthOffset = 26;

				long entryDataOffset = update.Entry.Offset + NameLengthOffset;

				// TODO: This wont work for SFX files!
				baseStream_.Seek(entryDataOffset, SeekOrigin.Begin);

				uint nameLength = ReadLEUshort();
				uint extraLength = ReadLEUshort();

				baseStream_.Seek(nameLength + extraLength, SeekOrigin.Current);

				CopyBytes(update, workFile.baseStream_, baseStream_, update.Entry.CompressedSize, false);
			}
			CopyDescriptorBytes(update, workFile.baseStream_, baseStream_);
		}

		void Reopen(Stream source)
		{
			if ( source == null ) {
				throw new ZipException("Failed to reopen archive - no source");
			}

			isNewArchive_ = false;
			baseStream_ = source;
			ReadEntries();
		}

		void Reopen()
		{
			Reopen(File.OpenRead(Name));
		}

		void UpdateCommentOnly()
		{
			long baseLength = baseStream_.Length;

			ZipHelperStream updateFile = null;

			if ( archiveStorage_.UpdateMode == FileUpdateMode.Safe ) {
				Stream copyStream = archiveStorage_.MakeTemporaryCopy(baseStream_);
				updateFile = new ZipHelperStream(copyStream);
				updateFile.IsStreamOwner = true;

				baseStream_.Close();
				baseStream_ = null;
			}
			else {
				baseStream_.Close();
				baseStream_ = null;

				updateFile = new ZipHelperStream(Name);
			}

			using ( updateFile ) {
				long locatedCentralDirOffset = 
					updateFile.LocateBlockWithSignature(ZipConstants.EndOfCentralDirectorySignature, 
														baseLength, ZipConstants.EndOfCentralRecordBaseSize, 0xffff);
				if ( locatedCentralDirOffset < 0 ) {
					throw new ZipException("Cannot find central directory");
				}

				const int CentralHeaderCommentSizeOffset = 16;
				updateFile.Position += CentralHeaderCommentSizeOffset;

				byte[] rawComment = newComment_.RawComment;

				updateFile.WriteLEShort(rawComment.Length);
				updateFile.Write(rawComment, 0, rawComment.Length);
				updateFile.SetLength(updateFile.Position);
			}

			if ( archiveStorage_.UpdateMode == FileUpdateMode.Safe ) {
				Reopen(archiveStorage_.ConvertTemporaryToFinal());
			}
			else {
				Reopen();
			}
		}

		/// <summary>
		/// Class used to sort updates.
		/// </summary>
		class UpdateComparer : IComparer
		{
			/// <summary>
			/// Compares two objects and returns a value indicating whether one is 
			/// less than, equal to or greater than the other.
			/// </summary>
			/// <param name="x">First object to compare</param>
			/// <param name="y">Second object to compare.</param>
			/// <returns>Compare result.</returns>
			public int Compare(
				object x,
				object y)
			{
				ZipUpdate zx = x as ZipUpdate;
				ZipUpdate zy = y as ZipUpdate;

				int xCmdValue = ((zx.Command == UpdateCommand.Copy) || (zx.Command == UpdateCommand.Modify)) ? 0 : 1;
				int yCmdValue = ((zy.Command == UpdateCommand.Copy) || (zy.Command == UpdateCommand.Modify)) ? 0 : 1;

				int result = xCmdValue - yCmdValue;
				if ( result == 0 ) {
					long offsetDiff = zx.Entry.Offset - zy.Entry.Offset;
					if ( offsetDiff < 0 ) {
						result = -1;
					}
					else if ( offsetDiff == 0 ) {
						result = 0;
					}
					else {
						result = 1;
					}
				}

				return result;
			}

		}

		void RunUpdates()
		{
			long sizeEntries = 0;
			long endOfStream = 0;
			bool allOk = true;
			bool directUpdate = false;
			long destinationPosition = 0; // NOT SFX friendly

			ZipFile workFile;

			if ( IsNewArchive ) {
				workFile = this;
				workFile.baseStream_.Position = 0;
				directUpdate = true;
			}
			else if ( archiveStorage_.UpdateMode == FileUpdateMode.Direct ) {
				workFile = this;
				workFile.baseStream_.Position = 0;
				directUpdate = true;

				// Sort the updates with order offset within copies/modifies, then adds.
				// This ensures that copies will not overwrite any required data.
				updates_.Sort(new UpdateComparer());
			}
			else
			{
				workFile = ZipFile.Create(archiveStorage_.GetTemporaryOutput());
			}

			try {
				foreach ( ZipUpdate update in updates_ ) {
					switch ( update.Command ) {
						case UpdateCommand.Copy:
							if ( directUpdate ) {
								CopyEntryDirect(workFile, update, ref destinationPosition);
							}
							else {
								CopyEntry(workFile, update);
							}
							break;

						case UpdateCommand.Modify:
							// TODO: Direct modifying of an entry.
							ModifyEntry(workFile, update);
							break;

						case UpdateCommand.Add:
							if ( !IsNewArchive && directUpdate ) {
								workFile.baseStream_.Position = destinationPosition;
							}

							AddEntry(workFile, update);

							if ( directUpdate ) {
								destinationPosition = workFile.baseStream_.Position;
							}
							break;
					}
				}

				if ( !IsNewArchive && directUpdate ) {
					workFile.baseStream_.Position = destinationPosition;
				}

				long centralDirOffset = workFile.baseStream_.Position;

				foreach ( ZipUpdate update in updates_ ) {
					sizeEntries += workFile.WriteCentralDirectoryHeader(update.OutEntry);
				}

				byte[] theComment = (newComment_ != null) ? newComment_.RawComment : ZipConstants.ConvertToArray(comment_);
				using ( ZipHelperStream zhs = new ZipHelperStream(workFile.baseStream_) ) {
					zhs.WriteEndOfCentralDirectory(updates_.Count, sizeEntries, centralDirOffset, theComment);
				}

				endOfStream = workFile.baseStream_.Position;
 
				// And now patch entries...
				foreach ( ZipUpdate update in updates_ ) {
					// If the size of the entry is zero leave the crc as 0 as well.
					// The calculated crc will be all bits on...
					if ( (update.CrcPatchOffset > 0) && (update.OutEntry.CompressedSize > 0) ) {
						workFile.baseStream_.Position = update.CrcPatchOffset;
						workFile.WriteLEInt(( int )update.OutEntry.Crc);
					}

					if ( update.SizePatchOffset > 0 ) {
						workFile.baseStream_.Position = update.SizePatchOffset;
						if ( update.Entry.LocalHeaderRequiresZip64 ) {
							workFile.WriteLeLong(update.OutEntry.Size);
							workFile.WriteLeLong(update.OutEntry.CompressedSize);
						}
						else {
							workFile.WriteLEInt(( int )update.OutEntry.CompressedSize);
							workFile.WriteLEInt(( int )update.OutEntry.Size);
						}
					}
				}
			}
			catch(Exception) {
				allOk = false;
			}
			finally {
				if ( directUpdate ) {
					if ( allOk ) {
						workFile.baseStream_.Flush();
						workFile.baseStream_.SetLength(endOfStream);
					}
				}
				else {
					workFile.Close();
				}
			}

			if ( allOk ) {
				if ( directUpdate ) {
					isNewArchive_ = false;
					workFile.baseStream_.Flush();
					ReadEntries();
				}
				else {
					baseStream_.Close();
					Reopen(archiveStorage_.ConvertTemporaryToFinal());
				}
			}
			else {
				workFile.Close();
				if ( !directUpdate && (workFile.Name != null) ) {
					File.Delete(workFile.Name);
				}
			}
		}

		void CheckUpdating()
		{
			if ( updates_ == null ) {
				throw new ZipException("Cannot update until BeginUpdate has been called");
			}
		}

		#endregion
		#region ZipUpdate class
		/// <summary>
		/// Represents a pending update to a Zip file.
		/// </summary>
		class ZipUpdate
		{
			#region Constructors
			public ZipUpdate(string fileName, string entryName, CompressionMethod compressionMethod)
			{
				command_ = UpdateCommand.Add;
				entry_ = new ZipEntry(entryName);
#if FORCE_ZIP64
				entry_.ForceZip64();
#endif
				entry_.CompressionMethod = compressionMethod;
				filename_ = fileName;
			}
			
			public ZipUpdate(string fileName, string entryName)
			{
				command_ = UpdateCommand.Add;
				entry_ = new ZipEntry(entryName);
#if FORCE_ZIP64
				entry_.ForceZip64();
#endif
				filename_ = fileName;
			}

			public ZipUpdate(IStaticDataSource dataSource, string entryName, CompressionMethod compressionMethod)
			{
				command_ = UpdateCommand.Add;
				entry_ = new ZipEntry(entryName);
				entry_.CompressionMethod = compressionMethod;
#if FORCE_ZIP64
				entry_.ForceZip64();
#endif
				dataSource_ = dataSource;
			}

			public ZipUpdate(ZipEntry original, ZipEntry updated)
			{
				throw new ZipException("Modify not currently supported");
				/*
								command_ = UpdateCommand.Modify;
								entry_ = ( ZipEntry )original.Clone();
								outEntry_ = ( ZipEntry )updated.Clone();
				#if FORCE_ZIP64
								entry_.ForceZip64();
				#endif
				*/
			}

			public ZipUpdate(UpdateCommand command, ZipEntry entry)
			{
				command_ = command;
				entry_ = ( ZipEntry )entry.Clone();
#if FORCE_ZIP64
				entry_.ForceZip64();
#endif
			}


			/// <summary>
			/// Copy an existing entry.
			/// </summary>
			/// <param name="entry">The existing entry to copy.</param>
			public ZipUpdate(ZipEntry entry)
				: this(UpdateCommand.Copy, entry)
			{
				// Do nothing.
			}
			#endregion

			/// <summary>
			/// Get the <see cref="ZipEntry"/> for this update.
			/// </summary>
			/// <remarks>This is the source or original entry.</remarks>
			public ZipEntry Entry
			{
				get { return entry_; }
			}

			/// <summary>
			/// Get the <see cref="ZipEntry"/> that will be written to the updated/new file.
			/// </summary>
			public ZipEntry OutEntry
			{
				get {
					if ( outEntry_ == null ) {
						outEntry_ = (ZipEntry)entry_.Clone();
					}

					return outEntry_; 
				}
			}

			/// <summary>
			/// Get the command for this update.
			/// </summary>
			public UpdateCommand Command
			{
				get { return command_; }
			}

			/// <summary>
			/// Get the filename if any for this update.
			/// </summary>
			public string Filename
			{
				get { return filename_; }
			}

			/// <summary>
			/// Get/set the location of the size patch for this update.
			/// </summary>
			public long SizePatchOffset
			{
				get { return sizePatchOffset_; }
				set { sizePatchOffset_ = value; }
			}

			/// <summary>
			/// Get /set the location of the crc patch for this update.
			/// </summary>
			public long CrcPatchOffset
			{
				get { return crcPatchOffset_; }
				set { crcPatchOffset_ = value; }
			}

			public Stream GetSource()
			{
				Stream result = null;
				if ( dataSource_ != null ) {
					result = dataSource_.GetSource();
				}

				return result;
			}

			#region Instance Fields
			ZipEntry entry_;
			ZipEntry outEntry_;
			UpdateCommand command_;
			IStaticDataSource dataSource_;
			string filename_;
			long sizePatchOffset_ = -1;
			long crcPatchOffset_ = -1;
			#endregion
		}

		#endregion
		#endregion
		#region Disposing
		#region IDisposable Members
		void IDisposable.Dispose()
		{
			Close();
		}
		#endregion

		void DisposeInternal(bool disposing)
		{
			if ( !isDisposed_ ) {
				isDisposed_ = true;
				entries_ = null;
				if ( IsStreamOwner ) {
					lock(baseStream_) {
						baseStream_.Close();
					}
				}
			}
		}

		/// <summary>
		/// Releases the unmanaged resources used by the this instance and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources;
		/// false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			DisposeInternal(disposing);
		}

		#endregion
		#region Internal routines
		#region Reading
		/// <summary>
		/// Read an unsigned short in little endian byte order.
		/// </summary>
		/// <returns>Returns the value read.</returns>
		/// <exception cref="IOException">
		/// An i/o error occurs.
		/// </exception>
		/// <exception cref="EndOfStreamException">
		/// The file ends prematurely
		/// </exception>
		ushort ReadLEUshort()
		{
			int data1 = baseStream_.ReadByte();

			if ( data1 < 0 ) {
				throw new IOException("End of stream");
			}

			int data2 = baseStream_.ReadByte();

			if ( data2 < 0 ) {
				throw new IOException("End of stream");
			}


			return unchecked((ushort)((ushort)data1 | (ushort)(data2 << 8)));
		}

		/// <summary>
		/// Read a uint in little endian byte order.
		/// </summary>
		/// <returns>Returns the value read.</returns>
		/// <exception cref="IOException">
		/// An i/o error occurs.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The file ends prematurely
		/// </exception>
		uint ReadLEUint()
		{
			return (uint)(ReadLEUshort() | (ReadLEUshort() << 16));
		}

		ulong ReadLEUlong()
		{
			return ReadLEUint() | (ReadLEUint() << 32);
		}

		#endregion
		// NOTE this returns the offset of the first byte after the signature.
		long LocateBlockWithSignature(int signature, long endLocation, int minimumBlockSize, int maximumVariableData)
		{
			using ( ZipHelperStream les = new ZipHelperStream(baseStream_) ) {
				return les.LocateBlockWithSignature(signature, endLocation, minimumBlockSize, maximumVariableData);
			}
		}
		
		/// <summary>
		/// Search for and read the central directory of a zip file filling the entries array.
		/// </summary>
		/// <exception cref="System.IO.IOException">
		/// An i/o error occurs.
		/// </exception>
		/// <exception cref="_videoConference.ZipUnzip.Zip.ZipException">
		/// The central directory is malformed or cannot be found
		/// </exception>
		void ReadEntries()
		{
			// Search for the End Of Central Directory.  When a zip comment is
			// present the directory will start earlier
			// 
			// The search is limited to 64K which is the maximum size of a trailing comment field to aid speed.
			// This should be compatible with both SFX and ZIP files but has only been tested for Zip files
			// If a SFX file has the Zip data attached as a resource and there are other resources occuring later then
			// this could be invalid.
			// Could also speed this up by reading memory in larger blocks.			

			if (baseStream_.CanSeek == false) {
				throw new ZipException("ZipFile stream must be seekable");
			}
			
			long locatedEndOfCentralDir = LocateBlockWithSignature(ZipConstants.EndOfCentralDirectorySignature,
				baseStream_.Length, ZipConstants.EndOfCentralRecordBaseSize, 0xffff);
			
			if (locatedEndOfCentralDir < 0) {
				throw new ZipException("Cannot find central directory");
			}

			// Read end of central directory record
			ushort thisDiskNumber           = ReadLEUshort();
			ushort startCentralDirDisk      = ReadLEUshort();
			ulong entriesForThisDisk        = ReadLEUshort();
			ulong entriesForWholeCentralDir = ReadLEUshort();
			ulong centralDirSize            = ReadLEUint();
			long offsetOfCentralDir         = ReadLEUint();
			uint commentSize                = ReadLEUshort();
			
			if ( commentSize > 0 ) {
				byte[] comment = new byte[commentSize]; 

				StreamUtils.ReadFully(baseStream_, comment);
				comment_ = ZipConstants.ConvertToString(comment); 
			}
			else
			{
				comment_ = string.Empty;
			}
			
			bool isZip64 = false;

			// Check if zip64 header information is required.
			if ( (thisDiskNumber == 0xffff) ||
				(startCentralDirDisk == 0xffff) ||
				(entriesForThisDisk == 0xffff) ||
				(entriesForWholeCentralDir == 0xffff) ||
				(centralDirSize == 0xffffffff) ||
				(offsetOfCentralDir == 0xffffffff) ) {
				isZip64 = true;

				long offset = LocateBlockWithSignature(ZipConstants.Zip64CentralDirLocatorSignature, locatedEndOfCentralDir, 0, 0x1000);
				if ( offset < 0 ) {
					throw new ZipException("Cannot find Zip64 locator");
				}

				// number of the disk with the start of the zip64 end of central directory 4 bytes 
				// relative offset of the zip64 end of central directory record 8 bytes 
				// total number of disks 4 bytes 
				ReadLEUint(); // startDisk64 is not currently used
				ulong offset64 = ReadLEUlong();
				uint totalDisks = ReadLEUint();

				baseStream_.Position = (long)offset64;
				long sig64 = ReadLEUint();

				if ( sig64 != ZipConstants.Zip64CentralFileHeaderSignature ) {
					throw new ZipException(string.Format("Invalid Zip64 Central directory signature at {0:X}", offset64));
				}

				// NOTE: Record size = SizeOfFixedFields + SizeOfVariableData - 12.
				ulong recordSize = ( ulong )ReadLEUlong();
				int versionMadeBy = ReadLEUshort();
				int versionToExtract = ReadLEUshort();
				uint thisDisk = ReadLEUint();
				uint centralDirDisk = ReadLEUint();
				entriesForThisDisk = ReadLEUlong();
				entriesForWholeCentralDir = ReadLEUlong();
				centralDirSize = ReadLEUlong();
				offsetOfCentralDir = (long)ReadLEUlong();

				// NOTE: zip64 extensible data sector (variable size) is ignored.
			}
			
			entries_ = new ZipEntry[entriesForThisDisk];
			
			// SFX/embedded support, find the offset of the first entry vis the start of the stream
			// This applies to Zip files that are appended to the end of an SFX stub.
			// Or are appended as a resource to an executable.
			// Zip files created by some archivers have the offsets altered to reflect the true offsets
			// and so dont require any adjustment here...
			// TODO: Difficulty with Zip64 and SFX offset handling needs resolution - maths?
			if ( !isZip64 && (offsetOfCentralDir < locatedEndOfCentralDir - (4 + (long)centralDirSize)) ) {
				offsetOfFirstEntry = locatedEndOfCentralDir - (4 + (long)centralDirSize + offsetOfCentralDir);
				if (offsetOfFirstEntry <= 0) {
					throw new ZipException("Invalid embedded zip archive");
				}
			}

			baseStream_.Seek(offsetOfFirstEntry + offsetOfCentralDir, SeekOrigin.Begin);
			
			for (ulong i = 0; i < entriesForThisDisk; i++) {
				if (ReadLEUint() != ZipConstants.CentralHeaderSignature) {
					throw new ZipException("Wrong Central Directory signature");
				}
				
				int versionMadeBy      = ReadLEUshort();
				int versionToExtract   = ReadLEUshort();
				int bitFlags           = ReadLEUshort();
				int method             = ReadLEUshort();
				uint dostime           = ReadLEUint();
				uint crc               = ReadLEUint();
				long csize             = (long)ReadLEUint();
				long size              = (long)ReadLEUint();
				int nameLen            = ReadLEUshort();
				int extraLen           = ReadLEUshort();
				int commentLen         = ReadLEUshort();
				
				int diskStartNo        = ReadLEUshort();  // Not currently used
				int internalAttributes = ReadLEUshort();  // Not currently used

				uint externalAttributes = ReadLEUint();
				long offset             = ReadLEUint();
				
				byte[] buffer = new byte[Math.Max(nameLen, commentLen)];
				
				StreamUtils.ReadFully(baseStream_, buffer, 0, nameLen);
				string name = ZipConstants.ConvertToString(buffer, nameLen);
				
				ZipEntry entry = new ZipEntry(name, versionToExtract, versionMadeBy, (CompressionMethod)method);
				entry.Crc = crc & 0xffffffffL;
				entry.Size = size & 0xffffffffL;
				entry.CompressedSize = csize & 0xffffffffL;
				entry.Flags = bitFlags;
				entry.DosTime = (uint)dostime;

				if ((bitFlags & 8) == 0) {
					entry.CryptoCheckValue = (byte)(crc >> 24);
				}
				else {
					entry.CryptoCheckValue = (byte)((dostime >> 8) & 0xff);
				}

				if (extraLen > 0) {
					byte[] extra = new byte[extraLen];
					StreamUtils.ReadFully(baseStream_, extra);
					entry.ExtraData = extra;
				}

				entry.ProcessExtraData(false);
				
				if (commentLen > 0) {
					StreamUtils.ReadFully(baseStream_, buffer, 0, commentLen);
					entry.Comment = ZipConstants.ConvertToString(buffer, commentLen);
				}
				
				entry.ZipFileIndex           = (long)i;
				entry.Offset                 = offset;
				entry.ExternalFileAttributes = (int)externalAttributes;
				
				entries_[i] = entry;
			}
		}

		/// <summary>
		/// Locate the data for a given entry.
		/// </summary>
		/// <returns>
		/// The start offset of the data.
		/// </returns>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The stream ends prematurely
		/// </exception>
		/// <exception cref="_videoConference.ZipUnzip.Zip.ZipException">
		/// The local header signature is invalid, the entry and central header file name lengths are different
		/// or the local and entry compression methods dont match
		/// </exception>
		long LocateEntry(ZipEntry entry)
		{
			return TestLocalHeader(entry, HeaderTest.Extract);
		}
		
		Stream CreateAndInitDecryptionStream(Stream baseStream, ZipEntry entry)
		{
			CryptoStream result = null;

			if ( (entry.Version < ZipConstants.VersionStrongEncryption)
				|| (entry.Flags & (int)GeneralBitFlags.StrongEncryption) == 0) {
				PkzipClassicManaged classicManaged = new PkzipClassicManaged();

				OnKeysRequired(entry.Name);
				if (HaveKeys == false) {
					throw new ZipException("No password available for encrypted stream");
				}

				result = new CryptoStream(baseStream, classicManaged.CreateDecryptor(key, null), CryptoStreamMode.Read);
				CheckClassicPassword(result, entry);
			}
			else {
				throw new ZipException("Decryption method not supported");
			}

			return result;
		}

		Stream CreateAndInitEncryptionStream(Stream baseStream, ZipEntry entry)
		{
			CryptoStream result = null;
			if ( (entry.Version < ZipConstants.VersionStrongEncryption)
				|| (entry.Flags & (int)GeneralBitFlags.StrongEncryption) == 0) {
				PkzipClassicManaged classicManaged = new PkzipClassicManaged();

				OnKeysRequired(entry.Name);
				if (HaveKeys == false) {
					throw new ZipException("No password available for encrypted stream");
				}

				// Closing a CryptoStream will close the base stream as well so wrap it in an UncompressedStream
				// which doesnt do this.
				result = new CryptoStream(new UncompressedStream(baseStream),
					classicManaged.CreateEncryptor(key, null), CryptoStreamMode.Write);

				if ( (entry.Crc < 0) || (entry.Flags & 8) != 0) {
					WriteEncryptionHeader(result, entry.DosTime << 16);
				}
				else {
					WriteEncryptionHeader(result, entry.Crc);
				}
			}
			return result;
		}

		static void CheckClassicPassword(CryptoStream classicCryptoStream, ZipEntry entry)
		{
			byte[] cryptbuffer = new byte[ZipConstants.CryptoHeaderSize];
			StreamUtils.ReadFully(classicCryptoStream, cryptbuffer);
			if (cryptbuffer[ZipConstants.CryptoHeaderSize - 1] != entry.CryptoCheckValue) {
				throw new ZipException("Invalid password");
			}
		}

		static void WriteEncryptionHeader(Stream stream, long crcValue)
		{
			byte[] cryptBuffer = new byte[ZipConstants.CryptoHeaderSize];
			Random rnd = new Random();
			rnd.NextBytes(cryptBuffer);
			cryptBuffer[11] = (byte)(crcValue >> 24);
			stream.Write(cryptBuffer, 0, cryptBuffer.Length);
		}

		#endregion
		#region Instance Fields
		bool       isDisposed_;
		string     name_;
		string     comment_;
		Stream     baseStream_;
		bool       isStreamOwner = true;
		long       offsetOfFirstEntry;
		ZipEntry[] entries_;
		byte[] key;
		bool isNewArchive_;
		#region Zip Update Instance Fields
		ArrayList updates_;
		IArchiveStorage archiveStorage_;
		IDynamicDataSource updateDataSource_;
		bool contentsEdited_;
		int bufferSize_ = DefaultBufferSize;
		byte[] copyBuffer_;
		ZipString newComment_;
		bool commentEdited_;
		INameTransform updateNameTransform_ = new ZipNameTransform();
		string tempDirectory_ = string.Empty;
		#endregion
		#endregion
		#region Support Classes
		/// <summary>
		/// Represents a string from a <see cref="ZipFile"/> which is stored as an array of bytes.
		/// </summary>
		class ZipString
		{
			#region Constructors
			/// <summary>
			/// Initialise a <see cref="ZipString"/> with a string.
			/// </summary>
			/// <param name="comment">The textual string form.</param>
			public ZipString(string comment)
			{
				comment_ = comment;
				sourceIsString_ = true;
			}

			/// <summary>
			/// Initialise a <see cref="ZipString"/> using a string in its binary 'raw' form.
			/// </summary>
			/// <param name="rawString"></param>
			public ZipString(byte[] rawString)
			{
				rawComment_ = rawString;
			}
			#endregion

			/// <summary>
			/// Get the length of the comment when represented as raw bytes.
			/// </summary>
			public int RawLength
			{
				get {
					MakeBytesAvailable();
					return rawComment_.Length;
				}
			}

			/// <summary>
			/// Get the comment in its 'raw' form as plain bytes.
			/// </summary>
			public byte[] RawComment
			{
				get {
					MakeBytesAvailable();
					return (byte[])rawComment_.Clone();
				}
			}

			/// <summary>
			/// Reset the comment to its initial state.
			/// </summary>
			public void Reset()
			{
				if ( sourceIsString_ ) {
					rawComment_ = null;
				}
				else {
					comment_ = null;
				}
			}

			void MakeTextAvailable() 
			{
				if ( comment_ == null ) {
					comment_ = ZipConstants.ConvertToString(rawComment_);
				}
			}

			void MakeBytesAvailable()
			{
				if ( rawComment_ == null ) {
					rawComment_ = ZipConstants.ConvertToArray(comment_);
				}
			}

			/// <summary>
			/// Implicit conversion of comment to a string.
			/// </summary>
			/// <param name="comment">The <see cref="ZipString"/> to convert to a string.</param>
			/// <returns>The string value for the comment.</returns>
			static public implicit operator string(ZipString comment)
			{
				comment.MakeTextAvailable();
				return comment.comment_;
			}

			#region Instance Fields
			string comment_;
			byte[] rawComment_;
			bool sourceIsString_;
			#endregion
		}
		
		class ZipEntryEnumerator : IEnumerator
		{
			#region Constructors
			public ZipEntryEnumerator(ZipEntry[] entries)
			{
				array = entries;
			}
			
			#endregion
			#region IEnumerator Members
			public object Current 
			{
				get {
					return array[index];
				}
			}
			
			public void Reset()
			{
				index = -1;
			}
			
			public bool MoveNext() 
			{
				return (++index < array.Length);
			}
			#endregion
			#region Instance Fields
			ZipEntry[] array;
			int index = -1;
			#endregion
		}

		/// <summary>
		/// An <see cref="UncompressedStream"/> is a stream that you can write uncompressed data
		/// to and flush, but cannot read, seek or do anything else to.
		/// </summary>
		class UncompressedStream : Stream
		{
			#region Constructors
			public UncompressedStream(Stream baseStream)
			{
				baseStream_ = baseStream;
			}

			#endregion

			/// <summary>
			/// Close this stream instance.
			/// </summary>
			public override void Close()
			{
				// Do nothing
			}

			/// <summary>
			/// Gets a value indicating whether the current stream supports reading.
			/// </summary>
			public override bool CanRead
			{
				get {
					return false;
				}
			}

			/// <summary>
			/// Write any buffered data to underlying storage.
			/// </summary>
			public override void Flush()
			{
				baseStream_.Flush();
			}

			/// <summary>
			/// Gets a value indicating whether the current stream supports writing.
			/// </summary>
			public override bool CanWrite
			{
				get {
					return baseStream_.CanWrite;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the current stream supports seeking.
			/// </summary>
			public override bool CanSeek
			{
				get {
					return false;
				}
			}

			/// <summary>
			/// Get the length in bytes of the stream.
			/// </summary>
			public override long Length
			{
				get {
					return 0;
				}
			}

			/// <summary>
			/// Gets or sets the position within the current stream.
			/// </summary>
			public override long Position
			{
				get	{
					return baseStream_.Position;
				}
				
				set
				{
				}
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				return 0;
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				return 0;
			}

			public override void SetLength(long value)
			{
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				baseStream_.Write(buffer, offset, count);
			}

			#region Instance Fields
			Stream baseStream_;
			#endregion
		}
		
		/// <summary>
		/// A <see cref="PartialInputStream"/> is an <see cref="InflaterInputStream"/>
		/// whose data is only a part or subsection of a file.
		/// </summary>
		class PartialInputStream : InflaterInputStream
		{
			#region Constructors
			/// <summary>
			/// Initialise a new instance of the <see cref="PartialInputStream"/> class.
			/// </summary>
			/// <param name="baseStream">The underlying stream to use for IO.</param>
			/// <param name="start">The start of the partial data.</param>
			/// <param name="length">The length of the partial data.</param>
			public PartialInputStream(Stream baseStream, long start, long length)
				: base(baseStream)
			{
				baseStream_ = baseStream;
				filepos_ = start;
				end_ = start + length;
			}
			
			#endregion

			/// <summary>
			/// Skip the specified number of input bytes.
			/// </summary>
			/// <param name="count">The maximum number of input bytes to skip.</param>
			/// <returns>The actuial number of input bytes skipped.</returns>
			public long SkipBytes(long count)
			{
				if (count < 0) {
					throw new ArgumentOutOfRangeException("count", "is less than zero");
				}
				
				if (count > end_ - filepos_) {
					count = end_ - filepos_;
				}
				
				filepos_ += count;
				return count;
			}

			public override int Available 
			{
				get {
					long amount = end_ - filepos_;
					if (amount > int.MaxValue) {
						return int.MaxValue;
					}
					
					return (int) amount;
				}
			}

			/// <summary>
			/// Read a byte from this stream.
			/// </summary>
			/// <returns>Returns the byte read or -1 on end of stream.</returns>
			public override int ReadByte()
			{
				if (filepos_ == end_) {
					 // -1 is the correct value at end of stream.
					return -1;
				}
				
				lock( baseStream_ ) {
					baseStream_.Seek(filepos_++, SeekOrigin.Begin);
					return baseStream_.ReadByte();
				}
			}
			
			/// <summary>
			/// Close this <see cref="PartialInputStream">partial input stream</see>.
			/// </summary>
			/// <remarks>
			/// The underlying stream is not closed.  Close the parent ZipFile class to do that.
			/// </remarks>
			public override void Close()
			{
				// Do nothing at all!
			}
			
			public override int Read(byte[] buffer, int offset, int count)
			{
				if (count > end_ - filepos_) {
					count = (int) (end_ - filepos_);
					if (count == 0) {
						return 0;
					}
				}
				
				lock(baseStream_) {
					baseStream_.Seek(filepos_, SeekOrigin.Begin);
					int readCount = baseStream_.Read(buffer, offset, count);
					if (readCount > 0) {
						filepos_ += readCount;
					}
					return readCount;
				}
			}
			
			#region Instance Fields
			Stream baseStream_;
			long filepos_;
			long end_;
			#endregion	
		}
		#endregion
	}

	#endregion
	#region DataSources
	/// <summary>
	/// Provides a static way to obtain a source of data for an entry.
	/// </summary>
	/// <remarks>The </remarks>
	public interface IStaticDataSource
	{
		/// <summary>
		/// Get a data source.
		/// </summary>
		/// <returns>Returns a <see cref="Stream"/> to use for compression input.</returns>
		Stream GetSource();
	}

	/// <summary>
	/// Represents a source of data that dynamically provide multiple <see cref="Stream">data sources</see>
	/// based on the parameters passed.
	/// </summary>
	public interface IDynamicDataSource
	{
		/// <summary>
		/// Get a data source.
		/// </summary>
		/// <param name="entry">The <see cref="ZipEntry"/> to get a source for.</param>
		/// <param name="name">The name for data if known.</param>
		/// <returns>Returns a <see cref="Stream"/> to use for compression input.</returns>
		Stream GetSource(ZipEntry entry, string name);
	}

	/// <summary>
	/// Default implementation of a <see cref="IStaticDataSource"/>
	/// </summary>
	class StaticDiskDataSource : IStaticDataSource
	{
		/// <summary>
		/// Initialise a new instnace of <see cref="StaticDiskDataSource"/>
		/// </summary>
		/// <param name="fileName">The name of the file to obtain data from.</param>
		public StaticDiskDataSource(string fileName)
		{
			fileName_ = fileName;
		}

		#region IDataSource Members

		/// <summary>
		/// Get a <see cref="Stream"/> providing data.
		/// </summary>
		/// <returns>Returns a <see cref="Stream"/> provising data.</returns>
		public Stream GetSource()
		{
			return File.OpenRead(fileName_);
		}

		#endregion
		#region Instance Fields
		string fileName_;
		#endregion
	}


	/// <summary>
	/// Default implementation of <see cref="IDynamicDataSource"/>
	/// </summary>
	class DynamicDiskDataSource : IDynamicDataSource
	{
		/// <summary>
		/// Initialise a default instance of <see cref="DynamicDiskDataSource"/>.
		/// </summary>
		public DynamicDiskDataSource()
		{
		}

		#region IDataSource Members
		/// <summary>
		/// Get a <see cref="Stream"/> providing data for an entry.
		/// </summary>
		/// <param name="entry">The entry to provide data for.</param>
		/// <param name="name">The file name for data if known.</param>
		/// <returns>Returns a stream providing data; or null if not available</returns>
		public Stream GetSource(ZipEntry entry, string name)
		{
			Stream result = null;

			if ( name != null ) {
				result = File.OpenRead(name);
			}

			return result;
		}

		#endregion
	}

	#endregion
	#region Archive Storage
	/// <summary>
	/// Describes facilities for generic storage when updating Zip Archives.
	/// </summary>
	public interface IArchiveStorage
	{
		/// <summary>
		/// Get the <see cref="FileUpdateMode"/> to apply during updates.
		/// </summary>
		FileUpdateMode UpdateMode { get; }

		/// <summary>
		/// Get an empty <see cref="Stream"/> that can be used as for temporary output
		/// </summary>
		/// <returns>Returns a temporary output <see cref="Stream"/></returns>
		Stream GetTemporaryOutput();

		/// <summary>
		/// Convert a temporary stream to a final stream.
		/// </summary>
		/// <returns>The resulting final <see cref="Stream"/></returns>
		Stream ConvertTemporaryToFinal();

		/// <summary>
		/// Make a temporary copy of the original stream.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/> to copy.</param>
		/// <returns>Returns a temporary output <see cref="Stream"/> that is a copy of the input.</returns>
		Stream MakeTemporaryCopy(Stream stream);

		/// <summary>
		/// Dispose of this instance.
		/// </summary>
		void Dispose();
	}

	/// <summary>
	/// An abstract <see cref="IArchiveStorage"/> suitable for extension by inheritance.
	/// </summary>
	abstract public class BaseArchiveStorage : IArchiveStorage
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="BaseArchiveStorage"/> class.
		/// </summary>
		/// <param name="updateMode">The update mode.</param>
		public BaseArchiveStorage(FileUpdateMode updateMode)
		{
			updateMode_ = updateMode;
		}
		#endregion
		#region IArchiveStorage Members

		/// <summary>
		/// Gets the temporary output <see cref="Stream"/>
		/// </summary>
		/// <returns>Returns the temporary output stream.</returns>
		public abstract Stream GetTemporaryOutput();

		/// <summary>
		/// Converts the temporary <see cref="Stream"/> to its final form.
		/// </summary>
		/// <returns>Returns a <see cref="Stream"/> that can be used to read
		/// the final storage for the archive.</returns>
		public abstract Stream ConvertTemporaryToFinal();

		/// <summary>
		/// Make a temporary copy of the a <see cref="Stream"/>.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/> to make a copy of.</param>
		/// <returns>Returns a temporary output <see cref="Stream"/> that is a copy of the input.</returns>
		public abstract Stream MakeTemporaryCopy(Stream stream);

		/// <summary>
		/// Disposes this instance.
		/// </summary>
		public abstract void Dispose();

		/// <summary>
		/// Gets the update mode.
		/// </summary>
		/// <value>The update mode.</value>
		public FileUpdateMode UpdateMode
		{
			get {
				return updateMode_;
			}
		}

		#endregion
		#region Instance Fields
		FileUpdateMode updateMode_;
		#endregion
	}

	/// <summary>
	/// An <see cref="IArchiveStorage"/> implementation suitable for hard disks.
	/// </summary>
	public class DiskArchiveStorage : BaseArchiveStorage
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="DiskArchiveStorage"/> class.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="updateMode">The update mode.</param>
		public DiskArchiveStorage(ZipFile file, FileUpdateMode updateMode)
			: base(updateMode)
		{
			if ( file.Name == null ) {
				throw new ZipException("Cant handle non file archives");
			}

			fileName_ = file.Name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DiskArchiveStorage"/> class.
		/// </summary>
		/// <param name="file">The file.</param>
		public DiskArchiveStorage(ZipFile file)
			: this(file, FileUpdateMode.Safe)
		{
		}
		#endregion
		#region IArchiveStorage Members

		/// <summary>
		/// Gets the temporary output <see cref="Stream"/>
		/// </summary>
		/// <returns>Returns the temporary output stream.</returns>
		public override Stream GetTemporaryOutput()
		{
			if ( temporaryName_ != null ) {
				temporaryName_ = GetTempFileName(temporaryName_, true);
				temporaryStream_ = File.OpenWrite(temporaryName_);
			}
			else {
				// Determine where to place files based on internal strategy.
				// Currently this is always done in system temp directory.
				temporaryName_ = Path.GetTempFileName();
				temporaryStream_ = File.OpenWrite(temporaryName_);
			}

			return temporaryStream_;
		}

		/// <summary>
		/// Converts the temporary <see cref="Stream"/> to its final form.
		/// </summary>
		/// <returns>Returns a <see cref="Stream"/> that can be used to read
		/// the final storage for the archive.</returns>
		public override Stream ConvertTemporaryToFinal()
		{
			if ( temporaryStream_ == null ) {
				throw new ZipException("No temporary stream has been created");
			}

			Stream result = null;

			string moveTempName = GetTempFileName(fileName_, false);
			bool newFileCreated = false;

			try	{
				temporaryStream_.Close();
				File.Move(fileName_, moveTempName);
				File.Move(temporaryName_, fileName_);
				newFileCreated = true;
				File.Delete(moveTempName);

				result = File.OpenRead(fileName_);
			}
			catch(Exception) {
				result  = null;

				// Try to roll back changes...
				if ( !newFileCreated ) {
					File.Move(moveTempName, fileName_);
					File.Delete(temporaryName_);
				}

				throw;
			}

			return result;
		}

		/// <summary>
		/// Make a temporary copy of the a stream.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/> to copy.</param>
		/// <returns>Returns a temporary output <see cref="Stream"/> that is a copy of the input.</returns>
		public override Stream MakeTemporaryCopy(Stream stream)
		{
			stream.Close();

			string tempName = GetTempFileName(fileName_, true);
			File.Copy(fileName_, tempName, true);

			temporaryStream_ = new FileStream(tempName, 
				FileMode.Open, 
				FileAccess.ReadWrite);
			return temporaryStream_;
		}

		/// <summary>
		/// Disposes this instance.
		/// </summary>
		public override void Dispose()
		{
			if ( temporaryStream_ != null ) {
				temporaryStream_.Close();
			}
		}

		#endregion
		#region Internal routines
		string GetTempFileName(string original, bool makeTempFile)
		{
			string result = null;
				
			if ( original == null ) {
				result = Path.GetTempFileName();
			}
			else {
				int counter = 0;
				int suffixSeed = DateTime.Now.Second;

				while ( result == null ) {
					counter += 1;
					string newName = string.Format("{0}.{1}{2}.tmp", original, suffixSeed, counter);
					if ( !File.Exists(newName) ) {
						if ( makeTempFile) {
							try	{
								// Try and create the file.
								using ( FileStream stream = File.Create(newName) ) {
								}
								result = newName;
							}
							catch {
								suffixSeed = DateTime.Now.Second;
							}
						}
						else {
							result = newName;
						}
					}
				}
			}
			return result;
		}
		#endregion
		#region Instance Fields
		Stream temporaryStream_;
		string fileName_;
		string temporaryName_;
		#endregion
	}

	/// <summary>
	/// An <see cref="IArchiveStorage"/> implementation suitable for in memory streams.
	/// </summary>
	public class MemoryArchiveStorage : BaseArchiveStorage
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryArchiveStorage"/> class.
		/// </summary>
		public MemoryArchiveStorage() 
			: base(FileUpdateMode.Direct)
		{
		}
		#endregion
		#region IArchiveStorage Members

		/// <summary>
		/// Gets the temporary output <see cref="Stream"/>
		/// </summary>
		/// <returns>Returns the temporary output stream.</returns>
		public override Stream GetTemporaryOutput()
		{
			temporaryStream_ = new MemoryStream();
			return temporaryStream_;
		}

		/// <summary>
		/// Converts the temporary <see cref="Stream"/> to its final form.
		/// </summary>
		/// <returns>Returns a <see cref="Stream"/> that can be used to read
		/// the final storage for the archive.</returns>
		public override Stream ConvertTemporaryToFinal()
		{
			if ( temporaryStream_ == null ) {
				throw new ZipException("No temporary stream has been created");
			}

			return new MemoryStream(temporaryStream_.ToArray());
		}

		/// <summary>
		/// Make a temporary copy of the original stream.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/> to copy.</param>
		/// <returns>Returns a temporary output <see cref="Stream"/> that is a copy of the input.</returns>
		public override Stream MakeTemporaryCopy(Stream stream)
		{
			temporaryStream_ = new MemoryStream();
			stream.Position = 0;
			StreamUtils.Copy(stream, temporaryStream_, new byte[4096]);
			return temporaryStream_;
		}

		/// <summary>
		/// Disposes this instance.
		/// </summary>
		public override void Dispose()
		{
			if ( temporaryStream_ != null ) {
				temporaryStream_.Close();
			}
		}

		#endregion
		#region Instance Fields
		MemoryStream temporaryStream_;
		#endregion
	}

	#endregion
}