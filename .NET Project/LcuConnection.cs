using System.Text;
using System.Text.Json.Nodes;


namespace rune_maker
{
    internal class LcuConnection
    {
        private HttpClientHandler handler;
        private HttpClient client;
        private string hostClient;
        JsonNode PerkIdsNode;

        public LcuConnection(string p_authorization, string p_hostClient)
        {
            handler = new HttpClientHandler();
            PerkIdsNode = 0;
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };

            client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(p_authorization)));

            hostClient = p_hostClient;
        }
        public async void DeletePage(string id)
        {
            await client.DeleteAsync(hostClient + "/lol-perks/v1/pages/" + id);
        }
        public async Task<string> PostPage(JsonNode p_page)
        {
            var content = new StringContent(p_page.ToString(), Encoding.UTF8, "application/json");
            var result = await client.PostAsync(hostClient + "/lol-perks/v1/pages", content);
            return result.ToString();
        }

        public async Task<string> GetIdsummoner()
        {
            var jsonString = await client.GetStringAsync(hostClient + "/lol-summoner/v1/current-summoner");

            var forecastNode = JsonNode.Parse(jsonString)!;
            var idsummonerNode = forecastNode!["displayName"]!;

            return idsummonerNode.ToString();
        }

        public async Task<JsonNode> GetCurrentPerks()
        {
            var jsonString = await client.GetStringAsync(hostClient + "/lol-perks/v1/currentpage");
            var forecastNode = JsonNode.Parse(jsonString)!;
            return forecastNode;
        }
        public async Task<bool> perksChanged()
        {
            var jsonString = await client.GetStringAsync(hostClient + "/lol-perks/v1/currentpage");
            var forecastNode = JsonNode.Parse(jsonString)!;
            var PerksCheck = forecastNode!["selectedPerkIds"]!;
            string[] perksNames = new string[9];
            int checksum = 0;
            for (int i = 0; i < 9; i++)
            {
                if (PerksCheck[i]!.ToString() == PerkIdsNode[i]!.ToString())
                {
                    checksum++;
                }
            }
            if (checksum == 9)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<string> GetPerkName(string p_id)
        {
            var jsonString = await client.GetStringAsync(hostClient + "/lol-perks/v1/perks");

            var forecastNode = JsonNode.Parse(jsonString)!;
            var i = 0;
            JsonNode perk;
            var m_id = "";
            while (true)
            {
                perk = forecastNode[i]!;
                m_id = perk!["id"]!.ToString();
                if (m_id == p_id)
                {
                    break;
                }
                else
                {
                    i++;
                }
            }
            //return forecastNode!.ToString();
            return perk!["name"]!.ToString();
        }

        public async Task<string> GetPerkPathPicture(string p_id)
        {
            var jsonString = await client.GetStringAsync(hostClient + "/lol-perks/v1/perks");

            var forecastNode = JsonNode.Parse(jsonString)!;
            var i = 0;
            JsonNode perk;
            var m_id = "";
            while (true)
            {
                perk = forecastNode[i]!;
                m_id = perk!["id"]!.ToString();
                if (m_id == p_id)
                {
                    break;
                }
                else
                {
                    i++;
                }
            }
            //return forecastNode!.ToString();
            return perk!["iconPath"]!.ToString();
        }



        public async Task<Bitmap> GetPerkPicture(string path)
        {
            var response = await client.GetAsync(hostClient + path);
            var inputStream = await response.Content.ReadAsStreamAsync();
            var bitmap = new Bitmap(inputStream);
            Bitmap resized = new Bitmap(bitmap, new Size(50, 50));
            return resized;
        }

        public async Task<JsonNode> GetStyles()
        {
            var jsonString = await client.GetStringAsync(hostClient + "/lol-perks/v1/styles");
            var forecastNode = JsonNode.Parse(jsonString)!;
            return forecastNode;
        }
    }
}