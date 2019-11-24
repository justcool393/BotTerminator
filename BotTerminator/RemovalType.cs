using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator
{
	public enum RemovalType
	{
		None   = 0,
		Remove = 1 << 0,
		Spam   = 1 << 1,
	}
}
