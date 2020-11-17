using System;
using AirPlay.Listeners;

namespace AirPlay.Models
{
	public class RaopBuffer
    {
		public bool IsEmpty { get; set; }
		public ushort FirstSeqNum { get; set; }
		public ushort LastSeqNum { get; set; }

		public RaopBufferEntry[] Entries = new RaopBufferEntry[AudioListener.RAOP_BUFFER_LENGTH];

		public int BufferSize { get; set; }
		public byte[] Buffer { get; set; }
	}
}
