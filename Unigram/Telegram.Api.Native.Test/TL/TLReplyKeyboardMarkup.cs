// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL
{
	public partial class TLReplyKeyboardMarkup : TLReplyMarkupBase 
	{
		[Flags]
		public enum Flag : Int32
		{
			Resize = (1 << 0),
			SingleUse = (1 << 1),
			Selective = (1 << 2),
		}

		public bool IsResize { get { return Flags.HasFlag(Flag.Resize); } set { Flags = value ? (Flags | Flag.Resize) : (Flags & ~Flag.Resize); } }
		public bool IsSingleUse { get { return Flags.HasFlag(Flag.SingleUse); } set { Flags = value ? (Flags | Flag.SingleUse) : (Flags & ~Flag.SingleUse); } }
		public bool IsSelective { get { return Flags.HasFlag(Flag.Selective); } set { Flags = value ? (Flags | Flag.Selective) : (Flags & ~Flag.Selective); } }

		public Flag Flags { get; set; }
		public TLVector<TLKeyboardButtonRow> Rows { get; set; }

		public TLReplyKeyboardMarkup() { }
		public TLReplyKeyboardMarkup(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.ReplyKeyboardMarkup; } }

		public override void Read(TLBinaryReader from)
		{
			Flags = (Flag)from.ReadInt32();
			Rows = TLFactory.Read<TLVector<TLKeyboardButtonRow>>(from);
		}

		public override void Write(TLBinaryWriter to)
		{
			to.WriteInt32((Int32)Flags);
			to.WriteObject(Rows);
		}
	}
}