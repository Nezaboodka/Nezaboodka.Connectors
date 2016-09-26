﻿using System;
using System.IO;
using System.Reflection;

namespace Nezaboodka
{
    public partial class FileContentHandler
    {
        public const int ReadingToFile = 0;
        public const int ReadingToStream = 1;
        public const int ReadingToBuffer = 2;

        // Public

        public int ReadingDestination { get; set; }
        public string LocalFilesFolder { get; set; }

        public FileContentHandler()
        {
            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            LocalFilesFolder = Path.Combine(Path.GetDirectoryName(assembly.Location), "NezaboodkaCachedFiles");
            Directory.CreateDirectory(LocalFilesFolder);
        }

        public virtual void WriteFilesToStream(WriteObjectsRequest request, Stream stream)
        {
            if (request.FileObjects != null)
                foreach (FileObject fileObject in request.FileObjects)
                    WriteFileToStream(fileObject, fileObject.FileContent.FileRange.Position,
                        fileObject.FileContent.FileRange.Length, stream);
        }

        public virtual void ReadFilesFromStream(ReadObjectsResponse response, Stream stream)
        {
            if (response.FileObjects != null)
                foreach (FileObject fileObject in response.FileObjects)
                    ReadFileFromStream(fileObject, fileObject.FileContent.FileRange.Position,
                        fileObject.FileContent.FileRange.Length, stream);
        }

        // WriteFilesToStream

        protected virtual void WriteFileToStream(FileObject fileObject, long position, long length, Stream stream)
        {
            object fileData = fileObject.FileContent.FileData;
            if (fileData is string)
                WriteFromFile(fileData as string, position, length, stream);
            else if (fileData is Stream)
                WriteFromStream(fileData as Stream, position, length, stream);
            else if (fileData.GetType() == typeof(byte[]))
            {
                byte[] buffer = fileData as byte[];
                if (position >= 0 && position <= int.MaxValue && length >= 0 && length <= buffer.Length)
                    WriteFromBuffer(buffer, position, length, stream);
                else
                    throw new NezaboodkaException(string.Format(
                        "wrong buffer range [{0}, {1}] specified for file object {2} (file name: {3})",
                        position, length, fileObject.Key, fileObject.FileName));
            }
            else
                throw new NezaboodkaException(string.Format(
                    "type {0} in FileObject.FileObjectMetadata.FileData is not supported, file object {1} (file name: {2})",
                    fileData.GetType(), fileObject.Key, fileObject.FileName));
        }

        protected virtual void WriteFromFile(string filePath, long position, long length, Stream destination)
        {
            using (var source = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                WriteFromStream(source, position, length, destination);
            }
        }

        protected virtual void WriteFromStream(Stream source, long position, long length, Stream destination)
        {
            if (position != 0)
                source.Seek(position, SeekOrigin.Begin);
            var buffer = new byte[Const.DefaultFileBlockSize];
            int count = 0;
            while (length - count > 0)
            {
                int n = (int)Math.Min(buffer.Length, length - count);
                n = source.Read(buffer, 0, n);
                if (n > 0)
                {
                    destination.Write(buffer, 0, n);
                    count += n;
                }
            }
        }

        protected virtual void WriteFromBuffer(byte[] source, long position, long length, Stream destination)
        {
            destination.Write(source, (int)position, (int)length);
        }

        // ReadFilesFromStream

        protected virtual void ReadFileFromStream(FileObject fileObject, long position, long length, Stream stream)
        {
            switch (ReadingDestination)
            {
                case ReadingToFile:
                    string filePath = Path.Combine(LocalFilesFolder, fileObject.Key.ToString());
                    fileObject.FileContent.FileData = filePath;
                    ReadToFile(stream, filePath, position, length);
                    break;
                case ReadingToStream:
                    var destination = CreateStream(fileObject, position, length);
                    fileObject.FileContent.FileData = destination;
                    ReadToStream(stream, destination, position, length);
                    break;
                case ReadingToBuffer:
                    var buffer = new byte[(int)fileObject.FileContent.FileRange.Length];
                    fileObject.FileContent.FileData = buffer;
                    ReadToBuffer(stream, buffer, position, length);
                    break;
                default:
                    throw new NezaboodkaException(string.Format("unknown value {0} in FileContentHandler.ReadingDestination",
                        ReadingDestination));
            }
        }

        protected virtual Stream CreateStream(FileObject fileObject, long position, long length)
        {
            return new MemoryStream((int)fileObject.FileContent.FileRange.Length);
        }

        protected virtual void ReadToFile(Stream source, string filePath, long position, long length)
        {
            using (var destination = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                ReadToStream(source, destination, position, length);
            }
        }

        protected virtual void ReadToStream(Stream source, Stream destination, long position, long length)
        {
            destination.Position = position;
            var buffer = new byte[Const.DefaultFileBlockSize];
            int count = 0;
            while (length - count > 0)
            {
                int n = source.Read(buffer, 0, (int)Math.Min(buffer.Length, length - count));
                destination.Write(buffer, 0, n);
                count += n;
            }
        }

        protected virtual void ReadToBuffer(Stream source, byte[] destination, long position, long length)
        {
            int count = 0;
            while (length - count > 0)
            {
                count += source.Read(destination, count, (int)length - count);
            }
        }
    }
}