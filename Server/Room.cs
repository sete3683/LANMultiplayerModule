using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Room
    {
        List<MyServerSession> _sessions = new List<MyServerSession>();
        object _lock = new object();

        public void Enter(MyServerSession session)
        {
            lock (_lock)
            {
                _sessions.Add(session);
            }
        }

        public void Leave(MyServerSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
            }
        }

        public void Clear()
        {
            _sessions.Clear();
        }

        public void Broadcast(ArraySegment<byte> segment)
        {
            foreach (MyServerSession session in _sessions)
            {
                session.Send(segment);
            }
        }
    }
}
