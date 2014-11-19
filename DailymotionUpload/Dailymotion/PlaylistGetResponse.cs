using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrohisUploadLib.Dailymotion
{
	public class PlaylistGetResponse
	{
		/*
		 "page": 1,
  "limit": 100,
  "explicit": false,
  "total": 12,
  "has_more": false,
  "list": [
	{
	  "id": "x2p3xn",
	  "name": "Lets ReduZock",
	  "owner": "x1b4bbe"
	}
  ]*/
		public int page { get; set; }
		public int limit { get; set; }
		public bool Explicit { get; set; }
		public int total { get; set; }
		public bool has_more { get; set; }
		public Playlist[] list { get; set; }
	}
}
