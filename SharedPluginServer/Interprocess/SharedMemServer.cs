using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedMemory;


namespace SharedPluginServer
{
    public class SharedMemServer:IDisposable
    {
        private SharedMemory.SharedArray<byte> _sharedBuf;

        private bool _isOpen;

        private static readonly log4net.ILog log =
   log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


       

        public void Init(int size)
        {
            _sharedBuf=new SharedArray<byte>("MainSharedMem",size);
            _isOpen = true;


        }

        public void Resize(int newSize)
        {


            if (_sharedBuf.Length != newSize)
            {
                _sharedBuf.Close();
                _sharedBuf = new SharedArray<byte>("MainSharedMem", newSize);
            }
        }

        public void WriteBytes(byte[] bytes)
        {
            if (_isOpen)
            {
                if (bytes.Length > _sharedBuf.Length)
                {
                  //  log.Info("______RESIZING SHARED MEM");
                    Resize(bytes.Length);
                }
                _sharedBuf.Write(bytes);
            }
        }

        public void Dispose()
        {
            _isOpen = false;
            _sharedBuf.Close();
        }

      

      

    }

   

  

}
