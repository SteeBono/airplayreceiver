using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AirPlay.Models;
using AirPlay.Models.Enums;

namespace AirPlay.Services.Implementations
{
    public class SessionManager
    {
        private static SessionManager _current = null;
        private ConcurrentDictionary<string, Session> _sessions;

        public static SessionManager Current => _current ?? (_current = new SessionManager());

        private SessionManager()
        {
            _sessions = new ConcurrentDictionary<string, Session>();
        }

        public Task<Session> GetSessionAsync(string key)
        {
            _sessions.TryGetValue(key, out Session _session);
            return Task.FromResult(_session ?? new Session(key));
        }

        public Task CreateOrUpdateSessionAsync(string key, Session session)
        {
            _sessions.AddOrUpdate(key, session, (k, old) =>
            {
                var s = new Session(k)
                {
                    EcdhOurs = session.EcdhOurs ?? old.EcdhOurs,
                    EcdhTheirs = session.EcdhTheirs ?? old.EcdhTheirs,
                    EdTheirs = session.EdTheirs ?? old.EdTheirs,
                    EcdhShared = session.EcdhShared ?? old.EcdhShared,
                    PairVerified = session.PairVerified ?? old.PairVerified,
                    AesKey = session.AesKey ?? old.AesKey,
                    AesIv = session.AesIv ?? old.AesIv,
                    StreamConnectionId = session.StreamConnectionId ?? old.StreamConnectionId,
                    KeyMsg = session.KeyMsg ?? old.KeyMsg,
                    DecryptedAesKey = session.DecryptedAesKey ?? old.DecryptedAesKey,
                    MirroringListener = session.MirroringListener ?? old.MirroringListener,
                    AudioControlListener = session.AudioControlListener ?? old.AudioControlListener,
                    SpsPps  = session.SpsPps  ?? old.SpsPps,
                    Pts = session.Pts ?? old.Pts,
                    WidthSource = session.WidthSource ?? old.WidthSource,
                    HeightSource = session.HeightSource ?? old.HeightSource,
                    MirroringSession = session.MirroringSession ?? old.MirroringSession,
                    AudioFormat = session.AudioFormat == AudioFormat.Unknown ? old.AudioFormat : session.AudioFormat
                };
                return s;
            });
            return Task.CompletedTask;
        }
    }
}
