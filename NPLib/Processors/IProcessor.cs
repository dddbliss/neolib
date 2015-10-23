using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib.Processors
{
	public interface IProcessor
	{
		void Process(string _pageSource);
	}
}
