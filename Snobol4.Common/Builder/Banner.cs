namespace Snobol4.Common;

public partial class Builder
{
    public void DisplaySignOnBanner()
    {
        if (!BuildOptions.SuppressSignOnMessage)
            DisplayBanner(Console.Out);
    }

    // SPITBOL's listing (-LIST): the first listed line brings the head (three blank lines, the identity and
    // clock lines unless -h, three more blank lines) and every page opens with "page N" after 112 blanks and
    // a blank line; a page holds 55 listed lines, the first 51 under the head, and the next page starts with
    // three blank lines, as sbl lays it out
    private int _listingPage;
    private int _listingLinesOnPage;

    // The identity line of the listing head: this compiler's own (whether to print SPITBOL's is Lon's question,
    // as for HOST())
    private const string ListingIdentity = "snobol4dotnet version 0.1";

    internal void WriteListingLine(string line)
    {
        var writer = ListingWriter;
        if (_listingPage == 0)
        {
            writer.Write("\n\n\n");
            if (!BuildOptions.SuppressListingHeader)
            {
                var now = DateTime.Now;
                var platform = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture switch
                {
                    System.Runtime.InteropServices.Architecture.X64 => "x86-64",
                    var other => other.ToString().ToLowerInvariant()
                };
                writer.Write($"{ListingIdentity}\n{platform}  {now:ddd MMM} {now.Day,2} {now:HH:mm:ss yyyy}\n\n\n\n");
            }
            StartListingPage(writer, BuildOptions.SuppressListingHeader ? 0 : 4);
        }
        else if (_listingLinesOnPage >= 55)
        {
            writer.Write("\n\n\n");
            StartListingPage(writer, 0);
        }

        writer.Write(line + "\n");
        _listingLinesOnPage++;
    }

    private void StartListingPage(TextWriter writer, int used)
    {
        _listingPage++;
        writer.Write($"{new string(' ', 112)}page {_listingPage}\n\n");
        _listingLinesOnPage = used;
    }

    private static void DisplayBanner(TextWriter writer)
    {
        var now = DateTime.Now;
        var banner = $"""
                      Snobol4.NET - Multi-platform - v 0.1
                      Copyright © 2024-{now.Year} Jeffrey A. Cooper. Covered by MIT License https://opensource.org/license/mit
                      {now.DayOfWeek} {now.Day} {now:MMMM} {now.Year} {now:HH:mm:ss}
                                                                    

                      
                      """;
        writer.WriteLine(banner);
    }
}