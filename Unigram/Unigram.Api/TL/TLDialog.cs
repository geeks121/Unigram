// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL
{
	public partial class TLDialog : TLObject, ITLReadMaxId 
	{
		[Flags]
		public enum Flag : Int32
		{
			Pinned = (1 << 2),
			Pts = (1 << 0),
			Draft = (1 << 1),
		}

		public bool IsPinned { get { return Flags.HasFlag(Flag.Pinned); } set { Flags = value ? (Flags | Flag.Pinned) : (Flags & ~Flag.Pinned); } }
		public bool HasPts { get { return Flags.HasFlag(Flag.Pts); } set { Flags = value ? (Flags | Flag.Pts) : (Flags & ~Flag.Pts); } }
		public bool HasDraft { get { return Flags.HasFlag(Flag.Draft); } set { Flags = value ? (Flags | Flag.Draft) : (Flags & ~Flag.Draft); } }

		public Flag Flags { get; set; }
		public TLPeerBase Peer { get; set; }
		public Int32 TopMessage { get; set; }
		public Int32 ReadInboxMaxId { get; set; }
		public Int32 ReadOutboxMaxId { get; set; }
		public Int32 UnreadCount { get; set; }
		public Int32 UnreadMentionsCount { get; set; }
		public TLPeerNotifySettingsBase NotifySettings { get; set; }
		public Int32? Pts { get; set; }
		public TLDraftMessageBase Draft { get; set; }

		public TLDialog() { }
		public TLDialog(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.Dialog; } }

		public override void Read(TLBinaryReader from)
		{
			Flags = (Flag)from.ReadInt32();
			Peer = TLFactory.Read<TLPeerBase>(from);
			TopMessage = from.ReadInt32();
			ReadInboxMaxId = from.ReadInt32();
			ReadOutboxMaxId = from.ReadInt32();
			UnreadCount = from.ReadInt32();
			UnreadMentionsCount = from.ReadInt32();
			NotifySettings = TLFactory.Read<TLPeerNotifySettingsBase>(from);
			if (HasPts) Pts = from.ReadInt32();
			if (HasDraft) Draft = TLFactory.Read<TLDraftMessageBase>(from);
		}

		public override void Write(TLBinaryWriter to)
		{
			UpdateFlags();

			to.WriteInt32((Int32)Flags);
			to.WriteObject(Peer);
			to.WriteInt32(TopMessage);
			to.WriteInt32(ReadInboxMaxId);
			to.WriteInt32(ReadOutboxMaxId);
			to.WriteInt32(UnreadCount);
			to.WriteInt32(UnreadMentionsCount);
			to.WriteObject(NotifySettings);
			if (HasPts) to.WriteInt32(Pts.Value);
			if (HasDraft) to.WriteObject(Draft);
		}

		private void UpdateFlags()
		{
			HasPts = Pts != null;
			HasDraft = Draft != null;
		}
	}
}