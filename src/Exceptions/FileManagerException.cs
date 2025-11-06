namespace Lib.FileManager.Net8.Src.Exceptions;

public class FileManagerException : Exception
{
    public FileManagerException(string message) : base(message)
    {
    }

    public FileManagerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class FileValidationException(string message) : FileManagerException(message)
{
}

public class FileStorageException : FileManagerException
{
    public FileStorageException(string message) : base(message)
    {
    }

    public FileStorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}