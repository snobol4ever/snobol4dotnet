namespace Snobol4.Common;

public partial class Builder
{
    public void DisplaySignOnBanner()
    {
        if (!BuildOptions.SuppressSignOnMessage)
            DisplayBanner(Console.Out);
    }

    // SPITBOL's listing (-LIST, -l): the first listed line brings the head (three blank lines, the identity and
    // clock lines unless -h, three more blank lines) and every page opens with "page N" after the page width
    // less eight blanks (112 at the default 120, -t sets it) and a blank line. A page holds -g less five
    // listed lines (55 at the default 60), the first four fewer under the head, and the next page starts with
    // three blank lines. Under -p or -z, the long format, a form feed takes the place of the three blank lines
    // (before the head and between pages) and a page holds -g less two lines. As sbl lays it out.
    private int _listingPage;
    private int _listingLinesOnPage;

    private bool LongListing => BuildOptions.PrinterListing || BuildOptions.FormFeedListing;

    // The identity line of the listing head: this compiler's own (whether to print SPITBOL's is Lon's question,
    // as for HOST())
    private const string ListingIdentity = "snobol4dotnet version 0.1";

    internal void WriteListingLine(string line)
    {
        var writer = ListingWriter;
        if (_listingPage == 0)
        {
            WriteListingHead(writer);
            StartListingPage(writer, BuildOptions.SuppressListingHeader ? 0 : 4);
        }
        else if (_listingPage > 0 && _listingLinesOnPage >= BuildOptions.LinesPerPage - (LongListing ? 2 : 5))
        {
            writer.Write(LongListing ? "\f" : "\n\n\n");
            StartListingPage(writer, 0);
        }

        // a line longer than the width of the listing is cut into lines of that width, mid-word, as sbl prints it; it takes one
        // line of the page
        for (; line.Length > BuildOptions.PageWidth && BuildOptions.PageWidth > 0; line = line[BuildOptions.PageWidth..])
            writer.Write(line[..BuildOptions.PageWidth] + "\n");
        writer.Write(line + "\n");
        _listingLinesOnPage++;
    }

    // The head of a listing: three blank lines (a form feed under -p and -z), the identity and clock lines unless -h, and three
    // blank lines
    private void WriteListingHead(TextWriter writer)
    {
        writer.Write(LongListing ? "\f" : "\n\n\n");
        if (BuildOptions.SuppressListingHeader)
            return;
        var now = DateTime.Now;
        var platform = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture switch
        {
            System.Runtime.InteropServices.Architecture.X64 => "x86-64",
            var other => other.ToString().ToLowerInvariant()
        };
        writer.Write($"{ListingIdentity}\n{platform}  {now:ddd MMM} {now.Day,2} {now:HH:mm:ss yyyy}\n\n\n\n");
    }

    private void StartListingPage(TextWriter writer, int used)
    {
        _listingPage++;
        var indent = BuildOptions.ListingPageFlushLeft ? "" : new string(' ', Math.Max(0, BuildOptions.PageWidth - 8));
        writer.Write($"{indent}page {_listingPage}\n\n");
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