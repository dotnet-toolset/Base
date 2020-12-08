using Base.Lang;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Base.Logging.Impl
{
    class FileAppender : AsyncAppender
    {
        protected readonly string Base;
        protected readonly string _name;
        private readonly bool _append;

        protected TextWriter Writer;
        public int RollFiles = 3;

        internal FileAppender(string aBase, string aName, bool aAppend)
        {
            _name = aName;
            Base = aBase;
            _append = aAppend;
        }

        protected override async Task FlushAsync(CancellationToken ct)
        {
            try
            {
                if (IsDisposed || Writer == null) return;
                var writer = Writer;
                if (writer != null) await Writer.FlushAsync();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Bugs.Break(e);
            }
        }


        protected Stream CheckWritable(string path)
        {
            Stream result = null;
            try
            {
                var dir = Path.GetDirectoryName(path);
                var name = Path.GetFileName(path);

                void Roll(string p, int i)
                {
                    if (!File.Exists(p)) return;
                    if (i >= RollFiles)
                        File.Delete(p);
                    else
                        File.Move(p, Path.Combine(dir, name + '.' + (i + 1)));
                }

                try
                {
                    var rolled = Directory.GetFiles(dir, name + ".*")
                        .Select(n => n.SplitLast('.', out _, out var num) && int.TryParse(num, out var v) ? v : -1)
                        .Where(n => n > 0).OrderByDescending(n => n).ToList();
                    foreach (var i in rolled)
                    {
                        var p = Path.Combine(dir, name + '.' + i);
                        Roll(p, i);
                    }

                    if (File.Exists(path))
                        Roll(path, 0);
                }
                catch
                {
                }

                result = new FileStream(path, _append ? FileMode.Append : FileMode.Create, FileAccess.Write,
                    FileShare.Read);
            }
            catch
            {
                try
                {
                    result?.Dispose();
                }
                catch
                {
                }
            }

            return result;
        }


        protected virtual bool Check()
        {
            if (Writer != null) return true;
            if (!Directory.Exists(Base))
            {
                Directory.CreateDirectory(Base);
                if (!Directory.Exists(Base)) return false;
            }

            try
            {
                var vStream = CheckWritable(Path.Combine(Base, _name + ".log")) ??
                              CheckWritable(Path.Combine(Base,
                                  _name + ("-" + Process.GetCurrentProcess().Id) + ".log"));
                if (vStream != null)
                    Writer = new StreamWriter(vStream);
            }
            catch
            {
                return false;
            }

            return Writer != null;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            var writer = Writer;
            if (writer != null)
            {
                Writer.Flush();
                Writer.Close();
                Writer = null;
            }
        }

        protected override async Task AppendAsync(ILogger logger, LogMessage message, CancellationToken ct)
        {
            if (IsDisposed || !Check()) return;
            try
            {
                var line = logger.Formatter.Format(logger, message);
                await Writer.WriteLineAsync(line);
                if (message.Exception != null)
                    await Writer.WriteLineAsync(message.Exception.ToString());
            }
            catch (ThreadAbortException)
            {
                // not sure why but happens sometimes in tests
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                // this must be something serious. 
                Bugs.Break(e);
            }
        }
    }
}