﻿using Jack.RemoteLog.WebApi.Dtos;

namespace Jack.RemoteLog.WebApi.Infrastructures
{
    public class FileLogChannel : ILogChannel
    {

        public string ApplicationContext { get; }
        ISourceContextCollection _sourceContexts;
        ILogContentWriter _contentWriter;
        ILogContentReader _contentReader;
        public FileLogChannel(string applicationContext)
        {
            ApplicationContext = applicationContext;
            var folderPath = Global.Configuration["DataPath"] + "/" + applicationContext;
            var logFolderPath = $"{folderPath}/logs";

            if (Directory.Exists(logFolderPath) == false)
            {
                Directory.CreateDirectory(logFolderPath);
            }
            _sourceContexts = new FileSourceContextCollection(folderPath);
            _contentWriter = new LuceneContentWriter(logFolderPath);
            _contentReader = new LuceneContentReader(logFolderPath, _sourceContexts);
        }

        public void WriteLog(WriteLogModel request)
        {
            _sourceContexts.Add(request.SourceContext);
            request.SourceContextId = _sourceContexts.GetId(request.SourceContext);
            _contentWriter.Write(request);
        }

        public LogItem[] Read(string sourceContext, LogLevel? level, long startTimeStamp, long? endTimeStamp, string keyWord, string? traceId)
        {
            return _contentReader.Read(_sourceContexts, sourceContext, level, startTimeStamp, endTimeStamp, keyWord, traceId);
        }

        /// <summary>
        /// 删除指定时间之前的日志
        /// </summary>
        /// <param name="endTime"></param>
        public void DeleteLogs(long endTime)
        {
            _contentWriter.DeleteLogs(endTime);

        }

        public string[] GetAllSourceContext()
        {
            return _sourceContexts.ToArray();
        }

        public void Dispose()
        {
            _contentWriter?.Dispose();
            _contentReader?.Dispose();

            _contentWriter = null;
            _contentReader = null;
        }
    }
}
