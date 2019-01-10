using PT.PM.Common.Exceptions;
using PT.PM.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using PT.PM.Common.Files;

namespace PT.PM.Common.CodeRepository
{
    public class FileCodeRepository : SourceCodeRepository
    {
        protected IEnumerable<string> fullNames;

        public FileCodeRepository(string fileName, Language language = null, SerializationFormat? format = null)
            : this(new string[] { fileName }, language, format)
        {
        }

        public FileCodeRepository(IEnumerable<string> fileNames, Language language = null, SerializationFormat? format = null)
            : base(format)
        {
            fullNames = fileNames;
            if (language != null)
            {
                Languages = new HashSet<Language>() { language };
            }
            RootPath = string.Join("; ", fileNames);
        }

        public override IEnumerable<string> GetFileNames() => fullNames;

        public override IFile ReadFile(string fileName)
        {
            IFile result;
            string rootPath = "";
            string name = "";

            try
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    rootPath = Path.GetDirectoryName(fileName);
                }
                name = Path.GetFileName(fileName);
                result = Read(fileName);
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                result = CodeFile.Empty;
                Logger.LogError(new ReadException(result, ex));
            }

            result.RootPath = rootPath;
            result.Name = name;

            return result;
        }

        public override bool IsFileIgnored(string fileName, bool withParser)
        {
            if (Languages.Count == 1)
            {
                return withParser && !Languages.First().IsParserExists();
            }

            return base.IsFileIgnored(fileName, withParser);
        }

        protected virtual IFile Read(string fileName)
        {
            IFile result;
            if (Format == SerializationFormat.MsgPack)
            {
                result = new BinaryFile(FileExt.ReadAllBytes(fileName));
            }
            else
            {
                result = new CodeFile(FileExt.ReadAllText(fileName));
            }
            return result;
        }
    }
}
