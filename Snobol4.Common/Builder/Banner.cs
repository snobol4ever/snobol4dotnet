namespace Snobol4.Common;

public partial class Builder
{
    public void DisplaySignOnBanner()
    {
        if (!SuppressSignOnMessage)
            DisplayBanner(false);

    }

    public void DisplayListingBanner()
    {
        if ((ShowListing || ListFileName != "") && !SuppressListingHeader)
            DisplayBanner(true);
    }

    private static void DisplayBanner(bool error)
    {
        var banner = $"""
                      Snobol4.NET - Multi-platform - v 0.1
                      Copyright © 2024-{DateTime.Now.Year} Jeffrey A. Cooper. Covered by MIT License https://opensource.org/license/mit
                      {DateTime.Now.DayOfWeek} {DateTime.Now.Day} {DateTime.Now:MMMM} {DateTime.Now.Year} {DateTime.Now:HH:mm:ss}
                                                                    

                      """;
        if (error)
            Console.Error.WriteLine(banner);
        else
            Console.WriteLine(banner);
    }
}