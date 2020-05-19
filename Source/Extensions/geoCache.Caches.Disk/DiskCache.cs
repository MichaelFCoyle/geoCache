//
// File: DiskCache.cs
//
// Author:
//   Steinar Herland <steinar.herland@gecko.no>
//
// Copyright (C) 2006-2007 MetaCarta, Inc.
// Copyright (C) 2008 Steinar Herland
// Copyright (C) 2008 Gecko Informasjonssystmer AS (http://www.gecko.no)
// Copyright (C) 2015 Blue Toque Software (http://www.BlueToque.ca)
//
// Licensed under the terms of the GNU Lesser General Public License
// (http://www.opensource.org/licenses/lgpl-license.php)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GeoCache.Core;

namespace GeoCache.Caches.Disk
{
	public class DiskCache : Cache
	{
		#region python - constructor
		/*
    def __init__ (self, base = None, umask = '002', **kwargs):
        Cache.__init__(self, **kwargs)
        self.basedir = base
        self.umask = int(umask, 0)
        
        if sys.platform.startswith("java"):
            from java.io import File
            self.file_module = File
            self.platform = "jython"
        else:
            self.platform = "cpython"
        
        if not self.access(base, 'read'):
            self.makedirs(base)
        */
		#endregion
		public DiskCache() : this("/tmp", "002") { }

		private DiskCache(string baseDir, string umask)
		{
			BaseDir = baseDir;
			UMask = umask;
		}

		public string BaseDir { get; set; }
		public string SecondaryDir { get; set; }
		public string UMask { get; set; }

		#region python - makedirs
		/*
    def makedirs(self, path):
        old_umask = os.umask(self.umask)
        os.makedirs(path)
        os.umask(old_umask)
        */
		#endregion
		private static void MakeDirs(string path) => Directory.CreateDirectory(path);

		#region python - access
		/*
    def access(self, path, type='read'):
        if self.platform == "jython":
            if type == "read":
                return self.file_module(path).canRead()
            else:
                return self.file_module(path).canWrite()
        else:
            if type =="read":
                return os.access(path, os.R_OK)
            else:
                return os.access(path, os.W_OK)
        */
		#endregion

		private static bool Access(string path, AccessType accessType)
		{
			switch (accessType)
			{
				case AccessType.Read: return File.Exists(path);
				case AccessType.Write: return Directory.Exists(path);
				default: throw new ArgumentOutOfRangeException("accessType");
			}
		}

		public override string GetKey(ITile tile)
		{
			if (tile == null) throw new ArgumentNullException("tile");
			if (tile.Layer == null) throw new ArgumentNullException("tile.Layer");
			if (string.IsNullOrEmpty(tile.Layer.Name)) throw new ArgumentNullException("tile.Layer.Name");

			//Return filename with highest priority.
			foreach (var name in GetFileNames(tile))
				return name;
			throw new InvalidOperationException("DiskCache was unable to get a valid file-name.");
		}

		private XYFileName m_fileNameProvider;

		private IEnumerable<string> GetFileNames(ITile tile)
		{
			if (m_fileNameProvider == null)
				m_fileNameProvider = UseQuadKey ? new QuadKeyFileName() : new XYFileName();
			
			return m_fileNameProvider.GetFileNames(tile);
		}

		public bool UseQuadKey { get; set; }

		public override byte[] Get(ITile tile)
		{
			foreach (var name in GetFileNames(tile))
			{
				var result = Get(name);
				if (result != null)
					return result;
			}
			return null;
		}

		#region python - get
		/*
    def get (self, tile):
        filename = self.getKey(tile)
        if self.access(filename, 'read'):
            tile.data = file(filename, "rb").read()
            return tile.data
        else:
            return None
        */
		#endregion

		private byte[] Get(string key)
		{
			string baseFile = Path.Combine(BaseDir, key);
			if (Access(baseFile, AccessType.Read))
				return GetFile(baseFile);
			if (!string.IsNullOrEmpty(SecondaryDir))
			{
				string secondaryFile = Path.Combine(SecondaryDir, key);
				if (File.Exists(secondaryFile))
				{
#if !COPY_FILE_FROM_PRIMARY_TO_SECONDARY_STORAGE
					return GetFile(secondaryFile);
#else
	//Copy file to basedir
					try
					{
						File.Copy(secondaryFile, baseFile);
						Trace.WriteLine("Copied " + key + " from secondary to primary storage", "DiskCache.Get");
						return GetFile(baseFile);
					}
					catch (Exception ex)
					{
						Trace.WriteLine("Failed to copy secondary file to base-dir: " + ex.Message, "DiskCache.Get");
					}
#endif
				}
				else
                {
					Trace.TraceError("DiskCache.Get: File not found at secondary storage {0}", key );
                }
			}
			return null;
		}

		public byte[] GetFile(string filename)
		{
#if DEBUG
			Trace.TraceInformation("DiskCache.GetFile: Reading from file {0}", filename );
#endif
			return File.ReadAllBytes(filename);
		}

		#region python - set
		/*
    def set (self, tile, data):
        if self.readonly: return data
        filename = self.getKey(tile)
        dirname  = os.path.dirname(filename)
        if not self.access(dirname, 'write'):
            self.makedirs(dirname)
        tmpfile = filename + ".%d.tmp" % os.getpid()
        old_umask = os.umask(self.umask)
        output = file(tmpfile, "wb")
        output.write(data)
        output.close()
        os.umask( old_umask );
        try:
            os.rename(tmpfile, filename)
        except OSError:
            os.unlink(filename)
            os.rename(tmpfile, filename)
        tile.data = data
        return data
        */
		#endregion

		public override byte[] Set(ITile tile, byte[] data)
		{
			if (ReadOnly)
				return data;

			string filename = Path.Combine(BaseDir, GetKey(tile));
			string dirname = Path.GetDirectoryName(filename);
			if (!Access(dirname, AccessType.Write))
				MakeDirs(dirname);
			try
			{
#if DEBUG
				Trace.TraceInformation("DiskCache.Set: Writing to file {0}", filename);
#endif
				File.WriteAllBytes(filename, data);
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
			return data;
		}

		#region python - delete
		/*
    def delete (self, tile):
        filename = self.getKey(tile)
        if self.access(filename, 'read'):
            os.unlink(filename)
        */
		#endregion

		public override void Delete(ITile tile)
		{
			string filename = Path.Combine(BaseDir, GetKey(tile));
			if (Access(filename, AccessType.Read))
				File.Delete(filename);
		}

		#region python - attemptLock
		/*
    def attemptLock (self, tile):
        name = self.getLockName(tile)
        try: 
            self.makedirs(name)
            return True
        except OSError:
            pass
        try:
            st = os.stat(name)
            if st.st_ctime + self.stale < time.time():
                warnings.warn("removing stale lock %s" % name)
                # remove stale lock
                self.unlock()
                self.makedirs(name)
                return True
        except OSError:
            pass
        return False 
        */
		#endregion

		public override bool AttemptLock(ITile tile)
		{
			string name = GetLockName(tile);
			try
			{
				MakeDirs(name);
				return true;
			}
			catch (UnauthorizedAccessException)
			{
			}
			try
			{
				var st = new FileInfo(name);
				if (st.CreationTime + StaleInterval < DateTime.Now)
				{
					Trace.TraceWarning("DiskCache: Warning, removing stale lock {0}", name);
					Unlock(tile);
					MakeDirs(name);
					return true;
				}
			}
			catch (UnauthorizedAccessException)
			{
			}
			return false;
		}

		#region python - unlock
		/*
    def unlock (self, tile):
        name = self.getLockName(tile)
        try:
            os.rmdir(name)
        except OSError, E:
            print >>sys.stderr, "unlock %s failed: %s" % (name, str(E))
		 */
		#endregion
		public override void Unlock(ITile tile)
		{
			string name = GetLockName(tile);
			try
			{
				Directory.Delete(name, true);
			}
			catch (Exception ex)
			{
				Trace.TraceError("DiskCache: Warning, Unlock {0} failed:\r\n{1}", name, ex);
			}
		}


		#region Nested type: AccessType
		private enum AccessType
		{
			Read,
			Write
		} ;
		#endregion
	}
}