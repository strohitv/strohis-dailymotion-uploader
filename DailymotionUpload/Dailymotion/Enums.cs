namespace StrohisUploadLib.Dailymotion
{
	public class DmUploadConstants
	{
		public static string[] Language =
		{
			"AR",
			"BN",
			"BG",
			"DE",
			"DA",
			"EN",
			"ET",
			"EU",
			"FI",
			"FR",
			"EL",
			"HE",
			"HI",
			"IS",
			"IT",
			"JV",
			"CA",
			"HR",
			"MS",
			"MR",
			"NL",
			"NO",
			"PA",
			"FA",
			"FL",
			"PL",
			"PT",
			"RU",
			"SV",
			"SR",
			"SK",
			"ES",
			"TA",
			"TE",
			"TH",
			"CS",
			"UK",
			"HU",
			"UR",
			"VI",
			"ZH",
			"HT",
			"ID",
			"JA",
			"KO",
			"LV",
			"LT",
			"RO",
			"SL",
			"TR"
		};

		public static string[] Channels = 
		{
			"auto",
			"school",
			"fun",
			"webcam",
			"shortfilms",
			"creation",
			"people",
			"lifestyle",
			"music",
			"news",
			"travel",
			"sexy",
			"sport",
			"tv",
			"tech",
			"animals",
			"videogames"
		};

		public static string[] BitUnits = 
		{
			"Bit",
			"KBit",
			"MBit",
			"GBit"
		};

		public static bool[] YesNoValue = 
		{ 
			false, 
			true 
		};
	}

	public enum Language
	{
		//ar Arabisch
		//bn Bengalisch
		//bg Bulgarisch
		//de Deutsch
		//da Dänisch
		//en Englisch
		//et Estnisch
		//eu Euskara
		arabic = 0,
		bengal = 1,
		bulgarian = 2,
		german = 3,
		danish = 4,
		english = 5,
		estonian = 6,
		euskara = 7,

		//fi Finnisch
		//fr Französisch
		//el Griechisch
		//he Hebräisch
		//hi Hindi
		//is Isländisch
		//it Italienisch
		//jv Javanisch
		finnish = 8,
		french = 9,
		greek = 10,
		hebrew = 11,
		hindi = 12,
		icelandic = 13,
		italian = 14,
		javanese = 15,

		//ca Katalan
		//hr Kroatisch
		//ms Malaiisch
		//mr Marathi
		//nl Niederländisch
		//no Norwegisch
		//pa Pandschabi
		//fa Persisch
		catalan = 16,
		croatian = 17,
		malayan = 18,
		marathi = 19,
		netherlandic = 20,
		norwegian = 21,
		punjabi = 22,
		persian = 23,

		//fl Philppinisch
		//pl Polnisch
		//pt Portugiesisch
		//ru Russisch
		//sv Schwedisch
		//sr Serbisch
		//sk Slowakisch
		//es Spanisch
		filipino = 24,
		polish = 25,
		portuguese = 26,
		russian = 27,
		swedish = 28,
		serbian = 29,
		slovakian = 30,
		spanish = 31,

		//ta Tamil
		//te Telugu
		//th Thai
		//cs Tschechisch
		//uk Ukrainisch
		//hu Ungarisch
		//ur Urdu
		//vi Vietnamesisch
		tamil = 32,
		telugu = 33,
		thai = 34,
		czech = 35,
		ukrainian = 36,
		hungarian = 37,
		urdu = 38,
		vietnamese = 39,

		//zh chinesisch
		//ht ha­i­ti­a­nisch
		//id indonesisch
		//ja japanisch
		//ko koreanisch
		//lv lettisch
		//lt litauisch
		//ro rumänisch
		chinese = 40,
		haitian = 41,
		indonesian = 42,
		japanese = 43,
		korean = 44,
		latvian = 45,
		lithuanian = 46,
		romanian = 47,

		//sl slowenisch
		//tr türkisch
		slovenian = 48,
		turkish = 49
	}

	public enum Channel
	{
		//auto - Auto & Motor
		//school - Bildung
		//fun - Comedy & Entertainment
		//webcam - Community & Blogs
		//shortfilms - Film & Kino
		//creation - Kunst & Kreatives
		//people - Leute & Familie
		//lifestyle - Lifestyle & Ratgeber
		//music - Musik
		//news - Nachrichten
		//travel - Reisen
		//sexy - Sexy
		//sport - Sport
		//tv - TV
		//tech - Tech
		//animals - Tierwelt
		//videogames - Videospiele
		auto = 0,
		school = 1,
		fun = 2,
		webcam = 3,
		shortfilms = 4,
		creation = 5,
		people = 6,
		lifestyle = 7,
		music = 8,
		news = 9,
		travel = 10,
		sexy = 11,
		sport = 12,
		tv = 13,
		tech = 14,
		animals = 15,
		videogames = 16
	}

	public enum YesNoValue
	{
		YES = 1,
		NO = 0
	}

	public enum BitUnit
	{
		Bit = 1,
		KBit = 2,
		MBit = 3,
		GBit = 4
	}
}
