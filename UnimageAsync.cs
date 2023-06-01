using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Unimage
{
    public static class UnimageAsync
    {
        private static readonly Queue<UnimageAsyncOperation> _queue = new();
        private class UnimageAsyncOperation
        {
            public UnimageAsyncOperationType OperationType;
            public UnimageHandle Handle;
            public TaskCompletionSource<bool> TaskCompletionSource;
            public object Parameter1;
            public object Parameter2;
            public object Parameter3;
        }
        private enum UnimageAsyncOperationType
        {
            Load,
            LoadRaw,
            Resize,
            CopyTo
        }

        private static object _workerThreadLock = new();
        private static Thread _workerThread = null;

        private static void WorkerThread()
        {
            var idleTime = 0;
            while (true)
            {
                if (idleTime > 20)
                {
                    break;
                }

                UnimageAsyncOperation asyncOperation = null;
                lock (_queue)
                {
                    if (!_queue.TryDequeue(out asyncOperation))
                        idleTime++;
                    else idleTime = 0;
                }

                if (asyncOperation != null) ExecuteAsyncOperation(asyncOperation);
            }

            lock (_workerThreadLock) _workerThread = null;
        }

        private static void ExecuteAsyncOperation(UnimageAsyncOperation operation)
        {
            switch (operation.OperationType)
            {
                case UnimageAsyncOperationType.Resize:
                    ExecuteResizeOperation(operation);
                    break;

                case UnimageAsyncOperationType.Load:
                    ExecuteLoadOperation(operation);
                    break;
                case UnimageAsyncOperationType.LoadRaw:
                    break;
                case UnimageAsyncOperationType.CopyTo:
                    ExecuteCopyToOperation(operation);
                    break;
            }
        }

        private static void ExecuteCopyToOperation(UnimageAsyncOperation operation)
        {
            operation.TaskCompletionSource.SetResult(operation.Handle.CopyTo((IntPtr) operation.Parameter1));
        }

        private static void ExecuteLoadOperation(UnimageAsyncOperation operation)
        {
            operation.TaskCompletionSource.SetResult(operation.Handle.Load((byte[]) operation.Parameter1));
        }

        private static void ExecuteResizeOperation(UnimageAsyncOperation operation)
        {
            operation.Handle.Resize((int) operation.Parameter1, (int) operation.Parameter2);
            operation.TaskCompletionSource.SetResult(true);
        }

        private static void RunWorkerThreadIfNeed()
        {
            lock (_workerThreadLock)
            {
                if (_workerThread != null) return;
                
                _workerThread = new Thread(WorkerThread);
                _workerThread.Start();
            }
        }

        public static void ResizeAsync(UnimageHandle unimageHandle, int width, int height,
            TaskCompletionSource<bool> completionSource)
        {
            lock (_queue)
            {
                _queue.Enqueue(new UnimageAsyncOperation
                {
                    Handle = unimageHandle,
                    OperationType = UnimageAsyncOperationType.Resize,
                    TaskCompletionSource = completionSource,
                    Parameter1 = width,
                    Parameter2 = height,
                });
            }

            RunWorkerThreadIfNeed();
        }
        public static void LoadAsync(UnimageHandle unimageHandle, byte[] data,
            TaskCompletionSource<bool> completionSource)
        {
            lock (_queue)
            {
                _queue.Enqueue(new UnimageAsyncOperation
                {
                    Handle = unimageHandle,
                    OperationType = UnimageAsyncOperationType.Load,
                    TaskCompletionSource = completionSource,
                    Parameter1 = data,
                });
            }

            RunWorkerThreadIfNeed();
        }
        public static void CopyToAsync(UnimageHandle unimageHandle, IntPtr pointer,
            TaskCompletionSource<bool> completionSource)
        {
            lock (_queue)
            {
                _queue.Enqueue(new UnimageAsyncOperation
                {
                    Handle = unimageHandle,
                    OperationType = UnimageAsyncOperationType.CopyTo,
                    TaskCompletionSource = completionSource,
                    Parameter1 = pointer,
                });
            }

            RunWorkerThreadIfNeed();
        }
    }

    public static class UnimageAsyncExtension
    {
        public static Task ResizeAsync(this UnimageHandle unimageHandle, int width, int height)
        {
            var completionSource = new TaskCompletionSource<bool>();
            UnimageAsync.ResizeAsync(unimageHandle, width, height, completionSource);
            return completionSource.Task;
        }

        public static Task<bool> LoadAsync(this UnimageHandle unimageHandle, byte[] data)
        {
            var completionSource = new TaskCompletionSource<bool>();
            UnimageAsync.LoadAsync(unimageHandle, data, completionSource);
            return completionSource.Task;
        }

        public static async Task<Texture2D> ToTexture2DAsync(this UnimageHandle unimageHandle, bool mipmap = false, bool linear = true, bool noLongerReadable = true)
        {
            var texture2D = new Texture2D(unimageHandle.Width, unimageHandle.Height, unimageHandle.Format,
                mipmap, linear, noLongerReadable);
            var pointer = GetPointer(texture2D.GetRawTextureData<byte>());
            await CopyToAsync(unimageHandle, pointer);
            texture2D.Apply();
            return texture2D;
        }

        public static Task<bool> CopyToAsync(this UnimageHandle unimageHandle, IntPtr pointer)
        {
            var completionSource = new TaskCompletionSource<bool>();
            UnimageAsync.CopyToAsync(unimageHandle, pointer, completionSource);
            return completionSource.Task;
        }
        
        private static unsafe IntPtr GetPointer(NativeArray<byte> array) => new IntPtr(array.GetUnsafePtr());
    }
}