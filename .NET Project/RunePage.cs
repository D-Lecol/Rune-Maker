using System.Text.Json.Nodes;

namespace rune_maker
{
    internal class RunePage
    {
        public string[,,] styleIds { get; private set; } = new string[5, 5, 4];
        public string[,,] styleNames { get; private set; } = new string[5, 5, 4];
        public string[,,] styleIconPath { get; private set; } = new string[5, 5, 4];
        public Bitmap[,,] styleIconBitmap { get; private set; } = new Bitmap[5, 5, 4];

        public string[,] statsIds { get; private set; } = new string[3, 3];
        public string[,] statsNames { get; private set; } = new string[3, 3];
        public string[,] statsIconPath { get; private set; } = new string[3, 3];
        public Bitmap[,] statsIconBitmap { get; private set; } = new Bitmap[3, 3];

        //private string[,] statsBranch = new string[3, 3];

        private LcuConnection con;
        public RunePage(string authorization, string hostClient)
        {
            con = new LcuConnection(authorization, hostClient);
        }

        public async Task InitRunePage()
        {
            JsonNode stylesNode = await con.GetStyles();

            //initialisation des styles
            for (int i = 0; i < 5; i++)
            {
                styleIds[i, 0, 0] = stylesNode[i]!["id"]!.ToString();
                styleNames[i, 0, 0] = stylesNode[i]!["name"]!.ToString();
                styleIconPath[i, 0, 0] = stylesNode[i]!["iconPath"]!.ToString();
                styleIconBitmap[i, 0, 0] = await con.GetPerkPicture(styleIconPath[i, 0, 0]);

                //Initialisation des keystones
                for (int y = 0; y < 4; y++)
                {
                    try
                    {
                        styleIds[i, 1, y] = stylesNode[i]!["slots"]![0]!["perks"]![y]!.ToString();
                        styleNames[i, 1, y] = await con.GetPerkName(styleIds[i, 1, y]);
                        styleIconPath[i, 1, y] = await con.GetPerkPathPicture(styleIds[i, 1, y]);
                        styleIconBitmap[i, 1, y] = await con.GetPerkPicture(styleIconPath[i, 1, y]);
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        styleIds[i, 1, y] = "";
                        styleNames[i, 1, y] = "";
                        styleIconPath[i, 1, y] = "";
                    }

                }
                //Initialisation des runes
                for (int x = 2; x < 5; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        try
                        {
                            styleIds[i, x, y] = stylesNode[i]!["slots"]![x - 1]!["perks"]![y]!.ToString();
                            styleNames[i, x, y] = await con.GetPerkName(styleIds[i, x, y]);
                            styleIconPath[i, x, y] = await con.GetPerkPathPicture(styleIds[i, x, y]);
                            styleIconBitmap[i, x, y] = await con.GetPerkPicture(styleIconPath[i, x, y]);
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            styleIds[i, x, y] = "";
                            styleNames[i, x, y] = "";
                            styleIconPath[i, x, y] = "";
                        }
                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int y = 0; y < 3; y++)
                {
                    statsIds[i, y] = stylesNode[0]!["slots"]![i + 4]!["perks"]![y]!.ToString();
                    statsNames[i, y] = await con.GetPerkName(statsIds[i, y]);
                    statsIconPath[i, y] = await con.GetPerkPathPicture(statsIds[i, y]);
                    statsIconBitmap[i, y] = await con.GetPerkPicture(statsIconPath[i, y]);
                }
            }

        }

        public int GetRowStyleByName(string p_name)
        {
            int x = -1;
            for (int i = 0; i < 5; i++)
            {
                if (styleNames[i, 0, 0] == p_name)
                {
                    x = i;
                }
            }
            return x;
        }
        public string GetStylesNames(int i, int y, int x)
        {
            return styleNames[i, y, x];
        }
        public string GetStylesIconPath(int i, int y, int x)
        {
            return styleIconPath[i, y, x];
        }
    }
}
