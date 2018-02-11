using System;
using Facility.Definition;
using LspPosition = OmniSharp.Extensions.LanguageServer.Protocol.Models.Position;
using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Facility.LanguageServer
{
	struct Position : IComparable<Position>, IEquatable<Position>
	{
		public readonly long Line;
		public readonly long Character;

		public Position(long line, long character)
		{
			Line = line;
			Character = character;
		}

		public Position(NamedTextPosition position)
		{
			Line = position.LineNumber - 1;
			Character = position.ColumnNumber - 1;
		}

		public Position(LspPosition position)
		{
			Line = position.Line;
			Character = position.Character;
		}

		public override bool Equals(object obj)
		{
			if (obj is Position p)
				return this == p;
			return false;
		}

		public bool Equals(Position other)
		{
			return this == other;
		}

		public override int GetHashCode() => (int)(1 + 17 * Line + 13 * Character);

		public int CompareTo(Position other)
		{
			return this < other ? -1 :
				this == other ? 0 :
				1;
		}

		public static implicit operator LspPosition(Position position) => new LspPosition(position.Line, position.Character);

		public static bool operator ==(Position a, Position b) => a.Line == b.Line && a.Character == b.Character;

		public static bool operator !=(Position a, Position b) => a.Line != b.Line || a.Character != b.Character;

		public static bool operator >(Position a, Position b) => a.Line > b.Line || a.Line == b.Line && a.Character > b.Character;

		public static bool operator <(Position a, Position b) => a.Line < b.Line || a.Line == b.Line && a.Character < b.Character;

		public static bool operator >=(Position a, Position b) => a > b || a == b;

		public static bool operator <=(Position a, Position b) => a < b || a == b;

		public static bool operator ==(Position a, NamedTextPosition b) => b != null && a == new Position(b);

		public static bool operator !=(Position a, NamedTextPosition b) => b == null || a != new Position(b);

		public static bool operator >(Position a, NamedTextPosition b) => b == null || a > new Position(b);

		public static bool operator >=(Position a, NamedTextPosition b) => b == null || a >= new Position(b);

		public static bool operator <(Position a, NamedTextPosition b) => b != null && a < new Position(b);

		public static bool operator <=(Position a, NamedTextPosition b) => b != null && a <= new Position(b);
	}
}
