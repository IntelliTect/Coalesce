using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Models
{
    internal class QueryableByteStream : Stream
    {
        private DbCommand? _cmd;
        private DbDataReader? _reader;
        private Stream? _queryStream;

        private int? _length;
        private int _position;
        private readonly IQueryable<byte[]> _query;

        public QueryableByteStream(IQueryable<byte[]> query)
        {
            _query = query;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _length ??= _query.Select(c => c == null ? 0 : c.Length).FirstOrDefault();

        public override long Position
        {
            get => _position;
            set
            {
                CloseStream();
                _position = (int)value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = GetStream().Read(buffer, offset, count);
            _position += read;
            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var read = await GetStream().ReadAsync(buffer, offset, count, cancellationToken);
            _position += read;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long tempPosition = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => Length + offset,
                _ => throw new ArgumentException(nameof(origin))
            };

            if (tempPosition < 0 || tempPosition > Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return Position = tempPosition;
        }

        private Stream GetStream()
        {
            if (_queryStream != null) return _queryStream;

            CloseStream();

            _cmd = _query.CreateDbCommand();
            _cmd.Connection!.Open();


            if (_position > 0)
            {
                var connectionTypeName = _cmd.Connection.GetType().FullName ?? "";
                var cmdText = _cmd.CommandText;
                var selectEnd = cmdText.IndexOf("SELECT") + 7;
                var fromStart = cmdText.IndexOf("FROM");

                // EF Core has no query translation mechanism for selecting a subset of a byte array.
                // So, we'll build our own by injecting SUBSTRING into the query (substring works on binary too).
                if (connectionTypeName.Contains("SqlClient", StringComparison.Ordinal))
                {
                    _cmd.CommandText = cmdText
                        // Plus one to position because SQL Server's SUBSTRING is 1-based, not 0-based.
                        // https://docs.microsoft.com/en-us/sql/t-sql/functions/substring-transact-sql?view=sql-server-ver15
                        .Insert(fromStart, $", {_position + 1}, {Length - _position}) ")
                        .Insert(selectEnd, $" SUBSTRING(");
                }
                else if (connectionTypeName.Contains("Sqlite", StringComparison.Ordinal))
                {
                    // To get streamable bytes from sqlite, we need to include "rowid" in the query.
                    // See https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitedatareader.getstream?view=msdata-sqlite-6.0.0
                    // Unfortunately, we need to qualify "rowid" when we inject it because otherwise it'll
                    // be ambiguous if the query contains any joins.

                    // So, we'll settle for non-streamable results since Sqlite is really only for running tests anyway.
                    // Nobody actually uses it in prod.
                    _cmd.CommandText = cmdText
                        .Insert(fromStart, $", {_position + 1}, {Length - _position}) ")
                        .Insert(selectEnd, $" substr(");
                }
                else
                {
                    // TODO: consider adding support for other provides. Postgres probably?
                    throw new NotSupportedException("Unknown or unsupported database provider for file streaming: " + connectionTypeName);
                }
            }

            _reader = _cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess);
            return _queryStream = _reader.Read() && !_reader.IsDBNull(0)
                ? _reader.GetStream(0)
                : new MemoryStream();
        }

        private void CloseStream()
        {
            _queryStream?.Dispose();
            _reader?.Dispose();
            _cmd?.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseStream();
            }
            base.Dispose(disposing);
        }

        public override void Flush() { }

        public override void SetLength(long value) => throw new InvalidOperationException();

        public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException();
    }
}
