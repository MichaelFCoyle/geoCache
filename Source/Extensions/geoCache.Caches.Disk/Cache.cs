//
// File: Cache.cs
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
using System.Threading;
using GeoCache.Core;

namespace GeoCache.Caches.Disk
{
    public abstract class Cache : ICache
    {
        public TimeSpan Timeout { get; set; }

        public TimeSpan StaleInterval { get; set; }

        public bool ReadOnly { get; set; }

        #region python
        /*
    def __init__ (self, timeout = 30.0, stale_interval = 300.0, readonly = False, **kwargs):
        self.stale    = float(stale_interval)
        self.timeout = float(timeout)
        self.readonly = readonly
		*/
        #endregion
        public Cache() : this(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(300), false) { }

        public Cache(TimeSpan timeout, TimeSpan staleInterval, bool readOnly)
        {
            Timeout = timeout;
            StaleInterval = staleInterval;
            ReadOnly = readOnly;
        }

        #region python
        /*        
    def lock (self, tile, blocking = True):
        start_time = time.time()
        result = self.attemptLock(tile)
        if result:
            return True
        elif not blocking:
            return False
        while result is not True:
            if time.time() - start_time > self.timeout:
                raise Exception("You appear to have a stuck lock. You may wish to remove the lock named:\n%s" % self.getLockName(tile)) 
            time.sleep(0.25)
            result = self.attemptLock(tile)
        return True

		 */
        #endregion
        public void Lock(ITile tile) => Lock(tile, true);

        public bool Lock(ITile tile, bool blocking)
        {
            var startTime = DateTime.Now;
            var result = AttemptLock(tile);
            
            if (result) return true;
            if (!blocking) return false;

            while (result != true)
            {
                if (DateTime.Now - startTime > Timeout)
                    throw new Exception(string.Format("You appear to have a stuck lock. You may wish to remove the lock named:\n{0}", this.GetLockName(tile)));
                Thread.Sleep(TimeSpan.FromSeconds(0.25));
                result = AttemptLock(tile);
            }
            return true;
        }

        #region python
        /*
    def getLockName (self, tile):
        return self.getKey(tile) + ".lck"
		*/
        #endregion
        public string GetLockName(ITile tile) => GetKey(tile) + ".lck";

        #region python
        /*
    def getKey (self, tile):
        raise NotImplementedError()
		*/
        #endregion
        public abstract string GetKey(ITile tile);

        #region python
        /*
	def attemptLock (self, tile):
		raise NotImplementedError()

		*/
        #endregion
        public abstract bool AttemptLock(ITile tile);

        #region python
        /*
	def unlock (self, tile):
		raise NotImplementedError()
		*/
        #endregion
        public abstract void Unlock(ITile tile);

        #region python
        /*
	def get (self, tile):
		raise NotImplementedError()
		*/
        #endregion
        public abstract byte[] Get(ITile tile);

        #region python
        /*
	def set (self, tile, data):
		raise NotImplementedError()
		*/
        #endregion
        public abstract byte[] Set(ITile tile, byte[] data);

        #region python
        /*
	def delete(self, tile):
		raise NotImplementedError()
		 */
        #endregion
        public virtual void Delete(ITile tile) { throw new NotSupportedException(); }
    }
}
