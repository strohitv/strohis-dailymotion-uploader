using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrohisUploadLib.Dailymotion
{
	class GroupGetResponse
	{
		/*"page": 1,
  "limit": 10,
  "explicit": false,
  "total": 140623,
  "has_more": true,
  "list": [*/
		public int page { get; set; }
		public int limit { get; set; }
		public bool Explicit { get; set; }
		public int total { get; set; }
		public bool has_more { get; set; }
		public Group[] list { get; set; }
	}
}
