using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Data
{
	public interface IBotDatabase
	{
		Task<bool> CheckUserAsync(String name);

		Task UpdateUserAsync(String name, Boolean value, Boolean force = false);
	}
}
