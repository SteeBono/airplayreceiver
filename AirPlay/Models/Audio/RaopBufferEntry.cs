namespace AirPlay.Models
{
	public struct RaopBufferEntry
	{
		public bool Available { get; set; }

		public byte Flags { get; set; }
		public byte Type { get; set; }
		public ushort SeqNum { get; set; }
		public uint TimeStamp { get; set; }
		public uint SSrc { get; set; }

		public int AudioBufferSize { get; set; }
		public int AudioBufferLen { get; set; }
		public byte[] AudioBuffer { get; set; }
	}
}
