using Blish_HUD.Content;
using Gw2Sharp.WebApi;

public static class AssetIdHelper
{
    public static int GetAssetIdFromRenderUrl(this RenderUrl? url)
    {
        if (url == null) return 0;

        string s = url.ToString();
        int pos = s.LastIndexOf("/") + 1;

        return int.TryParse(s.Substring(pos, s.Length - pos - 4), out int id) ? id : 0;
    }

    public static int GetAssetIdFromRenderUrl(this RenderUrl url)
    {
        if (url == null)
            return 0;

        if (url.Url is null)
            return 0;

        string s = url.ToString();
        if (s is null) return 0;

        int pos = s.LastIndexOf("/") + 1;

        return int.TryParse(s.Substring(pos, s.Length - pos - 4), out int id) ? id : 0;
    }

    public static int GetAssetIdFromRenderUrl(this string s)
    {
        int pos = s.LastIndexOf("/") + 1;

        return int.TryParse(s.Substring(pos, s.Length - pos - 4), out int id) ? id : 0;
    }

    public static AsyncTexture2D GetAssetFromRenderUrl(this RenderUrl? url)
    {
        if (url == null) return null;

        string s = url.ToString();
        int pos = url.ToString().LastIndexOf("/") + 1;

        if (int.TryParse(s.Substring(pos, s.Length - pos - 4), out int id))
        {
            return AsyncTexture2D.FromAssetId(id);
        }

        return null;
    }
}