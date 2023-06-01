using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Unimage
{
    public class UnimageHandle : IDisposable
    {
        private unsafe void* _handle;
        
        public int Width => GetWidthUnsafe();
        public int Height => GetHeightUnsafe();
        public TextureFormat Format => GetFormatUnsafe();
        private bool _disposed = false;
        
        public UnimageHandle()
        {
            CreateUnsafe();
        }

        ~UnimageHandle() {
            Dispose();
        }

        public bool CopyTo(IntPtr intPtr)
        {
            return CopyToUnsafe(intPtr);
        }
        
        public bool Resize(int width, int height)
        {
            return ResizeUnsafe((uint) width, (uint) height);
        }

        public Texture2D ToTexture2D(bool mipmap = false, bool linear = true, bool noLongerReadable = true)
        {
            var width = GetWidthUnsafe();
            var height = GetHeightUnsafe();
            var format = GetFormatUnsafe();

            var texture = new Texture2D(width, height, format, false, linear);
            var array = texture.GetRawTextureData<byte>();
            CopyToUnsafe(array);
            texture.Apply(mipmap, noLongerReadable);

            return texture;
        }

        public bool Load(byte[] data)
        {
            return LoadUnsafe(data);
        }

        private unsafe void CreateUnsafe()
        {
            _handle = Interop.unimage_create();
        }

        public string GetErrorMessage() {
            return GetErrorMessageUnsafe();
        }
        
        private unsafe bool ResizeUnsafe(uint width, uint height)
        {
            return Interop.unimage_resize(_handle, width, height) != 0;
        }

        private unsafe bool CopyToUnsafe(IntPtr intPtr)
        {
            return Interop.unimage_copy_to(_handle, intPtr.ToPointer()) != 0;
        }
        
        private unsafe bool CopyToUnsafe(NativeArray<byte> array)
        {
            var buffer = array.GetUnsafePtr();
            return Interop.unimage_copy_to(_handle, buffer) != 0;
        }

        private unsafe int GetWidthUnsafe()
        {
            return (int) Interop.unimage_get_width(_handle);
        }

        private unsafe int GetHeightUnsafe()
        {
            return (int) Interop.unimage_get_height(_handle);
        }
        
        private unsafe string GetErrorMessageUnsafe() {
            return Marshal.PtrToStringAnsi(new IntPtr(Interop.unimage_get_error_message(_handle)));
        }
        
        private unsafe TextureFormat GetFormatUnsafe()
        {
            var format = Interop.unimage_get_format(_handle);

            return format switch
            {
                1 => TextureFormat.RGB24,
                2 => TextureFormat.RGBA32,
                _ => TextureFormat.RGB24
            };
        }

        private unsafe bool LoadUnsafe(byte[] data)
        {
            fixed (void* buffer = data)
            {
                return Interop.unimage_load(_handle, buffer, (uint) data.Length) != 0;
            }
        }

        public unsafe void Dispose()
        {
            if (_disposed) return;
            Interop.unimage_free(_handle);
            _disposed = true;
        }
    }
}