namespace Snobol4.Common;

public partial class Builder
{
    public void DisplaySignOnBanner()
    {
        if (!BuildOptions.SuppressSignOnMessage)
            DisplayBanner(Console.Out);
    }

    public void DisplayListingBanner()
    {
        if ((BuildOptions.ShowListing || BuildOptions.ListFileName != "") && !BuildOptions.SuppressListingHeader)
            DisplayBanner(Console.Error);
    }

    private static void DisplayBanner(TextWriter writer)
    {
        var now = DateTime.Now;
        var banner = $"""
                      Snobol4.NET - Multi-platform - v 0.1
                      Copyright © 2024-{now.Year} Jeffrey A. Cooper and Lon Jones Cherryholmes. Licensed under AGPL-3.0 https://www.gnu.org/licenses/agpl-3.0.html
                      {now.DayOfWeek} {now.Day} {now:MMMM} {now.Year} {now:HH:mm:ss}
                                                                    

                      """;
        writer.WriteLine(banner);
    }
}