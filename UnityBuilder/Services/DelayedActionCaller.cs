using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace UnityBuilder.Services
{
    internal class DelayedActionCaller : IDisposable
    {
        private readonly Action<string> _action;
        private readonly object _lock;
        private Timer _timer;
        private readonly List<string> _data;

        public DelayedActionCaller(Action<string> action, int delay = 300)
        {
            _action = action;
            _data = new List<string>();
            _lock = new object();
            if (action != null)
                _timer = new Timer(HandleInternal, null, 0, delay);
        }

        public void Handle(string data)
        {
            if (_action == null)
                return;
            lock (_lock)
            {
                _data.Add(data);
            }
        }

        private void HandleInternal(object state)
        {
            StringBuilder sb = new StringBuilder();
            lock (_lock)
            {
                foreach (var s in _data)
                    sb.Append($"{s}\n");
            }
            if (sb.Length > 0)
                _action?.Invoke(sb.ToString());
        }

        public void Dispose()
        {
            _timer.Dispose();
            _timer = null;
            HandleInternal(null);
        }
    }
}
